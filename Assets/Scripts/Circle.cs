using UnityEngine;

public class Circle : MonoBehaviour
{
    public GameObject circle; // 拖拽 Circle 的 GameObject 到这里
    private float touchStartTime = 0f; // 记录触摸开始时间
    private bool isTouching = false; // 是否正在触摸
    private Vector2 touchStartPosition; // 记录触摸起始位置
    private bool isCircleActive = false; // Circle 是否已激活

    private void Update()
    {
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        // 检测触摸输入
        if (Input.touchCount > 0) // 如果有触摸输入
        {
            Touch touch = Input.GetTouch(0); // 获取第一个触摸点

            switch (touch.phase)
            {
                case TouchPhase.Began: // 触摸开始
                    isTouching = true;
                    touchStartTime = Time.time; // 记录触摸开始时间
                    touchStartPosition = touch.position; // 记录触摸起始位置
                    Debug.Log("触摸开始，位置: " + touchStartPosition);
                    break;

                case TouchPhase.Stationary: // 触摸保持
                    if (isTouching && Time.time - touchStartTime >= 2.5f) // 触摸时间大于等于 2.5 秒
                    {
                        // 设置 Circle 的位置
                        SetCirclePosition(touchStartPosition);
                        // 激活 Circle
                        circle.SetActive(true);
                        isCircleActive = true;
                        Debug.Log("Circle 已激活");
                    }
                    break;

                case TouchPhase.Moved: // 触摸移动
                    if (isCircleActive) // 如果 Circle 已激活
                    {
                        // 检测滑动方向
                        DetectSwipe(touch.position);
                    }
                    break;

                case TouchPhase.Ended: // 触摸结束
                case TouchPhase.Canceled: // 触摸取消
                    isTouching = false;
                    isCircleActive = false;
                    // 手指离开屏幕时，隐藏 Circle
                    circle.SetActive(false);
                    Debug.Log("触摸结束，Circle 已隐藏");
                    break;
            }
        }
    }

    private void SetCirclePosition(Vector2 screenPosition)
    {
        if (circle != null)
        {
            Debug.Log("屏幕坐标: " + screenPosition);
            // 将屏幕坐标转换为世界坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                circle.GetComponent<RectTransform>().parent as RectTransform,
                screenPosition,
                null,
                out Vector2 localPosition
            );
            Debug.Log("转换后的局部坐标: " + localPosition);

            // 设置 Circle 的位置
            circle.GetComponent<RectTransform>().localPosition = localPosition;
        }
    }

    private void DetectSwipe(Vector2 currentTouchPosition)
    {
        // 计算滑动距离
        float swipeDistance = currentTouchPosition.y - touchStartPosition.y;

        // 滑动距离阈值（例如 50 像素）
        float swipeThreshold = 50f;

        if (Mathf.Abs(swipeDistance) > swipeThreshold)
        {
            if (swipeDistance > 0) // 向上滑动
            {
                Debug.Log("向上滑动，执行 Flags 方法");
                Flags();
            }
            else // 向下滑动
            {
                Debug.Log("向下滑动，执行 Question 方法");
                Question();
            }

            // 重置触摸起始位置，避免重复触发
            touchStartPosition = currentTouchPosition;
        }
    }

    private void Flags()
    {
        // TODO: 实现 Flags 方法
        Debug.Log("Flags 方法被调用");
    }

    private void Question()
    {
        // TODO: 实现 Question 方法
        Debug.Log("Question 方法被调用");
    }
}