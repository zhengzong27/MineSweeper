using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweepLogic : MonoBehaviour
{
    private Game game; // 引用Game类

    private void Awake()
    {
        game = GetComponent<Game>();
    }

    public void Reveal()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(game.TouchPosition);
        Vector3Int cellPosition = game.board.tilemap.WorldToCell(worldPosition);
        Cell cell = game.GetCell(cellPosition.x, cellPosition.y);

        if (!game.isInitialized)
        {
            // 首次点击时初始化地图
            game.generate.InitializeWithFirstClick(new Vector2Int(cellPosition.x, cellPosition.y));
            game.isInitialized = true;
        }

        if (cell.type == Cell.Type.Invalid || cell.flagged)
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
            case Cell.Type.Number:
                Debug.Log("按下数字单元格");
                if (cell.revealed)
                {
                    game.CheckQuickReveal(cellPosition.x, cellPosition.y);
                }
                else
                {
                    cell.revealed = true;
                    game.state[cellPosition.x, cellPosition.y] = cell;
                    ifWin();
                }
                break;
        }

        game.board.Draw(game.state);
    }

    public void Question(Vector3Int cellPosition)
    {
        Debug.Log("进入 Question 方法");
        Cell cell = game.GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            Debug.Log("单元格无效或已揭开，直接返回");
            return;
        }

        cell.flagged = false;
        cell.questioned = !cell.questioned;

        if (cell.questioned)
        {
            Debug.Log("设置单元格为问号 Tile");
            cell.tile = game.board.tileQuestion;
            game.board.tilemap.SetTile(cellPosition, game.board.tileQuestion);
        }
        else
        {
            Debug.Log("恢复单元格为未知 Tile");
            cell.tile = game.board.tileUnknown;
            game.board.tilemap.SetTile(cellPosition, game.board.tileUnknown);
        }

        game.state[cellPosition.x, cellPosition.y] = cell;

        if (cell.questioned)
        {
            Handheld.Vibrate();
        }

        game.board.tilemap.RefreshAllTiles();
        Debug.Log("Question 方法作用于单元格: (" + cellPosition.x + ", " + cellPosition.y + ")");
    }

    public void Flags(Vector3Int cellPosition)
    {
        Cell cell = game.GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }

        cell.flagged = !cell.flagged;
        game.state[cellPosition.x, cellPosition.y] = cell;

        if (cell.flagged)
        {
            Handheld.Vibrate();
        }

        game.board.Draw(game.state);
        Debug.Log("Flags 方法作用于单元格: (" + cellPosition.x + ", " + cellPosition.y + ")");
    }

    public void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid || cell.flagged) return;

        cell.revealed = true;
        game.state[cell.position.x, cell.position.y] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int x = cell.position.x + dx;
                    int y = cell.position.y + dy;

                    if (game.IsValid(x, y))
                    {
                        Cell neighbor = game.GetCell(x, y);
                        Flood(neighbor);
                    }
                }
            }
        }
        game.board.Draw(game.state);
    }
    public void Explode(Cell cell)//触雷
    {
        Debug.Log("你输了!");
        game.Restart.gameObject.SetActive(true);
        game.GameOver = true;
        cell.revealed = true;
        cell.exploded = true;
        game.state[cell.position.x, cell.position.y] = cell;
        for (int x = 0; x < game.width; x++)
        {
            for (int y = 0; y < game.height; y++)
            {
                cell = game.state[x, y];
                if (cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    game.state[x, y] = cell;
                }
            }
        }
    }
    public void ifWin()
    {
        for (int x = 0; x < game.width; x++)
        {
            for (int y = 0; y < game.height; y++)
            {
                Cell cell = game.state[x, y];
                if (cell.type != Cell.Type.Mine && !cell.revealed)
                {
                    return;
                }
            }
        }

        Debug.Log("你赢了！");
        game.Restart.gameObject.SetActive(true);
        game.GameOver = true;
        for (int x = 0; x < game.width; x++)
        {
            for (int y = 0; y < game.height; y++)
            {
                Cell cell = game.state[x, y];
                if (cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    game.state[x, y] = cell;
                }
            }
        }
    }//胜利判断
}