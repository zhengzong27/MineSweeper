/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    private bool isInitialized = false;
    public GameObject circle; // ��ק Circle �� GameObject ������
    public Button Restart;
    private bool GameOver;
    private bool isCircleActive = false; // Circle �Ƿ��Ѽ���isCir
    float touchTime = 0f; // ��������ʱ��
    bool isTouching = false;
    public Vector2 TouchPosition;//��ѹλ��
    public int width = 8;
    public int height = 16;
    public int mineCount = 20;
    private Board board;
    private Cell[,] state;
    private Vector2 initialTouchPosition; // ��ʼ����λ��
    private Vector3Int initialCellPosition; // ��ʼ��Ԫ��λ��
    private enum SwipeDirection { None, Up, Down } // ��������ö��
    private SwipeDirection swipeDirection = SwipeDirection.None; // ��ǰ��������
    private void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width + height);
    }
    private void Awake()
    {
        board = GetComponentInChildren<Board>();
        Restart.onClick.AddListener(RestartGame);
    }
    private void Start()
    {
        NewGame();
    }
    private void NewGame()
    {
        circle.SetActive(false);
        isInitialized = false; //��ʼ��״̬
        GameOver = false;
        Restart.gameObject.SetActive(false);
        state = new Cell[width, height];
        GenerateCells();//ֻ�����հ׵�Ԫ����ҵ�һ�ΰ��º����ɵ�ͼ
        Camera.main.transform.position = new Vector3(0, 0, -10f);
        board.Draw(state);
    }
    private void GenerateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;
            }
        }
    }

    private void InitializeWithFirstClick(Vector2Int firstClick)
    {
        // ����1: ������ȫ����
        HashSet<Vector2Int> forbiddenArea = new HashSet<Vector2Int>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int x = Mathf.Clamp(firstClick.x + dx, 0, width - 1);
                int y = Mathf.Clamp(firstClick.y + dy, 0, height - 1);
                forbiddenArea.Add(new Vector2Int(x, y));
            }
        }

        // ����2: ���ɺ�ѡλ��
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!forbiddenArea.Contains(new Vector2Int(x, y)))
                {
                    candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        // ����3: �������
        int mineCount = Mathf.Min(this.mineCount, candidates.Count);
        System.Random rng = new System.Random();
        for (int i = 0; i < mineCount; i++)
        {
            int index = rng.Next(i, candidates.Count);
            Vector2Int temp = candidates[i];
            candidates[i] = candidates[index];
            candidates[index] = temp;

            Vector2Int pos = candidates[i];
            state[pos.x, pos.y].type = Cell.Type.Mine;
        }

        // ����4: ��������
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (state[x, y].type != Cell.Type.Mine)
                {
                    int count = CountMines(x, y);
                    state[x, y].Number = count;
                    state[x, y].type = count > 0 ? Cell.Type.Number : Cell.Type.Empty;
                }
            }
        }

        isInitialized = true;
    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0;
        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0)
                {
                    continue;
                }
                int x = cellX + adjacentX;
                int y = cellY + adjacentY;
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue;
                }
                if (state[x, y].type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }
        return count;
    }
   *//* void Update()
    {
        if (!GameOver)
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
                                Reveal(); // �������
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
        }
    }*/

    /*    private void SetCirclePosition(Vector2 screenPosition)
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
    }*/
/*private void DetectSwipe(Vector2 currentTouchPosition)
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
    }*/
/*    private void HandleSwipeAction()
    {
        if (swipeDirection == SwipeDirection.Up) // �ϻ�����
        {
            Debug.Log("ִ�� Flags ����");
            Flags(initialCellPosition); // �����ڳ�ʼ��Ԫ��
        }
        else if (swipeDirection == SwipeDirection.Down) // �»��ʺ�
        {
            Debug.Log("ִ�� Question ����");
            Question(initialCellPosition); // �����ڳ�ʼ��Ԫ��
        }
    }*/
