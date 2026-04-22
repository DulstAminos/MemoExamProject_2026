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
        PlayerPrefs.SetString("TankHeroSave", json);
        PlayerPrefs.Save();
    }

    // 加载存档
    public void Load()
    {
        if (PlayerPrefs.HasKey("TankHeroSave"))
        {
            string json = PlayerPrefs.GetString("TankHeroSave");
            currentSave = JsonUtility.FromJson<SaveData>(json);
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
}