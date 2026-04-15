using UnityEngine;

/// <summary>
/// 通用坦克底盘旋转基类（玩家/敌方均可复用）
/// 负责：底盘跟随移动方向旋转
/// </summary>
public class BaseChassisRotator : MonoBehaviour
{
    [Header("旋转配置")]
    public float rotateSpeed = 20f;

    [Tooltip("绑定的移动组件")]
    public BaseMovement movement; // 手动绑定或自动获取
    private bool _isInitialized;

    private void Init()
    {
        if (movement == null)
            movement = GetComponent<BaseMovement>();

        if (movement == null)
        {
            Debug.LogError($"[{nameof(BaseChassisRotator)}] 物体{gameObject.name}未找到BaseMovement组件！");
            enabled = false;
            return;
        }
        _isInitialized = true;
    }

    private void Awake() => Init();

    private void Update()
    {
        if (!_isInitialized || !movement.IsMoving()) return;
        RotateChassisToMoveDir();
    }

    // 底盘旋转核心逻辑
    private void RotateChassisToMoveDir()
    {
        Vector3 moveDir = movement.GetMoveDir();
        moveDir.y = 0;

        if (moveDir.magnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }

    // 预留扩展：强制旋转底盘到指定方向
    public virtual void ForceRotateToDir(Vector3 dir, float forceRotateSpeed)
    {
        dir.y = 0;
        if (dir.magnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRot,
            forceRotateSpeed * Time.deltaTime
        );
    }
}
