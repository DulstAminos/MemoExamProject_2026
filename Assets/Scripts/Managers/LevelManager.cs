using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("关卡配置列表")]
    public List<LevelConfig> levelConfigs;

    private float levelStartTime;
    private LevelConfig currentLevelConfig;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // 获取当前关卡的配置
    public LevelConfig GetCurrentLevelConfig()
    {
        string currentName = SceneManager.GetActiveScene().name;

        // 主菜单不需要LevelConfig，直接返回null即可
        if (currentName == "MainMenu") return null;

        currentLevelConfig = levelConfigs.Find(c => c.sceneName == currentName);

        if (currentLevelConfig == null)
        {
            Debug.LogError($"未找到关卡 {currentName} 的配置！请检查LevelManager列表。");
        }
        return currentLevelConfig;
    }

    // 获取当前关卡在列表中的序号（1表示第一关）
    public int GetCurrentLevelNumber()
    {
        if (currentLevelConfig == null) return 1;
        int index = levelConfigs.IndexOf(currentLevelConfig);
        return index + 1; // 索引0 对应 关卡1
    }

    // 开始计时
    public void StartTimer() => levelStartTime = Time.time;

    // 获取通关时间
    public float GetElapsedTime() => Time.time - levelStartTime;

    // 计算星级
    public int CalculateStars(float time)
    {
        if (currentLevelConfig == null) return 1;
        if (time <= currentLevelConfig.star3Time) return 3;
        if (time <= currentLevelConfig.star2Time) return 2;
        return 1;
    }

    // 场景切换接口
    public void LoadLevel(string name)
    {
        DataManager.Instance.Save(); // 切换前自动保存
        SceneManager.LoadScene(name);
    }

    // 关卡重玩
    public void RestartCurrentLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    // 加载主菜单
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}