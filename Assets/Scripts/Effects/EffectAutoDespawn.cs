using UnityEngine;

public class EffectAutoDespawn : MonoBehaviour
{
    [Header("特效持续时间")]
    public float duration = 2.0f;

    // 当物体被激活（从对象池取出）时执行
    private void OnEnable()
    {
        // 获取物体及所有子物体的粒子系统并重置播放
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in particles)
        {
            p.Clear(); // 清除残留
            p.Play();  // 重新开始
        }

        // 延迟执行回收操作
        Invoke("ReturnToPool", duration);
    }

    private void ReturnToPool()
    {
        // 使用PoolManager单例进行回收
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(gameObject);
        }
    }

    private void OnDisable()
    {
        // 如果物体意外被隐藏，取消之前的延时任务，防止报错
        CancelInvoke();
    }
}