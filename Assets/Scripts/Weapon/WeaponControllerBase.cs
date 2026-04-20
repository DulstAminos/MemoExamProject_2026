using UnityEngine;

public abstract class WeaponControllerBase : MonoBehaviour
{
    [Header("武器基础配置")]
    public float fireCooldown = 1f;
    protected float lastFireTime;

    protected int bonusBounceCount = 0; // 反弹增加Buff的值

    protected virtual void OnEnable()
    {
        lastFireTime = 0f;
    }

    // 暴露给坦克的尝试开火接口
    public bool TryFire()
    {
        if (Time.time >= lastFireTime + fireCooldown)
        {
            lastFireTime = Time.time;
            Fire(); // 调用子类具体的开火逻辑
            return true;
        }
        return false;
    }

    // 具体开火逻辑，由子类实现
    protected abstract void Fire();

    // 接口：设置额外反弹次数（供Buff系统调用）
    public virtual void SetBonusBounce(int bonus)
    {
        bonusBounceCount = bonus;
    }
}