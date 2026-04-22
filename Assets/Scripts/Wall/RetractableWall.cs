using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class RetractableWall : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float dropDepth = 2.0f;       // 下降深度
    [SerializeField] private float moveSpeed = 3.0f;       // 移动速度
    [SerializeField] private float resetDelay = 5.0f;      // 保持下降状态的时间
    [SerializeField] private LayerMask obstacleLayers;      // 升起时需要检测的障碍层
    [SerializeField] private LayerMask triggerLayers;       // 触发开关的层

    private Vector3 _raisedPos;
    private Vector3 _loweredPos;
    private bool _isSequenceRunning = false;

    private BoxCollider _collider;
    private Rigidbody _rb;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _rb = GetComponent<Rigidbody>();

        // 初始化物理属性
        _rb.isKinematic = true;

        // 记录局部坐标位置
        _raisedPos = transform.localPosition;
        _loweredPos = _raisedPos + Vector3.down * dropDepth;
    }

    // 情况1：处理实体子弹碰撞
    private void OnCollisionEnter(Collision collision)
    {
        TryTriggerRetraction(collision.gameObject);
    }

    // 情况2：处理声波炮（Trigger）进入
    private void OnTriggerEnter(Collider other)
    {
        TryTriggerRetraction(other.gameObject);
    }

    // 核心逻辑：判断物体层级并启动序列
    private void TryTriggerRetraction(GameObject go)
    {
        // 检查是否已经在运行并检查碰撞物体的层级是否在 triggerLayers 列表中
        if (!_isSequenceRunning && ((1 << go.layer) & triggerLayers) != 0)
        {
            StartCoroutine(RetractionSequence());
        }
    }

    // 提供公共方法，以便激光炮调用
    public void TriggerRetraction()
    {
        // 如果已经在运行中，则忽略请求
        if (!_isSequenceRunning)
        {
            StartCoroutine(RetractionSequence());
        }
    }

    private IEnumerator RetractionSequence()
    {
        _isSequenceRunning = true;

        // 下降过程
        yield return MoveToPosition(_loweredPos);

        // 到达底部后关闭碰撞体，允许通行
        _collider.enabled = false;
        yield return new WaitForSeconds(resetDelay);

        // 安全检测：如果上方有单位，则循环等待直到区域清空
        while (!IsAreaClear())
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 准备升起：先开启碰撞体以便产生推力或阻挡
        _collider.enabled = true;
        yield return MoveToPosition(_raisedPos);

        _isSequenceRunning = false;
    }

    /// <summary>
    /// 平滑移动到目标局部坐标
    /// </summary>
    private IEnumerator MoveToPosition(Vector3 targetPos)
    {
        while (Vector3.SqrMagnitude(transform.localPosition - targetPos) > 0.0001f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        transform.localPosition = targetPos;
    }

    /// <summary>
    /// 使用物理重叠盒检测原始升起位置是否被角色占用
    /// </summary>
    private bool IsAreaClear()
    {
        // 计算原始位置的世界坐标中心
        // 注意：transform.parent 是为了处理父物体移动的情况
        Vector3 worldCenter = (transform.parent != null)
            ? transform.parent.TransformPoint(_raisedPos + _collider.center)
            : _raisedPos + _collider.center;

        // 计算缩放后的半长轴
        Vector3 halfExtents = Vector3.Scale(_collider.size, transform.lossyScale) * 0.5f;

        // 使用 CheckBox 替代 OverlapBox 以获得更高性能（只需知道有没有，不需要知道是谁）
        return !Physics.CheckBox(worldCenter, halfExtents, transform.rotation, obstacleLayers);
    }
}