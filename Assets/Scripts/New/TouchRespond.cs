using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class TouchRespond : MonoBehaviour
{
    private bool gameOver;
    private bool isTouching = false;
    private float touchTime = 0f;
    public Vector2 touchPosition;
    private Dictionary<Vector2Int, Cell> cellStates; // 使用字典存储单元格状态
    private enum SwipeDirection { None, Up, Down }
    private SwipeDirection swipeDirection = SwipeDirection.None;
    private Vector2 initialTouchPosition;
    private Vector3Int initialCellPosition;
    private Board board => GameManager.Instance.board;

    private void Awake()
    {
        // 通过公共属性获取字典
    }

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }
        if (!GameManager.Instance.GameOver) // 修改点3：通过GameManager判断游戏状态
        {
            GetTouch();
        }
    }

    // 获取单元格（兼容字典存储）
    private Cell GetCell(int x, int y)
    {
        // 修改点5：通过GameManager获取cellStates
        Vector2Int pos = new Vector2Int(x, y); 
        if (board == null)
        {
            Debug.LogError("Board is null!");
            return new Cell(pos, Cell.Type.Invalid, null);
        }
        return GameManager.Instance.sweep.CellStates.TryGetValue(pos, out Cell cell)
            ? cell
            : new Cell(pos, Cell.Type.Invalid, null);
    }

    public void GetTouch()
    {
        if (!gameOver && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchBegan(touch);
                    break;

                case TouchPhase.Stationary:
                    HandleTouchStationary();
                    break;

                case TouchPhase.Moved:
                    HandleTouchMoved(touch);
                    break;

                case TouchPhase.Ended:
                    HandleTouchEnded();
                    break;

                case TouchPhase.Canceled:
                    HandleTouchCanceled();
                    break;
            }
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        isTouching = true;
        touchTime = Time.time;
        touchPosition = touch.position;
        swipeDirection = SwipeDirection.None;

        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type != Cell.Type.Invalid && !cell.revealed)
        {
            initialTouchPosition = touchPosition;
            initialCellPosition = cellPosition;
        }
    }

    private void HandleTouchStationary()
    {
        // No circle-related functionality remains
    }

    private void HandleTouchMoved(Touch touch)
    {
        float swipeDistance = touch.position.y - initialTouchPosition.y;

        if (Mathf.Abs(swipeDistance) > 50f) // 滑动阈值
        {
            swipeDirection = swipeDistance > 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }
    }

    private void HandleTouchEnded()
    {
        if (swipeDirection == SwipeDirection.None)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPosition);
            Vector3Int cellPos = board.tilemap.WorldToCell(worldPos);
            Vector2Int cellKey = new Vector2Int(cellPos.x, cellPos.y);

            GameManager.Instance.sweep.Reveal(cellKey);
        }
        else
        {
            Vector2Int flagPos = new Vector2Int(initialCellPosition.x, initialCellPosition.y);
            GameManager.Instance.sweep.ToggleFlag(flagPos);
        }

        if (isTouching)
        {
            if (Time.time - touchTime < 0.25f)
            {
                // 短按-揭开单元格
                RevealCell();
            }
            else if (swipeDirection != SwipeDirection.None)
            {
                // 滑动-标记单元格
                HandleSwipeAction();
            }
        }
        isTouching = false;
        swipeDirection = SwipeDirection.None;
    }

    private void HandleTouchCanceled()
    {
        isTouching = false;
    }

    private void HandleSwipeAction()
    {
        if (GameManager.Instance.sweep == null)
        {
            Debug.LogError("Sweep is null!");
            return;
        }
        if (GameManager.Instance.sweep.CellStates == null)
        {
            Debug.LogError("CellStates is null!");
            return;
        }
        Vector2Int cellPos = new Vector2Int(initialCellPosition.x, initialCellPosition.y);

        if (cellStates.TryGetValue(cellPos, out Cell cell))
        {
            // 根据滑动方向切换标记状态
            switch (swipeDirection)
            {
                case SwipeDirection.Up:
                    cell.flagged = !cell.flagged;
                    cell.questioned = false;
                    break;

                case SwipeDirection.Down:
                    cell.questioned = !cell.questioned;
                    cell.flagged = false;
                    break;
            }

            cellStates[cellPos] = cell;
            board.Draw(cellStates);

            if (cell.flagged)
            {
                Handheld.Vibrate(); // 标记时震动反馈
            }
        }
    }

    private void RevealCell()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        GetComponent<Sweep>().RevealAtPosition(cellPosition.x, cellPosition.y);
    }
}