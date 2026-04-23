using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance;

    [Header("主画布 (跨场景控制)")]
    public Canvas mainCanvas;

    [Header("关卡开局提示")]
    public GameObject introPanel;
    public Image levelNameImage;
    public Sprite[] levelNameSprites; // 存LEVEL-1到LEVEL-5的图片

    [Header("暂停与设置界面")]
    public GameObject pausePanel;
    public Toggle soundToggle;
    public Toggle musicToggle;
    public Toggle cameraToggle;

    [Header("结算界面")]
    public GameObject endPanel;
    public TMP_Text endTitleText; // 显示 VICTORY 或 FAILURE
    public GameObject[] starIcons; // 三颗星星的Image对象
    public Button nextLevelBtn;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitSettings(); // 初始化设置界面的数据
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 根据场景决定是否显示游戏UI
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            mainCanvas.gameObject.SetActive(false);
        }
        else
        {
            mainCanvas.gameObject.SetActive(true);
            pausePanel.SetActive(false);
            endPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // 监听ESC键呼出/关闭暂停菜单
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameStateManager.Instance.CurrentState == GameState.Playing)
                ShowPauseMenu();
            else if (GameStateManager.Instance.CurrentState == GameState.Paused)
                ResumeGame();
        }

        // 更新设置 UI 表现
        UpdateSettings();
    }

    #region 开局提示逻辑

    public void ShowLevelIntro()
    {
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        introPanel.SetActive(true);
        GameStateManager.Instance.TogglePause(); // 暂停游戏

        // 设置对应的Level图片
        int currentLevelNum = LevelManager.Instance.GetCurrentLevelNumber();
        if (currentLevelNum - 1 < levelNameSprites.Length)
        {
            levelNameImage.sprite = levelNameSprites[currentLevelNum - 1];
        }

        // 等待2秒（因为Time.timeScale=0，所以必须用Realtime）
        yield return new WaitForSecondsRealtime(2f);

        introPanel.SetActive(false);
        GameStateManager.Instance.TogglePause(); // 恢复游戏
    }

    #endregion

    #region 结算界面逻辑

    public void ShowEndScreen(bool isWin, int stars)
    {
        endPanel.SetActive(true);

        if (isWin)
        {
            endTitleText.text = "VICTORY";
            nextLevelBtn.interactable = LevelManager.Instance.HasNextLevel();
            // 显示对应数量的星星
            for (int i = 0; i < starIcons.Length; i++)
            {
                starIcons[i].SetActive(i < stars);
            }
        }
        else
        {
            endTitleText.text = "FAILURE";
            nextLevelBtn.interactable = false; // 失败禁用下一关
            // 失败默认1星
            for (int i = 0; i < starIcons.Length; i++)
            {
                starIcons[i].SetActive(i == 0);
            }
        }
    }

    #endregion

    #region 按钮事件绑定 (在Inspector中绑定)

    public void ShowPauseMenu()
    {
        pausePanel.SetActive(true);
        GameStateManager.Instance.TogglePause();
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        GameStateManager.Instance.TogglePause();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1; // 确保时间恢复
        LevelManager.Instance.RestartCurrentLevel();
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1;
        LevelManager.Instance.LoadNextLevel();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        LevelManager.Instance.LoadMainMenu();
    }

    #endregion

    #region 设置界面逻辑 (复用你提供的逻辑)

    private void InitSettings()
    {
        soundToggle.isOn = DataManager.Instance.currentSave.soundOn;
        musicToggle.isOn = DataManager.Instance.currentSave.musicOn;
        cameraToggle.isOn = DataManager.Instance.currentSave.cameraFollowOn;

        soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        cameraToggle.onValueChanged.AddListener(OnCameraToggleChanged);
    }

    private void UpdateSettings()
    {
        // 从 DataManager 读取当前存档数据，更新 UI 表现
        soundToggle.isOn = DataManager.Instance.currentSave.soundOn;
        musicToggle.isOn = DataManager.Instance.currentSave.musicOn;
        cameraToggle.isOn = DataManager.Instance.currentSave.cameraFollowOn;
    }

    private void OnSoundToggleChanged(bool isOn)
    {
        DataManager.Instance.currentSave.soundOn = isOn;
        DataManager.Instance.Save();
    }

    private void OnMusicToggleChanged(bool isOn)
    {
        DataManager.Instance.currentSave.musicOn = isOn;
        DataManager.Instance.Save();
    }

    private void OnCameraToggleChanged(bool isOn)
    {
        DataManager.Instance.currentSave.cameraFollowOn = isOn;
        DataManager.Instance.Save();
    }

    #endregion
}