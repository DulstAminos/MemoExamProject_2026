using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("炮塔配置")]
    // 拖拽赋值：坦克的炮台子物体
    public GameObject turret;
    // 旋转平滑度（0=瞬间转向，越大越慢，建议0）
    public float rotateSpeed = 0f;

    // 鼠标点击的世界坐标
    private Vector3 targetClickPos;

    void Update()
    {
        // 监听鼠标左键点击（开火按键）
        if (Input.GetMouseButtonDown(0))
        {
            // 获取鼠标点击的地面世界坐标
            GetMouseClickWorldPos();

            // 执行炮塔转向
            RotateTurretToClickPos();
        }
    }

    /// <summary>
    /// 获取鼠标点击的地面世界坐标（上帝视角专用）
    /// </summary>
    void GetMouseClickWorldPos()
    {
        // 从主相机向鼠标位置发射射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 射线碰撞到地面层，记录点击坐标
        if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Ground")))
        {
            targetClickPos = hit.point;
        }
    }

    /// <summary>
    /// 炮塔仅绕Y轴，转向点击位置（水平面）
    /// </summary>
    void RotateTurretToClickPos()
    {
        if (turret == null) return;

        // 计算水平方向向量（忽略Y轴高度差，保证仅水平面旋转）
        Vector3 dir = targetClickPos - turret.transform.position;
        dir.y = 0;

        // 向量为0时不旋转（防止报错）
        if (dir.magnitude < 0.1f) return;

        // 计算目标旋转角度（仅Y轴）
        Quaternion targetRot = Quaternion.LookRotation(dir);

        // 旋转炮塔：瞬间转向/平滑转向
        if (rotateSpeed == 0)
        {
            // 瞬间转向（符合你的需求：点击直接朝向）
            turret.transform.rotation = targetRot;
        }
        else
        {
            // 平滑转向（可选，如需微调手感）
            turret.transform.rotation = Quaternion.Lerp(turret.transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }
}
