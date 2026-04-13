using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局事件中心单例（简易版）
/// 负责：事件注册、移除、触发，实现模块解耦
/// </summary>
public class EventManager : MonoBehaviour
{
    // 静态唯一实例
    public static EventManager Instance { get; private set; }

    // 事件容器
    // 无参事件（如：游戏胜利、游戏失败、暂停游戏）
    private Dictionary<string, Action> _eventDict = new Dictionary<string, Action>();
    // 带参事件（如：玩家受伤、敌人死亡、拾取道具）
    private Dictionary<string, Action<object>> _paramEventDict = new Dictionary<string, Action<object>>();

    // 单例初始化
    private void Awake()
    {
        // 防止重复实例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region 无参事件
    /// <summary>
    /// 注册无参事件
    /// </summary>
    public void AddListener(string eventName, Action callBack)
    {
        if (!_eventDict.ContainsKey(eventName))
            _eventDict.Add(eventName, null);
        _eventDict[eventName] += callBack;
    }

    /// <summary>
    /// 移除无参事件
    /// </summary>
    public void RemoveListener(string eventName, Action callBack)
    {
        if (_eventDict.ContainsKey(eventName))
            _eventDict[eventName] -= callBack;
    }

    /// <summary>
    /// 触发无参事件
    /// </summary>
    public void TriggerEvent(string eventName)
    {
        if (_eventDict.TryGetValue(eventName, out Action action))
            action?.Invoke();
    }
    #endregion

    #region 带参事件
    /// <summary>
    /// 注册带参事件
    /// </summary>
    public void AddParamListener(string eventName, Action<object> callBack)
    {
        if (!_paramEventDict.ContainsKey(eventName))
            _paramEventDict.Add(eventName, null);
        _paramEventDict[eventName] += callBack;
    }

    /// <summary>
    /// 移除带参事件
    /// </summary>
    public void RemoveParamListener(string eventName, Action<object> callBack)
    {
        if (_paramEventDict.ContainsKey(eventName))
            _paramEventDict[eventName] -= callBack;
    }

    /// <summary>
    /// 触发带参事件
    /// </summary>
    public void TriggerParamEvent(string eventName, object param)
    {
        if (_paramEventDict.TryGetValue(eventName, out Action<object> action))
            action?.Invoke(param);
    }
    #endregion

    /// <summary>
    /// 清空所有事件（场景切换时调用）
    /// </summary>
    public void ClearAllEvents()
    {
        _eventDict.Clear();
        _paramEventDict.Clear();
    }
}
