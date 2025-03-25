/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    private bool isInitialized = false;
    public GameObject circle; // 拖拽 Circle 的 GameObject 到这里
    public Button Restart;
    private bool GameOver;
    private bool isCircleActive = false; // Circle 是否已激活isCir
    float touchTime = 0f; // 触摸持续时间
    bool isTouching = false;
    public Vector2 TouchPosition;//按压位置
    public int width = 8;
    public int height = 16;
    public int mineCount = 20;
    private Board board;
    private Cell[,] state;
    private Vector2 initialTouchPosition; // 初始触摸位置
    private Vector3Int initialCellPosition; // 初始单元格位置
    private enum SwipeDirection { None, Up, Down } // 滑动方向枚举
    private SwipeDirection swipeDirection = SwipeDirection.None; // 当前滑动方向
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
        isInitialized = false; //初始化状态
        GameOver = false;
        Restart.gameObject.SetActive(false);
        state = new Cell[width, height];
        GenerateCells();//只生产空白单元格，玩家第一次按下后生成地图
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
        // 步骤1: 创建安全区域
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

        // 步骤2: 生成候选位置
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

        // 步骤3: 随机布雷
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

        // 步骤4: 计算数字
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
            if (Input.touchCount > 0) // 检查是否有触摸点
            {
                Touch touch = Input.GetTouch(0); // 获取第一个触摸点

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        isTouching = true;
                        touchTime = Time.time; // 记录触摸开始时间
                        TouchPosition = touch.position;
                        swipeDirection = SwipeDirection.None; // 重置滑动方向

                        // 检测触摸位置对应的单元格是否已揭开
                        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
                        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
                        Cell cell = GetCell(cellPosition.x, cellPosition.y);

                        if (cell.type != Cell.Type.Invalid && !cell.revealed) // 如果单元格未揭开
                        {
                            // 记录初始触摸位置和单元格
                            initialTouchPosition = TouchPosition;
                            initialCellPosition = cellPosition;
                            isCircleActive = true; // 允许 Circle 出现
                            Debug.Log("触摸到未揭开的单元格，允许 Circle 出现");
                        }
                        else // 如果单元格已揭开或无效
                        {
                            // 禁止 Circle 出现
                            isCircleActive = false;
                            Debug.Log("触摸到已揭开的单元格，禁止 Circle 出现");
                        }
                        break;

                    case TouchPhase.Stationary:
                        if (isTouching && isCircleActive && Time.time - touchTime >= 0.25f) // 触摸时间大于等于 0.25 秒
                        {
                            // 设置 Circle 的位置（基于初始触摸位置）
                            SetCirclePosition(initialTouchPosition);
                            // 激活 Circle
                            circle.SetActive(true);
                            Debug.Log("触摸时间大于等于 0.25 秒，Circle 已激活");
                        }
                        break;

                    case TouchPhase.Moved:
                        if (isCircleActive && circle.activeSelf) // 如果 Circle 已激活
                        {
                            // 检测滑动方向
                            DetectSwipe(touch.position);
                        }
                        break;

                    case TouchPhase.Ended:
                        circle.SetActive(false);
                        if (isTouching)
                        {
                            if (Time.time - touchTime < 0.25f) // 短按操作
                            {
                                Reveal(); // 点击操作
                                Debug.Log("揭开操作");
                            }
                            else if (swipeDirection != SwipeDirection.None) // 滑动操作
                            {
                                // 根据滑动方向切换单元格状态
                                HandleSwipeAction();
                            }
                        }
                        isTouching = false; // 重置状态
                        break;

                    case TouchPhase.Canceled:
                        isTouching = false;
                        circle.SetActive(false);
                        Debug.Log("触摸取消");
                        break;
                }
            }
        }
    }*/

    /*    private void SetCirclePosition(Vector2 screenPosition)
    {
        if (circle != null)
        {
            // 将屏幕坐标转换为世界坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                circle.GetComponent<RectTransform>().parent as RectTransform,
                screenPosition,
                null,
                out Vector2 localPosition
            );

            // 设置 Circle 的位置
            circle.GetComponent<RectTransform>().localPosition = localPosition;
        }
    }*/
/*private void DetectSwipe(Vector2 currentTouchPosition)
    {
        // 计算滑动距离
        float swipeDistance = currentTouchPosition.y - initialTouchPosition.y;

        // 滑动距离阈值（例如 50 像素）
        float swipeThreshold = 50f;

        if (Mathf.Abs(swipeDistance) > swipeThreshold)
        {
            if (swipeDistance > 0) // 向上滑动
            {
                swipeDirection = SwipeDirection.Up;
                Debug.Log("向上滑动");
            }
            else // 向下滑动
            {
                swipeDirection = SwipeDirection.Down;
                Debug.Log("向下滑动");
            }
        }
    }*/
/*    private void HandleSwipeAction()
    {
        if (swipeDirection == SwipeDirection.Up) // 上滑插旗
        {
            Debug.Log("执行 Flags 方法");
            Flags(initialCellPosition); // 作用于初始单元格
        }
        else if (swipeDirection == SwipeDirection.Down) // 下滑问号
        {
            Debug.Log("执行 Question 方法");
            Question(initialCellPosition); // 作用于初始单元格
        }
    }*/
