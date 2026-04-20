using System.Collections.Generic;
using UnityEngine;

public class SonicProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 15f;
    public float lifeTime = 3f;
    public float scaleSpeed = 2f; // 渐变变大的速度

    private Vector3 initialScale;
    private HashSet<Collider> hitEnemies = new HashSet<Collider>(); // 记录已伤害过的敌人，防止每帧重复扣血

    void Awake()
    {
        initialScale = transform.localScale;
    }

    void OnEnable()
    {
        transform.localScale = initialScale;
        hitEnemies.Clear();
        Invoke(nameof(Despawn), lifeTime);
    }

    void Despawn() => PoolManager.Instance.Despawn(gameObject);

    void Update()
    {
        // 向前移动
        transform.position += transform.forward * speed * Time.deltaTime;
        // 渐变变大
        transform.localScale += Vector3.one * scaleSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // 穿透墙壁不管，只检测坦克
        if (other.TryGetComponent<TankBase>(out TankBase tank) && tank.gameObject.tag != "Player") // 假设这是玩家发射的
        {
            if (!hitEnemies.Contains(other))
            {
                tank.TakeDamage(damage);
                hitEnemies.Add(other);
            }
        }
    }
}