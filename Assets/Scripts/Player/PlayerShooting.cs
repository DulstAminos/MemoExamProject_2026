using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("开火配置")]
    public GameObject bulletPrefab; // 子弹预制体
    public Transform firePoint;     // 开火点

    public void Fire()
    {
        // 在 FirePoint 的位置生成子弹，并保持 FirePoint 的旋转方向
        PoolManager.Instance.Spawn(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}