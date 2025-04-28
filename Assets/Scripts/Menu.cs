using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Menu : MonoBehaviour
{
    [SerializeField] private Button continueButton; // 继续游戏按钮
    [SerializeField] private Button mainMenuButton; // 返回主菜单按钮
    [SerializeField] private Button itemButton; // Item按钮
    [SerializeField] private GameObject menuPanel; // 菜单面板
    [Header("Audio")]
    public AudioSource audioSource; // 音频源组件
    public AudioClip TouchUI;

    private void Awake()
    {
        // 添加场景加载完成的监听
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 移除场景加载完成的监听
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 重新获取所有UI组件的引用
        if (menuPanel == null)
        {
            menuPanel = GameObject.Find("MenuPanel");
        }
        if (continueButton == null)
        {
            continueButton = GameObject.Find("ContinueButton")?.GetComponent<Button>();
        }
        if (mainMenuButton == null)
        {
            mainMenuButton = GameObject.Find("MainMenuButton")?.GetComponent<Button>();
        }
        if (itemButton == null)
        {
            itemButton = GameObject.Find("ItemButton")?.GetComponent<Button>();
        }

        // 重新初始化UI
        InitializeUI();
    }

    private void InitializeUI()
    {
        // 确保菜单面板初始状态为关闭
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        // 设置按钮点击事件
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        if (itemButton != null)
        {
            itemButton.onClick.AddListener(OnItemButtonClicked);
        }
    }

    // 显示菜单
    public void ShowMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            // 暂停游戏
            Time.timeScale = 0f;
        }
    }

    // 隐藏菜单
    public void HideMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            // 恢复游戏
            Time.timeScale = 1f;
        }
    }

    // 检查菜单是否处于激活状态
    public bool IsMenuActive()
    {
        return menuPanel != null && menuPanel.activeSelf;
    }

    // 继续游戏按钮点击事件
    private void OnContinueClicked()
    {
        audioSource.PlayOneShot(TouchUI);
        HideMenu();
    }

    // 返回主菜单按钮点击事件
    private void OnMainMenuClicked()
    {
        // 播放音效并等待完成
        if (audioSource != null && TouchUI != null)
        {
            audioSource.PlayOneShot(TouchUI);
            // 等待音效播放完成后再加载场景
            StartCoroutine(LoadMainMenuAfterSound());
        }
        else
        {
            // 如果没有音效，直接加载场景
            SceneManager.LoadScene("BeginScene");
        }
    }

    private IEnumerator LoadMainMenuAfterSound()
    {
        // 等待音效播放完成
        yield return new WaitForSeconds(TouchUI.length);
        // 加载场景
        SceneManager.LoadScene("BeginScene");
    }

    // Item按钮点击事件
    private void OnItemButtonClicked()
    {
        audioSource.PlayOneShot(TouchUI);
        if (ItemMenuController.Instance != null)
        {
            ItemMenuController.Instance.ShowItemMenu();
            HideMenu(); // 隐藏主菜单
        }
    }
}