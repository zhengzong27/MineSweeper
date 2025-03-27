using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    public bool isInitialized = false;
    public GameObject circle; // 拖拽 Circle 的 GameObject 到这里
    public Button Restart;
    public bool GameOver;
    public bool isCircleActive = false; // Circle 是否已激活isCir
    float touchTime = 0f; // 触摸持续时间
    bool isTouching = false;
    public Vector2 TouchPosition;//按压位置
    public int width = 8;
    public int height = 16;
    public int mineCount = 20;
    public Board board;
    public MapGenerate generate;
    public SweepLogic sweepLogic;
    public Cell[,] state;
    private Vector2 initialTouchPosition; // 初始触摸位置
    private Vector3Int initialCellPosition; // 初始单元格位置
    private enum SwipeDirection { None, Up, Down } // 滑动方向枚举
    private SwipeDirection swipeDirection = SwipeDirection.None; // 当前滑动方向

    public void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width + height);
    }
    public void Awake()
    {
        board = GetComponentInChildren<Board>();
        Restart.onClick.AddListener(RestartGame);
        sweepLogic = GetComponent<SweepLogic>(); // 获取SweepLogic组件
    }
    public void Start()
    {
        NewGame();
    }
    public void NewGame()//初始化游戏，调用GenerateCells生成空白单元格
    {
        circle.SetActive(false);
        isInitialized = false; //初始化状态
        GameOver = false;
        Restart.gameObject.SetActive(false);
        state = new Cell[width, height];
        generate.GenerateCells();//只生产空白单元格，玩家第一次按下后生成地图
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
                            sweepLogic.Reveal(); // 点击操作
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
    }//点击反馈
    public void DetectSwipe(Vector2 currentTouchPosition)
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
    }//检测玩家上下滑动
    public void HandleSwipeAction()
    {
        if (swipeDirection == SwipeDirection.Up) // 上滑插旗
        {
            Debug.Log("执行 Flags 方法");
            sweepLogic.Flags(initialCellPosition); // 作用于初始单元格
        }
        else if (swipeDirection == SwipeDirection.Down) // 下滑问号
        {
            Debug.Log("执行 Question 方法");
            sweepLogic.Question(initialCellPosition); // 作用于初始单元格
        }
    }//依据滑动方向进行反馈
    public void SetCirclePosition(Vector2 screenPosition)
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
    }//设置Circle位置
    public void CheckQuickReveal(int x, int y)
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
            // 快速揭示条件不满足，触发闪烁
            Debug.Log("快速揭示条件不满足，触发闪烁");
            StartCoroutine(BlinkCells(cellsToBlink));
        }
    }//判断能否快速揭开

    public IEnumerator BlinkCells(List<Vector2Int> cellsToBlink)
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
    }//闪烁

    public Cell GetCell(int x,int y)//获取单元格坐标
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
    }//判断是否为边界

    public void RestartGame()
    {
        GameOver = false;
        Restart.gameObject.SetActive(false); // 隐藏按钮

        NewGame();
    }//重新开始
}


