using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private float touchTime = 0f;
    private bool isTouching = false;
    private Vector2 initialTouchPosition;
    private Vector3Int initialCellPosition;
    public enum SwipeDirection { None, Up, Down }
    private SwipeDirection swipeDirection = SwipeDirection.None;

    public Vector2 TouchPosition { get; private set; }
    public bool IsTouching => isTouching;
    public float TouchDuration => touchTime;
    public Vector2 InitialTouchPosition => initialTouchPosition;
    public Vector3Int InitialCellPosition => initialCellPosition;
    public SwipeDirection CurrentSwipeDirection => swipeDirection;

    private void Update()
    {
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartTouch();
        }
        else if (Input.GetMouseButton(0))
        {
            ContinueTouch();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndTouch();
        }
    }

    private void StartTouch()
    {
        isTouching = true;
        touchTime = 0f;
        initialTouchPosition = Input.mousePosition;
        TouchPosition = initialTouchPosition;
    }

    private void ContinueTouch()
    {
        if (!isTouching) return;

        touchTime += Time.deltaTime;
        TouchPosition = Input.mousePosition;
        DetectSwipe(TouchPosition);
    }

    private void EndTouch()
    {
        isTouching = false;
        touchTime = 0f;
        swipeDirection = SwipeDirection.None;
    }

    private void DetectSwipe(Vector2 currentTouchPosition)
    {
        float verticalSwipe = currentTouchPosition.y - initialTouchPosition.y;
        float swipeThreshold = Screen.height * 0.1f;

        if (Mathf.Abs(verticalSwipe) > swipeThreshold)
        {
            swipeDirection = verticalSwipe > 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }
    }

    public void SetInitialCellPosition(Vector3Int position)
    {
        initialCellPosition = position;
    }
} 