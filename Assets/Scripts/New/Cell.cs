using UnityEngine;
using UnityEngine.Tilemaps;

public struct Cell
{
    public Vector2Int position; // 改为 Vector2Int
    public Type type;
    public int Number;
    public bool revealed;
    public bool flagged;
    public bool exploded;
    public bool questioned;
    public Tile tile;

    // 修改构造函数
    public Cell(Vector2Int position, Type type, Tile tile)
    {
        this.position = position;
        this.type = type;
        this.tile = tile;//关联的 Tile 对象，用于在 Tilemap 中渲染单元格
        this.Number = 0; // 默认值
        this.revealed = false; // 默认值
        this.flagged = false; // 默认值
        this.exploded = false; // 默认值
        this.questioned = false; // 默认值
    }

    public enum Type
    {
        Invalid,
        Empty,
        Mine,
        Number
    }
}