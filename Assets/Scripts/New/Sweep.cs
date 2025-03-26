using System.Collections;  // ��������ʹ��IEnumerator��WaitForSeconds
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;  // ��������ʹ��TileBase
public class Sweep : MonoBehaviour
{
    // ���������������ע�͵���
     private Board board;
    // private Touch touch;
    // private Restart restart;

    // ��Ϸ״̬
    private Dictionary<Vector2Int, Cell> _cellStates = new Dictionary<Vector2Int, Cell>();
    public Dictionary<Vector2Int, Cell> CellStates => _cellStates;
    private bool isInitialized = false;
    private bool gameOver = false;
    private int mineCount = 10;

    // ��ʱ������ԭTouchPosition��
    private Vector2 touchPosition;

    private void Reveal(Cell cell)
    {
        // ��ȡ���λ�ã�ԭTouchPosition��
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector3Int cellPosition = new Vector3Int(Mathf.FloorToInt(worldPosition.x), Mathf.FloorToInt(worldPosition.y), 0);
        Vector2Int cellKey = new Vector2Int(cellPosition.x, cellPosition.y);
        Vector2Int cellPos = new Vector2Int(cellPosition.x, cellPosition.y); // ��ȷ���� cellPos

        // ��ȡ��Ԫ������������򴴽�Ĭ�ϵ�Ԫ��
        if (!_cellStates.TryGetValue(cellKey, out Cell cellToReveal))
        {
            cell = new Cell(cellKey, Cell.Type.Invalid, null);
            _cellStates[cellKey] = cell;
        }

        if (!isInitialized)
        {
            // �״ε��ʱ��ʼ����ͼ
            InitializeWithFirstClick(cellKey);
            isInitialized = true;
        }

        if (cell.type == Cell.Type.Invalid || cell.flagged) // �����Ԫ����Ч���������Ϊ�ʺ�
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
                Debug.Log("�������ֵ�Ԫ��");
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
        // ������ʾ��ԭboard.Draw��
        // board.Draw(cellStates);
    }

    private void Explode(Cell cell)
    {
        Debug.Log("������!");
        // restart.gameObject.SetActive(true);
        gameOver = true;
        
        cell.revealed = true;
        cell.exploded = true;
        _cellStates[cell.position] = cell;
        
        // ��ʾ���е���
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
            // �˷���ݹ�ҿ�
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // ��������

                    Vector2Int neighborPos = new Vector2Int(cell.position.x + dx, cell.position.y + dy);
                    
                    // ����ھӲ����ڣ�����Ĭ�Ͽյ�Ԫ��
                    if (!_cellStates.ContainsKey(neighborPos))
                    {
                        _cellStates[neighborPos] = new Cell(neighborPos, Cell.Type.Empty, null);
                    }
                    
                    Flood(_cellStates[neighborPos]);
                }
            }
        }
        
        // ������ʾ
        // board.Draw(cellStates);
    }

    private void CheckWinCondition()
    {
        foreach (var kvp in _cellStates)
        {
            Cell cell = kvp.Value;
            if (cell.type != Cell.Type.Mine && !cell.revealed)
            {
                return; // ����δ�ҿ��ķǵ��׵�Ԫ��
            }
        }
        
        Debug.Log("��Ϸʤ��!");
        // ʤ���߼�...
    }

    private void InitializeWithFirstClick(Vector2Int firstClickPos)
    {
        // TODO: ʵ���״ε����ʼ���߼�
        // ��Ҫȷ���״ε��λ����Χû�е���
        // ����MinesCreate���ɵ���
        
        Debug.Log($"�״ε��λ��: {firstClickPos}, ���ڳ�ʼ����ͼ...");
    }

    private void CheckQuickReveal(Vector2Int cellPos)
    {
        // TODO: ʵ�ֿ��ٽҿ��߼���˫�����ֵ�Ԫ��
        // �����Χ���������Ƿ��������
        
        Debug.Log($"���Կ��ٽҿ�: {cellPos}");
    }

    // ��������
    private bool IsValid(Vector2Int pos)
    {
        // ���޵�ͼ����Ҫ�߽���
        return true;
    }
    private void CheckQuickReveal(int x, int y)
    {
        Vector2Int centerPos = new Vector2Int(x, y);

        // ��ȡ���ĵ�Ԫ������������򷵻أ�
        if (!_cellStates.TryGetValue(centerPos, out Cell centerCell) ||
            !centerCell.revealed ||
            centerCell.type != Cell.Type.Number)
        {
            return;
        }

        int flagCount = 0;
        List<Vector2Int> cellsToReveal = new List<Vector2Int>();
        List<Vector2Int> cellsToBlink = new List<Vector2Int>();

        // ͳ����Χ��ǵĵ�����������Ҫ��ʾ�ĵ�Ԫ��
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector2Int neighborPos = new Vector2Int(x + dx, y + dy);

                // ��ȡ�ھӵ�Ԫ������������򴴽�Ĭ�ϣ�
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

        // ���ٽ�ʾ�����ж�
        if (flagCount >= centerCell.Number)
        {
            foreach (Vector2Int pos in cellsToReveal)
            {
                // ��ȡ�򴴽���Ԫ��
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
            // ���ٽ�ʾ���������㣬������˸
            Debug.Log("���ٽ�ʾ���������㣬������˸");
            StartCoroutine(BlinkCells(cellsToBlink));
        }
    }

    private IEnumerator BlinkCells(List<Vector2Int> cellsToBlink)
    {
        Debug.Log("��ʼ��˸");
        int blinkCount = 2;
        float blinkDuration = 0.05f;

        // ������˸ǰ��Tile
        Dictionary<Vector2Int, TileBase> previousTiles = new Dictionary<Vector2Int, TileBase>();
        foreach (var pos in cellsToBlink)
        {
            if (_cellStates.TryGetValue(pos, out Cell cell) && cell.tile != null)
            {
                previousTiles[pos] = cell.tile;
            }
        }

        // ��˸�߼�
        for (int i = 0; i < blinkCount; i++)
        {
            // ����Ϊ��ɫTile
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

            // �ָ�ԭ����Tile
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
        Debug.Log("��˸����");
    }
    public void RevealAtPosition(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        // ���Ի�ȡ��Ԫ������������򴴽��µ�Ԫ��
        if (!_cellStates.TryGetValue(pos, out Cell cellToReveal))
        {
            cellToReveal = new Cell(pos, Cell.Type.Empty, null);
            _cellStates[pos] = cellToReveal;
        }

        // ���ú��Ľ�ʾ�߼�
        Reveal(cellToReveal);
    }
}