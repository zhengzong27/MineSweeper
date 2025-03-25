using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 0.01f; // 摄像头移动速度
    private Vector2 touchStartPos; // 触摸起始位置
    private bool isDragging = false; // 是否正在拖动
    private float debugInterval = 1f; // 输出坐标的时间间隔
    private float debugTimer = 0f; // 计时器

    void Start()
    {
        // 初始化计时器
        debugTimer = debugInterval;
    }

    void Update()
    {
        HandleTouchInput();
        DebugCameraPosition();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // 记录触摸起始位置
                    touchStartPos = touch.position;
                    isDragging = true;
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        // 计算触摸移动的偏移量
                        Vector2 touchDelta = touch.position - touchStartPos;

                        // 根据偏移量移动摄像头
                        MoveCamera(touchDelta);

                        // 更新触摸起始位置
                        touchStartPos = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // 结束拖动
                    isDragging = false;
                    break;
            }
        }
    }

    void MoveCamera(Vector2 delta)
    {
        // 根据偏移量移动摄像头
        Vector3 moveDirection = new Vector3(-delta.x, -delta.y, 0) * moveSpeed;
        transform.Translate(moveDirection, Space.World);
    }

    void DebugCameraPosition()
    {
        // 计时器更新
        debugTimer -= Time.deltaTime;

        // 如果计时器小于等于0，输出摄像头坐标并重置计时器
        if (debugTimer <= 0f)
        {
            Debug.Log("Camera Position: " + transform.position);
            debugTimer = debugInterval;
        }
    }
}