using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Menu : MonoBehaviour
{
    [SerializeField] private Button continueButton; // 继续游戏按钮
    [SerializeField] private Button mainMenuButton; // 返回主菜单按钮
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
            mainMenuButton.onClick.AddListener(OnMainMenuClicked); // 绑定到void方法
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

    // 返回主菜单按钮点击事件（改为void方法）
    private void OnMainMenuClicked()
    {
        audioSource.PlayOneShot(TouchUI);
        StartCoroutine(LoadMainMenuAfterSound()); // 通过协程处理延迟加载
    }

    // 协程：等待音效播放完毕后加载主菜单
    IEnumerator LoadMainMenuAfterSound()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        // 加载主菜单场景
        SceneManager.LoadScene(0);
    }
}