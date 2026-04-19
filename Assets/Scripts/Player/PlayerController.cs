using UnityEngine;

public class PlayerController : TankBase
{
    private Vector3 movementInput;
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    void Update()
    {
        // 获取移动输入
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        movementInput = new Vector3(moveX, 0f, moveZ).normalized;

        // 获取瞄准与开火输入
        bool isTryingToFire = Input.GetMouseButton(0);
        if (isTryingToFire)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out float rayDistance))
            {
                Vector3 aimPoint = ray.GetPoint(rayDistance);

                // 调用基类的统合方法：瞄准，如果按了左键且瞄准完成则自动开火
                AimAndFire(aimPoint, isTryingToFire);
            }
        }
    }

    void FixedUpdate()
    {
        // 调用基类的物理移动
        MoveTank(movementInput);
    }

    protected override void Die()
    {
        Debug.Log("玩家阵亡！");
        // 玩家死亡不需要回收，直接隐藏，留给GameManager处理游戏失败
        gameObject.SetActive(false);
    }
}