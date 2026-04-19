using System.Drawing;
using UnityEngine;

// 抽象基类：定义了坦克所有外在表现的基础能力，不可直接挂载
[RequireComponent(typeof(Rigidbody))]
public abstract class TankBase : MonoBehaviour
{
    [Header("生命与模型配置")]
    public float maxHealth = 30f;
    public Transform chassisTransform; // 底盘模型
    public Transform turretTransform;  // 炮塔模型
    public Transform firePoint;        // 开火点

    [Header("运动与旋转配置")]
    public float moveSpeed = 5f;
    public float chassisTurnSpeed = 10f; // 底盘旋转平滑度

    [Header("战斗配置")]
    public GameObject bulletPrefab;      // 对应的子弹预制体
    public float fireCooldown = 1f;      // 开火冷却时间

    protected float currentHealth;
    protected Rigidbody rb;
    protected float lastFireTime;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void OnEnable()
    {
        currentHealth = maxHealth; // 对象池复用时重置生命值
        lastFireTime = 0f;
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
        if (bulletPrefab != null && firePoint != null && (lastFireTime + fireCooldown <= Time.time))
        {
            PoolManager.Instance.Spawn(bulletPrefab, firePoint.position, firePoint.rotation);
            lastFireTime = Time.time;
        }
    }

    /// <summary>
    /// 受击处理
    /// </summary>
    public virtual void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡逻辑：设为虚方法允许玩家/敌人有不同的死亡表现
    /// </summary>
    protected virtual void Die()
    {
        // 通用死亡逻辑：回收自身
        PoolManager.Instance.Despawn(gameObject);
    }
}