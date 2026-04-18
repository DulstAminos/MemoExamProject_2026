using UnityEngine;

public class TankMovement : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float turnSpeed = 10f; // 底盘旋转的平滑速度

    [Header("模型引用")]
    public Transform chassisTransform; // 拖入底盘模型

    private Rigidbody rb;
    private Vector3 movementInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 获取键盘WASD输入
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // 组合成移动向量，并将其归一化（防止斜着走速度变快）
        movementInput = new Vector3(moveX, 0f, moveZ).normalized;
    }

    void FixedUpdate()
    {
        // 物理移动要在 FixedUpdate 里进行
        // 给刚体一个速度 = 方向 * 速度
        rb.velocity = movementInput * moveSpeed;

        // 处理底盘的旋转（如果玩家正在移动）
        if (movementInput.magnitude > 0.1f)
        {
            // 计算目标旋转角度
            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            // 平滑插值旋转，让底盘慢慢转向移动方向
            chassisTransform.rotation = Quaternion.Slerp(chassisTransform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }
    }
}