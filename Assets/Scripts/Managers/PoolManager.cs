using System.Collections.Generic;
using UnityEngine;

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