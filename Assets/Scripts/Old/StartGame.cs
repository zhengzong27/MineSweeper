using UnityEngine;
using UnityEngine.SceneManagement; // ���볡�����������ռ�
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
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
        SceneManager.LoadScene(1); // "GameScene" ��Ŀ�곡��������
    }
}