using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 全局游戏管理单例
/// 负责：游戏状态、暂停、全局变量、场景控制
/// </summary>
public class GameManager : MonoBehaviour
{
    // 静态唯一实例
    public static GameManager Instance { get; private set; }

    // 游戏状态枚举
    public enum GameState
    {
        MainMenu,    // 主菜单
        LevelSelect, // 选关
        GamePlay,    // 游戏中
        Pause,       // 暂停
        Victory,     // 胜利
        Defeat       // 失败
    }

    // 当前游戏状态
    public GameState CurrentState { get; private set; }

    // 全局通用变量
    public int CurrentLevel { get; set; } // 当前关卡
    public float LevelStartTime { get; set; } // 关卡开始时间（用于评分）

    // 场景名称常量
    private const string Scene_MainMenu = "MainMenuScene";
    private const string Scene_LevelSelect = "LevelSelectScene";
    private const string Scene_GamePlay = "GamePlayScene";

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
        DontDestroyOnLoad(gameObject); // 切换场景不销毁

        // 注册事件监听
        AddEventListener();
    }

    private void Start()
    {
        // 初始状态：主菜单
        SwitchState(GameState.MainMenu);
    }

    private void OnDestroy()
    {
        // 销毁是移除事件监听
        RemoveEventListener();
    }

    /// <summary>
    /// 切换游戏状态，设置为public以便外部统一调用
    /// </summary>
    public void SwitchState(GameState newState)
    {
        CurrentState = newState;

        // 状态触发逻辑
        switch (newState)
        {
            // 切换为“游戏中”则继续游戏
            case GameState.GamePlay:
                ResumeGame();
                break;
            // 切换为“暂停”则暂停游戏
            case GameState.Pause:
                PauseGame();
                break;
            // 切换为“胜利”则暂停游戏并广播游戏胜利事件
            case GameState.Victory:
                PauseGame();
                EventManager.Instance.TriggerEvent("OnGameVictory");
                break;
            // 切换为“失败”则暂停游戏并广播游戏失败事件
            case GameState.Defeat:
                PauseGame();
                EventManager.Instance.TriggerEvent("OnGameDefeat");
                break;
        }
    }

    /// <summary>
    /// 暂停游戏（冻结时间）
    /// </summary>
    public void PauseGame() => Time.timeScale = 0;

    /// <summary>
    /// 继续游戏（恢复时间）
    /// </summary>
    public void ResumeGame() => Time.timeScale = 1;

    // 场景切换核心方法
    /// <summary> 打开主菜单 </summary>
    public void LoadMainMenu()
    {
        SwitchState(GameState.MainMenu);
        SceneManager.LoadScene(Scene_MainMenu);
    }

    /// <summary> 打开选关界面 </summary>
    public void LoadLevelSelect()
    {
        SwitchState(GameState.LevelSelect);
        SceneManager.LoadScene(Scene_LevelSelect);
    }

    /// <summary> 打开游戏战斗场景 </summary>
    public void LoadGamePlay()
    {
        SwitchState(GameState.GamePlay);
        SceneManager.LoadScene(Scene_GamePlay);
        LevelStartTime = Time.time; // 记录关卡开始时间（用于后续评分）
    }

    /// <summary> 退出游戏 </summary>
    public void ExitGame()
    {
        Application.Quit();
        // 编辑器下模拟退出
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// 注册事件监听
    /// </summary>
    private void AddEventListener()
    {
        // 注册对玩家死亡的监听
        EventManager.Instance.AddListener("OnPlayerDead", OnPlayerDeadHandler);
    }

    /// <summary>
    /// 移除事件监听
    /// </summary>
    private void RemoveEventListener()
    {
        // 移除对玩家死亡的监听
        EventManager.Instance.RemoveListener("OnPlayerDead", OnPlayerDeadHandler);
    }

    // 玩家死亡后的处理逻辑
    private void OnPlayerDeadHandler()
    {
        SwitchState(GameState.Defeat);
    }
}
