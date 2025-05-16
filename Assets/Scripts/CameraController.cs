using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;

    [Header("Touch Settings")]
    [SerializeField] private float minSwipeDistance = 20f; // 最小滑动距离
    [SerializeField] private float touchDelay = 0.1f; // 触摸延迟判定时间
    
    private Camera controlledCamera;
    private Vector2 lastTouchPosition;
    private bool isDragging = false;
    private float touchStartTime; // 触摸开始时间
    private bool isMoving = false; // 是否正在移动
    private Vector2 touchStartPosition; // 触摸开始位置

    public bool IsMoving => isMoving; // 供其他脚本查询是否在移动

    private void Awake()
    {
        controlledCamera = GetComponent<Camera>();
    }

    public void HandleTouchInput()
    {
        if (Input.touchCount == 1) // 单指触摸
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartTime = Time.time;
                    touchStartPosition = touch.position;
                    lastTouchPosition = touch.position;
                    isDragging = true;
                    isMoving = false;
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        // 计算从触摸开始到现在移动的总距离
                        float totalMovement = Vector2.Distance(touchStartPosition, touch.position);
                        
                        // 如果移动距离超过阈值，标记为移动状态
                        if (totalMovement > minSwipeDistance)
                        {
                            isMoving = true;
                        }

                        // 只有在确认是移动状态时才进行相机移动
                        if (isMoving)
                        {
                            Vector2 delta = touch.position - lastTouchPosition;
                            
                            float orthoSize = controlledCamera.orthographicSize;
                            float screenHeight = Screen.height;
                            float worldSpaceMove = (orthoSize * 2f) / screenHeight;
                            
                            Vector3 moveDirection = new Vector3(-delta.x * worldSpaceMove, -delta.y * worldSpaceMove, 0);
                            transform.position += moveDirection * moveSpeed * Time.deltaTime;
                        }
                        
                        lastTouchPosition = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // 如果触摸时间短且移动距离小，则不认为是移动操作
                    if (Time.time - touchStartTime < touchDelay && 
                        Vector2.Distance(touchStartPosition, touch.position) < minSwipeDistance)
                    {
                        isMoving = false;
                    }
                    
                    isDragging = false;
                    break;
            }
        }
        else if (Input.touchCount == 2) // 双指触摸
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;
            ZoomCamera(difference * 0.01f * zoomSpeed);
            
            isMoving = true; // 双指操作时也标记为移动状态
        }
        else
        {
            isMoving = false;
        }
    }

    /// <summary>
    /// 平移缩放
    /// </summary>
    private void ZoomCamera(float increment)
    {
        float newSize = controlledCamera.orthographicSize - increment;
        controlledCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }

    #region Public Interface

    /// <summary>
    /// 设置移动速度
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0.1f, speed);
    }

    /// <summary>
    /// 设置缩放速度
    /// </summary>
    public void SetZoomSpeed(float speed)
    {
        zoomSpeed = speed;
    }

    /// <summary>
    /// 设置缩放范围
    /// </summary>
    public void SetZoomRange(float min, float max)
    {
        minZoom = min;
        maxZoom = max;
    }

    /// <summary>
    /// 设置最小滑动距离
    /// </summary>
    public void SetMinSwipeDistance(float distance)
    {
        minSwipeDistance = Mathf.Max(1f, distance);
    }

    /// <summary>
    /// 设置触摸延迟判定时间
    /// </summary>
    public void SetTouchDelay(float delay)
    {
        touchDelay = Mathf.Max(0.01f, delay);
    }

    #endregion
}