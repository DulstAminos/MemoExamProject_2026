using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("模型引用")]
    public Transform turretTransform; // 拖入炮塔模型

    [Header("开火脚本引用")]
    public PlayerShooting shooting;

    // 创建一个数学上的无限大地平面，高度在Y=0，朝向向上。用来接收鼠标射线。
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 从主摄像机发出一条经过鼠标屏幕位置的射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float rayDistance;

            // 判断射线是否与我们的虚拟地平面相交
            if (groundPlane.Raycast(ray, out rayDistance))
            {
                // 获取相交点的世界坐标
                Vector3 point = ray.GetPoint(rayDistance);

                // 让炮塔看向这个点。锁死Y轴，防止炮塔上下翻转
                Vector3 lookPoint = new Vector3(point.x, turretTransform.position.y, point.z);

                // 瞬间转向鼠标位置
                turretTransform.LookAt(lookPoint);
                shooting.Fire();
            }
        }
    }
}