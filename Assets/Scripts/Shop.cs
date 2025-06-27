using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Shop : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip Initgame; // 音频源文件
    public Button backButton; // 返回按钮引用

    private void Start()
    {
        // 绑定按钮点击事件
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void OnBackButtonClicked()
    {
        // 播放开始游戏音效
        audioSource.PlayOneShot(Initgame);
        StartCoroutine(LoadScene1AfterSound()); // 通过协程处理延迟加载
    }
    
    IEnumerator LoadScene1AfterSound()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        // 加载场景1
        SceneManager.LoadScene(0);
    }
} 