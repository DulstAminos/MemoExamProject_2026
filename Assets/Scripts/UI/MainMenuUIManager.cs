using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject levelSelectPanel;
    public GameObject settingsPanel;

    [Header("Level Selection")]
    public Button[] levelButtons; // 顺序拖入Btn_Level1, Btn_Level2, Btn_Level3等
    public string[] levelSceneNames = { "Level_1", "Level_2", "Level_3", "Level_4", "Level_5" }; // 对应的场景名

    [Header("Settings Toggles")]
    public Toggle soundToggle;
    public Toggle musicToggle;
    public Toggle cameraToggle;

    private void Start()
    {
        // 初始状态只显示主菜单
        ShowPanel(mainPanel);

        // 初始化设置界面的状态
        InitSettings();
    }

    private void Update()
    {
        // 监听是否按下了 Esc 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 判断当前是否在“选关界面”或“设置界面”
            if (levelSelectPanel.activeSelf || settingsPanel.activeSelf)
            {
                // 如果是，则调用回到主界面的方法
                Click_BackToMain();
            }
        }
    }

    // --- 面板切换逻辑 ---
    private void ShowPanel(GameObject panelToShow)
    {
        mainPanel.SetActive(panelToShow == mainPanel);
        levelSelectPanel.SetActive(panelToShow == levelSelectPanel);
        settingsPanel.SetActive(panelToShow == settingsPanel);
    }

    public void Click_OpenLevelSelect()
    {
        RefreshLevelButtons(); // 每次打开选关界面都刷新一次解锁状态
        ShowPanel(levelSelectPanel);
    }

    public void Click_OpenSettings()
    {
        ShowPanel(settingsPanel);
    }

    public void Click_BackToMain() // 给选关和设置界面的“返回”按钮使用（如果有的话），或者按ESC调用
    {
        ShowPanel(mainPanel);
    }

    public void Click_QuitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }

    // --- 选关界面逻辑 ---
    private void RefreshLevelButtons()
    {
        // 获取当前最高解锁关卡 (由DataManager管理)
        int reached = DataManager.Instance.currentSave.reachedLevel;

        Debug.Log($"当前存档最高解锁关卡是: {reached}");

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1; // 关卡1对应 index 1
            if (levelIndex <= reached)
            {
                levelButtons[i].interactable = true; // 已解锁，高亮并可点击
            }
            else
            {
                levelButtons[i].interactable = false; // 未解锁，置灰且不可点击
            }
        }
    }

    // 绑定到各个选关按钮的点击事件上
    public void Click_LoadLevel(int levelIndexZeroBased)
    {
        // 调用 LevelManager 切换场景
        string sceneToLoad = levelSceneNames[levelIndexZeroBased];
        LevelManager.Instance.LoadLevel(sceneToLoad);
    }

    // --- 设置界面逻辑 ---
    private void InitSettings()
    {
        // 从 DataManager 读取当前存档数据，更新 UI 表现
        soundToggle.isOn = DataManager.Instance.currentSave.soundOn;
        musicToggle.isOn = DataManager.Instance.currentSave.musicOn;
        cameraToggle.isOn = DataManager.Instance.currentSave.cameraFollowOn;

        // 绑定 Toggle 值改变时的事件
        soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        cameraToggle.onValueChanged.AddListener(OnCameraToggleChanged);
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
}