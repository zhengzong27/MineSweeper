using UnityEngine;
using UnityEngine.UI;

public class ScreenAdapter : MonoBehaviour
{
    public Camera mainCamera;
    public Canvas canvas;
    public Image backgroundImage;

    void Start()
    {
        AdjustCamera();
        AdjustBackground();
    }

    void AdjustCamera()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float designAspect = 9f / 18f; // 设计宽高比

        if (screenAspect > designAspect)
        {
            // 屏幕更宽，调整相机视口
            mainCamera.orthographicSize = Screen.height / 2f / 100f; // 2D 游戏
        }
        else
        {
            // 屏幕更高，调整相机视口
            mainCamera.orthographicSize = Screen.width / 2f / 100f / screenAspect;
        }
    }

    void AdjustBackground()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float designAspect = 9f / 16f;

        if (screenAspect > designAspect)
        {
            // 屏幕更宽，调整背景图片大小
            backgroundImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        }
        else
        {
            // 屏幕更高，调整背景图片大小
            backgroundImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        }
    }
}