using System.Collections;
using UnityEngine;

public class BuffSystem : MonoBehaviour
{
    private PlayerController playerController;
    private WeaponControllerBase currentWeapon;

    [Header("Buff 状态记录")]
    public bool isShielded = false;
    public int currentBonusBounce = 0;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    // 供外部通知武器已更换（确保新武器也能吃到现有的反弹Buff）
    public void NotifyWeaponChanged(WeaponControllerBase newWeapon)
    {
        currentWeapon = newWeapon;
        if (currentWeapon != null) currentWeapon.SetBonusBounce(currentBonusBounce);
    }

    public void ApplyShield(float duration)
    {
        StartCoroutine(ShieldRoutine(duration));
    }

    public void ApplySpeed(float duration, float bonusSpeed)
    {
        StartCoroutine(SpeedRoutine(duration, bonusSpeed));
    }

    public void ApplyBounce(float duration, int bonusBounces)
    {
        StartCoroutine(BounceRoutine(duration, bonusBounces));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        isShielded = true;
        // 可选：在这里激活护盾特效 GameObject
        yield return new WaitForSeconds(duration);
        isShielded = false;
    }

    private IEnumerator SpeedRoutine(float duration, float bonusSpeed)
    {
        playerController.AddSpeed(bonusSpeed); // 通知玩家脚本加移速
        yield return new WaitForSeconds(duration);
        playerController.AddSpeed(-bonusSpeed); // 时间到减回来
    }

    private IEnumerator BounceRoutine(float duration, int bonusBounces)
    {
        currentBonusBounce = bonusBounces;
        if (currentWeapon != null) currentWeapon.SetBonusBounce(currentBonusBounce);

        yield return new WaitForSeconds(duration);

        currentBonusBounce = 0;
        if (currentWeapon != null) currentWeapon.SetBonusBounce(0);
    }
}