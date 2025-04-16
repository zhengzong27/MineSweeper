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
    private Vector3 lastTouchPosition;
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
                    lastTouchPosition = GetWorldPosition(touch.position);
                    isDragging = true;
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector3 currentPosition = GetWorldPosition(touch.position);
                        Vector3 delta = lastTouchPosition - currentPosition;
                        MoveCamera(delta);
                        lastTouchPosition = currentPosition;
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
    /// 平移移动
    /// </summary>
    private void MoveCamera(Vector3 direction)
    {
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// 平移缩放
    /// </summary>
    private void ZoomCamera(float increment)
    {
        float newSize = controlledCamera.orthographicSize - increment;
        controlledCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }

    /// <summary>
    /// 屏幕转换为世界坐标
    /// </summary>
    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        return controlledCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, controlledCamera.nearClipPlane));
    }

    #region Public Interface

    /// <summary>
    /// 设置移动速度
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
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