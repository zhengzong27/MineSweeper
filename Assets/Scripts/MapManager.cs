using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int blockSize = 8;
    [SerializeField] private int blockBuffer = 2;
    [SerializeField] private float mineDensity = 0.15f;
    
    public int BlockSize => blockSize;
    public int BlockBuffer => blockBuffer;
    public float MineDensity => mineDensity;
    
    private GameState gameState;
    private Board board;
    private System.Random random;

    public void Initialize(GameState state, Board gameBoard)
    {
        gameState = state;
        board = gameBoard;
        random = new System.Random();
    }

    public void InitializeBlock(Vector2Int blockCoord)
    {
        Debug.Log($"Initializing block at {blockCoord}");
        
        if (gameState.InitializedBlocks.ContainsKey(blockCoord))
        {
            Debug.Log($"Block {blockCoord} already initialized");
            return;
        }

        int startX = blockCoord.x * blockSize;
        int startY = blockCoord.y * blockSize;
        int endX = startX + blockSize - 1;
        int endY = startY + blockSize - 1;

        Debug.Log($"Block bounds: X({startX}-{endX}), Y({startY}-{endY})");

        int blockMineCount = Mathf.RoundToInt(blockSize * blockSize * mineDensity);
        blockMineCount = Mathf.Max(1, blockMineCount);
        Debug.Log($"Generating {blockMineCount} mines in this block");

        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!gameState.SafeZone.Contains(pos) && !gameState.Cells.ContainsKey(new Vector3Int(x, y, 0)))
                {
                    candidates.Add(pos);
                }
            }
        }

        Debug.Log($"Found {candidates.Count} valid positions for mines");

        HashSet<Vector2Int> minesInBlock = new HashSet<Vector2Int>();
        for (int i = 0; i < Mathf.Min(blockMineCount, candidates.Count); i++)
        {
            int index = random.Next(i, candidates.Count);
            Vector2Int temp = candidates[i];
            candidates[i] = candidates[index];
            candidates[index] = temp;

            Vector2Int minePos = candidates[i];
            minesInBlock.Add(minePos);

            Vector3Int position = new Vector3Int(minePos.x, minePos.y, 0);
            gameState.Cells[position] = new Cell(position, Cell.Type.Mine, board.tileMine);
        }

        gameState.BlockMinePositions[blockCoord] = minesInBlock;
        gameState.InitializedBlocks[blockCoord] = true;

        Debug.Log($"Block {blockCoord} initialized with {minesInBlock.Count} mines");

        CalculateNumbersInBlock(blockCoord);
        UpdateAdjacentBlocksNumbers(blockCoord);
    }

    private void CalculateNumbersInBlock(Vector2Int blockCoord)
    {
        int startX = blockCoord.x * blockSize;
        int startY = blockCoord.y * blockSize;
        int endX = startX + blockSize - 1;
        int endY = startY + blockSize - 1;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (!gameState.Cells.ContainsKey(pos) || gameState.Cells[pos].type != Cell.Type.Mine)
                {
                    int mines = CountAdjacentMines(pos);
                    Cell newCell;
                    if (mines > 0)
                    {
                        newCell = new Cell(pos, Cell.Type.Number, board.tileNumbers[mines]);
                        newCell.Number = mines;
                    }
                    else
                    {
                        newCell = new Cell(pos, Cell.Type.Empty, board.tileEmpty);
                    }
                    gameState.Cells[pos] = newCell;
                }
            }
        }
    }

    private int CountAdjacentMines(Vector3Int pos)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Vector3Int neighbor = new Vector3Int(pos.x + dx, pos.y + dy, 0);
                if (gameState.Cells.TryGetValue(neighbor, out Cell cell) && cell.type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private void UpdateAdjacentBlocksNumbers(Vector2Int blockCoord)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Vector2Int neighborCoord = new Vector2Int(blockCoord.x + dx, blockCoord.y + dy);
                if (gameState.InitializedBlocks.ContainsKey(neighborCoord))
                {
                    CalculateNumbersInBlock(neighborCoord);
                }
            }
        }
    }
} 