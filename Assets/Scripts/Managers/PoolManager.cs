using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance; // 单例访问点

    // 核心数据结构：字典。Key是预制体的名字，Value是存放该预制体实例的队列
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    [Header("默认最大缓存数量")]
    public int defaultCacheCount = 10;

    void Awake()
    {
        // 初始化单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // 注册场景加载事件
    private void OnEnable()
    {
        // 订阅场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 注销场景加载事件
    private void OnDisable()
    {
        // 注销事件，防止内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 当任何场景加载完毕时，自动触发此方法
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearPool(); // 自动清理对象池
    }

    // 清理字典数据
    public void ClearPool()
    {
        // 因为旧场景的 GameObject 已经被 Unity 引擎自动销毁了，只需要把字典和队列清空，丢弃这些失效的引用即可。
        poolDictionary.Clear();
        Debug.Log($"对象池已自动清理。进入新场景：{SceneManager.GetActiveScene().name}");
    }

    // 从对象池取物体
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string poolKey = prefab.name;

        // 如果字典里还没有这个队列，就建一个
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<GameObject>());
        }

        GameObject objToSpawn;
        // 如果队列里有闲置的物体，取出一个
        if (poolDictionary[poolKey].Count > 0)
        {
            objToSpawn = poolDictionary[poolKey].Dequeue();
            // 设置位置、旋转并激活
            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;
            objToSpawn.SetActive(true);
        }
        else
        {
            // 如果队列空了，新建一个
            objToSpawn = Instantiate(prefab, position, rotation);
            objToSpawn.name = poolKey; // 统一名字，去掉 "(Clone)" 后缀
        }

        return objToSpawn;
    }

    // 将物体放回对象池
    public void Despawn(GameObject obj)
    {
        obj.SetActive(false); // 隐藏物体
        string poolKey = obj.name;

        if (poolDictionary.ContainsKey(poolKey) && poolDictionary[poolKey].Count <= defaultCacheCount)
        {
            poolDictionary[poolKey].Enqueue(obj); // 放回队列
        }
        else
        {
            // 如果不属于任何池子(防御性编程)，或超过最大缓存，就直接销毁
            Destroy(obj);
        }
    }
}