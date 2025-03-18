using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    private bool isInitialized = false;

    public Button Restart;
    private bool GameOver;
    float touchTime = 0f; // 触摸持续时间
    bool isTouching = false;
    public Vector2 TouchPosition;
    public int width = 8;
    public int height = 16;
    public int mineCount = 20;
    private Board board;
    private Cell[,] state;
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            NewGame();
        }
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

        if (cell.type==Cell.Type.Invalid||cell.revealed||cell.flagged)
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
            default:
                cell.revealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                ifWin();
                break;
        }
        board.Draw(state);

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
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid||cell.flagged) return;

        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell; // 若state引用已存在，可删除此行

        if (cell.type == Cell.Type.Empty)
        {
            // 四方向
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.y)); // 修正参数为 y
            Flood(GetCell(cell.position.x, cell.position.y - 1));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
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
        board.Draw(state);
    }
    private Cell GetCell(int x,int y)
    {
        if (IsValid(x, y))
        {
            return state[x, y];
        }
        else {
            return new Cell();
        }
    }
    private bool IsValid(int x,int y)
    {
        return x >= 0 && x < width && y >= 0 && y <= height;
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
}


