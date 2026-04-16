/// <summary>
/// 对象池可复用对象接口
/// 作用：从对象池获取时自动初始化，重置状态避免脏数据
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// 初始化复用对象（重置位置、旋转、状态、参数等）
    /// 对象池GetObject时自动调用
    /// </summary>
    void Init();
}
