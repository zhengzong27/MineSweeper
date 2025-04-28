using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class ItemMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject itemMenuPanel; // ItemMenu面板
    [SerializeField] private GameObject menuPanel; // 主菜单面板
    [SerializeField] private Toggle audioToggle; // 音效开关
    [SerializeField] private Toggle vibrationToggle; // 震动开关
    [SerializeField] private Button backButton; // 返回按钮

    [Header("Audio")]
    [SerializeField] private AudioSource[] allAudioSources; // 所有音频源
    [SerializeField] private AudioClip buttonClickSound; // 按钮点击音效

    // 单例模式
    public static ItemMenuController Instance { get; private set; }

    // 设置状态
    public bool IsAudioEnabled { get; private set; }
    public bool IsVibrationEnabled { get; private set; }

    // 保存UI组件的完整路径
    private string itemMenuPanelPath;
    private string menuPanelPath;
    private string audioTogglePath;
    private string vibrationTogglePath;
    private string backButtonPath;

    private void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 保存UI组件的完整路径
            if (itemMenuPanel != null)
            {
                itemMenuPanelPath = GetGameObjectPath(itemMenuPanel);
                DontDestroyOnLoad(itemMenuPanel);
            }
            if (menuPanel != null)
            {
                menuPanelPath = GetGameObjectPath(menuPanel);
                DontDestroyOnLoad(menuPanel);
            }
            if (audioToggle != null)
            {
                audioTogglePath = GetGameObjectPath(audioToggle.gameObject);
                DontDestroyOnLoad(audioToggle.gameObject);
            }
            if (vibrationToggle != null)
            {
                vibrationTogglePath = GetGameObjectPath(vibrationToggle.gameObject);
                DontDestroyOnLoad(vibrationToggle.gameObject);
            }
            if (backButton != null)
            {
                backButtonPath = GetGameObjectPath(backButton.gameObject);
                DontDestroyOnLoad(backButton.gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 添加场景加载完成的监听
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 移除场景加载完成的监听
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    private GameObject FindGameObjectByPath(string path)
    {
        string[] names = path.Split('/');
        GameObject current = GameObject.Find(names[0]);
        
        for (int i = 1; i < names.Length && current != null; i++)
        {
            Transform child = current.transform.Find(names[i]);
            if (child != null)
            {
                current = child.gameObject;
            }
            else
            {
                return null;
            }
        }
        
        return current;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 使用保存的路径重新获取UI组件
        if (itemMenuPanel == null && !string.IsNullOrEmpty(itemMenuPanelPath))
        {
            itemMenuPanel = FindGameObjectByPath(itemMenuPanelPath);
            if (itemMenuPanel != null)
            {
                DontDestroyOnLoad(itemMenuPanel);
            }
        }
        if (menuPanel == null && !string.IsNullOrEmpty(menuPanelPath))
        {
            menuPanel = FindGameObjectByPath(menuPanelPath);
            if (menuPanel != null)
            {
                DontDestroyOnLoad(menuPanel);
            }
        }
        if (audioToggle == null && !string.IsNullOrEmpty(audioTogglePath))
        {
            GameObject audioToggleObj = FindGameObjectByPath(audioTogglePath);
            if (audioToggleObj != null)
            {
                audioToggle = audioToggleObj.GetComponent<Toggle>();
                if (audioToggle != null)
                {
                    DontDestroyOnLoad(audioToggle.gameObject);
                }
            }
        }
        if (vibrationToggle == null && !string.IsNullOrEmpty(vibrationTogglePath))
        {
            GameObject vibrationToggleObj = FindGameObjectByPath(vibrationTogglePath);
            if (vibrationToggleObj != null)
            {
                vibrationToggle = vibrationToggleObj.GetComponent<Toggle>();
                if (vibrationToggle != null)
                {
                    DontDestroyOnLoad(vibrationToggle.gameObject);
                }
            }
        }
        if (backButton == null && !string.IsNullOrEmpty(backButtonPath))
        {
            GameObject backButtonObj = FindGameObjectByPath(backButtonPath);
            if (backButtonObj != null)
            {
                backButton = backButtonObj.GetComponent<Button>();
                if (backButton != null)
                {
                    DontDestroyOnLoad(backButton.gameObject);
                }
            }
        }

        // 重新初始化UI
        InitializeUI();
    }

    private void InitializeUI()
    {
        // 确保ItemMenu初始状态为隐藏
        if (itemMenuPanel != null)
        {
            itemMenuPanel.SetActive(false);
        }

        // 设置Toggle监听器
        if (audioToggle != null)
        {
            audioToggle.onValueChanged.AddListener(OnAudioToggleChanged);
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggleChanged);
        }

        // 设置返回按钮监听器
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void OnEnable()
    {
        // 确保ItemMenu初始状态为隐藏
        if (itemMenuPanel != null)
        {
            itemMenuPanel.SetActive(false);
        }

        // 设置Toggle监听器
        if (audioToggle != null)
        {
            audioToggle.onValueChanged.AddListener(OnAudioToggleChanged);
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggleChanged);
        }

        // 设置返回按钮监听器
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    // 显示ItemMenu
    public void ShowItemMenu()
    {
        if (itemMenuPanel != null)
        {
            itemMenuPanel.SetActive(true);
        }
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    // 隐藏ItemMenu
    private void HideItemMenu()
    {
        if (itemMenuPanel != null)
        {
            itemMenuPanel.SetActive(false);
        }
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
    }

    // 音效开关变化处理
    private void OnAudioToggleChanged(bool isOn)
    {
        IsAudioEnabled = isOn;
        UpdateAudioState();
        SaveSettings();
    }

    // 震动开关变化处理
    private void OnVibrationToggleChanged(bool isOn)
    {
        IsVibrationEnabled = isOn;
        SaveSettings();
    }

    // 更新音频状态
    private void UpdateAudioState()
    {
        if (allAudioSources != null)
        {
            foreach (var audioSource in allAudioSources)
            {
                if (audioSource != null)
                {
                    audioSource.mute = !IsAudioEnabled;
                }
            }
        }
    }

    // 返回按钮点击处理
    private void OnBackButtonClicked()
    {
        // 播放按钮音效
        if (IsAudioEnabled && buttonClickSound != null)
        {
            AudioSource.PlayClipAtPoint(buttonClickSound, Camera.main.transform.position);
        }
        
        HideItemMenu();
    }

    // 保存设置
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("AudioEnabled", IsAudioEnabled ? 1 : 0);
        PlayerPrefs.SetInt("VibrationEnabled", IsVibrationEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 加载设置
    private void LoadSettings()
    {
        // 默认值为true
        IsAudioEnabled = PlayerPrefs.GetInt("AudioEnabled", 1) == 1;
        IsVibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;

        // 更新UI状态
        if (audioToggle != null)
        {
            audioToggle.isOn = IsAudioEnabled;
        }
        if (vibrationToggle != null)
        {
            vibrationToggle.isOn = IsVibrationEnabled;
        }

        // 更新音频状态
        UpdateAudioState();
    }

    // 触发震动（供其他脚本调用）
    public void TriggerVibration()
    {
        if (IsVibrationEnabled)
        {
            Handheld.Vibrate();
        }
    }
} 