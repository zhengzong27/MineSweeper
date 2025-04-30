using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap; // ������ Tilemap ���

    [Header("Tile Assets")]
    public Tile tileUnknown;    // δ�ҿ���Ĭ����ͼ
    public Tile tileEmpty;       // �ѽҿ��Ŀհ���ͼ
    public Tile tileMine;       // ������ͼ
    public Tile tileFlag;       // ������ͼ
    public Tile tileQuestion;   // �ʺ���ͼ
    public Tile tileRed;        // ��ɫ��˸��ͼ�����ڴ�����ʾ��
    public Tile[] tileNumbers;  // ������ͼ���飨���� 0~8��

    // ���������ͼ���������õ�ͼ��
    public void ClearAllTiles()
    {
        tilemap.ClearAllTiles();
    }

    // ���Ƶ�����Ԫ�񣨸�����״̬��
    public void DrawCell(Vector3Int position, Cell cell)
    {
        if (cell.flagged)
        {
            tilemap.SetTile(position, tileFlag);
            return;
        }
        else if (cell.questioned)
        {
            tilemap.SetTile(position, tileQuestion);
            return;
        }
        // ��������߼�
        if (cell.revealed)
        {
            DrawRevealedCell(position, cell);
        }
        else
        {
            tilemap.SetTile(position, tileUnknown);
        }
    }

    // �����ѽҿ��ĵ�Ԫ��
    private void DrawRevealedCell(Vector3Int position, Cell cell)
    {
        if (cell.type == Cell.Type.Mine)
        {
            tilemap.SetTile(position, tileMine);
        }
        else if (cell.type == Cell.Type.Number)
        {
            // ���Ӱ�ȫ���
            if (tileNumbers == null || tileNumbers.Length == 0)
            {
                Debug.LogError("TileNumbers array is not initialized!");
                return;
            }

            // ȷ����������Ч��Χ�� (1-8)
            int number = Mathf.Clamp(cell.Number, 1, 8);
            if (number >= 0 && number < tileNumbers.Length)
            {
                tilemap.SetTile(position, tileNumbers[number]);
            }
            else
            {
                Debug.LogError($"Invalid number index: {number}. Array length: {tileNumbers.Length}");
                tilemap.SetTile(position, tileEmpty); // ���˵��հ���ͼ
            }
        }
        else
        {
            tilemap.SetTile(position, tileEmpty);
        }

        if (cell.exploded)
        {
            tilemap.SetTile(position, tileRed);
        }
    }

    // ����δ�ҿ��ĵ�Ԫ��
    private void DrawUnrevealedCell(Vector3Int position, Cell cell)
    {
        if (cell.flagged)
        {
            tilemap.SetTile(position, tileFlag);
        }
        else if (cell.questioned)
        {
            tilemap.SetTile(position, tileQuestion);
        }
        else
        {
            tilemap.SetTile(position, tileUnknown);
        }
    }

    // ����ָ��λ�õ���ͼ��������Ұ�ⵥԪ��
    public void ClearTile(Vector3Int position)
    {
        tilemap.SetTile(position, null);
    }
}