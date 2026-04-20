using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum PickupType { Weapon, ShieldBuff, SpeedBuff, BounceBuff }
    public PickupType type;

    [Header("武器配置")]
    public GameObject weaponPrefab; // 若为Weapon类型，配置此项

    [Header("Buff配置")]
    public float duration = 5f;
    public float speedBonus = 5f;   // 若为SpeedBuff，配置此项
    public int bounceBonus = 2;     // 若为BounceBuff，配置此项

    private void OnTriggerEnter(Collider other)
    {
        // 只有玩家能拾取
        if (other.CompareTag("Player") && other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            if (type == PickupType.Weapon)
            {
                player.SwitchWeapon(weaponPrefab);
            }
            else
            {
                BuffSystem buffSystem = player.GetComponent<BuffSystem>();
                if (buffSystem != null)
                {
                    switch (type)
                    {
                        case PickupType.ShieldBuff: buffSystem.ApplyShield(duration); break;
                        case PickupType.SpeedBuff: buffSystem.ApplySpeed(duration, speedBonus); break;
                        case PickupType.BounceBuff: buffSystem.ApplyBounce(duration, bounceBonus); break;
                    }
                }
            }

            // 拾取后销毁球体
            Destroy(gameObject);
        }
    }
}