using UnityEngine;
using UnityEngine.UI;
using System;

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

    private void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化UI组件
        InitializeUI();
        
        // 加载保存的设置
        LoadSettings();
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