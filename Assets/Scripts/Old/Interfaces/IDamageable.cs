/// <summary>
/// 通用可受伤接口：所有能被攻击的对象（玩家、敌人、可破坏物）均实现此接口
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 核心方法：接收伤害
    /// </summary>
    /// <param name="damage">受到的伤害数值</param>
    void TakeDamage(float damage);

    /// <summary>
    /// 只读属性：当前生命值（供UI血条/伤害计算使用）
    /// </summary>
    float CurrentHealth { get; }

    /// <summary>
    /// 只读属性：最大生命值（供血条比例计算使用）
    /// </summary>
    float MaxHealth { get; }

    /// <summary>
    /// 只读属性：是否死亡（防止重复触发死亡/伤害）
    /// </summary>
    bool IsDead { get; }
}
