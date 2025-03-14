using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
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
        for(int x=0;x<width;x++)
        {
            for(int y=0;y<height;y++)
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
        for(int i=0;i<mineCount;i++)
        {//随机产生
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            while(state[x,y].type == Cell.Type.Mine)//如果当前格已有地雷，重新生成
            {
                x ++;
                if(x>=width)
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
    {for(int x=0;x<width;x++)
        {
            for(int y=0;y<height;y++)
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
    private int CountMines(int cellX,int cellY)
    {
        int count = 0;
        for(int adjacentX=-1;adjacentX<=1;adjacentX++)
        {
            for(int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if(adjacentX == 0 && adjacentY == 0)
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
}
