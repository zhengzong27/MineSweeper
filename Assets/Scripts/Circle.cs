using UnityEngine;
using UnityEngine.UI;

public class Circle : MonoBehaviour
{
    public GameObject circle; // 拖拽 Circle 的 GameObject 到这里
    public float longPressDuration = 2.5f; // 长按时间阈值
    private Vector2 pressPosition; // 按压位置

  /*  private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // 如果是触摸设备
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                StartPress(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                EndPress();
            }
        }

        // 如果正在按压且时间超过阈值，显示 Circle
        if (isPressing && Time.time - pressStartTime >= longPressDuration)
        {
            ShowCircle();
        }
    }*/

    private void StartPress(Vector2 position)
    {

        pressPosition = position;
    }

    private void EndPress()
    {

        HideCircle();
    }

    private void ShowCircle()
    {
        if (circle != null)
        {
            // 将 Circle 设置为显示
            circle.SetActive(true);

            // 将 Circle 的位置设置为按压位置
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                circle.GetComponent<RectTransform>().parent as RectTransform,
                pressPosition,
                null,
                out Vector2 localPoint
            );
            circle.GetComponent<RectTransform>().localPosition = localPoint;
        }
    }

    private void HideCircle()
    {
        if (circle != null)
        {
            // 将 Circle 设置为隐藏
            circle.SetActive(false);
        }
    }
}