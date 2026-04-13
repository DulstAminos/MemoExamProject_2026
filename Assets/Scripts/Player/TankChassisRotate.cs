using UnityEngine;

public class TankChassisRotate : MonoBehaviour
{
    [Header("底盘旋转速度")]
    public float rotateSpeed = 20f;

    private TankMovement _tankMove;

    void Awake()
    {
        _tankMove = GetComponent<TankMovement>();
    }

    void Update()
    {
        // 只有移动时才旋转底盘
        if (_tankMove.IsMoving())
        {
            RotateChassisToMoveDir();
        }
    }

    // 底盘绕Y轴转向移动方向
    void RotateChassisToMoveDir()
    {
        // 获取当前移动方向（世界空间）
        Vector3 moveDir = _tankMove.GetMoveDir();
        moveDir.y = 0; // 严格保持水平，忽略Y轴

        if (moveDir.magnitude > 0.01f)
        {
            // 计算目标朝向
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            // 平滑旋转底盘
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
    }
}
