using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : TankBase // 继承你的坦克基类
{
    public enum AIState
    {
        Patrol,         // 游走
        ChaseAndFire,   // 追逐和开火
        Death           // 死亡
    }

    [Header("[AI] 视野与感知配置")]
    public float viewRadius = 15f;      // 视野距离
    [Range(0, 360)] public float fovAngle = 120f; // 视野角度
    public float attackRange = 10f;     // 攻击距离
    public LayerMask obstacleMask; // 用于配置哪些图层会阻挡视野

    [Header("[AI] 游走配置")]
    public float patrolRadius = 20f;    // 游走范围半径

    private AIState currentState;
    private NavMeshAgent agent;
    private Transform targetPlayer;

    private Vector3 lastKnownPlayerPos; // 记忆点
    private bool hasMemory = false;     // 是否有记忆点

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();

        // 剥夺 Agent 的控制权，只让它算路，统一使用 TankBase 的刚体移动和旋转
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable(); // 重置血量

        // 对象池复用时的状态重置
        currentState = AIState.Patrol;
        hasMemory = false;

        agent.enabled = true;
        agent.speed = moveSpeed; // 同步基类速度给Agent用于计算

        // 重新寻找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) targetPlayer = player.transform;

        SetRandomPatrolPoint();
    }

    void Update()
    {
        if (currentState == AIState.Death) return;

        // 同步物理位置给 Agent，让它基于当前真实物理位置算路
        agent.nextPosition = transform.position;

        // 视野感知刷新
        CheckFOV();

        // 状态机行为更新
        switch (currentState)
        {
            case AIState.Patrol:
                UpdatePatrol();
                break;
            case AIState.ChaseAndFire:
                UpdateChaseAndFire();
                break;
        }
    }

    void FixedUpdate()
    {
        if (currentState == AIState.Death) return;

        // 调用基类统合能力,让基类推着刚体按 Agent 算出的方向走
        Vector3 moveDir = agent.desiredVelocity.normalized;
        MoveTank(moveDir);
    }

    private void CheckFOV()
    {
        if (targetPlayer == null || !targetPlayer.gameObject.activeInHierarchy) return;

        Vector3 dirToPlayer = (targetPlayer.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

        // 初步判断：距离足够，且在炮塔正前方的夹角内
        bool inRangeAndAngle = distanceToPlayer <= viewRadius && Vector3.Angle(turretTransform.forward, dirToPlayer) <= fovAngle / 2f;

        if (inRangeAndAngle)
        {
            // 射线检测判断遮挡
            if (!Physics.Raycast(turretTransform.position, dirToPlayer, distanceToPlayer, obstacleMask))
            {
                // 看到玩家，刷新记忆点，进入追逐
                lastKnownPlayerPos = targetPlayer.position;
                hasMemory = true;
                currentState = AIState.ChaseAndFire;
            }
        }
    }

    private void UpdatePatrol()
    {
        // 炮塔朝向移动方向（边走边看前面）
        AimAndFire(transform.position + agent.desiredVelocity, false);

        // 如果接近目标点，找下一个点
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            SetRandomPatrolPoint();
        }
    }

    private void UpdateChaseAndFire()
    {
        if (!hasMemory) return;

        agent.SetDestination(lastKnownPlayerPos);
        float distanceToMemory = Vector3.Distance(transform.position, lastKnownPlayerPos);

        // 无论如何，炮塔瞄准记忆点；如果距离小于攻击距离，尝试开火
        bool tryFire = distanceToMemory <= attackRange;
        AimAndFire(lastKnownPlayerPos, tryFire);

        // 如果到达了最后已知位置附近
        if (distanceToMemory <= 1f)
        {
            // 此时如果玩家不在视野里（刚才CheckFOV没有更新记忆点），说明追丢了
            // 恢复游走状态
            currentState = AIState.Patrol;
            hasMemory = false;
            SetRandomPatrolPoint();
        }
    }

    private void SetRandomPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir.y = 0f;
        randomDir += transform.position;

        // 找到最近的 NavMesh 点
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            // 预先计算路径，检查是否可达
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(hit.position, path))
            {
                // 只有当路径状态是 Complete（完全连通）时才去执行
                // 如果是 PathPartial（部分连通，即断路），则放弃本次选点
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(hit.position);
                }
                else
                {
                    // 递归调用且输出日志，表示选到了不可达区域
                    Debug.Log("选点在不可达的孤岛上，重试...");
                    SetRandomPatrolPoint();
                }
            }
        }
    }

    protected override void Die()
    {
        currentState = AIState.Death;
        agent.enabled = false;

        // TODO: 明天在这里接入爆炸特效 PoolManager.Instance.Spawn(...)

        base.Die(); // 调用基类回收自身
    }

#if UNITY_EDITOR
    // 这是一个只在编辑器显示的辅助功能，帮你可视化视野
    private void OnDrawGizmosSelected()
    {
        if (turretTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, viewRadius);

            Vector3 rightLimit = Quaternion.AngleAxis(fovAngle / 2f, Vector3.up) * turretTransform.forward;
            Vector3 leftLimit = Quaternion.AngleAxis(-fovAngle / 2f, Vector3.up) * turretTransform.forward;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(turretTransform.position, rightLimit * viewRadius);
            Gizmos.DrawRay(turretTransform.position, leftLimit * viewRadius);
        }
    }
#endif
}