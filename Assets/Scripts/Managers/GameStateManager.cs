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
    private Coroutine gameOverCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // 使用事件监听场景加载
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            CurrentState = GameState.MainMenu;
            isGameOver = true; // 在主菜单不需要游戏判定
        }
        else
        {
            InitLevel();
            // 通知UI播放关卡提示（UI脚本中会控制暂停）
            InGameUIManager.Instance.ShowLevelIntro();
        }
    }

    // 关卡初始化
    public void InitLevel()
    {
        isGameOver = false;
        CurrentState = GameState.Playing;

        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = null;
        }

        LevelConfig config = LevelManager.Instance.GetCurrentLevelConfig();
        if (config != null)
        {
            remainingEnemies = config.enemyCount;
        }
        else
        {
            remainingEnemies = 0;
            Debug.LogWarning("当前场景无关卡配置，无法初始化敌人数量。");
        }

        LevelManager.Instance.StartTimer();
        Time.timeScale = 1;
    }

    // 玩家死亡接口
    public void OnPlayerDead()
    {
        if (isGameOver || CurrentState == GameState.Won) return; // 如果已经赢了，玩家死后不触发失败
        gameOverCoroutine = StartCoroutine(GameOverRoutine(GameState.Lost));
    }

    // 敌人死亡接口
    public void OnEnemyDead()
    {
        if (isGameOver) return;

        remainingEnemies--;
        if (remainingEnemies <= 0)
        {
            Debug.Log("敌人全部死亡");
            // 确保停止可能正在执行的失败协程（同归于尽算赢）
            if (gameOverCoroutine != null) StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = StartCoroutine(GameOverRoutine(GameState.Won));
        }
    }

    // 游戏结束协程
    private IEnumerator GameOverRoutine(GameState result)
    {
        // 先锁定状态，防止等待期间发生意外更改
        isGameOver = true;

        // 等待1.5秒
        yield return new WaitForSecondsRealtime(1.5f);

        // 二次检查，确保胜利优先（如果在等待期间敌人全灭了）
        if (remainingEnemies <= 0) result = GameState.Won;

        CurrentState = result;
        int stars = 1;

        if (result == GameState.Won)
        {
            float timeTaken = LevelManager.Instance.GetElapsedTime();
            stars = LevelManager.Instance.CalculateStars(timeTaken);
            Debug.Log($"胜利！用时：{timeTaken:F1}秒，获得 {stars} 星");

            // 基于配置列表计算下一关解锁
            int currentLevelNum = LevelManager.Instance.GetCurrentLevelNumber();
            DataManager.Instance.UpdateReachedLevel(currentLevelNum + 1);
        }
        else
        {
            Debug.Log("失败！");
        }

        // 呼出结算UI
        InGameUIManager.Instance.ShowEndScreen(result == GameState.Won, stars);
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