using System.Collections;
using UnityEngine;

// 激光炮台控制器
[RequireComponent(typeof(LineRenderer))]
public class LaserWeapon : WeaponControllerBase
{
    public Transform firePoint;
    public float maxRange = 50f;
    public float damage = 20f;
    public float laserDuration = 0.5f;
    public LayerMask blockMask; // 阻挡层

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    protected override void Fire()
    {
        StartCoroutine(ShootLaserCoroutine());
    }

    private IEnumerator ShootLaserCoroutine()
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, firePoint.position);

        Vector3 direction = firePoint.forward;
        float actualRange = maxRange;

        // 找墙：确定激光的最远终点
        if (Physics.Raycast(firePoint.position, direction, out RaycastHit wallHit, maxRange, blockMask))
        {
            actualRange = wallHit.distance;

            // 使激光也能让伸缩墙下降
            RetractableWall wall = wallHit.collider.GetComponent<RetractableWall>();
            if (wall != null)
            {
                wall.TriggerRetraction(); // 调用墙的下降方法
            }
        }
        lineRenderer.SetPosition(1, firePoint.position + direction * actualRange);

        // 穿透伤害：找出起止点之间所有的碰撞体
        RaycastHit[] hits = Physics.RaycastAll(firePoint.position, direction, actualRange);
        foreach (var hit in hits)
        {
            // 如果碰到了带有TankBase的物体（敌人），且不是己方坦克
            if (hit.collider.TryGetComponent<TankBase>(out TankBase tank) && tank.gameObject.layer != this.transform.root.gameObject.layer)
            {
                tank.TakeDamage(damage);
            }
        }

        // 显示一段时间后关闭
        yield return new WaitForSeconds(laserDuration);
        lineRenderer.enabled = false;
    }

    // 激光不需要反弹，复写为空
    public override void SetBonusBounce(int bonus) { }
}