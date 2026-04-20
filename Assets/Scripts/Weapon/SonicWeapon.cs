using UnityEngine;
public class SonicWeapon : WeaponControllerBase
{
    public GameObject sonicPrefab; // 拖入带有 SonicProjectile 的声波特效预制体
    public Transform firePoint;

    protected override void Fire()
    {
        PoolManager.Instance.Spawn(sonicPrefab, firePoint.position, firePoint.rotation);
    }

    public override void SetBonusBounce(int bonus) { } // 声波无反弹
}