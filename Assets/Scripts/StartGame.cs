using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartGame : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip Initgame; // ��ƵԴ���
    public Button startButton; // ��ק��ť������

    private void Start()
    {
        // �󶨰�ť����¼�
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void OnStartButtonClicked()
    {
        // �л�����Ϸ����
        audioSource.PlayOneShot(Initgame);
        StartCoroutine(LoadMainMenuAfterSound()); // ͨ��Э�̴����ӳټ���
    }
    IEnumerator LoadMainMenuAfterSound()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        // �������˵�����
        SceneManager.LoadScene(1);
    }
}