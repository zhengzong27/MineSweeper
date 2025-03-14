using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    float touchTime = 0f; // 触摸持续时间
    bool isTouching = false;
    bool isMoving = false;
    Vector2 initialTouchPosition;

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
                cell.revealed = true;
                state[x, y] = cell;
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
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isTouching = true;
                    isMoving = false;
                    touchTime = 0f;
                    initialTouchPosition = touch.position;
                    break;

                case TouchPhase.Moved:
                    isMoving = true;
                    break;

                case TouchPhase.Stationary:
                    if (isTouching && !isMoving)
                    {
                        touchTime += Time.deltaTime;
                        if (touchTime >= 0.25f)
                        {
                            Flag(GetCellPosition(initialTouchPosition));
                            ResetTouchState();
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isTouching)
                    {
                        if (!isMoving && touchTime < 0.25f)
                        {
                            RevealCell(GetCellPosition(touch.position));
                        }
                        ResetTouchState();
                    }
                    break;
            }
        }
    }
    Vector2Int GetCellPosition(Vector2 screenPosition)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x + 0.5f),
            Mathf.FloorToInt(worldPos.y + 0.5f)
        );
    }
    void Flag(Vector2Int cellPos)
    {
        if (IsValidCell(cellPos))
        {
            Cell cell = state[cellPos.x, cellPos.y];
            if (!cell.revealed)
            {
                cell.flagged = !cell.flagged;
                state[cellPos.x, cellPos.y] = cell;
                board.Draw(state);
            }
        }
    }

    void RevealCell(Vector2Int cellPos)
    {
        if (IsValidCell(cellPos))
        {
            Cell cell = state[cellPos.x, cellPos.y];
            if (!cell.flagged && !cell.revealed)
            {
                cell.revealed = true;
                state[cellPos.x, cellPos.y] = cell;
                board.Draw(state);

                if (cell.type == Cell.Type.Mine)
                    GameOver();
                else if (cell.type == Cell.Type.Empty)
                    FloodFill(cellPos.x, cellPos.y);
            }
        }
    }

    bool IsValidCell(Vector2Int pos) =>
        pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;

    void ResetTouchState()
    {
        isTouching = false;
        isMoving = false;
        touchTime = 0f;
    }

    // 需要补充的方法
    void FloodFill(int x, int y) { /* 实现洪水填充算法 */ }
    void GameOver() { /* 游戏结束逻辑 */ }
}
}
