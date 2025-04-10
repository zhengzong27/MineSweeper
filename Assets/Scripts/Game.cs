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

    [Header("Dynamic Map Optimization")]
    public bool enableDynamicUnloading = true; // 是否启用动态卸载
    private HashSet<Vector3Int> activeCells = new HashSet<Vector3Int>(); // 当前活跃单元格
    private HashSet<Vector3Int> lastActiveCells = new HashSet<Vector3Int>(); // 上一次活跃单元格

    [Header("Block Settings")]
    public int blockSize = 8; // 每个区块的大小（8x8格）
    public int blockBuffer = 2; // 视野外预加载的区块数量
    public float mineDensity = 0.15f; // 每个区块的地雷密度
    private Dictionary<Vector2Int, bool> initializedBlocks = new Dictionary<Vector2Int, bool>();
    private Dictionary<Vector2Int, HashSet<Vector2Int>> blockMinePositions = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private HashSet<Vector2Int> safeZone = new HashSet<Vector2Int>(); // 首次点击的安全区域

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
        isInitialized = false;
        GameOver = false;
        Restart.gameObject.SetActive(false);
        state = new Dictionary<Vector3Int, Cell>();
        initializedBlocks.Clear();
        blockMinePositions.Clear();
        safeZone.Clear();
        Camera.main.transform.position = new Vector3(0, 0, -10f);
        lastCameraCellPosition = new Vector2Int(int.MinValue, int.MinValue);
    }
    private void GenerateCells()
    {
        state = new Dictionary<Vector3Int, Cell>();
    }
    private void InitializeWithFirstClick(Vector2Int firstClick)
    {
        // 设置安全区域（3x3）
        safeZone.Clear();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                safeZone.Add(new Vector2Int(firstClick.x + dx, firstClick.y + dy));
            }
        }

        // 初始化点击区块及相邻区块
        Vector2Int blockCoord = new Vector2Int(
            Mathf.FloorToInt(firstClick.x / (float)blockSize),
            Mathf.FloorToInt(firstClick.y / (float)blockSize));

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                InitializeBlock(new Vector2Int(blockCoord.x + dx, blockCoord.y + dy));
            }
        }

        isInitialized = true;
    }

    private void InitializeBlock(Vector2Int blockCoord)
    {
        if (initializedBlocks.ContainsKey(blockCoord)) return;

        int startX = blockCoord.x * blockSize;
        int startY = blockCoord.y * blockSize;
        int endX = startX + blockSize - 1;
        int endY = startY + blockSize - 1;

        // 计算本区块地雷数量（基于密度）
        int blockMineCount = Mathf.RoundToInt(blockSize * blockSize * mineDensity);
        blockMineCount = Mathf.Max(1, blockMineCount);

        // 生成候选位置（避开区块边缘1格）
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = startX + 1; x <= endX - 1; x++)
        {
            for (int y = startY + 1; y <= endY - 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!safeZone.Contains(pos))
                {
                    candidates.Add(pos);
                }
            }
        }

        // 随机布雷
        System.Random rng = new System.Random();
        HashSet<Vector2Int> minesInBlock = new HashSet<Vector2Int>();

        for (int i = 0; i < Mathf.Min(blockMineCount, candidates.Count); i++)
        {
            int index = rng.Next(i, candidates.Count);
            Vector2Int temp = candidates[i];
            candidates[i] = candidates[index];
            candidates[index] = temp;

            Vector2Int minePos = candidates[i];
            minesInBlock.Add(minePos);

            // 初始化地雷单元格
            Vector3Int position = new Vector3Int(minePos.x, minePos.y, 0);
            state[position] = new Cell(position, Cell.Type.Mine, board.tileMine);
        }

        // 记录本区块地雷位置
        blockMinePositions[blockCoord] = minesInBlock;
        initializedBlocks[blockCoord] = true;

        // 计算本区块数字
        CalculateNumbersInBlock(blockCoord);

        // 更新相邻区块边缘数字
        UpdateAdjacentBlocksNumbers(blockCoord);
    }
    private bool IsForbiddenPosition(Vector2Int pos)
    {
        // 检查是否在任何区块的安全区域内
        Vector2Int blockPos = new Vector2Int(
            Mathf.FloorToInt(pos.x / (float)blockSize),
            Mathf.FloorToInt(pos.y / (float)blockSize));

        if (blockMinePositions.TryGetValue(blockPos, out HashSet<Vector2Int> forbiddenPositions))
        {
            return forbiddenPositions.Contains(pos);
        }
        return false;
    }

    private void CalculateNumbersInBlock(Vector2Int blockCoord)
    {
        int startX = blockCoord.x * blockSize;
        int startY = blockCoord.y * blockSize;
        int endX = startX + blockSize - 1;
        int endY = startY + blockSize - 1;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);

                // 只处理非地雷单元格
                if (!state.ContainsKey(position) || state[position].type != Cell.Type.Mine)
                {
                    int count = CountAdjacentMines(x, y);
                    Cell cell = new Cell(position,
                                       count > 0 ? Cell.Type.Number : Cell.Type.Empty,
                                       count > 0 ? board.tileNumbers[count] : board.tileEmpty);
                    cell.Number = count;
                    state[position] = cell;
                }
            }
        }
    }
    private int CountAdjacentMines(int x, int y)
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int checkX = x + dx;
                int checkY = y + dy;
                Vector3Int pos = new Vector3Int(checkX, checkY, 0);

                if (state.TryGetValue(pos, out Cell cell) && cell.type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }

        return count;
    }
    private void UpdateAdjacentBlocksNumbers(Vector2Int blockCoord)
    {
        // 更新相邻区块边缘数字
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector2Int adjacentBlock = new Vector2Int(blockCoord.x + dx, blockCoord.y + dy);
                if (initializedBlocks.ContainsKey(adjacentBlock))
                {
                    CalculateNumbersInBlock(adjacentBlock);
                }
            }
        }
    }
    private int CountMines(int cellX, int cellY)
    {
        int count = 0;
        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0) continue;

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;
                if (!IsValid(x, y)) continue; // 动态区块检查

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
        if (GameOver)
        {
            return;
        }
        if (!GameOver)
        {
            UpdateDynamicMap();
            cameraController.HandleTouchInput();
            Touch();      
        }
    }
    private void UpdateDynamicMap()
    {
        if(GameOver)
        {
            return;
        }
        // 获取摄像头中心位置对应的单元格坐标
        Vector3 cameraCenter = Camera.main.transform.position;
        Vector3Int cameraCellPosition = board.tilemap.WorldToCell(cameraCenter);
        Vector3Int cameraCellPos = board.tilemap.WorldToCell(cameraCenter);
        Vector2Int currentBlock = new Vector2Int(
        Mathf.FloorToInt(cameraCellPos.x / (float)blockSize),
        Mathf.FloorToInt(cameraCellPos.y / (float)blockSize));
        // 初始化视野内及缓冲区的区块
        for (int dx = -blockBuffer; dx <= blockBuffer; dx++)
        {
            for (int dy = -blockBuffer; dy <= blockBuffer; dy++)
            {
                InitializeBlock(new Vector2Int(currentBlock.x + dx, currentBlock.y + dy));
            }
        }
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
        // 记录当前活跃单元格
        HashSet<Vector3Int> currentActiveCells = new HashSet<Vector3Int>();
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                currentActiveCells.Add(position);

                // 动态生成新单元格到字典（如果不存在）
                if (!state.ContainsKey(position))
                {
                    state[position] = new Cell(position, Cell.Type.Empty, null);
                }
            }
        }

        // 清理视野外的贴图（仅在启用动态卸载时）
        if (enableDynamicUnloading)
        {
            // 计算需要清理的单元格：上一次活跃但当前不活跃的
            HashSet<Vector3Int> cellsToUnload = new HashSet<Vector3Int>(lastActiveCells);
            cellsToUnload.ExceptWith(currentActiveCells);

            // 清除这些单元格的贴图
            foreach (Vector3Int position in cellsToUnload)
            {
                // 仅清除贴图，不修改state字典
                board.tilemap.SetTile(position, null);
            }
        }
        // 绘制当前活跃单元格
        foreach (Vector3Int position in currentActiveCells)
        {
            if (!state.TryGetValue(position, out Cell cell))
            {
                // 如果单元格不存在，创建默认空单元格
                cell = new Cell(position, Cell.Type.Empty, null);
                state[position] = cell;
            }
            else { 
                // 根据单元格状态设置贴图
                if (cell.revealed)
                {
                    // 已揭开的单元格
                    if (cell.type == Cell.Type.Mine)
                    {
                        board.tilemap.SetTile(position, board.tileMine);
                    }
                    else if (cell.type == Cell.Type.Number)
                    {
                        board.tilemap.SetTile(position, board.tileNumbers[cell.Number]);
                    }
                    else
                    {
                        board.tilemap.SetTile(position, board.tileEmpty);
                    }
                }
                else
                {
                    // 未揭开的单元格
                    if (cell.flagged)
                    {
                        board.tilemap.SetTile(position, board.tileFlag);
                    }
                    else if (cell.questioned)
                    {
                        board.tilemap.SetTile(position, board.tileQuestion);
                    }
                    else
                    {
                        board.tilemap.SetTile(position, board.tileUnknown);
                    }
                }
            }
        }
        lastActiveCells = currentActiveCells;
    }
    private void Touch()
    {
        if(GameOver)
        {
            return;
        }
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
                    }
                    else // 如果单元格已揭开或无效
                    {
                        // 禁止 Circle 出现
                        isCircleActive = false;
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
        cell.flagged = false;
        cell.questioned = !cell.questioned;
        state[cellPosition] = cell;
        board.DrawCell(cellPosition, cell); // 局部更新这个单元格

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
            isInitialized = true;
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
                    board.DrawCell(cell.position, cell);
                    ifWin();
                }
                break;
        }
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
        }
        else
        {
            // 快速揭示条件不满足，触发闪烁
            Debug.Log("快速揭示条件不满足，触发闪烁");
            //StartCoroutine(BlinkCells(cellsToBlink));
        }
    }

    /* private IEnumerator BlinkCells(List<Vector2Int> cellsToBlink)
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
     }*/
    private void Explode(Cell cell)
    {
        Debug.Log("你输了!");
        Restart.gameObject.SetActive(true);
        GameOver = true; // 游戏结束标志
        cell.revealed = true;
        cell.exploded = true;
        state[cell.position] = cell;
        board.DrawCell(cell.position, cell); // 更新爆炸的地雷
                                             // 遍历所有区块中的地雷（不再依赖width/height）
        foreach (var block in blockMinePositions.Values)
        {
            foreach (Vector2Int minePos in block)
            {
                Vector3Int pos = new Vector3Int(minePos.x, minePos.y, 0);
                if (state.TryGetValue(pos, out Cell c))
                {
                    c.revealed = true;
                    state[pos] = c;
                    board.DrawCell(pos, c);
                }
            }
        }
    }

    private void Flood(Cell cell)
    {
        if (cell.revealed || cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid || cell.flagged)
            return;

        cell.revealed = true;
        state[cell.position] = cell;
        board.DrawCell(cell.position, cell);

        if (cell.type == Cell.Type.Empty)
        {
            // 八方向递归（不再需要全局边界检查）
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int x = cell.position.x + dx;
                    int y = cell.position.y + dy;

                    { // 自动调用新的区块检查
                                       if (IsValid(x, y))
                        {
                            Cell neighbor = GetCell(x, y);
                            Flood(neighbor);
                        }
                    }
                }
            }
        } 
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
        cell.flagged = !cell.flagged;
        state[cellPosition] = cell;
        state[cellPosition] = cell;
        board.DrawCell(cellPosition, cell); // 局部更新这个单元格
        // 如果标记成功，触发震动
        if (cell.flagged)
        {
            Handheld.Vibrate();
        }
        board.tilemap.RefreshAllTiles();
        // 更新棋盘渲染
        Debug.Log("Flags 方法作用于单元格: (" + cellPosition.x + ", " + cellPosition.y + ")");
    }
    private Cell GetCell(int x,int y)
    {
        Vector3Int position = new Vector3Int(x, y, 0);
        if (!state.ContainsKey(position))
        {
            // 如果单元格不存在，创建并返回一个默认的空单元格
            state[position] = new Cell(position, Cell.Type.Empty, null);
        }
        return state[position];
    }
    private bool IsValid(int x, int y)
    {
        // 动态检查：坐标是否在已初始化的区块内
        Vector2Int blockCoord = new Vector2Int(
            Mathf.FloorToInt(x / (float)blockSize),
            Mathf.FloorToInt(y / (float)blockSize)
        );
        return initializedBlocks.ContainsKey(blockCoord);
    }

    private void ifWin()
    {
        foreach (var block in initializedBlocks)
        {
            Vector2Int blockCoord = block.Key;
            int startX = blockCoord.x * blockSize;
            int startY = blockCoord.y * blockSize;
            int endX = startX + blockSize - 1;
            int endY = startY + blockSize - 1;

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (state.TryGetValue(pos, out Cell cell))
                    {
                        if (cell.type != Cell.Type.Mine && !cell.revealed)
                        {
                            return; // 还有未揭开的非地雷单元格
                        }
                    }
                }
            }
        }

        // 所有区块检查完毕都通过
        Debug.Log("你赢了！");
        Restart.gameObject.SetActive(true);
        GameOver = true;

        // 标记所有地雷
        foreach (var block in blockMinePositions)
        {
            foreach (var minePos in block.Value)
            {
                Vector3Int pos = new Vector3Int(minePos.x, minePos.y, 0);
                if (state.TryGetValue(pos, out Cell cell))
                {
                    cell.flagged = true;
                    state[pos] = cell;
                    board.DrawCell(pos, cell);
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


