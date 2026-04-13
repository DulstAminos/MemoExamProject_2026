using UnityEngine;

// 玩家坦克WASD移动脚本
public class TankMovement : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 8f; // 坦克移动速度

    private Rigidbody _rb;
    private Vector3 _worldMoveDir; // 世界空间的移动方向

    void Awake()
    {
        // 获取底盘的刚体
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 获取WASD输入
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");     // W/S

        // 计算相对于世界的移动方向
        _worldMoveDir = Vector3.forward * vertical + Vector3.right * horizontal;
        _worldMoveDir.Normalize(); // 防斜向超速
    }

    void FixedUpdate()
    {
        if (_worldMoveDir.magnitude > 0.1f)
        {
            // 刚体平滑移动
            Vector3 targetPos = _rb.position + _worldMoveDir * moveSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(targetPos);
        }
    }

    // 是否正在移动
    public bool IsMoving()
    {
        return _worldMoveDir.magnitude > 0.1f;
    }

    // 提供移动方向
    public Vector3 GetMoveDir()
    {
        return _worldMoveDir;
    }
}
