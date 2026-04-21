using System.Collections;
using UnityEngine;

public class BuffSystem : MonoBehaviour
{
    private PlayerController playerController;
    [SerializeField]
    private WeaponControllerBase currentWeapon;

    [Header("Buff 状态记录")]
    public bool isShielded = false;
    public int currentBonusBounce = 0;

    private Coroutine shieldCoroutine; // 记录当前运行的护盾协程
    private Coroutine speedCoroutine; // 记录当前运行的加速协程
    private Coroutine bounceCoroutine; // 记录当前运行的反弹协程

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        // 自动获取当前挂载的武器（假设武器在子物体或同级）
        currentWeapon = GetComponentInChildren<WeaponControllerBase>();
    }

    // 供外部通知武器已更换（确保新武器也能吃到现有的反弹Buff）
    public void NotifyWeaponChanged(WeaponControllerBase newWeapon)
    {
        currentWeapon = newWeapon;
        if (currentWeapon != null) currentWeapon.SetBonusBounce(currentBonusBounce);
    }

    public void ApplyShield(float duration)
    {
        // 如果已有正在运行的协程，先停止它（刷新 Buff 时间）
        if (shieldCoroutine != null) StopCoroutine(shieldCoroutine);
        shieldCoroutine = StartCoroutine(ShieldRoutine(duration));
    }

    public void ApplySpeed(float duration, float bonusSpeed)
    {
        // 如果已有正在运行的协程，先停止它（刷新 Buff 时间）
        if (speedCoroutine != null) StopCoroutine(speedCoroutine);
        speedCoroutine = StartCoroutine(SpeedRoutine(duration, bonusSpeed));
    }

    public void ApplyBounce(float duration, int bonusBounces)
    {
        // 如果已有正在运行的协程，先停止它（刷新 Buff 时间）
        if (bounceCoroutine != null) StopCoroutine(bounceCoroutine);
        bounceCoroutine = StartCoroutine(BounceRoutine(duration, bonusBounces));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        isShielded = true;
        // 可选：在这里激活护盾特效 GameObject
        yield return new WaitForSeconds(duration);
        isShielded = false;
        shieldCoroutine = null;
    }

    private IEnumerator SpeedRoutine(float duration, float bonusSpeed)
    {
        playerController.AddSpeed(bonusSpeed); // 通知玩家脚本加移速
        yield return new WaitForSeconds(duration);
        playerController.AddSpeed(-bonusSpeed); // 时间到减回来
        speedCoroutine = null;
    }

    private IEnumerator BounceRoutine(float duration, int bonusBounces)
    {
        currentBonusBounce = bonusBounces;
        UpdateWeaponBounce();

        yield return new WaitForSeconds(duration);

        currentBonusBounce = 0;
        UpdateWeaponBounce();
        bounceCoroutine = null;
    }

    private void UpdateWeaponBounce()
    {
        // 如果 currentWeapon 为空，尝试重新获取一次
        if (currentWeapon == null) currentWeapon = GetComponentInChildren<WeaponControllerBase>();

        if (currentWeapon != null)
        {
            currentWeapon.SetBonusBounce(currentBonusBounce);
        }
    }
}