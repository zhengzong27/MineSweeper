using UnityEngine;
using UnityEngine.SceneManagement; // 引入场景管理命名空间
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
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
        SceneManager.LoadScene(1); // "GameScene" 是目标场景的名称
    }
}