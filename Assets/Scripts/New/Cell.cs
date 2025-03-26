using UnityEngine;
using UnityEngine.Tilemaps;

public struct Cell
{
    public Vector2Int position;//×Öµä¼ü
    public Type type;
    public int Number;
    public bool revealed;
    public bool flagged;
    public bool exploded;
    public bool questioned;
    public TileBase tile;

    public Cell(Vector2Int position, Type type, Tile tile)
    {
        this.position = position;
        this.type = type;
        this.tile = tile;
        this.Number = 0;
        this.revealed = false;
        this.flagged = false;
        this.exploded = false;
        this.questioned = false;
    }

    public enum Type
    {
        Invalid,
        Empty,
        Mine,
        Number
    }
}