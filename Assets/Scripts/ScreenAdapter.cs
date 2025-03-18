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
        float designAspect = 9f / 18f; // ��ƿ�߱�

        if (screenAspect > designAspect)
        {
            // ��Ļ������������ӿ�
            mainCamera.orthographicSize = Screen.height / 2f / 100f; // 2D ��Ϸ
        }
        else
        {
            // ��Ļ���ߣ���������ӿ�
            mainCamera.orthographicSize = Screen.width / 2f / 100f / screenAspect;
        }
    }

    void AdjustBackground()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float designAspect = 9f / 16f;

        if (screenAspect > designAspect)
        {
            // ��Ļ������������ͼƬ��С
            backgroundImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        }
        else
        {
            // ��Ļ���ߣ���������ͼƬ��С
            backgroundImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        }
    }
}