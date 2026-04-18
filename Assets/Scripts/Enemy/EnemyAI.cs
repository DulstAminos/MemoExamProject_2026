using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("状态参数")]
    public float attackRange = 8f; // 攻击距离
    public float fireCooldown = 2f; // 开火冷却
    private float fireTimer;

    [Header("组件引用")]
    public Transform turretTransform; // 敌人的炮塔模型
    public Transform firePoint;       // 敌人的开火点
    public GameObject bulletPrefab;   // 子弹预制体

    private NavMeshAgent agent;
    private Transform playerTransform;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // 自动寻找场景中Tag为Player的物体
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return; // 玩家死了就不动了

        // 计算与玩家的距离
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            // 进入攻击范围：停止寻路，瞄准开火
            agent.isStopped = true;
            AimAtPlayer();

            fireTimer += Time.deltaTime;
            if (fireTimer >= fireCooldown)
            {
                Fire();
                fireTimer = 0f;
            }
        }
        else
        {
            // 超出攻击范围：追逐玩家
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);

            // 底盘朝向由 NavMeshAgent 自动处理，让炮塔也朝向正前方或者保持不动即可
            turretTransform.rotation = Quaternion.Slerp(turretTransform.rotation, transform.rotation, Time.deltaTime * 5f);
        }
    }

    void AimAtPlayer()
    {
        // 计算炮塔的目标方向 (忽略Y轴高度差)
        Vector3 direction = (playerTransform.position - turretTransform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // 平滑旋转炮塔
        turretTransform.rotation = Quaternion.Slerp(turretTransform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    void Fire()
    {
        // 利用池化系统发射子弹
        PoolManager.Instance.Spawn(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}