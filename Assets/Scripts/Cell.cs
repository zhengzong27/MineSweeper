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

    // ��ӹ��캯��
    public Cell(Vector3Int position, Type type, Tile tile)
    {
        this.position = position;
        this.type = type;
        this.tile = tile;
        this.Number = 0; // Ĭ��ֵ
        this.revealed = false; // Ĭ��ֵ
        this.flagged = false; // Ĭ��ֵ
        this.exploded = false; // Ĭ��ֵ
    }

    public enum Type
    {
        Invalid,
        Empty,
        Mine,
        Number
    }
}