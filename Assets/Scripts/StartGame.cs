using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartGame : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip Initgame; // 音频源文件
    public Button startButton; // 开始按钮引用
    public Button startButton2; // 跳转到场景2的按钮引用

    private void Start()
    {
        // 绑定按钮点击事件
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        
        // 绑定场景2按钮点击事件
        if (startButton2 != null)
        {
            startButton2.onClick.AddListener(OnStartButton2Clicked);
        }
    }

    private void OnStartButtonClicked()
    {
        // 播放开始游戏音效
        audioSource.PlayOneShot(Initgame);
        StartCoroutine(LoadMainMenuAfterSound()); // 通过协程处理延迟加载
    }
    
    private void OnStartButton2Clicked()
    {
        // 播放开始游戏音效
        audioSource.PlayOneShot(Initgame);
        StartCoroutine(LoadScene2AfterSound()); // 通过协程处理延迟加载
    }
    
    IEnumerator LoadMainMenuAfterSound()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        // 加载主菜单场景
        SceneManager.LoadScene(1);
    }
    
    IEnumerator LoadScene2AfterSound()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        // 加载场景2
        SceneManager.LoadScene(2);
    }

}