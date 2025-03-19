using UnityEngine;
using UnityEngine.Tilemaps;
public struct Cell
{
    public Vector3Int position;
    public Type type;
    public int Number;
    public bool revealed;
    public bool flagged;
    public bool exploded;
    public Tile tile;

    // 添加构造函数
    public Cell(Vector3Int position, Type type, Tile tile)
    {
        this.position = position;
        this.type = type;
        this.tile = tile;
        this.Number = 0; // 默认值
        this.revealed = false; // 默认值
        this.flagged = false; // 默认值
        this.exploded = false; // 默认值
    }

    public enum Type
    {
        Invalid,
        Empty,
        Mine,
        Number
    }
}