/*private void Question(Vector3Int cellPosition)
{
    Debug.Log("���� Question ����");

    // ��ȡ��ʼ��Ԫ��
    Cell cell = GetCell(cellPosition.x, cellPosition.y);

    // �����Ԫ����Ч���ѽҿ���ֱ�ӷ���
    if (cell.type == Cell.Type.Invalid || cell.revealed)
    {
        Debug.Log("��Ԫ����Ч���ѽҿ���ֱ�ӷ���");
        return;
    }

    // �л��ʺű��״̬
    cell.flagged = false; // �������״̬
    cell.questioned = !cell.questioned; // �л��ʺ�״̬
                                        // ���µ�Ԫ��� Tile
    if (cell.questioned)
    {
        Debug.Log("���õ�Ԫ��Ϊ�ʺ� Tile");
        cell.tile = board.tileQuestion; // ���� tile ����
        board.tilemap.SetTile(cellPosition, board.tileQuestion); // ���� Tilemap �е� Tile
    }
    else
    {
        Debug.Log("�ָ���Ԫ��Ϊδ֪ Tile");
        cell.tile = board.tileUnknown; // ���� tile ����
        board.tilemap.SetTile(cellPosition, board.tileUnknown); // ���� Tilemap �е� Tile
    }
    state[cellPosition.x, cellPosition.y] = cell;// ����״̬����

    // ���µ�Ԫ����ͼ
    if (cell.questioned)
    {
        Debug.Log("���õ�Ԫ��Ϊ�ʺ���ͼ");
        board.tilemap.SetTile(cellPosition, board.tileQuestion); // ����Ϊ�ʺ���ͼ
    }
    else
    {
        Debug.Log("�ָ���Ԫ��Ϊδ֪��ͼ");
        board.tilemap.SetTile(cellPosition, board.tileUnknown); // �ָ�Ϊδ֪��ͼ
    }

    // �����ǳɹ���������
    if (cell.questioned)
    {
        Handheld.Vibrate();
    }

    // ǿ��ˢ�� Tilemap
    board.tilemap.RefreshAllTiles();

    Debug.Log("Question ���������ڵ�Ԫ��: (" + cellPosition.x + ", " + cellPosition.y + ")");
}*/
/*  private void Reveal()
  {
      Vector2 worldPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
      Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
      Cell cell = GetCell(cellPosition.x, cellPosition.y);

      if (!isInitialized)
      {
          // �״ε��ʱ��ʼ����ͼ
          InitializeWithFirstClick(new Vector2Int(cellPosition.x, cellPosition.y));
      }

      if (cell.type == Cell.Type.Invalid || cell.flagged) // �����Ԫ����Ч���������Ϊ�ʺ�
      {
          return;
      }

      switch (cell.type)
      {
          case Cell.Type.Mine:
              Explode(cell);
              break;
          case Cell.Type.Empty:
              Flood(cell);
              ifWin();
              break;
          case Cell.Type.Number: // �������ٽҿ��߼�
              Debug.Log("�������ֵ�Ԫ��");
              if (cell.revealed)
              {
                  CheckQuickReveal(cellPosition.x, cellPosition.y);
              }
              else
              {
                  cell.revealed = true;
                  state[cellPosition.x, cellPosition.y] = cell;
                  ifWin();
              }
              break;
      }

      board.Draw(state);
  }*//*
  private void CheckQuickReveal(int x, int y)
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
                  Explode(cell);
                  return;
              }

              if (!cell.revealed && !cell.flagged)
              {
                  if (cell.type == Cell.Type.Empty)
                  {
                      Flood(cell);
                  }
                  else
                  {
                      state[pos.x, pos.y].revealed = true;
                  }
              }
          }
          ifWin();
          board.Draw(state);
      }
      else
      {
          // ���ٽ�ʾ���������㣬������˸
          Debug.Log("���ٽ�ʾ���������㣬������˸");
          StartCoroutine(BlinkCells(cellsToBlink));
      }
  }

  private IEnumerator BlinkCells(List<Vector2Int> cellsToBlink)
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
  }
  private void Explode(Cell cell)
  {
      Debug.Log("������!");
      Restart.gameObject.SetActive(true);
      GameOver = true;
      cell.revealed = true;
      cell.exploded = true;
      state[cell.position.x, cell.position.y] = cell;
      for (int x = 0; x < width; x++)
      {
          for (int y = 0; y < height; y++)
          {
              cell = state[x, y];
              if (cell.type == Cell.Type.Mine)
              {
                  cell.revealed = true;
                  state[x, y] = cell;
              }
          }
      }
  }
  private void Flood(Cell cell)
  {
      if (cell.revealed) return;
      if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid || cell.flagged) return;

      cell.revealed = true;
      state[cell.position.x, cell.position.y] = cell;

      if (cell.type == Cell.Type.Empty)
      {
          // �˷���ݹ�ҿ�
          for (int dx = -1; dx <= 1; dx++)
          {
              for (int dy = -1; dy <= 1; dy++)
              {
                  if (dx == 0 && dy == 0) continue; // ��������

                  int x = cell.position.x + dx;
                  int y = cell.position.y + dy;

                  if (IsValid(x, y))
                  {
                      Cell neighbor = GetCell(x, y);
                      Flood(neighbor);
                  }
              }
          }
      }
      board.Draw(state);
  }
 *//* private void Flags(Vector3Int cellPosition)
  {
      // ��ȡ��ʼ��Ԫ��
      Cell cell = GetCell(cellPosition.x, cellPosition.y);

      // �����Ԫ����Ч���ѽҿ���ֱ�ӷ���
      if (cell.type == Cell.Type.Invalid || cell.revealed)
      {
          return;
      }

      // �л����״̬
      cell.flagged = !cell.flagged;
      state[cellPosition.x, cellPosition.y] = cell;

      // �����ǳɹ���������
      if (cell.flagged)
      {
          Handheld.Vibrate();
      }

      // ����������Ⱦ
      board.Draw(state);

      Debug.Log("Flags ���������ڵ�Ԫ��: (" + cellPosition.x + ", " + cellPosition.y + ")");
  }*/
/*    private Cell GetCell(int x, int y)
    {
        if (IsValid(x, y))
        {
            return state[x, y];
        }
        else
        {
            return new Cell { type = Cell.Type.Invalid };
        }
    }*/
/*private bool IsValid(int x, int y)
{
    return x >= 0 && x < width && y >= 0 && y < height;
}

private void ifWin()
{
    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            Cell cell = state[x, y];
            if (cell.type != Cell.Type.Mine && !cell.revealed)
            {
                return;
            }
        }
    }

    Debug.Log("��Ӯ�ˣ�");
    Restart.gameObject.SetActive(true);
    GameOver = true;
    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            Cell cell = state[x, y];
            if (cell.type == Cell.Type.Mine)
            {
                cell.flagged = true;
                state[x, y] = cell;
            }
        }
    }
}
private void RestartGame()
{
    GameOver = false;
    Restart.gameObject.SetActive(false); // ���ذ�ť

    NewGame();
}*//*
}


*/