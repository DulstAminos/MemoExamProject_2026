using UnityEngine;

// 普通炮台控制器
public class NormalWeapon : WeaponControllerBase
{
    public GameObject bulletPrefab;
    public Transform firePoint;

    protected override void Fire()
    {
        GameObject bullet = PoolManager.Instance.Spawn(bulletPrefab, firePoint.position, firePoint.rotation);
        // 传递Buff带来的额外反弹次数
        if (bullet.TryGetComponent<BulletBase>(out BulletBase bulletScript))
        {
            bulletScript.AddBonusBounce(bonusBounceCount);
        }
    }
}