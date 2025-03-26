using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Tile tileUnknown;
    public Tile tileQuestion;
    public Tile tileRed;
    public Tile tileEmpty;
    public Tile tileMine;
    public Tile tileExplode;
    public Tile tileFlag;
    public Tile tileNum1;
    public Tile tileNum2;
    public Tile tileNum3;
    public Tile tileNum4;
    public Tile tileNum5;
    public Tile tileNum6;
    public Tile tileNum7;
    public Tile tileNum8;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void Draw(Dictionary<Vector2Int, Cell> state)
    {

        foreach (var cell in state.Values)
        {
            Vector3Int position = new Vector3Int(cell.position.x, cell.position.y, 0);

            if (cell.exploded)
            {
                tilemap.SetTile(position, tileExplode);
            }
            else if (cell.flagged)
            {
                tilemap.SetTile(position, tileFlag);
            }
            else if (cell.questioned)
            {
                tilemap.SetTile(position, tileQuestion);
            }
            else if (!cell.revealed)
            {
                tilemap.SetTile(position, tileUnknown);
            }
            else
            {
                switch (cell.type)
                {
                    case Cell.Type.Empty:
                        tilemap.SetTile(position, tileEmpty);
                        break;
                    case Cell.Type.Mine:
                        tilemap.SetTile(position, tileMine);
                        break;
                    case Cell.Type.Number:
                        SetNumberTile(position, cell.Number);
                        break;
                }
            }
        }
    }

    private void SetNumberTile(Vector3Int position, int number)
    {
        switch (number)
        {
            case 1: tilemap.SetTile(position, tileNum1); break;
            case 2: tilemap.SetTile(position, tileNum2); break;
            case 3: tilemap.SetTile(position, tileNum3); break;
            case 4: tilemap.SetTile(position, tileNum4); break;
            case 5: tilemap.SetTile(position, tileNum5); break;
            case 6: tilemap.SetTile(position, tileNum6); break;
            case 7: tilemap.SetTile(position, tileNum7); break;
            case 8: tilemap.SetTile(position, tileNum8); break;
        }
    }// 生成指定格子的地图
    public void GenerateTile(Vector3Int cellPos)
    {
        // 在这里实现地图生成逻辑（比如随机地形）
        tilemap.SetTile(cellPos, tileUnknown);
    }

    // 清除指定格子的地图
    public void ClearTile(Vector3Int cellPos)
    {
        tilemap.SetTile(cellPos, null); // 移除格子
    }

} 