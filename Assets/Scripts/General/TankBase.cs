using System.Drawing;
using UnityEngine;

// 抽象基类：定义了坦克所有外在表现的基础能力，不可直接挂载
[RequireComponent(typeof(Rigidbody))]
public abstract class TankBase : MonoBehaviour
{
    [Header("[基础] 生命与模型配置")]
    public float maxHealth = 30f;
    public Transform chassisTransform; // 底盘根物体
    public Transform turretTransform;  // 炮塔根物体

    [Header("[基础] 运动与旋转配置")]
    public float moveSpeed = 5f;
    public float chassisTurnSpeed = 10f; // 底盘旋转平滑度

    [Header("[武器] 管理")]
    public GameObject defaultWeaponPrefab; // 默认炮台预制体（决定敌人类别/玩家初始状态）
    protected WeaponControllerBase currentWeapon; // 当前挂载的武器控制器

    [Header("[UI] 血条")]
    public HealthBarController healthBar;

    protected float currentHealth;
    // 提供获取当前生命值的公共接口
    public float CurrentHealth { get => currentHealth; }

    protected Rigidbody rb;
    private bool isDead = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        healthBar = GetComponentInChildren<HealthBarController>();
    }

    protected virtual void OnEnable()
    {
        currentHealth = maxHealth; // 对象池复用时重置生命值
        isDead = false;
        // 每次激活时，检查并重置为默认武器
        ResetWeaponToDefault();
        // 每次激活时，更新血条
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void ResetWeaponToDefault()
    {
        if (defaultWeaponPrefab == null) return;

        // 尝试从炮塔子物体中获取已有的武器控制器
        currentWeapon = turretTransform.GetComponentInChildren<WeaponControllerBase>();

        if (currentWeapon != null)
        {
            // 比对已有武器是否为默认武器。
            // 使用名字比对（去除 Instantiate 自动生成的 "(Clone)" 后缀）
            string currentName = currentWeapon.gameObject.name.Replace("(Clone)", "").Trim();
            if (currentName == defaultWeaponPrefab.name)
            {
                // 是默认武器，状态正确，直接返回
                return;
            }
            else
            {
                // 不是默认武器（可能是上一次存活时切换的），将其销毁
                Destroy(currentWeapon.gameObject);
                currentWeapon = null;
            }
        }

        // 此时 currentWeapon 必定为 null，直接初始化默认炮台
        InstantiateDefaultWeapon();
    }

    // 初始化默认武器
    private void InstantiateDefaultWeapon()
    {
        GameObject weaponObj = Instantiate(defaultWeaponPrefab, turretTransform);
        // 重命名去掉(Clone)，保证下次OnEnable时比对名字不出错
        weaponObj.name = defaultWeaponPrefab.name;
        // 确保模型位置正确归零
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;
        currentWeapon = weaponObj.GetComponent<WeaponControllerBase>();
    }

    //---------------------------------------------------------
    // 以下为提供给“大脑（Player/EnemyAI）”调用的公共底层能力
    //---------------------------------------------------------

    /// <summary>
    /// 移动底盘并自动转向
    /// </summary>
    /// <param name="moveDir">期望的移动方向（需为归一化向量）</param>
    public void MoveTank(Vector3 moveDir)
    {
        // 物理刚体移动
        rb.velocity = new Vector3(moveDir.x * moveSpeed, rb.velocity.y, moveDir.z * moveSpeed);

        // 底盘模型平滑转向
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            chassisTransform.rotation = Quaternion.Slerp(chassisTransform.rotation, targetRotation, chassisTurnSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 瞄准目标，并根据指令决定是否在瞄准完成后开火
    /// </summary>
    /// <param name="targetPos">目标世界坐标</param>
    /// <param name="tryFire">是否尝试开火</param>
    public void AimAndFire(Vector3 targetPos, bool tryFire)
    {
        Vector3 lookPoint = new Vector3(targetPos.x, turretTransform.position.y, targetPos.z);

        // 炮塔立刻转向
        turretTransform.LookAt(lookPoint);

        // 判断是否完成转向并开火
        if (tryFire)
        {
            ExecuteFire();
        }

    }

    /// <summary>
    /// 内部逻辑：执行开火
    /// </summary>
    private void ExecuteFire()
    {
        if (currentWeapon != null)
        {
            currentWeapon.TryFire(); // 调用武器控制器的接口
        }
    }

    // 提供可重写的武器切换接口
    public virtual void SwitchWeapon(GameObject newWeaponPrefab) { }

    /// <summary>
    /// 受击处理
    /// </summary>
    public virtual void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        // 防止血量变成负数
        currentHealth = Mathf.Max(currentHealth, 0);
        // 每次受伤时，调用血条脚本更新UI并显示
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        // 已死亡便不再调用死亡方法
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            Die();
        }
    }

    /// <summary>
    /// 死亡逻辑：设为虚方法允许玩家/敌人有不同的死亡表现
    /// </summary>
    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}