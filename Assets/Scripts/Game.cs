using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    public bool isInitialized = false;
    public GameObject circle; // ��ק Circle �� GameObject ������
    public Button Restart;
    public bool GameOver;
    public bool isCircleActive = false; // Circle �Ƿ��Ѽ���isCir
    float touchTime = 0f; // ��������ʱ��
    bool isTouching = false;
    public Vector2 TouchPosition;//��ѹλ��
    public int width = 8;
    public int height = 16;
    public int mineCount = 20;
    public Board board;
    public MapGenerate generate;
    public SweepLogic sweepLogic;
    public Cell[,] state;
    private Vector2 initialTouchPosition; // ��ʼ����λ��
    private Vector3Int initialCellPosition; // ��ʼ��Ԫ��λ��
    private enum SwipeDirection { None, Up, Down } // ��������ö��
    private SwipeDirection swipeDirection = SwipeDirection.None; // ��ǰ��������

    public void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width + height);
    }
    public void Awake()
    {
        board = GetComponentInChildren<Board>();
        Restart.onClick.AddListener(RestartGame);
        sweepLogic = GetComponent<SweepLogic>(); // ��ȡSweepLogic���
    }
    public void Start()
    {
        NewGame();
    }
    public void NewGame()//��ʼ����Ϸ������GenerateCells���ɿհ׵�Ԫ��
    {
        circle.SetActive(false);
        isInitialized = false; //��ʼ��״̬
        GameOver = false;
        Restart.gameObject.SetActive(false);
        state = new Cell[width, height];
        generate.GenerateCells();//ֻ�����հ׵�Ԫ����ҵ�һ�ΰ��º����ɵ�ͼ
        Camera.main.transform.position = new Vector3(0, 0, -10f);
        board.Draw(state);
    }

    void Update()
    {
        if (!GameOver)
        {
            TouchRespond();
        }
    }
    public void TouchRespond()
    {
        if (Input.touchCount > 0) // ����Ƿ��д�����
        {
            Touch touch = Input.GetTouch(0); // ��ȡ��һ��������

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isTouching = true;
                    touchTime = Time.time; // ��¼������ʼʱ��
                    TouchPosition = touch.position;
                    swipeDirection = SwipeDirection.None; // ���û�������

                    // ��ⴥ��λ�ö�Ӧ�ĵ�Ԫ���Ƿ��ѽҿ�
                    Vector2 worldPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
                    Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
                    Cell cell = GetCell(cellPosition.x, cellPosition.y);

                    if (cell.type != Cell.Type.Invalid && !cell.revealed) // �����Ԫ��δ�ҿ�
                    {
                        // ��¼��ʼ����λ�ú͵�Ԫ��
                        initialTouchPosition = TouchPosition;
                        initialCellPosition = cellPosition;
                        isCircleActive = true; // ���� Circle ����
                        Debug.Log("������δ�ҿ��ĵ�Ԫ������ Circle ����");
                    }
                    else // �����Ԫ���ѽҿ�����Ч
                    {
                        // ��ֹ Circle ����
                        isCircleActive = false;
                        Debug.Log("�������ѽҿ��ĵ�Ԫ�񣬽�ֹ Circle ����");
                    }
                    break;

                case TouchPhase.Stationary:
                    if (isTouching && isCircleActive && Time.time - touchTime >= 0.25f) // ����ʱ����ڵ��� 0.25 ��
                    {
                        // ���� Circle ��λ�ã����ڳ�ʼ����λ�ã�
                        SetCirclePosition(initialTouchPosition);
                        // ���� Circle
                        circle.SetActive(true);
                        Debug.Log("����ʱ����ڵ��� 0.25 �룬Circle �Ѽ���");
                    }
                    break;

                case TouchPhase.Moved:
                    if (isCircleActive && circle.activeSelf) // ��� Circle �Ѽ���
                    {
                        // ��⻬������
                        DetectSwipe(touch.position);
                    }
                    break;

                case TouchPhase.Ended:
                    circle.SetActive(false);
                    if (isTouching)
                    {
                        if (Time.time - touchTime < 0.25f) // �̰�����
                        {
                            sweepLogic.Reveal(); // �������
                            Debug.Log("�ҿ�����");
                        }
                        else if (swipeDirection != SwipeDirection.None) // ��������
                        {
                            // ���ݻ��������л���Ԫ��״̬
                            HandleSwipeAction();
                        }
                    }
                    isTouching = false; // ����״̬
                    break;

                case TouchPhase.Canceled:
                    isTouching = false;
                    circle.SetActive(false);
                    Debug.Log("����ȡ��");
                    break;
            }
        }
    }//�������
    public void DetectSwipe(Vector2 currentTouchPosition)
    {
        // ���㻬������
        float swipeDistance = currentTouchPosition.y - initialTouchPosition.y;

        // ����������ֵ������ 50 ���أ�
        float swipeThreshold = 50f;

        if (Mathf.Abs(swipeDistance) > swipeThreshold)
        {
            if (swipeDistance > 0) // ���ϻ���
            {
                swipeDirection = SwipeDirection.Up;
                Debug.Log("���ϻ���");
            }
            else // ���»���
            {
                swipeDirection = SwipeDirection.Down;
                Debug.Log("���»���");
            }
        }
    }//���������»���
    public void HandleSwipeAction()
    {
        if (swipeDirection == SwipeDirection.Up) // �ϻ�����
        {
            Debug.Log("ִ�� Flags ����");
            sweepLogic.Flags(initialCellPosition); // �����ڳ�ʼ��Ԫ��
        }
        else if (swipeDirection == SwipeDirection.Down) // �»��ʺ�
        {
            Debug.Log("ִ�� Question ����");
            sweepLogic.Question(initialCellPosition); // �����ڳ�ʼ��Ԫ��
        }
    }//���ݻ���������з���
    public void SetCirclePosition(Vector2 screenPosition)
    {
        if (circle != null)
        {
            // ����Ļ����ת��Ϊ��������
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                circle.GetComponent<RectTransform>().parent as RectTransform,
                screenPosition,
                null,
                out Vector2 localPosition
            );

            // ���� Circle ��λ��
            circle.GetComponent<RectTransform>().localPosition = localPosition;
        }
    }//����Circleλ��
    public void CheckQuickReveal(int x, int y)
    {
        Cell centerCell = GetCell(x, y);
        if (!centerCell.revealed || centerCell.type != Cell.Type.Number)
            return;

        int flagCount = 0;
        List<Vector2Int> cellsToReveal = new List<Vector2Int>();
        List<Vector2Int> cellsToBlink = new List<Vector2Int>();

        // ͳ����Χ��ǵĵ�����������Ҫ��ʾ�ĵ�Ԫ��
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int checkX = x + dx;
                int checkY = y + dy;
                if (IsValid(checkX, checkY))
                {
                    Cell neighbor = GetCell(checkX, checkY);
                    if (neighbor.flagged)
                    {
                        flagCount++;
                    }
                    else if (!neighbor.revealed && !neighbor.flagged)
                    {
                        cellsToReveal.Add(new Vector2Int(checkX, checkY));
                        cellsToBlink.Add(new Vector2Int(checkX, checkY)); // ��ӵ���˸�б�
                    }
                }
            }
        }

        // ���ٽ�ʾ�����ж�
        if (flagCount >= centerCell.Number)
        {
            foreach (Vector2Int pos in cellsToReveal)
            {
                if (!IsValid(pos.x, pos.y)) continue;

                Cell cell = GetCell(pos.x, pos.y);
                if (cell.type == Cell.Type.Mine)
                {
                    sweepLogic.Explode(cell);
                    return;
                }

                if (!cell.revealed && !cell.flagged)
                {
                    if (cell.type == Cell.Type.Empty)
                    {
                        sweepLogic.Flood(cell);
                    }
                    else
                    {
                        state[pos.x, pos.y].revealed = true;
                    }
                }
            }
            sweepLogic.ifWin();
            board.Draw(state);
        }
        else
        {
            // ���ٽ�ʾ���������㣬������˸
            Debug.Log("���ٽ�ʾ���������㣬������˸");
            StartCoroutine(BlinkCells(cellsToBlink));
        }
    }//�ж��ܷ���ٽҿ�

    public IEnumerator BlinkCells(List<Vector2Int> cellsToBlink)
    {
        Debug.Log("��ʼ��˸");
        int blinkCount = 2;
        float blinkDuration = 0.05f;

        // ������˸ǰ�� Tile
    Dictionary<Vector2Int, Tile> previousTiles = new Dictionary<Vector2Int, Tile>();
        foreach (var pos in cellsToBlink)
        {
            if (state[pos.x, pos.y].tile == null)
            {
                Debug.LogError($"Tile at position ({pos.x}, {pos.y}) is null!");
            }
            else
            {
                previousTiles[pos] = state[pos.x, pos.y].tile; // ����ԭʼ Tile
                Debug.Log("��˸ǰ�� Tile: " + previousTiles[pos]);
            }
        }

        // ��˸�߼�
        for (int i = 0; i < blinkCount; i++)
        {
            Debug.Log("����Ϊ��ɫ Tile");
            foreach (var pos in cellsToBlink)
            {
                Debug.Log("��ɺ�ɫ������");
                state[pos.x, pos.y].tile = board.tileRed; // �滻Ϊ��ɫ Tile
                board.tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), board.tileRed); // ǿ������ Tile
            }
            board.tilemap.RefreshAllTiles(); // ǿ��ˢ�� Tilemap
            yield return new WaitForSeconds(blinkDuration);
            Debug.Log("�ָ�����˸ǰ�� Tile");
            foreach (var pos in cellsToBlink)
            {
                Debug.Log("��˸��� Tile: " + state[pos.x, pos.y].tile);
                state[pos.x, pos.y].tile = previousTiles[pos]; // �ָ�����˸ǰ�� Tile
                board.tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), previousTiles[pos]); // ǿ������ Tile
                board.tilemap.RefreshTile(new Vector3Int(pos.x, pos.y, 0)); // ǿ��ˢ�µ��� Tile
            }
            board.Draw(state); // ���� Tilemap ��Ⱦ
            yield return new WaitForSeconds(blinkDuration);
        }
        Debug.Log("��˸����");
    }//��˸

    public Cell GetCell(int x,int y)//��ȡ��Ԫ������
    {
        if (IsValid(x, y))
        {
            return state[x, y];
        }
        else {
            return new Cell { type = Cell.Type.Invalid };
        }
    }
    public bool IsValid(int x,int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }//�ж��Ƿ�Ϊ�߽�

    public void RestartGame()
    {
        GameOver = false;
        Restart.gameObject.SetActive(false); // ���ذ�ť

        NewGame();
    }//���¿�ʼ
}


