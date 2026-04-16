using System.Collections;
using UnityEngine;


/// <summary>
/// 通用炮台控制基类（玩家/敌方均可复用）
/// 负责：炮台转向目标点、开火、炮弹发射/回收、武器切换
/// </summary>
public class BaseTurretController : MonoBehaviour
{
    // 通用事件（玩家/敌方均可监听）
    public static readonly string EVENT_ON_FIRE = "OnTurretFire";

    [Header("炮台基础配置")]
    public Transform turretRoot; // 炮台根物体
    public float rotateSpeed = 0f; // 旋转平滑度（0=瞬间转向，越大越快）

    [Header("开火配置")]
    public Transform muzzlePoint; // 炮口
    public float fireCooldown = 0.5f; // 冷却时间
    public float shellSpeed = 20f; // 炮弹速度
    public float shellLifeTime = 10f; // 炮弹生命周期

    [Header("武器配置")]
    public GameObject defaultShellPrefab; // 默认炮弹
    private GameObject _currentShellPrefab; // 当前使用的炮弹
    private bool _isFireable; // 是否可以开火
    private float _lastFireTime; // 上次开火时间
    private Vector3 _targetDir; // 外部传入的目标方向
    private bool _isRotatingToTarget; // 是否正在转向目标

    private bool _isInitialized; // 是否初始化

    /// <summary>
    /// 外部设置炮台转向的目标点（由InputHandler/Ai传入）
    /// </summary>
    /// <param name="targetWorldPos">世界空间目标点</param>
    public void SetTargetPos(Vector3 targetWorldPos)
    {
        if (!_isInitialized) Init();
        if (turretRoot == null) return;

        // 计算目标方向（仅水平）
        Vector3 dir = targetWorldPos - turretRoot.position;
        dir.y = 0;
        if (dir.magnitude < 0.1f)
        {
            _isRotatingToTarget = false;
            return;
        }

        _targetDir = dir.normalized;
        _isRotatingToTarget = true;
    }

    /// <summary>
    /// 切换武器（道具切换炮弹）
    /// </summary>
    /// <param name="newShellPrefab">新炮弹预制体</param>
    public virtual void SwitchShellType(GameObject newShellPrefab)
    {
        if (newShellPrefab == null)
        {
            Debug.LogWarning($"[{nameof(BaseTurretController)}] 炮弹预制体不能为空！");
            return;
        }
        _currentShellPrefab = newShellPrefab;
    }

    // 初始化
    private void Init()
    {
        _lastFireTime = -fireCooldown; // 初始无冷却
        _currentShellPrefab = defaultShellPrefab ?? defaultShellPrefab;
        _isRotatingToTarget = false;
        _isFireable = true;

        // 校验必选组件
        if (turretRoot == null)
            Debug.LogError($"[{nameof(BaseTurretController)}] 炮台根物体未配置！");
        if (muzzlePoint == null)
            Debug.LogError($"[{nameof(BaseTurretController)}] 炮口Transform未配置！");

        _isInitialized = true;
    }

    private void Awake() => Init();

    private void Update()
    {
        if (!_isInitialized || !_isRotatingToTarget) return;
        RotateTurretToTarget();
    }

    // 炮台转向核心逻辑
    private void RotateTurretToTarget()
    {
        Quaternion targetRot = Quaternion.LookRotation(_targetDir);
        if (rotateSpeed == 0)
        {
            // 立即转向
            turretRoot.rotation = targetRot;
            _isRotatingToTarget = false;
        }
        else
        {
            // 平滑转向
            turretRoot.rotation = Quaternion.Lerp(
                turretRoot.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );

            // 转向完成判断
            if (Quaternion.Angle(turretRoot.rotation, targetRot) < 0.1f)
            {
                turretRoot.rotation = targetRot;
                _isRotatingToTarget = false;
            }
        }

        // 尝试开火
        TryFire();
    }

    // 尝试触发开火（需先完成转向）
    private void TryFire()
    {
        if (!_isFireable) return;
        if (!_isInitialized || _isRotatingToTarget) return; // 未转向完成则不开火
        if (Time.time - _lastFireTime < fireCooldown) return; // 冷却中
        if (muzzlePoint == null || _currentShellPrefab == null)
        {
            Debug.LogWarning($"[{nameof(BaseTurretController)}] 炮口/炮弹预制体未配置！");
            return;
        }

        FireShell();
        _lastFireTime = Time.time;
    }

    // 触发开火带参事件，将正在开火的坦克的炮台作为参数传递
    private void TriggerFireEvent()
    {
        EventManager.Instance?.TriggerParamEvent(EVENT_ON_FIRE, gameObject);
    }

    // 开火核心逻辑（保留原有逻辑，优化对象池调用）
    private void FireShell()
    {
        // 从对象池获取炮弹
        GameObject shell = ObjectPoolManager.Instance?.GetObject(
            _currentShellPrefab,
            muzzlePoint.position,
            muzzlePoint.rotation
        );
        if (shell == null) return;

        // 触发开火事件（供音效/动画监听）
        TriggerFireEvent();

        // 炮弹移动+回收
        StartCoroutine(ShellMoveAndRecycle(shell));
    }

    // 炮弹移动与回收
    private IEnumerator ShellMoveAndRecycle(GameObject shell)
    {
        if (shell == null) yield break;

        Rigidbody rb = shell.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(muzzlePoint.forward * shellSpeed, ForceMode.VelocityChange);
        }

        float lifeTimer = 0f;
        while (lifeTimer < shellLifeTime && shell.activeSelf)
        {
            if (rb == null)
                shell.transform.Translate(Vector3.forward * shellSpeed * Time.deltaTime);
            lifeTimer += Time.deltaTime;
            yield return null;
        }

        // 回收炮弹到对象池
        ObjectPoolManager.Instance?.ReturnObject(shell);
    }

    // 禁止开火
    public void BanFire() => _isFireable = false;

    // 允许开火
    public void AllowFire() => _isFireable = true;

    // 暂停开火
    public virtual void PauseFire(float pauseTime)
    {
        StartCoroutine(PauseFireCoroutine(pauseTime));
    }

    private IEnumerator PauseFireCoroutine(float pauseTime)
    {
        BanFire();
        float timer = 0f;
        while (timer < pauseTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        AllowFire();
    }
}
