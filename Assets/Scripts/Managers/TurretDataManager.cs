using System;
using System.Collections.Generic;
using UnityEngine;

// 炮台种类枚举
public enum TurretType
{
    Normal,     // 普通炮台
    DoubleBarrel, // 双管炮
    Laser,      // 激光炮
    Sonic       // 声波炮
}

// 可序列化配置类：用于Unity面板绑定 枚举 + 炮台预制体
[Serializable]
public class TurretConfig
{
    [Tooltip("炮台类型")]
    public TurretType turretType;

    [Tooltip("对应的炮台预制体")]
    public GameObject turretPrefab;
}

public class TurretDataManager : MonoBehaviour
{
    // 单例实例（全局唯一访问点）
    public static TurretDataManager Instance { get; private set; }

    // Unity面板配置列表：直接拖拽预制体绑定
    [Header("炮台配置")]
    [Tooltip("按枚举类型绑定对应的炮台预制体，请勿重复配置同一种类型")]
    [SerializeField] private List<TurretConfig> turretConfigs;

    // 缓存字典：用于快速查找预制体
    private Dictionary<TurretType, GameObject> _turretDict;

    // 单例初始化 + 字典缓存
    private void Awake()
    {
        // 单例防重复逻辑
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 切换场景不销毁

        // 初始化字典
        InitTurretDictionary();
    }

    // 把面板配置的List转为字典，提升查找效率
    private void InitTurretDictionary()
    {
        _turretDict = new Dictionary<TurretType, GameObject>();
        foreach (var config in turretConfigs)
        {
            // 跳过空配置
            if (config.turretPrefab == null)
            {
                Debug.LogError($"炮台配置错误：{config.turretType} 未绑定预制体！");
                continue;
            }

            // 防止重复配置
            if (_turretDict.ContainsKey(config.turretType))
            {
                Debug.LogError($"炮台配置重复：{config.turretType} 已存在，已忽略重复配置！");
                continue;
            }

            _turretDict.Add(config.turretType, config.turretPrefab);
        }
    }

    // 全局公共方法：通过炮台类型获取预制体（核心接口）
    public GameObject GetTurretPrefab(TurretType type)
    {
        if (_turretDict.TryGetValue(type, out var prefab))
        {
            return prefab;
        }

        // 未找到时返回null+日志提示
        Debug.LogError($"未找到炮台类型：{type} 的预制体，请检查配置！");
        return null;
    }
}