/*private void Question(Vector3Int cellPosition)
{
    Debug.Log("进入 Question 方法");

    // 获取初始单元格
    Cell cell = GetCell(cellPosition.x, cellPosition.y);

    // 如果单元格无效或已揭开，直接返回
    if (cell.type == Cell.Type.Invalid || cell.revealed)
    {
        Debug.Log("单元格无效或已揭开，直接返回");
        return;
    }

    // 切换问号标记状态
    cell.flagged = false; // 清除插旗状态
    cell.questioned = !cell.questioned; // 切换问号状态
                                        // 更新单元格的 Tile
    if (cell.questioned)
    {
        Debug.Log("设置单元格为问号 Tile");
        cell.tile = board.tileQuestion; // 更新 tile 属性
        board.tilemap.SetTile(cellPosition, board.tileQuestion); // 设置 Tilemap 中的 Tile
    }
    else
    {
        Debug.Log("恢复单元格为未知 Tile");
        cell.tile = board.tileUnknown; // 更新 tile 属性
        board.tilemap.SetTile(cellPosition, board.tileUnknown); // 设置 Tilemap 中的 Tile
    }
    state[cellPosition.x, cellPosition.y] = cell;// 更新状态数组

    // 更新单元格贴图
    if (cell.questioned)
    {
        Debug.Log("设置单元格为问号贴图");
        board.tilemap.SetTile(cellPosition, board.tileQuestion); // 设置为问号贴图
    }
    else
    {
        Debug.Log("恢复单元格为未知贴图");
        board.tilemap.SetTile(cellPosition, board.tileUnknown); // 恢复为未知贴图
    }

    // 如果标记成功，触发震动
    if (cell.questioned)
    {
        Handheld.Vibrate();
    }

    // 强制刷新 Tilemap
    board.tilemap.RefreshAllTiles();

    Debug.Log("Question 方法作用于单元格: (" + cellPosition.x + ", " + cellPosition.y + ")");
}*/
/*  private void Reveal()
  {
      Vector2 worldPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
      Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
      Cell cell = GetCell(cellPosition.x, cellPosition.y);

      if (!isInitialized)
      {
          // 首次点击时初始化地图
          InitializeWithFirstClick(new Vector2Int(cellPosition.x, cellPosition.y));
      }

      if (cell.type == Cell.Type.Invalid || cell.flagged) // 如果单元格无效、插旗或标记为问号
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
          case Cell.Type.Number: // 新增快速揭开逻辑
              Debug.Log("按下数字单元格");
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

      // 统计周围标记的地雷数量和需要揭示的单元格
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
                      cellsToBlink.Add(new Vector2Int(checkX, checkY)); // 添加到闪烁列表
                  }
              }
          }
      }

      // 快速揭示条件判断
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
          // 快速揭示条件不满足，触发闪烁
          Debug.Log("快速揭示条件不满足，触发闪烁");
          StartCoroutine(BlinkCells(cellsToBlink));
      }
  }

  private IEnumerator BlinkCells(List<Vector2Int> cellsToBlink)
  {
      Debug.Log("开始闪烁");
      int blinkCount = 2;
      float blinkDuration = 0.05f;

      // 保存闪烁前的 Tile
      Dictionary<Vector2Int, Tile> previousTiles = new Dictionary<Vector2Int, Tile>();
      foreach (var pos in cellsToBlink)
      {
          if (state[pos.x, pos.y].tile == null)
          {
              Debug.LogError($"Tile at position ({pos.x}, {pos.y}) is null!");
          }
          else
          {
              previousTiles[pos] = state[pos.x, pos.y].tile; // 保存原始 Tile
              Debug.Log("闪烁前的 Tile: " + previousTiles[pos]);
          }
      }

      // 闪烁逻辑
      for (int i = 0; i < blinkCount; i++)
      {
          Debug.Log("设置为红色 Tile");
          foreach (var pos in cellsToBlink)
          {
              Debug.Log("变成红色！！！");
              state[pos.x, pos.y].tile = board.tileRed; // 替换为红色 Tile
              board.tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), board.tileRed); // 强制设置 Tile
          }
          board.tilemap.RefreshAllTiles(); // 强制刷新 Tilemap
          yield return new WaitForSeconds(blinkDuration);
          Debug.Log("恢复成闪烁前的 Tile");
          foreach (var pos in cellsToBlink)
          {
              Debug.Log("闪烁后的 Tile: " + state[pos.x, pos.y].tile);
              state[pos.x, pos.y].tile = previousTiles[pos]; // 恢复成闪烁前的 Tile
              board.tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), previousTiles[pos]); // 强制设置 Tile
              board.tilemap.RefreshTile(new Vector3Int(pos.x, pos.y, 0)); // 强制刷新单个 Tile
          }
          board.Draw(state); // 更新 Tilemap 渲染
          yield return new WaitForSeconds(blinkDuration);
      }
      Debug.Log("闪烁结束");
  }
  private void Explode(Cell cell)
  {
      Debug.Log("你输了!");
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
          // 八方向递归揭开
          for (int dx = -1; dx <= 1; dx++)
          {
              for (int dy = -1; dy <= 1; dy++)
              {
                  if (dx == 0 && dy == 0) continue; // 跳过自身

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
      // 获取初始单元格
      Cell cell = GetCell(cellPosition.x, cellPosition.y);

      // 如果单元格无效或已揭开，直接返回
      if (cell.type == Cell.Type.Invalid || cell.revealed)
      {
          return;
      }

      // 切换标记状态
      cell.flagged = !cell.flagged;
      state[cellPosition.x, cellPosition.y] = cell;

      // 如果标记成功，触发震动
      if (cell.flagged)
      {
          Handheld.Vibrate();
      }

      // 更新棋盘渲染
      board.Draw(state);

      Debug.Log("Flags 方法作用于单元格: (" + cellPosition.x + ", " + cellPosition.y + ")");
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

    Debug.Log("你赢了！");
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
    Restart.gameObject.SetActive(false); // 隐藏按钮

    NewGame();
}*//*
}


*/