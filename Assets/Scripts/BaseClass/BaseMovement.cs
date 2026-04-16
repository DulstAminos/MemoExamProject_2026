using UnityEngine;

/// <summary>
/// 通用坦克移动基类（玩家/敌方均可复用）
/// 负责：刚体移动、移动方向管理、移动状态判断
/// </summary>
public class BaseMovement : MonoBehaviour
{
    [Header("移动配置")]
    public float moveSpeed = 8f; // 移动速度

    private Rigidbody _rb;
    private Vector3 _worldMoveDir; // 世界空间移动方向
    private bool _isInitialized; // 初始化标记

    /// <summary>
    /// 外部设置移动方向（由InputHandler传入）
    /// </summary>
    /// <param name="dir">归一化的世界空间移动方向</param>
    public void SetMoveDirection(Vector3 dir)
    {
        if (!_isInitialized) Init();
        _worldMoveDir = dir.normalized;
    }

    /// <summary>
    /// 是否处于移动状态
    /// </summary>
    public bool IsMoving() => _worldMoveDir.magnitude > 0.1f;

    /// <summary>
    /// 获取当前移动方向
    /// </summary>
    public Vector3 GetMoveDir() => _worldMoveDir;

    private void Init()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError($"[{nameof(BaseMovement)}] 物体{gameObject.name}缺少Rigidbody组件！");
            enabled = false;
            return;
        }
        _isInitialized = true;
    }

    private void Awake() => Init();

    private void FixedUpdate()
    {
        if (!_isInitialized || !IsMoving()) return;
        if (_worldMoveDir.magnitude <= 0.1f) return;

        // 刚体平滑移动
        Vector3 targetPos = _rb.position + _worldMoveDir * moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(targetPos);
    }

    // 预留扩展：重置移动状态
    public virtual void ResetMovement()
    {
        _worldMoveDir = Vector3.zero;
    }
}
