using System.Collections;  // 添加这个以使用IEnumerator和WaitForSeconds
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;  // 添加这个以使用TileBase
public class Sweep : MonoBehaviour
{
    // 引用其他组件（先注释掉）
     private Board board;
    // private Touch touch;
    // private Restart restart;

    // 游戏状态
    private Dictionary<Vector2Int, Cell> _cellStates = new Dictionary<Vector2Int, Cell>();
    public Dictionary<Vector2Int, Cell> CellStates => _cellStates;
    private bool isInitialized = false;
    private bool gameOver = false;
    private int mineCount = 10;

    // 临时变量（原TouchPosition）
    private Vector2 touchPosition;

    private void Reveal(Cell cell)
    {
        // 获取点击位置（原TouchPosition）
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector3Int cellPosition = new Vector3Int(Mathf.FloorToInt(worldPosition.x), Mathf.FloorToInt(worldPosition.y), 0);
        Vector2Int cellKey = new Vector2Int(cellPosition.x, cellPosition.y);
        Vector2Int cellPos = new Vector2Int(cellPosition.x, cellPosition.y); // 明确定义 cellPos

        // 获取单元格（如果不存在则创建默认单元格）
        if (!_cellStates.TryGetValue(cellKey, out Cell cellToReveal))
        {
            cell = new Cell(cellKey, Cell.Type.Invalid, null);
            _cellStates[cellKey] = cell;
        }

        if (!isInitialized)
        {
            // 首次点击时初始化地图
            InitializeWithFirstClick(cellKey);
            isInitialized = true;
        }

        if (cell.type == Cell.Type.Invalid || cell.flagged) // 如果单元格无效、插旗或标记为问号
        {
            return;
        }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            case Cell.Type.Number:
                Debug.Log("按下数字单元格");
                if (cell.revealed)
                {
                    CheckQuickReveal(cellKey);
                }
                else
                {
                    cell.revealed = true;
                    _cellStates[cellKey] = cell;
                    CheckWinCondition();
                }
                break;
        }
        ZoneManager.Instance.CheckZoneLockState(cellPos);
        // 更新显示（原board.Draw）
        // board.Draw(cellStates);
    }

    private void Explode(Cell cell)
    {
        Debug.Log("你输了!");
        // restart.gameObject.SetActive(true);
        gameOver = true;
        
        cell.revealed = true;
        cell.exploded = true;
        _cellStates[cell.position] = cell;
        
        // 显示所有地雷
        foreach (var kvp in _cellStates)
        {
            Cell c = kvp.Value;
            if (c.type == Cell.Type.Mine)
            {
                c.revealed = true;
                _cellStates[kvp.Key] = c;
            }
        }
    }

    private void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid || cell.flagged) return;

        cell.revealed = true;
        _cellStates[cell.position] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            // 八方向递归揭开
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // 跳过自身

                    Vector2Int neighborPos = new Vector2Int(cell.position.x + dx, cell.position.y + dy);
                    
                    // 如果邻居不存在，创建默认空单元格
                    if (!_cellStates.ContainsKey(neighborPos))
                    {
                        _cellStates[neighborPos] = new Cell(neighborPos, Cell.Type.Empty, null);
                    }
                    
                    Flood(_cellStates[neighborPos]);
                }
            }
        }
        
        // 更新显示
        // board.Draw(cellStates);
    }

    private void CheckWinCondition()
    {
        foreach (var kvp in _cellStates)
        {
            Cell cell = kvp.Value;
            if (cell.type != Cell.Type.Mine && !cell.revealed)
            {
                return; // 还有未揭开的非地雷单元格
            }
        }
        
        Debug.Log("游戏胜利!");
        // 胜利逻辑...
    }

    private void InitializeWithFirstClick(Vector2Int firstClickPos)
    {
        // TODO: 实现首次点击初始化逻辑
        // 需要确保首次点击位置周围没有地雷
        // 调用MinesCreate生成地雷
        
        Debug.Log($"首次点击位置: {firstClickPos}, 正在初始化地图...");
    }

    private void CheckQuickReveal(Vector2Int cellPos)
    {
        // TODO: 实现快速揭开逻辑（双击数字单元格）
        // 检查周围旗帜数量是否等于数字
        
        Debug.Log($"尝试快速揭开: {cellPos}");
    }

    // 辅助方法
    private bool IsValid(Vector2Int pos)
    {
        // 无限地图不需要边界检查
        return true;
    }
    private void CheckQuickReveal(int x, int y)
    {
        Vector2Int centerPos = new Vector2Int(x, y);

        // 获取中心单元格（如果不存在则返回）
        if (!_cellStates.TryGetValue(centerPos, out Cell centerCell) ||
            !centerCell.revealed ||
            centerCell.type != Cell.Type.Number)
        {
            return;
        }

        int flagCount = 0;
        List<Vector2Int> cellsToReveal = new List<Vector2Int>();
        List<Vector2Int> cellsToBlink = new List<Vector2Int>();

        // 统计周围标记的地雷数量和需要揭示的单元格
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector2Int neighborPos = new Vector2Int(x + dx, y + dy);

                // 获取邻居单元格（如果不存在则创建默认）
                if (!_cellStates.TryGetValue(neighborPos, out Cell neighbor))
                {
                    neighbor = new Cell(neighborPos, Cell.Type.Empty, null);
                    _cellStates[neighborPos] = neighbor;
                }

                if (neighbor.flagged)
                {
                    flagCount++;
                }
                else if (!neighbor.revealed && !neighbor.flagged)
                {
                    cellsToReveal.Add(neighborPos);
                    cellsToBlink.Add(neighborPos);
                }
            }
        }

        // 快速揭示条件判断
        if (flagCount >= centerCell.Number)
        {
            foreach (Vector2Int pos in cellsToReveal)
            {
                // 获取或创建单元格
                if (!_cellStates.TryGetValue(pos, out Cell cell))
                {
                    cell = new Cell(pos, Cell.Type.Empty, null);
                    _cellStates[pos] = cell;
                }

                if (cell.type == Cell.Type.Mine)
                {
                    Explode(cell);
                    return;
                }

                if (!cell.revealed && !cell.flagged)
                {
                    if (cell.type == Cell.Type.Empty)
                    {
                        Flood(cell);
                    }
                    else
                    {
                        cell.revealed = true;
                        _cellStates[pos] = cell;
                    }
                }
            }
            CheckWinCondition();
            // board.Draw(cellStates);
        }
        else
        {
            // 快速揭示条件不满足，触发闪烁
            Debug.Log("快速揭示条件不满足，触发闪烁");
            StartCoroutine(BlinkCells(cellsToBlink));
        }
    }

    private IEnumerator BlinkCells(List<Vector2Int> cellsToBlink)
    {
        Debug.Log("开始闪烁");
        int blinkCount = 2;
        float blinkDuration = 0.05f;

        // 保存闪烁前的Tile
        Dictionary<Vector2Int, TileBase> previousTiles = new Dictionary<Vector2Int, TileBase>();
        foreach (var pos in cellsToBlink)
        {
            if (_cellStates.TryGetValue(pos, out Cell cell) && cell.tile != null)
            {
                previousTiles[pos] = cell.tile;
            }
        }

        // 闪烁逻辑
        for (int i = 0; i < blinkCount; i++)
        {
            // 设置为红色Tile
            foreach (var pos in cellsToBlink)
            {
                if (_cellStates.TryGetValue(pos, out Cell cell))
                {
                    cell.tile = board.tileRed;
                    _cellStates[pos] = cell;
                    board.tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), board.tileRed);
                }
            }
            board.tilemap.RefreshAllTiles();
            yield return new WaitForSeconds(blinkDuration);

            // 恢复原来的Tile
            foreach (var pos in cellsToBlink)
            {
                if (previousTiles.TryGetValue(pos, out TileBase originalTile) &&
                    _cellStates.TryGetValue(pos, out Cell cell))
                {
                    cell.tile = originalTile;
                    _cellStates[pos] = cell;
                    board.tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), originalTile);
                }
            }
            // board.Draw(cellStates);
            yield return new WaitForSeconds(blinkDuration);
        }
        Debug.Log("闪烁结束");
    }
    public void RevealAtPosition(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        // 尝试获取单元格，如果不存在则创建新单元格
        if (!_cellStates.TryGetValue(pos, out Cell cellToReveal))
        {
            cellToReveal = new Cell(pos, Cell.Type.Empty, null);
            _cellStates[pos] = cellToReveal;
        }

        // 调用核心揭示逻辑
        Reveal(cellToReveal);
    }
}