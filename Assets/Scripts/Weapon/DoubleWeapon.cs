using UnityEngine;

// 双管炮台控制器
public class DoubleWeapon : WeaponControllerBase
{
    public GameObject bulletPrefab;
    public Transform leftFirePoint;
    public Transform rightFirePoint;

    protected override void Fire()
    {
        // 左边开一炮
        GameObject bulletL = PoolManager.Instance.Spawn(bulletPrefab, leftFirePoint.position, leftFirePoint.rotation);
        if (bulletL.TryGetComponent<BulletBase>(out BulletBase scriptL)) scriptL.AddBonusBounce(bonusBounceCount);

        // 右边开一炮
        GameObject bulletR = PoolManager.Instance.Spawn(bulletPrefab, rightFirePoint.position, rightFirePoint.rotation);
        if (bulletR.TryGetComponent<BulletBase>(out BulletBase scriptR)) scriptR.AddBonusBounce(bonusBounceCount);
    }
}