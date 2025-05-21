using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap; // 引用 Tilemap 组件

    [Header("Tile Assets")]
    public Tile tileUnknown;    // 未揭开默认贴图
    public Tile tileEmpty;       // 已揭开的空白贴图
    public Tile tileMine;       // 地雷贴图
    public Tile tileFlag;       // 旗帜贴图
    public Tile tileQuestion;   // 问号贴图
    public Tile tileRed;        // 红色闪烁贴图（用于爆炸显示）
    public Tile tileGem;        // 新增：宝石贴图
    public Tile[] tileNumbers;  // 数字贴图数组（对应 0~8）

    // 清除所有单元格
    public void ClearAllTiles()
    {
        tilemap.ClearAllTiles();
    }

    // 绘制单元格
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
        // 添加安全检查
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
                tilemap.SetTile(position, tileEmpty); // 出错时显示空白贴图
            }
        }
        else if (cell.type == Cell.Type.Gem)
        {
            if (tileGem == null)
            {
                Debug.LogError("宝石贴图未设置！");
                tilemap.SetTile(position, tileEmpty);
            }
            else
            {
                tilemap.SetTile(position, tileGem);
                Debug.Log($"在位置 {position} 显示宝石贴图");
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

    // 清除指定位置的单元格
    public void ClearTile(Vector3Int position)
    {
        tilemap.SetTile(position, null);
    }
}