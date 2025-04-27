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
        audioSource.PlayOneShot(TouchUI);
        StartCoroutine(LoadMainMenuAfterSound());
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

    private IEnumerator LoadMainMenuAfterSound()
    {
        yield return new WaitForSeconds(TouchUI.length);
        SceneManager.LoadScene("MainMenu");
    }
}