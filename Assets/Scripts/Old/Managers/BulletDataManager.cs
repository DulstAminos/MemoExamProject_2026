using System;
using System.Collections.Generic;
using UnityEngine;

// ====================== 核心枚举 ======================
/// <summary>
/// 炮弹种类
/// </summary>
public enum BulletType
{
    NormalBullet,   // 普通炮弹
    LaserBullet,    // 激光
    SonicBullet     // 声波炮
}

/// <summary>
/// 炮弹回收方式
/// </summary>
public enum BulletRecycleType
{
    BounceRecycle,      // 反弹回收（反弹次数达标）
    TimeOutRecycle,     // 超时回收
    BounceOrTimeOutRecycle  // 反弹+超时回收
}

/// <summary>
/// 炮弹发射归属
/// </summary>
public enum BulletOwner
{
    Player, // 玩家发射
    Enemy   // 敌人发射
}

// ====================== 炮弹信息结构体 ======================
/// <summary>
/// 炮弹核心数据
/// 新增属性直接在此添加，面板自动支持配置
/// </summary>
[Serializable]
public struct BulletInfo
{
    [Header("基础配置")]
    public BulletType bulletType;            // 炮弹种类
    public BulletRecycleType recycleType;    // 回收方式
    public BulletOwner bulletOwner;          // 发射对象

    [Header("数值属性")]
    public float damage;                     // 伤害值
    public float flySpeed;                   // 飞行速度
    public float lifeTime;                   // 生命周期

    [Header("反弹配置")]
    public bool canBounce;                   // 是否反弹
    public int maxBounceCount;               // 最大反弹次数
}

// ====================== 面板配置类 ======================
/// <summary>
/// 炮弹预制体配置（类型 - 预制体）
/// </summary>
[Serializable]
public class BulletPrefabConfig
{
    [Tooltip("炮弹类型")]
    public BulletType bulletType;
    [Tooltip("对应的炮弹预制体")]
    public GameObject bulletPrefab;
}

/// <summary>
/// 炮弹默认信息配置（类型 - 完整属性）
/// </summary>
[Serializable]
public class BulletDefaultInfoConfig
{
    [Tooltip("炮弹类型")]
    public BulletType bulletType;
    [Tooltip("该炮弹的默认属性配置")]
    public BulletInfo defaultBulletInfo;
}

// ====================== 全局单例 炮弹数据管理器 ======================
public class BulletDataManager : MonoBehaviour
{
    // 全局单例
    public static BulletDataManager Instance { get; private set; }

    [Header("炮弹预制体配置")]
    [SerializeField] private List<BulletPrefabConfig> bulletPrefabConfigs;

    [Header("炮弹默认属性配置")]
    [SerializeField] private List<BulletDefaultInfoConfig> bulletDefaultInfoConfigs;

    // 缓存字典：高性能查找
    private Dictionary<BulletType, GameObject> _bulletPrefabDict;
    private Dictionary<BulletType, BulletInfo> _bulletInfoDict;

    #region 单例初始化
    private void Awake()
    {
        // 单例防重
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 切换场景不销毁

        // 初始化缓存
        InitPrefabDictionary();
        InitInfoDictionary();
    }
    #endregion

    #region 初始化缓存字典
    /// <summary>
    /// 初始化预制体字典
    /// </summary>
    private void InitPrefabDictionary()
    {
        _bulletPrefabDict = new Dictionary<BulletType, GameObject>();
        foreach (var config in bulletPrefabConfigs)
        {
            if (config.bulletPrefab == null)
            {
                Debug.LogError($"炮弹预制体配置错误：{config.bulletType} 未绑定预制体！");
                continue;
            }
            if (_bulletPrefabDict.ContainsKey(config.bulletType))
            {
                Debug.LogError($"炮弹预制体重复配置：{config.bulletType}");
                continue;
            }
            _bulletPrefabDict.Add(config.bulletType, config.bulletPrefab);
        }
    }

    /// <summary>
    /// 初始化默认信息字典
    /// </summary>
    private void InitInfoDictionary()
    {
        _bulletInfoDict = new Dictionary<BulletType, BulletInfo>();
        foreach (var config in bulletDefaultInfoConfigs)
        {
            if (_bulletInfoDict.ContainsKey(config.bulletType))
            {
                Debug.LogError($"炮弹默认属性重复配置：{config.bulletType}");
                continue;
            }
            _bulletInfoDict.Add(config.bulletType, config.defaultBulletInfo);
        }
    }
    #endregion

    #region 全局公共调用接口
    /// <summary>
    /// 通过炮弹类型获取预制体
    /// </summary>
    public GameObject GetBulletPrefab(BulletType type)
    {
        if (_bulletPrefabDict.TryGetValue(type, out var prefab))
            return prefab;

        Debug.LogError($"未找到炮弹预制体：{type}");
        return null;
    }

    /// <summary>
    /// 通过炮弹类型获取默认属性信息
    /// </summary>
    public BulletInfo GetBulletDefaultInfo(BulletType type)
    {
        if (_bulletInfoDict.TryGetValue(type, out var info))
            return info;

        Debug.LogError($"未找到炮弹默认信息：{type}");
        return default;
    }
    #endregion
}