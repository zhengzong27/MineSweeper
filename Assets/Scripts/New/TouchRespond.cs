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
    private Dictionary<Vector2Int, Cell> cellStates; // ʹ���ֵ�洢��Ԫ��״̬
    [Header("UI References")]
    public GameObject circle; // ����Circle��Ϸ��
    private enum SwipeDirection { None, Up, Down }
    private SwipeDirection swipeDirection = SwipeDirection.None;
    private Vector2 initialTouchPosition;
    private Vector3Int initialCellPosition;
    private bool isCircleActive = false;
    private Board board => GameManager.Instance.board;

    private void Awake()
    {
        // ͨ���������Ի�ȡ�ֵ�

    }

    private void Update()
    {
        if (!GameManager.Instance.GameOver) // �޸ĵ�3��ͨ��GameManager�ж���Ϸ״̬
        {
            GetTouch();
        }
    }
    // ��ȡ��Ԫ�񣨼����ֵ�洢��
    private Cell GetCell(int x, int y)
    {
        // �޸ĵ�5��ͨ��GameManager��ȡcellStates
        Vector2Int pos = new Vector2Int(x, y);
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
            isCircleActive = true;
            Debug.Log("������δ�ҿ��ĵ�Ԫ������Circle����");
        }
        else
        {
            isCircleActive = false;
            Debug.Log("�������ѽҿ��ĵ�Ԫ�񣬽�ֹCircle����");
        }
    }

    private void HandleTouchStationary()
    {
        if (isTouching && isCircleActive && Time.time - touchTime >= 0.25f)
        {

            /*SetCirclePosition(initialTouchPosition);
            circle.SetActive(true);
            Debug.Log("��������Circle");*/
        }
    }

    private void HandleTouchMoved(Touch touch)
    {
        if (isCircleActive && circle.activeSelf)
        {
            DetectSwipe(touch.position);
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
            GameManager.Instance.sweep.ToggleFlag(initialCellPosition.x, initialCellPosition.y);
        }

        if (isTouching)
        {
            if (Time.time - touchTime < 0.25f)
            {
                // �̰�-�ҿ���Ԫ��
                RevealCell();
            }
            else if (swipeDirection != SwipeDirection.None)
            {
                // ����-��ǵ�Ԫ��
                HandleSwipeAction();
            }
        }
        isTouching = false;
        isCircleActive = false;
        swipeDirection = SwipeDirection.None;
    }

    private void HandleTouchCanceled()
    {
        isTouching = false;
        circle.SetActive(false);
        Debug.Log("����ȡ��");
    }

    private void DetectSwipe(Vector2 currentPosition)
    {
        float swipeDistance = currentPosition.y - initialTouchPosition.y;

        if (Mathf.Abs(swipeDistance) > 50f) // ������ֵ
        {
            swipeDirection = swipeDistance > 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }
    }

    private void HandleSwipeAction()
    {
        Vector2Int cellPos = new Vector2Int(initialCellPosition.x, initialCellPosition.y);

        if (cellStates.TryGetValue(cellPos, out Cell cell))
        {
            // ���ݻ��������л����״̬
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
                Handheld.Vibrate(); // ���ʱ�𶯷���
            }
        }
    }

    private void RevealCell()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        GetComponent<Sweep>().RevealAtPosition(cellPosition.x, cellPosition.y);
    }

    private void SetCirclePosition(Vector2 position)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(position);
        worldPosition.z = 0;
        circle.transform.position = worldPosition;
    }
}