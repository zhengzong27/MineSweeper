using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    float touchTime = 0f; // 触摸持续时间
    bool isTouching = false;
    public Vector2 TouchPosition;

    public int width = 8;
    public int height = 16;
    public int mineCount = 20;
    private Board board;
    private Cell[,] state;
    private void Awake()
    {
        board = GetComponentInChildren<Board>();
    }
    private void Start()
    {
        NewGame();
    }
    private void NewGame()
    {
        state = new Cell[width, height];
        GenerateCells();
        GenerateMines();
        GenerateNumber();
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
    private void GenerateMines()
    {
        for (int i = 0; i < mineCount; i++)
        {//随机产生
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            while (state[x, y].type == Cell.Type.Mine)//如果当前格已有地雷，重新生成
            {
                x++;
                if (x >= width)
                {
                    x = 0;
                    y++;
                    if (y >= height) {
                        y = 0;
                    }
                }
            }
            //设置地雷
            state[x, y].type = Cell.Type.Mine;
            //地雷全亮检查生成状态
            //state[x, y].revealed = true;
        }
    }
    private void GenerateNumber()
    { for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                if (cell.type == Cell.Type.Mine)
                {
                    continue;
                }
                cell.Number = CountMines(x, y);
                if (cell.Number > 0)
                {
                    cell.type = Cell.Type.Number;
                }
                state[x, y] = cell;
                //显示数字
                //state[x, y].revealed = true;
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
                        if (touchTime > 0.25f) // 长按 1 秒
                        {
                            Flags();
                            Debug.Log("长按操作");
                            isTouching = false; // 重置状态
                            return;
                        }
                    }
                    break;

                case TouchPhase.Ended:
                    Reveal();
                    Debug.Log("揭开操作");
                    isTouching = false;
                    break;
                case TouchPhase.Canceled:
                    isTouching = false;
                    Debug.Log("触摸结束");
                    break;
            }
        }
    }
    private void Reveal()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);
        if (cell.type==Cell.Type.Invalid||cell.revealed||cell.revealed)
        {
            return;
        }
        if(cell.type== Cell.Type.Empty)
        {
            Flood(cell);
        }
        cell.revealed = true;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);

    }
    private void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;
        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.x));
            Flood(GetCell(cell.position.x, cell.position.y-1));
            Flood(GetCell(cell.position.x, cell.position.y+1));
        }
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

}


