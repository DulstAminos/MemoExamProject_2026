using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    // 存档数据结构
    [System.Serializable]
    public class SaveData
    {
        public int reachedLevel = 1;
        public bool soundOn = true;
        public bool musicOn = true;
        public bool cameraFollowOn = true;
    }

    public SaveData currentSave = new SaveData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else Destroy(gameObject);
    }

    // 保存存档
    public void Save()
    {
        string json = JsonUtility.ToJson(currentSave);
        // 存入硬盘，键名为 "TankHeroSave"
        PlayerPrefs.SetString("TankHeroSave", json);
        PlayerPrefs.Save(); // 强制立即写入硬盘
        Debug.Log("游戏已保存: " + json);
    }

    // 加载存档
    public void Load()
    {
        if (PlayerPrefs.HasKey("TankHeroSave"))
        {
            string json = PlayerPrefs.GetString("TankHeroSave");
            // 将 JSON 字符串还原为 SaveData 类
            currentSave = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("读取存档成功: " + json);
        }
        else
        {
            Debug.Log("未找到存档，创建新存档。");
            Save(); // 没有存档则生成一个默认存档
        }
    }

    // 更新关卡进度
    public void UpdateReachedLevel(int levelIndex)
    {
        if (levelIndex > currentSave.reachedLevel)
        {
            currentSave.reachedLevel = levelIndex;
            Save();
        }
    }

    // 重置存档
    public void ResetSave()
    {
        currentSave = new SaveData();
        Save();
    }

    // 异常保护：游戏退出时自动保存
    private void OnApplicationQuit()
    {
        Save();
    }
}