using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartGame : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip Initgame; // 音频源组件
    public Button startButton; // 拖拽按钮到这里

    private void Start()
    {
        // 绑定按钮点击事件
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void OnStartButtonClicked()
    {
        // 切换到游戏场景
        audioSource.PlayOneShot(Initgame);
        StartCoroutine(LoadMainMenuAfterSound()); // 通过协程处理延迟加载
    }
    IEnumerator LoadMainMenuAfterSound()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        // 加载主菜单场景
        SceneManager.LoadScene(1);
    }
}