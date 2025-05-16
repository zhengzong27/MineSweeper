using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;

    private Camera controlledCamera;
    private Vector2 lastTouchPosition;
    private bool isDragging = false;

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
                    lastTouchPosition = touch.position;
                    isDragging = true;
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 delta = touch.position - lastTouchPosition;
                        
                        float orthoSize = controlledCamera.orthographicSize;
                        float screenHeight = Screen.height;
                        float worldSpaceMove = (orthoSize * 2f) / screenHeight;
                        
                        Vector3 moveDirection = new Vector3(-delta.x * worldSpaceMove, -delta.y * worldSpaceMove, 0);
                        transform.position += moveDirection * moveSpeed * Time.deltaTime;
                        
                        lastTouchPosition = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
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

    #endregion
}