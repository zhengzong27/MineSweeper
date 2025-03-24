using System.Collections;
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
    public void Draw(Dictionary<Vector2Int, Cell[,]> initializedBlocks)
    {
        foreach (var block in initializedBlocks.Values)
        {
            int width = block.GetLength(0);
            int height = block.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Cell cell = block[x, y];
                    tilemap.SetTile(cell.position, GetTile(cell));
                }
            }
        }
    }
    private Tile GetTile(Cell cell)
    {
        if (cell.revealed)
        {
            return GetRevealedTile(cell);
        }
        else if (cell.flagged)
        {
            return tileFlag;
        }
        else if (cell.questioned) // ��Ӷ� questioned ״̬�ļ��
        {
            return tileQuestion;
        }
        else if (cell.exploded)
        {
            return tileExplode;
        }
        else
        {
            return tileUnknown;
        }
    }
    private Tile GetRevealedTile(Cell cell)
    {
        switch(cell.type)
        {
            case Cell.Type.Empty:return tileEmpty;
            case Cell.Type.Mine:return cell.exploded ? tileExplode : tileMine;
            case Cell.Type.Number:return GetNumberTile(cell);
            default:return null;
        }
    }
    private Tile GetNumberTile(Cell cell)
    {
        switch(cell.Number)
        {
            case 1:return tileNum1;
            case 2: return tileNum2;
            case 3: return tileNum3;
            case 4: return tileNum4;
            case 5: return tileNum5;
            case 6: return tileNum6;
            case 7: return tileNum7;
            case 8: return tileNum8;
            default: return null;
        }
    }
}
