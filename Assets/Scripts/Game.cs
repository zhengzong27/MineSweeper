using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    private bool isInitialized = false;
    public GameObject circle; // 拖拽 Circle 的 GameObject 到这里
    public CameraController cameraController;
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
    private Dictionary<Vector3Int, Cell> state;
    private Vector2 initialTouchPosition; // 初始触摸位置
    private Vector3Int initialCellPosition; // 初始单元格位置
    private enum SwipeDirection { None, Up, Down } // 滑动方向枚举
    private SwipeDirection swipeDirection = SwipeDirection.None; // 当前滑动方向

    [Header("Dynamic Map Settings")]
    public int viewportWidth = 8; // 摄像头可见宽度
    public int viewportHeight = 16; // 摄像头可见高度
    public int bufferSize = 3; // 缓冲区大小
    private Vector2Int lastCameraCellPosition; // 上次摄像头所在的单元格位置

    private void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width + height);
    }
    private void Awake()
    {
        board = GetComponentInChildren<Board>();
        Restart.onClick.AddListener(RestartGame);
        lastCameraCellPosition = new Vector2Int(int.MinValue, int.MinValue);
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
        state = new Dictionary<Vector3Int, Cell>();
        GenerateCells();//只生产空白单元格，玩家第一次按下后生成地图
        Camera.main.transform.position = new Vector3(0, 0, -10f);
        board.Draw(state);
    }
    private void GenerateCells()
    {
        state = new Dictionary<Vector3Int, Cell>();
    }
    private void InitializeWithFirstClick(Vector2Int firstClick)//雷是在地图生成后产生
    {
        // 步骤1: 创建安全区域
        HashSet<Vector2Int> forbiddenArea = new HashSet<Vector2Int>();
        for (int dx = -1; dx <= 1; dx++)//遍历存入安全数组
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int x = Mathf.Clamp(firstClick.x + dx, 0, width - 1);
                int y = Mathf.Clamp(firstClick.y + dy, 0, height - 1);
                forbiddenArea.Add(new Vector2Int(x, y));
            }
        }
        // 生成实际游戏单元格（仅在被点击的区域周围）
        for (int x = firstClick.x - 5; x <= firstClick.x + 5; x++)
        {
            for (int y = firstClick.y - 5; y <= firstClick.y + 5; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                if (!state.ContainsKey(position))
                {
                    state[position] = new Cell(position, Cell.Type.Empty, null);
                }
            }
        }
        // 步骤2: 生成候选位置
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = 0; x < width; x++)//遍历存入可布雷数组
        {
            for (int y = 0; y < height; y++)
            {
                if (!forbiddenArea.Contains(new Vector2Int(x, y)))
                {
                    candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        // 步骤3: 随机布雷，Fisher-Yates洗牌算法
        int mineCount = Mathf.Min(this.mineCount, candidates.Count);//限制地雷数小于Mine count
        System.Random rng = new System.Random();
        for (int i = 0; i < mineCount; i++)
        {
            int index = rng.Next(i, candidates.Count);//从剩余未处理的候选位置中随机选一个布雷
            //交换 candidates[i] 和 candidates[index]并布雷于[i]
            Vector2Int temp = candidates[i];
            candidates[i] = candidates[index];
            candidates[index] = temp;

            Vector2Int pos = candidates[i];
            Vector3Int position = new Vector3Int(pos.x, pos.y, 0);
            if (state.TryGetValue(position, out Cell cell))
            {
                cell.type = Cell.Type.Mine;
                state[position] = cell;
            }
        }

        // 步骤4: 计算数字
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                Cell cell = state[position];
                if (cell.type != Cell.Type.Mine)
                {
                    int count = CountMines(x, y);
                    cell.Number = count;
                    cell.type = count > 0 ? Cell.Type.Number : Cell.Type.Empty;
                    state[position] = cell;
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
                 Vector3Int position = new Vector3Int(x, y, 0);
                if (state.TryGetValue(position, out Cell cell) && cell.type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }
        return count;
    }
    void Update()
    {
        if (!GameOver)
        {
            UpdateDynamicMap();
            //Touch();
        }
    }
    private void UpdateDynamicMap()
    {
        // 获取摄像头中心位置对应的单元格坐标
        Vector3 cameraCenter = Camera.main.transform.position;
        Vector3Int cameraCellPosition = board.tilemap.WorldToCell(cameraCenter);

        // 如果摄像头位置没有显著变化，则不更新地图
        if (Mathf.Abs(cameraCellPosition.x - lastCameraCellPosition.x) < viewportWidth / 4 &&
            Mathf.Abs(cameraCellPosition.y - lastCameraCellPosition.y) < viewportHeight / 4)
        {
            return;
        }

        lastCameraCellPosition = new Vector2Int(cameraCellPosition.x, cameraCellPosition.y);

        // 计算需要生成的地图范围
        int startX = cameraCellPosition.x - viewportWidth / 2 - bufferSize;
        int endX = cameraCellPosition.x + viewportWidth / 2 + bufferSize;
        int startY = cameraCellPosition.y - viewportHeight / 2 - bufferSize;
        int endY = cameraCellPosition.y + viewportHeight / 2 + bufferSize;

        // 清除旧的地图（仅视觉效果）
        board.tilemap.ClearAllTiles();

        // 生成新的空白地图（仅视觉效果）
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                board.tilemap.SetTile(position, board.tileUnknown);
            }
        }

        // 绘制实际游戏状态（已揭示的单元格等）
        board.Draw(state);
    }
    private void Touch()
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
                    else
                    {
                        cameraController.HandleTouchInput();
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
    private void SetCirclePosition(Vector2 screenPosition)
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
    }
    private void DetectSwipe(Vector2 currentTouchPosition)
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
    }
    private void HandleSwipeAction()
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
    }
    private void Question(Vector3Int cellPosition)
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
        state[cell.position] = cell;

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
    }
    private void Reveal()
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
                    state[cell.position] = cell;
                    ifWin();
                }
                break;
        }
        
        board.Draw(state);
    }
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
                        Cell c = GetCell(pos.x, pos.y);
                        c.revealed = true;
                        state[c.position] = c;
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
            //StartCoroutine(BlinkCells(cellsToBlink));
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
            Vector3Int cellPos = new Vector3Int(pos.x, pos.y, 0);
            if (!state.TryGetValue(cellPos, out Cell cell) || cell.tile == null)
            {
                Debug.LogError($"Tile at position ({pos.x}, {pos.y}) is null or cell not found!");
                continue;
            }

            previousTiles[pos] = cell.tile; // 保存原始 Tile
            Debug.Log("闪烁前的 Tile: " + previousTiles[pos]);
        }

        // 闪烁逻辑
        for (int i = 0; i < blinkCount; i++)
        {
            Debug.Log("设置为红色 Tile");
            foreach (var pos in cellsToBlink)
            {
                Debug.Log("变成红色！！！");
                board.tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), board.tileRed); // 替换为红色 Tile
            }
            board.tilemap.RefreshAllTiles(); // 强制刷新 Tilemap
            yield return new WaitForSeconds(blinkDuration);
            Debug.Log("恢复成闪烁前的 Tile");
            foreach (var pos in cellsToBlink)
            {
                if (!previousTiles.TryGetValue(pos, out Tile originalTile))
                    continue;

                Vector3Int cellPos = new Vector3Int(pos.x, pos.y, 0);
                if (state.TryGetValue(cellPos, out Cell cell))
                {
                    cell.tile = originalTile;
                    state[cellPos] = cell;
                    board.tilemap.SetTile(cellPos, originalTile);
                    board.tilemap.RefreshTile(cellPos);
                }
            }
            board.Draw(state);
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
        state[cell.position] = cell;
        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (state.TryGetValue(pos, out Cell c) && c.type == Cell.Type.Mine)
                {
                    c.revealed = true;
                    state[pos] = c; // ✅ 正确更新字典
                }
            }
        }
    }
    private void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid || cell.flagged) return;

        Vector3Int cellPos = new Vector3Int(cell.position.x, cell.position.y, 0);
        cell.revealed = true;
        state[cellPos] = cell;  // 使用 Vector3Int 作为键

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
    private void Flags(Vector3Int cellPosition)
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
        state[cellPosition] = cell; 

        // 如果标记成功，触发震动
        if (cell.flagged)
        {
            Handheld.Vibrate();
        }

        // 更新棋盘渲染
        board.Draw(state);

        Debug.Log("Flags 方法作用于单元格: (" + cellPosition.x + ", " + cellPosition.y + ")");
    }
    private Cell GetCell(int x,int y)
    {
        Vector3Int position = new Vector3Int(x, y, 0);
        if (!state.ContainsKey(position))
        {
            // 如果单元格不在字典中，创建一个新的无效单元格
            return new Cell(position, Cell.Type.Invalid, null);
        }
        return state[position];
    }
    private bool IsValid(int x,int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private void ifWin()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y=0;y<height;y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (!state.TryGetValue(pos, out Cell cell)) continue;
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
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (state.TryGetValue(pos, out Cell cell) && cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    state[pos] = cell;
                }
            }
        }
    }
    private void RestartGame()
    {
        GameOver = false;
        Restart.gameObject.SetActive(false); // 隐藏按钮

        NewGame();
    }
}


