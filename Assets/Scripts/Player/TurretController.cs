using System.Collections;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    // 玩家开火事件字符常量
    private const string EVENT_ON_PLAYER_FIRE = "OnPlayerFire";

    [Header("炮塔配置")]
    // 坦克的炮台子物体
    public GameObject turret;
    // 旋转平滑度（0=瞬间转向，越大越快）
    public float rotateSpeed = 0f;

    [Header("开火核心设置")]
    [Tooltip("炮管口的Transform（炮弹生成位置/朝向）")]
    public Transform muzzlePoint;
    [Tooltip("当前使用的炮弹预制体（支持切换不同炮弹）")]
    public GameObject currentShellPrefab;
    [Tooltip("开火冷却时间（避免连续发射）")]
    public float fireCooldown = 0.5f;
    [Tooltip("炮弹飞行速度")]
    public float shellSpeed = 20f;
    [Tooltip("炮弹生命周期（超时自动回收）")]
    public float shellLifeTime = 10f;

    // 鼠标点击的世界坐标
    private Vector3 targetClickPos;
    // 上次开火时间（用于冷却判断）
    private float _lastFireTime;
    // 是否正在转向目标（用于判断转向完成状态）
    private bool _isRotatingToTarget;

    void Start()
    {
        // 初始化冷却时间（游戏开始时可立即开火）
        _lastFireTime = -fireCooldown;
        _isRotatingToTarget = false;
    }

    void Update()
    {
        // 监听鼠标左键点击（开火按键）
        if (Input.GetMouseButtonDown(0))
        {
            // 获取鼠标点击的地面世界坐标
            GetMouseClickWorldPos();
            _isRotatingToTarget = true;
            // 执行炮塔转向
            RotateTurretToClickPos();
        }

        // 持续检测转向状态（平滑旋转时需要帧更新判断）
        if (_isRotatingToTarget && turret != null)
        {
            RotateTurretToClickPos();
        }
    }

    /// <summary>
    /// 获取鼠标点击的地面世界坐标
    /// </summary>
    void GetMouseClickWorldPos()
    {
        // 从主相机向鼠标位置发射射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 射线碰撞到地面层，记录点击坐标
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, LayerMask.GetMask("Ground")))
        {
            targetClickPos = hit.point;
        }
    }

    /// <summary>
    /// 控制炮台转向目标位置，转向完成后触发开火
    /// </summary>
    void RotateTurretToClickPos()
    {
        if (turret == null)
        {
            _isRotatingToTarget = false;
            return;
        }

        // 计算水平方向向量（忽略Y轴高度差，保证仅水平面旋转）
        Vector3 dir = targetClickPos - turret.transform.position;
        dir.y = 0;

        // 目标点过近时直接退出（避免旋转抖动）
        if (dir.magnitude < 0.1f)
        {
            _isRotatingToTarget = false;
            return;
        }

        // 计算目标旋转角度（仅Y轴）
        Quaternion targetRot = Quaternion.LookRotation(dir);

        // 旋转炮塔：瞬间转向/平滑转向
        if (rotateSpeed == 0)
        {
            // 瞬间转向
            turret.transform.rotation = targetRot;
            _isRotatingToTarget = false;
            TryFireAfterRotation();
        }
        else
        {
            // 平滑转向
            turret.transform.rotation = Quaternion.Lerp(
                turret.transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
                );

            // 检测是否接近目标旋转（角度差小于0.1度视为完成）
            if (Quaternion.Angle(turret.transform.rotation, targetRot) < 0.1f)
            {
                turret.transform.rotation = targetRot; // 强制对齐
                _isRotatingToTarget = false;
                TryFireAfterRotation();
            }
        }
    }

    /// <summary>
    /// 转向完成后尝试开火（检查冷却和配置有效性）
    /// </summary>
    void TryFireAfterRotation()
    {
        // 校验核心配置（避免空引用）
        if (muzzlePoint == null)
        {
            Debug.LogWarning("未配置炮管口Transform！");
            return;
        }
        if (currentShellPrefab == null)
        {
            Debug.LogWarning("未配置炮弹预制体！");
            return;
        }

        // 检查冷却时间
        if (Time.time - _lastFireTime < fireCooldown) return;

        // 执行开火逻辑
        FireShell();
        _lastFireTime = Time.time; // 更新上次开火时间
    }

    /// <summary>
    /// 开火核心逻辑（对接对象池生成炮弹）
    /// </summary>
    void FireShell()
    {
        // 从对象池获取炮弹
        GameObject shell = ObjectPoolManager.Instance.GetObject(
            currentShellPrefab,
            muzzlePoint.position,
            muzzlePoint.rotation
        );

        // 广播玩家开关事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerEvent(EVENT_ON_PLAYER_FIRE);
        }

        // 启动炮弹飞行+自动回收协程
        StartCoroutine(ShellMoveAndRecycle(shell));
    }

    /// <summary>
    /// 控制炮弹飞行，并在生命周期结束后回收至对象池
    /// </summary>
    /// <param name="shell">炮弹对象</param>
    IEnumerator ShellMoveAndRecycle(GameObject shell)
    {
        if (shell == null) yield break;

        // 优先使用刚体实现物理运动
        Rigidbody rb = shell.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero; // 重置速度避免残留
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(shell.transform.forward * shellSpeed, ForceMode.VelocityChange);
        }

        float lifeTimer = 0f;
        while (lifeTimer < shellLifeTime && shell.activeSelf)
        {
            if (rb == null)
            {
                // 无刚体时使用直接位移（备用方案）
                shell.transform.Translate(Vector3.forward * shellSpeed * Time.deltaTime);
            }
            lifeTimer += Time.deltaTime;
            yield return null;
        }

        // 生命周期结束后回收炮弹（对象池自动管理缓存/销毁）
        ObjectPoolManager.Instance.ReturnObject(shell);
    }

    #region 扩展接口（供后续武器切换功能使用）
    /// <summary>
    /// 切换当前使用的炮弹类型（外部调用接口）
    /// </summary>
    /// <param name="newShellPrefab">新的炮弹预制体</param>
    public void SwitchShellType(GameObject newShellPrefab)
    {
        if (newShellPrefab == null)
        {
            Debug.LogWarning("切换的炮弹预制体不能为空！");
            return;
        }
        currentShellPrefab = newShellPrefab;
    }
    #endregion
}
