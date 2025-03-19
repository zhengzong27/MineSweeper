using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    private bool isInitialized = false;

    public Button Restart;
    private bool GameOver;
    float touchTime = 0f; // 触摸持续时间
    bool isTouching = false;
    public Vector2 TouchPosition;//按压位置
    public int width = 8;
    public int height = 16;
    public int mineCount = 20;
    private Board board;
    private Cell[,] state;
    public GameObject circle;

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
        if (circle != null)
        {
            circle.SetActive(false);
        }
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
    //private void GenerateMines()
    //{
    //    for (int i = 0; i < mineCount; i++)
    //    {//随机产生
    //        int x = Random.Range(0, width);
    //        int y = Random.Range(0, height);
    //        while (state[x, y].type == Cell.Type.Mine)//如果当前格已有地雷，重新生成
    //        {
    //            x++;
    //            if (x >= width)
    //            {
    //                x = 0;
    //                y++;
    //                if (y >= height) {
    //                    y = 0;
    //                }
    //            }
    //        }
    //        //设置地雷
    //        state[x, y].type = Cell.Type.Mine;
    //        //地雷全亮检查生成状态
    //        //state[x, y].revealed = true;
    //    }
    //}
    //private void GenerateNumber()
    //{ for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            Cell cell = state[x, y];
    //            if (cell.type == Cell.Type.Mine)
    //            {
    //                continue;
    //            }
    //            cell.Number = CountMines(x, y);
    //            if (cell.Number > 0)
    //            {
    //                cell.type = Cell.Type.Number;
    //            }
    //            state[x, y] = cell;
    //            //显示数字
    //            //state[x, y].revealed = true;
    //        }
    //    }
    //}
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
    void Update()
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
                        touchTime = 0f; // 重置计时器
                        TouchPosition = touch.position;
                        Debug.Log("触摸开始");
                        break;

                    case TouchPhase.Stationary:
                        if (isTouching)
                        {
                            touchTime += Time.deltaTime; // 累计触摸时间
                        }
                        break;

                    case TouchPhase.Ended:
                        if (isTouching)
                        {
                            if (touchTime > 0.25f) // 长按操作
                            {
                                //ShowCircle();
                                Flags();
                                Debug.Log("长按操作");
                            }
                            else // 点击操作
                            {
                                Reveal();
                                Debug.Log("揭开操作");
                            }
                            isTouching = false; // 重置状态
                        }
                        break;

                    case TouchPhase.Canceled:
                        isTouching = false;
                        HideCircle();
                        Debug.Log("触摸取消");
                        break;
                }
            }
        }
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

        if (cell.type==Cell.Type.Invalid||cell.flagged)
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

        // 保存原始 Tile
        Dictionary<Vector2Int, Tile> originalTiles = new Dictionary<Vector2Int, Tile>();
        foreach (var pos in cellsToBlink)
        {
            originalTiles[pos] = state[pos.x, pos.y].tile; // 保存原始 Tile
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

            Debug.Log("恢复原始 Tile");
            foreach (var pos in cellsToBlink)
            {
                state[pos.x, pos.y].tile = originalTiles[pos]; // 恢复为原始 Tile
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
            for(int y = 0; y < height; y++)
            {
                cell = state[x, y];
                if(cell.type==Cell.Type.Mine)
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
    private void Flags()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);
        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }
        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        if(cell.flagged)
        {
            Handheld.Vibrate();
        }
        board.Draw(state);
    }
    private Cell GetCell(int x,int y)
    {
        if (IsValid(x, y))
        {
            return state[x, y];
        }
        else {
            return new Cell { type = Cell.Type.Invalid };
        }
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
    }
    private void ShowCircle()
    {
        if (circle != null)
        {
            // 将 Circle 设置为显示
            circle.SetActive(true);

            // 将 Circle 的位置设置为按压位置
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                circle.GetComponent<RectTransform>().parent as RectTransform,
                TouchPosition,
                null,
                out Vector2 localPoint
            );
            circle.GetComponent<RectTransform>().localPosition = localPoint;
        }
    }
    private void HideCircle()
    {
        if (circle != null)
        {
            // 将 Circle 设置为隐藏
            circle.SetActive(false);
        }
    }
}


