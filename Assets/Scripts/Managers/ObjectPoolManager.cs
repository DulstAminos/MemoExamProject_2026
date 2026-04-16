using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用对象池单例
/// 负责：坦克、子弹、爆炸特效、道具球等游戏物体复用
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    // 静态唯一实例
    public static ObjectPoolManager Instance { get; private set; }

    // 对象池容器：key=预设名，value=该预设的对象队列
    private Dictionary<string, Queue<GameObject>> _poolDict = new Dictionary<string, Queue<GameObject>>();

    // 每个预设默认最大缓存数量
    [Header("默认最大缓存数量")]
    public int defaultCacheCount = 10;

    private void Awake()
    {
        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 从对象池获取物体
    /// </summary>
    /// <param name="prefab">要获取的物体预设</param>
    /// <param name="spawnPos">生成位置</param>
    /// <param name="spawnRot">生成旋转</param>
    public GameObject GetObject(GameObject prefab, Vector3 spawnPos, Quaternion spawnRot)
    {
        string key = prefab.name;
        GameObject obj;

        // 池子里有=>直接取
        if (_poolDict.ContainsKey(key) && _poolDict[key].Count > 0)
        {
            obj = _poolDict[key].Dequeue();
            obj.SetActive(true);
            obj.transform.position = spawnPos;
            obj.transform.rotation = spawnRot;
        }
        // 池子里没有=>新建
        else
        {
            obj = Instantiate(prefab, spawnPos, spawnRot);
            obj.name = key; // 统一名称，避免(Clone)导致key错误
        }

        // 实现了IPoolable就自动调用Init()
        if (obj.TryGetComponent(out IPoolable poolable))
        {
            poolable.Init();
        }

        return obj;
    }

    /// <summary>
    /// 把物体放回对象池
    /// </summary>
    public void ReturnObject(GameObject obj)
    {
        if (obj == null) return;

        string key = obj.name;
        obj.SetActive(false);

        // 没有池子=>创建池子
        if (!_poolDict.ContainsKey(key))
        {
            _poolDict[key] = new Queue<GameObject>();
        }

        // 没超最大缓存=>入队；超了=>直接销毁
        if (_poolDict[key].Count < defaultCacheCount)
        {
            _poolDict[key].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    /// <summary>
    /// 清空指定对象池（切换场景时调用）
    /// </summary>
    public void ClearPool(string prefabName)
    {
        if (_poolDict.ContainsKey(prefabName))
        {
            _poolDict[prefabName].Clear();
        }
    }

    /// <summary>
    /// 清空所有对象池（场景切换时调用）
    /// </summary>
    public void ClearAllPools()
    {
        _poolDict.Clear();
    }
}
