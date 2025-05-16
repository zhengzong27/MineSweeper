using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    private bool isInitialized = false;
    public CameraController cameraController;
    public Button Restart;
    public bool GameOver { get; private set; }
    float touchTime = 0f; // 触摸持续时间
    bool isTouching = false;
    public Vector2 TouchPosition;//按压位置
    private Board board;
    private Dictionary<Vector3Int, Cell> state;
    private Vector2 initialTouchPosition; // 初始触摸位置
    private Vector3Int initialCellPosition; // 初始单元格位置
    private ItemMenuController itemMenuController; // 添加ItemMenuController引用

    // 添加震动持续时间常量
    private const long VIBRATION_DURATION = 100; // 震动持续时间（毫秒）

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
    public float mineDensity = 0.2f; // 每个区块的地雷密度
    private Dictionary<Vector2Int, bool> initializedBlocks = new Dictionary<Vector2Int, bool>();
    private Dictionary<Vector2Int, HashSet<Vector2Int>> blockMinePositions = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private HashSet<Vector2Int> safeZone = new HashSet<Vector2Int>(); // 首次点击的安全区域
    private int score = 0; // 当前游戏积分

    [Header("UI Elements")]
    public TMP_Text scoreText; // 积分显示文本
    public TMP_Text highScoreText; // 最高分显示文本
    private int highScore = 0; // 最高分记录
    public Image repositionButton; // 视角回调按钮
    private Vector3 lastOperationPosition; // 记录最后一次操作位置
    [SerializeField] private Image itemButton; // Item按钮
    [SerializeField] private Menu menuManager; // Menu管理器

    [Header("Audio")]
    public AudioSource audioSource; // 音频源组件
    public AudioClip getScoreSound; // GetScore音频文件
    public AudioClip boomSound; // 爆炸音效
    public AudioClip Unbelievable;//unbelievable音效
    public AudioClip Floodsound; // 大量揭开音效
    public AudioClip TouchUI; // 点击UI音效 
    public AudioClip FlagSound; // 插旗音效 

    [Header("Animation")]
    public GameObject boomAnimation; // 爆炸动画对象
    [SerializeField] private TileBase tileRed; // 红色闪烁贴图
    [SerializeField] private float blinkDuration = 0.1f; // 闪烁持续时间
    [SerializeField] private int blinkCount = 1; // 闪烁次数

    private Dictionary<Vector3Int, TileBase> originalTiles = new Dictionary<Vector3Int, TileBase>(); // 存储原始贴图
    private Coroutine currentBlinkCoroutine; // 当前闪烁协程

    private void Awake()
    {
        board = GetComponentInChildren<Board>();
        Restart.onClick.AddListener(RestartGame);
        repositionButton.GetComponent<Button>().onClick.AddListener(RepositionCamera);
        
        // 设置Item按钮点击事件
        if (itemButton != null && itemButton.GetComponent<Button>() != null)
        {
            itemButton.GetComponent<Button>().onClick.AddListener(ItemOpen);
        }

        lastCameraCellPosition = new Vector2Int(int.MinValue, int.MinValue);
        
        // 加载保存的最高分
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // 获取ItemMenuController实例
        itemMenuController = ItemMenuController.Instance;
        
        UpdateScoreUI();
    }

    private void Start()
    {
        // 重新获取Menu组件引用
        if (menuManager == null)
        {
            menuManager = FindObjectOfType<Menu>();
        }

        // 确保获取ItemMenuController实例
        if (itemMenuController == null)
        {
            itemMenuController = ItemMenuController.Instance;
            Debug.Log("ItemMenuController状态: " + (itemMenuController != null ? "已获取" : "未获取"));
        }
        
        // 初始化VibrationHelper
        #if UNITY_ANDROID
        try
        {
            using (AndroidJavaClass vibrationHelper = new AndroidJavaClass("com.unity3d.player.VibrationHelper"))
            {
                vibrationHelper.CallStatic("Initialize");
                Debug.Log("VibrationHelper初始化成功");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("VibrationHelper初始化失败: " + e.Message);
        }
        #endif

        NewGame();
    }

    private void OnEnable()
    {
        // 确保在启用时获取ItemMenuController
        if (itemMenuController == null)
        {
            itemMenuController = ItemMenuController.Instance;
            Debug.Log("OnEnable时获取ItemMenuController状态: " + (itemMenuController != null ? "成功" : "失败"));
        }
    }

    private void OnDisable()
    {
        // 移除场景加载完成的监听
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 场景加载完成后重新获取Menu组件引用
        if (menuManager == null)
        {
            menuManager = FindObjectOfType<Menu>();
            Debug.Log("重新获取Menu组件引用: " + (menuManager != null ? "成功" : "失败"));
        }
    }

    private void NewGame()
    {
        isInitialized = false;
        GameOver = false;
        Restart.gameObject.SetActive(false);
        state = new Dictionary<Vector3Int, Cell>();
        initializedBlocks.Clear();
        blockMinePositions.Clear();
        safeZone.Clear();
        score = 0; // 重置积分
        UpdateScoreUI();
        Camera.main.transform.position = new Vector3(0, 0, -10f);
        lastCameraCellPosition = new Vector2Int(int.MinValue, int.MinValue);
        
        // 禁用爆炸动画
        if (boomAnimation != null)
        {
            boomAnimation.SetActive(false);
        }
    }

    void Update()
    {
        if (!GameOver && (menuManager == null || !menuManager.IsMenuActive())) // 游戏未结束且菜单未打开时
        {
            cameraController.HandleTouchInput();
            UpdateDynamicMap();
            Touch();
        }
        else if (!GameOver) // 游戏未结束但菜单打开时
        {
            UpdateDynamicMap();
        }
        else
        {
            // 游戏结束时禁用所有操作
            isTouching = false;
            touchTime = 0f;
        }
    }

    private void Touch()
    {
        if (GameOver) return; // 游戏结束时直接返回，不处理任何触摸操作

        // 检查是否点击了UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return; // 如果点击了UI，直接返回，不处理游戏操作
        }

        if (Input.touchCount > 0) // 检查是否有触摸点
        {
            Touch touch = Input.GetTouch(0); // 获取第一个触摸点

            // 如果相机正在移动，不处理游戏操作
            if (cameraController != null && cameraController.IsMoving)
            {
                isTouching = false;
                touchTime = 0f;
                return;
            }

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isTouching = true;
                    touchTime = Time.time; // 记录触摸开始时间
                    TouchPosition = touch.position;

                    // 检测触摸位置对应的单元格
                    Vector2 worldPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
                    Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
                    Cell cell = GetCell(cellPosition.x, cellPosition.y);

                    if (cell.type != Cell.Type.Invalid && !cell.revealed) // 如果单元格未揭开
                    {
                        // 记录初始触摸位置和单元格
                        initialTouchPosition = TouchPosition;
                        initialCellPosition = cellPosition;
                    }
                    break;

                case TouchPhase.Stationary:
                    if (isTouching && Time.time - touchTime >= 0.3f) // 触摸时间大于等于 0.3秒
                    {
                        // 获取当前触摸位置对应的单元格
                        Vector2 currentWorldPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
                        Vector3Int currentCellPosition = board.tilemap.WorldToCell(currentWorldPosition);
                        Cell currentCell = GetCell(currentCellPosition.x, currentCellPosition.y);

                        // 只在未揭开的单元格上执行插旗操作
                        if (currentCell.type != Cell.Type.Invalid && !currentCell.revealed)
                        {
                            // 执行插旗操作
                            Flags(currentCellPosition);
                        }
                        isTouching = false; // 重置触摸状态
                    }
                    break;

                case TouchPhase.Ended:
                    if (isTouching && !cameraController.IsMoving)
                    {
                        if (Time.time - touchTime < 0.25f) // 短按操作
                        {
                            Reveal(); // 点击操作
                        }
                    }
                    isTouching = false; // 重置状态
                    break;

                case TouchPhase.Canceled:
                    isTouching = false;
                    break;
            }
        }
    }

    private void UpdateDynamicMap()
    {
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
            // 检查该位置所在的区块是否已初始化
            Vector2Int blockCoord = new Vector2Int(
                Mathf.FloorToInt(position.x / (float)blockSize),
                Mathf.FloorToInt(position.y / (float)blockSize)
            );
            
            // 如果区块未初始化，跳过该单元格的处理
            if (!initializedBlocks.ContainsKey(blockCoord))
            {
                continue;
            }

            if (!state.TryGetValue(position, out Cell cell))
            {
                // 计算周围地雷数量
                int count = CountAdjacentMines(position.x, position.y);
                cell = new Cell(position,
                               count > 0 ? Cell.Type.Number : Cell.Type.Empty,
                               count > 0 ? board.tileNumbers[count] : board.tileEmpty);
                cell.Number = count;
                state[position] = cell;
            }
            
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
        lastActiveCells = currentActiveCells;
    }

    private void InitializeWithFirstClick(Vector2Int firstClick)
    {
        // 记录首次点击的位置
        Vector3Int firstClickPos = new Vector3Int(firstClick.x, firstClick.y, 0);
        
        // 将首次点击的单元格设置为空白
        state[firstClickPos] = new Cell(firstClickPos, Cell.Type.Empty, board.tileEmpty);
        
        // 获取首次点击所在的区块坐标
        Vector2Int blockCoord = new Vector2Int(
            Mathf.FloorToInt(firstClick.x / (float)blockSize),
            Mathf.FloorToInt(firstClick.y / (float)blockSize)
        );

        // 初始化点击区块及相邻区块
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int currentBlock = new Vector2Int(blockCoord.x + dx, blockCoord.y + dy);
                InitializeBlockWithFirstClick(currentBlock, firstClick);
            }
        }

        // 确保首次点击及其周围8格都不是地雷
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int x = firstClick.x + dx;
                int y = firstClick.y + dy;
                Vector3Int pos = new Vector3Int(x, y, 0);

                // 如果这个位置有地雷，移除它
                if (state.ContainsKey(pos) && state[pos].type == Cell.Type.Mine)
                {
                    // 找到这个地雷所在的区块
                    Vector2Int mineBlock = new Vector2Int(
                        Mathf.FloorToInt(x / (float)blockSize),
                        Mathf.FloorToInt(y / (float)blockSize)
                    );
                    
                    // 从区块的地雷集合中移除
                    if (blockMinePositions.ContainsKey(mineBlock))
                    {
                        blockMinePositions[mineBlock].Remove(new Vector2Int(x, y));
                    }
                    
                    // 重新计算这个位置为空白或数字
                    int count = CountAdjacentMines(x, y);
                    Cell cell = new Cell(pos,
                        count > 0 ? Cell.Type.Number : Cell.Type.Empty,
                        count > 0 ? board.tileNumbers[count] : board.tileEmpty);
                    cell.Number = count;
                    state[pos] = cell;
                }
            }
        }

        isInitialized = true;
    }

    private void InitializeBlockWithFirstClick(Vector2Int blockCoord, Vector2Int firstClick)
    {
        if (initializedBlocks.ContainsKey(blockCoord)) return;

        int startX = blockCoord.x * blockSize;
        int startY = blockCoord.y * blockSize;
        int endX = startX + blockSize - 1;
        int endY = startY + blockSize - 1;

        // 计算本区块地雷数量（基于密度）
        int blockMineCount = Mathf.RoundToInt(blockSize * blockSize * mineDensity);
        blockMineCount = Mathf.Max(1, blockMineCount);

        // 生成候选位置（排除首次点击及其相邻格子）
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                // 检查是否在首次点击的相邻范围内
                if (Mathf.Abs(pos.x - firstClick.x) > 1 || Mathf.Abs(pos.y - firstClick.y) > 1)
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
    }

    // 新增方法：只计算区块边缘的数字
    private void CalculateNumbersForBlockBorder(Vector2Int blockCoord, Vector2Int sourceBlock)
    {
        int startX = blockCoord.x * blockSize;
        int startY = blockCoord.y * blockSize;
        int endX = startX + blockSize - 1;
        int endY = startY + blockSize - 1;

        // 确定需要重新计算的边界范围
        bool isLeftBorder = sourceBlock.x < blockCoord.x;
        bool isRightBorder = sourceBlock.x > blockCoord.x;
        bool isTopBorder = sourceBlock.y > blockCoord.y;
        bool isBottomBorder = sourceBlock.y < blockCoord.y;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                // 只处理边界单元格
                if ((isLeftBorder && x == startX) ||
                    (isRightBorder && x == endX) ||
                    (isTopBorder && y == endY) ||
                    (isBottomBorder && y == startY))
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
                        
                        // 如果单元格已经揭开，立即更新显示
                        if (cell.revealed)
                        {
                            board.DrawCell(position, cell);
                        }
                    }
                }
            }
        }
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
        if (!initializedBlocks.ContainsKey(blockCoord)) return;

        int startX = blockCoord.x * blockSize;
        int startY = blockCoord.y * blockSize;
        int endX = startX + blockSize - 1;
        int endY = startY + blockSize - 1;

        // 计算本区块所有单元格的数字
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
                    
                    // 如果单元格已经揭开，立即更新显示
                    if (cell.revealed)
                    {
                        board.DrawCell(position, cell);
                    }
                }
            }
        }
    }

    private int CountAdjacentMines(int x, int y)
    {
        int count = 0;

        // 检查周围8个方向
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int checkX = x + dx;
                int checkY = y + dy;
                Vector3Int pos = new Vector3Int(checkX, checkY, 0);
                Vector2Int checkBlock = new Vector2Int(
                    Mathf.FloorToInt(checkX / (float)blockSize),
                    Mathf.FloorToInt(checkY / (float)blockSize)
                );

                // 检查该位置是否有地雷
                if (blockMinePositions.TryGetValue(checkBlock, out HashSet<Vector2Int> mines))
                {
                    if (mines.Contains(new Vector2Int(checkX, checkY)))
                    {
                        count++;
                    }
                }
            }
        }

        return count;
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

    private void Reveal()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        // 更新最后操作位置
        lastOperationPosition = new Vector3(cellPosition.x, cellPosition.y, 0);

        if (!isInitialized)
        {
            // 首次点击时初始化地图
            InitializeWithFirstClick(new Vector2Int(cellPosition.x, cellPosition.y));
            isInitialized = true;
            
            // 重新获取初始化后的单元格
            cell = GetCell(cellPosition.x, cellPosition.y);
            
            // 确保首次点击的单元格及周围是安全的
            EnsureSafeArea(cellPosition);
        }

        if (cell.type == Cell.Type.Invalid || cell.flagged) 
        {
            return;
        }

        // 如果单元格已经揭开，不播放音效
        if (cell.revealed)
        {
            if (cell.type == Cell.Type.Number)
            {
                CheckQuickReveal(cellPosition.x, cellPosition.y);
            }
            return;
        }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Empty:
                if (audioSource != null && Floodsound != null)
                {   
                    TriggerVibration();
                    audioSource.PlayOneShot(Floodsound);
                    audioSource.PlayOneShot(Unbelievable);
                }
                Flood(cell);
                UpdateScore();
                ifWin();
                break;
            case Cell.Type.Number:
                if (audioSource != null && getScoreSound != null)
                {
                    audioSource.PlayOneShot(getScoreSound);
                }
                cell.revealed = true;
                state[cell.position] = cell;
                board.DrawCell(cell.position, cell);
                UpdateScore();
                ifWin();
                break;
        }
    }

    private void CheckQuickReveal(int x, int y)
    {
        // 获取中心单元格
        Cell centerCell = GetCell(x, y);
        
        // 检查中心单元格是否已揭开且为数字类型
        if (!centerCell.revealed || centerCell.type != Cell.Type.Number)
            return;

        int flagCount = 0;
        List<Vector2Int> cellsToReveal = new List<Vector2Int>();
        List<Vector2Int> cellsToBlink = new List<Vector2Int>();

        // 统计周围插旗数量和需要揭开的单元格
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
                        cellsToBlink.Add(new Vector2Int(checkX, checkY));
                    }
                }
            }
        }

        // 如果插旗数量等于中心单元格的数字，则揭开所有未揭开的相邻单元格
        if (flagCount == centerCell.Number)
        {
            foreach (Vector2Int pos in cellsToReveal)
            {
                if (!IsValid(pos.x, pos.y)) continue;

                Cell cell = GetCell(pos.x, pos.y);
                
                // 如果遇到地雷，游戏结束
                if (cell.type == Cell.Type.Mine)
                {
                    Explode(cell);
                    return;
                }

                // 揭开单元格
                if (!cell.revealed && !cell.flagged)
                {
                    if (cell.type == Cell.Type.Empty)
                    {
                        // 如果是空白单元格，触发Flood操作
                        Flood(cell);
                    }
                    else
                    {
                        // 如果是数字单元格，直接揭开
                        cell.revealed = true;
                        state[cell.position] = cell;
                        board.DrawCell(cell.position, cell);
                    }
                }
            }
            
            // 更新积分并检查是否胜利
            UpdateScore();
            ifWin();
        }
        else
        {
            // 如果插旗数量不等于中心单元格的数字，触发闪烁效果
            if (cellsToBlink.Count > 0)
            {
                currentBlinkCoroutine = StartCoroutine(BlinkCells(cellsToBlink));
            }
        }
    }

    private void Explode(Cell cell)
    {
        TriggerVibration();
        Debug.Log("你输了!");
        Restart.gameObject.SetActive(true);
        GameOver = true;
        // 禁用Item按钮
        if (itemButton != null)
        {
            itemButton.GetComponent<Button>().interactable = false;
        }
        cell.revealed = true;
        cell.exploded = true;
        state[cell.position] = cell;
        board.DrawCell(cell.position, cell); // 更新爆炸的地雷

        // 启用并播放爆炸动画
        if (boomAnimation != null)
        {
            // 计算摄像机上中位置
            Camera cam = Camera.main;
            if (cam != null)
            {
                // z 取 boomAnimation 当前z与摄像机z的距离
                float zOffset = Mathf.Abs(boomAnimation.transform.position.z - cam.transform.position.z);
                Vector3 viewPos = new Vector3(0.5f, 0.65f, zOffset);
                Vector3 worldPos = cam.ViewportToWorldPoint(viewPos);
                worldPos.z = boomAnimation.transform.position.z; // 保持原z
                boomAnimation.transform.position = worldPos;
            }

            boomAnimation.SetActive(true);
            Animator animator = boomAnimation.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("BoomAnimation", 0, 0f);
            }
        }

        // 播放爆炸音效
        if (audioSource != null && boomSound != null)
        {
            audioSource.PlayOneShot(boomSound);
        }

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

        // 重新计算单元格类型
        int mineCount = CountAdjacentMines(cell.position.x, cell.position.y);
        Vector2Int blockCoord = new Vector2Int(
            Mathf.FloorToInt(cell.position.x / (float)blockSize),
            Mathf.FloorToInt(cell.position.y / (float)blockSize)
        );

        // 检查是否是地雷
        bool isMine = false;
        if (blockMinePositions.TryGetValue(blockCoord, out HashSet<Vector2Int> mines))
        {
            isMine = mines.Contains(new Vector2Int(cell.position.x, cell.position.y));
        }

        // 更新单元格类型
        if (isMine)
        {
            cell.type = Cell.Type.Mine;
            cell.tile = board.tileMine;
        }
        else if (mineCount > 0)
        {
            cell.type = Cell.Type.Number;
            cell.tile = board.tileNumbers[mineCount];
            cell.Number = mineCount;
        }
        else
        {
            cell.type = Cell.Type.Empty;
            cell.tile = board.tileEmpty;
        }

        cell.revealed = true;
        state[cell.position] = cell;
        board.DrawCell(cell.position, cell);

        // 如果是空白单元格，继续扩展
        if (cell.type == Cell.Type.Empty)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

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
        board.DrawCell(cellPosition, cell); // 局部更新这个单元格
        // 如果标记成功，触发震动
        if (cell.flagged)
        {
                audioSource.PlayOneShot(FlagSound);
        }
        board.tilemap.RefreshAllTiles();
        // 更新棋盘渲染
        Debug.Log("Flags 方法作用于单元格: (" + cellPosition.x + ", " + cellPosition.y + ")");
    }
    private Cell GetCell(int x, int y)
    {
        Vector3Int position = new Vector3Int(x, y, 0);
        if (!state.ContainsKey(position))
        {
            // 计算该位置周围的地雷数量
            int mineCount = CountAdjacentMines(x, y);
            
            // 检查该位置是否是地雷
            Vector2Int blockCoord = new Vector2Int(
                Mathf.FloorToInt(x / (float)blockSize),
                Mathf.FloorToInt(y / (float)blockSize)
            );
            bool isMine = false;
            if (blockMinePositions.TryGetValue(blockCoord, out HashSet<Vector2Int> mines))
            {
                isMine = mines.Contains(new Vector2Int(x, y));
            }

            // 根据地雷数量和是否是地雷确定单元格类型
            Cell cell;
            if (isMine)
            {
                cell = new Cell(position, Cell.Type.Mine, board.tileMine);
            }
            else if (mineCount > 0)
            {
                cell = new Cell(position, Cell.Type.Number, board.tileNumbers[mineCount]);
                cell.Number = mineCount;
            }
            else
            {
                cell = new Cell(position, Cell.Type.Empty, board.tileEmpty);
            }
            
            state[position] = cell;
            return cell;
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
        // 禁用Item按钮
        if (itemButton != null)
        {
            itemButton.GetComponent<Button>().interactable = false;
        }

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
        // 重新启用Item按钮
        if (itemButton != null)
        {
            itemButton.GetComponent<Button>().interactable = true;
        }

        NewGame();
    }

    // 更新积分的方法
    private void UpdateScore()
    {
        int newScore = 0;
        foreach (var cell in state.Values)
        {
            if (cell.revealed && cell.type == Cell.Type.Number)
            {
                newScore += cell.Number;
            }
        }
        if (newScore != score)
        {
            score = newScore;
            // 更新最高分
            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
            }
            // 更新UI显示
            UpdateScoreUI();
            Debug.Log($"当前积分: {score}");
        }
    }

    // 更新积分UI显示
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"当前积分: {score}";
        }
        if (highScoreText != null)
        {
            highScoreText.text = $"最高分: {highScore}";
        }
    }

    // 视角回调方法
    private void RepositionCamera()
    {
        audioSource.PlayOneShot(TouchUI);
        if (!GameOver)
        {
            // 获取相机组件
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // 计算相机应该移动到的位置
                // 由于是正交相机，我们需要考虑orthographicSize
                float cameraZ = mainCamera.transform.position.z;
                Vector3 targetPosition = new Vector3(
                    lastOperationPosition.x-4,
                    lastOperationPosition.y-7,
                    cameraZ
                );

                // 设置相机位置
                mainCamera.transform.position = targetPosition;
            }
        }
    }

    // Item按钮点击方法
    public void ItemOpen()
    {
        if (audioSource != null && TouchUI != null)
        {
            audioSource.PlayOneShot(TouchUI);
        }
        Debug.Log("点击成功");
        if (menuManager != null)
        {
            Debug.Log("显示成功1");
            menuManager.ShowMenu();
            // Menu打开时禁用游戏操作
            isTouching = false;
        }
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

        // 生成候选位置
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                candidates.Add(pos);
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

        // 记录本区块地雷位置并标记为已初始化
        blockMinePositions[blockCoord] = minesInBlock;
        initializedBlocks[blockCoord] = true;

        // 计算本区块所有单元格的数字（包括边界）
        for (int x = startX - 1; x <= endX + 1; x++)
        {
            for (int y = startY - 1; y <= endY + 1; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                Vector2Int posBlock = new Vector2Int(
                    Mathf.FloorToInt(x / (float)blockSize),
                    Mathf.FloorToInt(y / (float)blockSize)
                );

                // 跳过未初始化区块中的位置
                if (!initializedBlocks.ContainsKey(posBlock)) continue;

                // 跳过地雷位置
                if (state.TryGetValue(position, out Cell existingCell) && existingCell.type == Cell.Type.Mine)
                    continue;

                // 计算周围地雷数量
                int count = CountAdjacentMines(x, y);
                Cell cell = new Cell(position,
                    count > 0 ? Cell.Type.Number : Cell.Type.Empty,
                    count > 0 ? board.tileNumbers[count] : board.tileEmpty);
                cell.Number = count;

                // 保持已揭开状态
                if (state.TryGetValue(position, out Cell oldCell))
                {
                    cell.revealed = oldCell.revealed;
                    cell.flagged = oldCell.flagged;
                    cell.questioned = oldCell.questioned;
                }

                state[position] = cell;

                // 如果单元格已经揭开，立即更新显示
                if (cell.revealed)
                {
                    board.DrawCell(position, cell);
                }
            }
        }

        // 更新相邻区块的边界数字
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector2Int adjacentBlock = new Vector2Int(blockCoord.x + dx, blockCoord.y + dy);
                if (initializedBlocks.ContainsKey(adjacentBlock))
                {
                    CalculateNumbersForBlockBorder(adjacentBlock, blockCoord);
                }
            }
        }
    }

    private IEnumerator BlinkCells(List<Vector2Int> cellsToBlink)
    {
        // 如果已经有闪烁在进行，先停止它
        if (currentBlinkCoroutine != null)
        {
            StopCoroutine(currentBlinkCoroutine);
        }

        // 保存原始贴图和状态
        originalTiles.Clear();
        Dictionary<Vector3Int, bool> originalRevealedStates = new Dictionary<Vector3Int, bool>();
        Dictionary<Vector3Int, bool> originalFlaggedStates = new Dictionary<Vector3Int, bool>();
        Dictionary<Vector3Int, bool> originalQuestionedStates = new Dictionary<Vector3Int, bool>();

        foreach (var pos in cellsToBlink)
        {
            Vector3Int cellPos = new Vector3Int(pos.x, pos.y, 0);
            if (state.TryGetValue(cellPos, out Cell cell))
            {
                // 保存原始贴图
                originalTiles[cellPos] = cell.tile;
                // 保存原始状态
                originalRevealedStates[cellPos] = cell.revealed;
                originalFlaggedStates[cellPos] = cell.flagged;
                originalQuestionedStates[cellPos] = cell.questioned;
                
                // 确保单元格显示为未揭开状态
                cell.revealed = false;
                cell.flagged = false;
                cell.questioned = false;
                state[cellPos] = cell;
                board.tilemap.SetTile(cellPos, board.tileUnknown);
            }
        }
        board.tilemap.RefreshAllTiles();

        // 闪烁循环
        for (int i = 0; i < blinkCount; i++)
        {
            // 设置为红色
            foreach (var pos in cellsToBlink)
            {
                Vector3Int cellPos = new Vector3Int(pos.x, pos.y, 0);
                board.tilemap.SetTile(cellPos, tileRed);
            }
            board.tilemap.RefreshAllTiles();
            yield return new WaitForSeconds(blinkDuration);

            // 恢复为未揭开状态
            foreach (var pos in cellsToBlink)
            {
                Vector3Int cellPos = new Vector3Int(pos.x, pos.y, 0);
                board.tilemap.SetTile(cellPos, board.tileUnknown);
            }
            board.tilemap.RefreshAllTiles();
            yield return new WaitForSeconds(blinkDuration);
        }

        // 恢复原始状态
        foreach (var pos in cellsToBlink)
        {
            Vector3Int cellPos = new Vector3Int(pos.x, pos.y, 0);
            if (state.TryGetValue(cellPos, out Cell cell))
            {
                // 恢复原始状态
                cell.revealed = originalRevealedStates[cellPos];
                cell.flagged = originalFlaggedStates[cellPos];
                cell.questioned = originalQuestionedStates[cellPos];
                state[cellPos] = cell;

                // 根据状态设置正确的贴图
                if (cell.revealed)
                {
                    if (cell.type == Cell.Type.Mine)
                    {
                        board.tilemap.SetTile(cellPos, board.tileMine);
                    }
                    else if (cell.type == Cell.Type.Number)
                    {
                        board.tilemap.SetTile(cellPos, board.tileNumbers[cell.Number]);
                    }
                    else
                    {
                        board.tilemap.SetTile(cellPos, board.tileEmpty);
                    }
                }
                else
                {
                    if (cell.flagged)
                    {
                        board.tilemap.SetTile(cellPos, board.tileFlag);
                    }
                    else if (cell.questioned)
                    {
                        board.tilemap.SetTile(cellPos, board.tileQuestion);
                    }
                    else
                    {
                        board.tilemap.SetTile(cellPos, board.tileUnknown);
                    }
                }
            }
        }
        board.tilemap.RefreshAllTiles();

        // 清理
        originalTiles.Clear();
        originalRevealedStates.Clear();
        originalFlaggedStates.Clear();
        originalQuestionedStates.Clear();
        currentBlinkCoroutine = null;
    }

    // 新增方法：确保区域安全
    private void EnsureSafeArea(Vector3Int center)
    {
        // 检查中心位置和周围8个格子
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int x = center.x + dx;
                int y = center.y + dy;
                Vector3Int pos = new Vector3Int(x, y, 0);
                Vector2Int blockCoord = new Vector2Int(
                    Mathf.FloorToInt(x / (float)blockSize),
                    Mathf.FloorToInt(y / (float)blockSize)
                );

                // 如果发现地雷，移除它
                if (blockMinePositions.TryGetValue(blockCoord, out HashSet<Vector2Int> mines))
                {
                    Vector2Int checkPos = new Vector2Int(x, y);
                    if (mines.Contains(checkPos))
                    {
                        mines.Remove(checkPos);
                        
                        // 更新单元格为空白
                        Cell safeCell = new Cell(pos, Cell.Type.Empty, board.tileEmpty);
                        state[pos] = safeCell;
                    }
                }
            }
        }

        // 重新计算周围的数字
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                int x = center.x + dx;
                int y = center.y + dy;
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (state.TryGetValue(pos, out Cell cell) && cell.type != Cell.Type.Mine)
                {
                    // 重新计算周围地雷数量
                    int count = CountAdjacentMines(x, y);
                    cell.type = count > 0 ? Cell.Type.Number : Cell.Type.Empty;
                    cell.tile = count > 0 ? board.tileNumbers[count] : board.tileEmpty;
                    cell.Number = count;
                    state[pos] = cell;
                }
            }
        }
    }

    // 添加统一的震动方法
    private void TriggerVibration()
    {
        // 重新获取ItemMenuController实例（如果为null）
        if (itemMenuController == null)
        {
            itemMenuController = ItemMenuController.Instance;
            Debug.Log("重新获取ItemMenuController状态: " + (itemMenuController != null ? "成功" : "失败"));
        }

        bool shouldVibrate = itemMenuController != null && itemMenuController.IsVibrationEnabled;
        Debug.Log($"震动状态检查 - Controller存在: {itemMenuController != null}, 震动已启用: {itemMenuController?.IsVibrationEnabled}");

        if (shouldVibrate)
        {
            #if UNITY_ANDROID
            try
            {
                using (AndroidJavaClass vibrationHelper = new AndroidJavaClass("com.unity3d.player.VibrationHelper"))
                {
                    vibrationHelper.CallStatic("Vibrate", VIBRATION_DURATION);
                    Debug.Log("Android震动已触发");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Android震动触发失败: " + e.Message);
                // 如果原生方法失败，尝试使用Unity的方法
                Handheld.Vibrate();
                Debug.Log("已切换到Unity默认震动方法");
            }
            #else
            Handheld.Vibrate();
            Debug.Log("非Android平台震动已触发");
            #endif
        }
        else
        {
            Debug.Log("震动未触发 - 震动功能未启用或Controller未找到");
        }
    }
}


