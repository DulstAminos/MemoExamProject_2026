using System.Collections.Generic;
using UnityEngine;

public class SonicProjectile : MonoBehaviour
{
    [Header("声波战斗配置")]
    public float damage = 15f;
    public float lifeTime = 2f;
    public float speed = 10f; // 声波推进速度

    [Header("声波成长配置 (用于配合你的粒子效果)")]
    public float maxScaleMultiplier = 20f / 3f; // 声波到最后变大多少倍

    private HashSet<Collider> hitEnemies = new HashSet<Collider>();
    private float timer;
    private Vector3 initialScale;

    void Awake()
    {
        // 记录出生时原始尺寸
        initialScale = transform.localScale;
    }

    void OnEnable()
    {
        timer = 0f;
        transform.localScale = initialScale; // 每次开火重置回基础大小
        hitEnemies.Clear();
        Invoke(nameof(Despawn), lifeTime);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void Despawn() => PoolManager.Instance.Despawn(gameObject);

    void Update()
    {
        // 让声波向前移动
        transform.position += transform.forward * speed * Time.deltaTime;

        // 时间成长逻辑
        timer += Time.deltaTime;

        // 生命周期前 1/4 的临界点
        float delayPhase = lifeTime / 4f;

        if (timer > delayPhase)
        {
            // 剩下的 3/4 时间是用来渐渐扩大的
            float growDuration = lifeTime - delayPhase;

            // 计算放大进度 progress 从 0 平滑变到 1
            float progress = (timer - delayPhase) / growDuration;

            // 使用 Lerp 进行完美线性放大（只放大根节点的X和Y方向范围）
            // 注意这里放大了Z也没有问题，触发器变厚不影响检测，甚至防漏穿模
            transform.localScale = Vector3.Lerp(initialScale, initialScale * maxScaleMultiplier, progress);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<TankBase>(out TankBase tank))
        {
            if (!hitEnemies.Contains(other))
            {
                tank.TakeDamage(damage);
                hitEnemies.Add(other);
            }
        }
    }
}