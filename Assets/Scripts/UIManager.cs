using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject HomeSceenUI;
    public GameObject InGameUI;
    public GameObject WinScreenUI;
    public GameObject LoseScreenUI;
    public GameObject PauseMenuUI;
    public GameObject SettingsMenuUI;
    public GameObject BossRangeModifierUI;
    public Image progressBar;
    public TextMeshProUGUI homeScreenLevelCounter, inGameUILevelCounter;
    
    [Header("Player Settings")]
    public Slider sensitivitySlider;
    
    public static UIManager UIManagerInstance;
    public static int currentLevel;
    public static bool skipMenuOnReload = false;

    void Awake()
    {
        UIManagerInstance = this;
        
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        PlayerControl.userSensitivity = PlayerPrefs.GetFloat("UserSensitivity", 0.5f);
        sensitivitySlider.value = PlayerControl.userSensitivity;
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChange);
        
        DeactivateAllScreens();
    }
    void Start()
    {
        if (!skipMenuOnReload)
        {
            HomeSceenUI.SetActive(true);
            PlayerSpawner.playerSpawnerInstance.StickmansSetAnimStand();
            homeScreenLevelCounter.SetText($"LEVEL - {currentLevel}");
        }  
        else StartGame();
    }

    private void DeactivateAllScreens()
    {
        InGameUI.SetActive(false);
        WinScreenUI.SetActive(false);
        LoseScreenUI.SetActive(false);
        HomeSceenUI.SetActive(false);
        SettingsMenuUI.SetActive(false);
        PauseMenuUI.SetActive(false);
        BossRangeModifierUI.SetActive(false);
    }
    
    public void OpenHomeScreen()
    {
        ResetTimeScale();
        skipMenuOnReload = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void OnRestartPress()
    {
        ResetTimeScale();
        skipMenuOnReload = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        StartGame();
    }
    
    public void OnNextLevelPress()
    {
        skipMenuOnReload = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        currentLevel++;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        StartGame();
    }

    public void OnPauseGamePress()
    {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0;
        PlayerControl.gamestate = false;
    }

    public void OnResumeGamePress()
    {
        PauseMenuUI.SetActive(false);
        PlayerControl.gamestate = true;
        Time.timeScale = 1;
    }
    
    public void StartGame()
    {
        PlayerControl.gamestate = true;
        PlayerSpawner.playerSpawnerInstance.StickmansSetAnimRun();
        HomeSceenUI.SetActive(false);
        WinScreenUI.SetActive(false);
        LoseScreenUI.SetActive(false);
        InGameUI.SetActive(true);
        inGameUILevelCounter.SetText(currentLevel.ToString());
    }

    public void OnCloseSettingsPress()
    {
        SettingsMenuUI.SetActive(false);
    }

    public void OnOpenSettingsPress()
    {
        SettingsMenuUI.SetActive(true);
    }
    
    public void onApplicationQuitPress()
    {
        Application.Quit();

        // Для редактора Unity (використовується тільки під час тестування)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnSensitivityChange(float value)
    {
        PlayerControl.userSensitivity = value;
        PlayerPrefs.SetFloat("UserSensitivity", PlayerControl.userSensitivity);
    }
    
    public void OpenWinScreen()
    {
        PlayerControl.gamestate = false;
        InGameUI.SetActive(false);
        WinScreenUI.SetActive(true);
    }

    public void OpenLoseScreen()
    {
        InGameUI.SetActive(false);
        LoseScreenUI.SetActive(true);
    }

    private void ResetTimeScale()
    {
        Time.timeScale = 1;
    }

    public void OpenBossRangeModifierUI()
    {
        Time.timeScale = 0;
        BossRangeModifierUI.SetActive(true);
        PlayerControl.gamestate = false;
    }
    
    public void CloseBossRangeModifierUI()
    {
        Time.timeScale = 1;
        BossRangeModifierUI.SetActive(false);
        PlayerControl.gamestate = true;
    }
}
