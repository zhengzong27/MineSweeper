using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap; // 关联的 Tilemap 组件

    [Header("Tile Assets")]
    public Tile tileUnknown;    // 未揭开的默认贴图
    public Tile tileEmpty;       // 已揭开的空白贴图
    public Tile tileMine;       // 地雷贴图
    public Tile tileFlag;       // 旗帜贴图
    public Tile tileQuestion;   // 问号贴图
    public Tile tileRed;        // 红色闪烁贴图（用于错误提示）
    public Tile[] tileNumbers;  // 数字贴图数组（索引 0~8）

    // 清空所有贴图（用于重置地图）
    public void ClearAllTiles()
    {
        tilemap.ClearAllTiles();
    }

    // 绘制单个单元格（根据其状态）
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
        // 常规绘制逻辑
        if (cell.revealed)
        {
            DrawRevealedCell(position, cell);
        }
        else
        {
            tilemap.SetTile(position, tileUnknown);
        }
    }

    // 绘制已揭开的单元格
    private void DrawRevealedCell(Vector3Int position, Cell cell)
    {
        if (cell.type == Cell.Type.Mine)
        {
            tilemap.SetTile(position, tileMine);
        }
        else if (cell.type == Cell.Type.Number)
        {
            // 添加安全检查
            if (tileNumbers == null || tileNumbers.Length == 0)
            {
                Debug.LogError("TileNumbers array is not initialized!");
                return;
            }

            // 确保数字在有效范围内 (1-8)
            int number = Mathf.Clamp(cell.Number, 1, 8);
            if (number >= 0 && number < tileNumbers.Length)
            {
                tilemap.SetTile(position, tileNumbers[number]);
            }
            else
            {
                Debug.LogError($"Invalid number index: {number}. Array length: {tileNumbers.Length}");
                tilemap.SetTile(position, tileEmpty); // 回退到空白贴图
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

    // 绘制未揭开的单元格
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

    // 清理指定位置的贴图（用于视野外单元格）
    public void ClearTile(Vector3Int position)
    {
        tilemap.SetTile(position, null);
    }
}