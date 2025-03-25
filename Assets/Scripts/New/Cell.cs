using UnityEngine;
using UnityEngine.Tilemaps;

public struct Cell
{
    public Vector2Int position; // ��Ϊ Vector2Int
    public Type type;
    public int Number;
    public bool revealed;
    public bool flagged;
    public bool exploded;
    public bool questioned;
    public Tile tile;

    // �޸Ĺ��캯��
    public Cell(Vector2Int position, Type type, Tile tile)
    {
        this.position = position;
        this.type = type;
        this.tile = tile;//������ Tile ���������� Tilemap ����Ⱦ��Ԫ��
        this.Number = 0; // Ĭ��ֵ
        this.revealed = false; // Ĭ��ֵ
        this.flagged = false; // Ĭ��ֵ
        this.exploded = false; // Ĭ��ֵ
        this.questioned = false; // Ĭ��ֵ
    }

    public enum Type
    {
        Invalid,
        Empty,
        Mine,
        Number
    }
}