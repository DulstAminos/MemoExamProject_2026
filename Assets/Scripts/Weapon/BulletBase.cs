using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [Header("子弹属性配置")]
    public float speed = 15f;
    public int maxBounces = 2; // 最大反弹次数
    public float damageAmount = 10f; // 伤害
    public float lifeTime = 3f;      // 生命周期

    private int currentBounces = 0;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // 每次从对象池取出来（被激活）时调用
    void OnEnable()
    {
        currentBounces = 0; // 重置反弹次数
        rb.velocity = transform.forward * speed; // 重新赋予速度

        // 开启一个延迟回收，防止子弹永远在场上弹（3秒后回收自身）
        Invoke(nameof(DelayDespawn), lifeTime);
    }

    void OnDisable()
    {
        // 隐藏时取消所有延迟任务，防止出错
        CancelInvoke();
    }

    void DelayDespawn()
    {
        PoolManager.Instance.Despawn(gameObject);
    }

    void Update()
    {
        // 如果子弹正在移动，让其朝向对其速度方向
        if (rb.velocity.sqrMagnitude > 0.1f)
        {
            transform.forward = rb.velocity.normalized;
        }
    }

    // 强制子弹保持恒定速度（防止摩擦或碰撞导致变慢）
    void FixedUpdate()
    {
        rb.velocity = rb.velocity.normalized * speed;
    }

    // 物理碰撞检测
    void OnCollisionEnter(Collision collision)
    {
        // 判断是否撞到墙壁
        if (collision.gameObject.CompareTag("Wall"))
        {
            currentBounces++;
            if (currentBounces > maxBounces)
            {
                // 超过反弹次数，回收子弹
                PoolManager.Instance.Despawn(gameObject);
            }
        }
        // 如果撞到带有 Health 的物体（坦克）
        else if (collision.gameObject.GetComponent<Health>() != null)
        {
            collision.gameObject.GetComponent<Health>().TakeDamage(damageAmount); // 造成10点伤害
            PoolManager.Instance.Despawn(gameObject); // 子弹命中后回收
        }
    }
}