using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { MainMenu, Playing, Won, Lost, Paused }

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public GameState CurrentState { get; private set; }
    private int remainingEnemies;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 如果不是在主菜单，则自动开始游戏逻辑
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            InitLevel();
        }
    }

    // 关卡初始化
    public void InitLevel()
    {
        isGameOver = false;
        CurrentState = GameState.Playing;

        LevelConfig config = LevelManager.Instance.GetCurrentLevelConfig();
        if (config != null)
        {
            remainingEnemies = config.enemyCount;
        }

        LevelManager.Instance.StartTimer();
        Time.timeScale = 1;
    }

    // 玩家死亡接口
    public void OnPlayerDead()
    {
        if (isGameOver) return;
        StartCoroutine(GameOverRoutine(GameState.Lost));
    }

    // 敌人死亡接口
    public void OnEnemyDead()
    {
        if (isGameOver) return;

        remainingEnemies--;
        if (remainingEnemies <= 0)
        {
            StartCoroutine(GameOverRoutine(GameState.Won));
        }
    }

    // 游戏结束协程
    private IEnumerator GameOverRoutine(GameState result)
    {
        // 等待1.5秒
        yield return new WaitForSecondsRealtime(1.5f);

        // 二次检查，确保胜利优先（如果在等待期间敌人全灭了）
        if (remainingEnemies <= 0) result = GameState.Won;

        if (isGameOver) yield break;
        isGameOver = true;
        CurrentState = result;

        if (result == GameState.Won)
        {
            float timeTaken = LevelManager.Instance.GetElapsedTime();
            int stars = LevelManager.Instance.CalculateStars(timeTaken);
            Debug.Log($"胜利！获得 {stars} 星");

            // 更新存档进度（假设关卡名包含数字，或根据配置表索引）
            int currentLevelIdx = LevelManager.Instance.levelConfigs.FindIndex(c => c.sceneName == SceneManager.GetActiveScene().name);
            DataManager.Instance.UpdateReachedLevel(currentLevelIdx + 2); // 解锁下一关
        }
        else
        {
            Debug.Log("失败！");
        }

        // 此处应调用UI显示结算界面
        // UIManager.Instance.ShowEndScreen(result);
    }

    // 暂停切换
    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0;
        }
        else if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1;
        }
    }
}