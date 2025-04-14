using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField] private GameObject circle;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Button restartButton;
    [SerializeField] private Board board;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private InputHandler inputHandler;

    [Header("Game Settings")]
    [SerializeField] private int viewportWidth = 8;
    [SerializeField] private int viewportHeight = 16;
    [SerializeField] private int bufferSize = 3;

    private GameState gameState;
    private Vector2Int lastCameraCellPosition;

    private void Awake()
    {
        Debug.Log("Game Awake called");
        
        if (board == null)
        {
            board = GetComponentInChildren<Board>();
            Debug.Log($"Board found: {board != null}");
        }
        
        if (mapManager == null)
        {
            mapManager = GetComponent<MapManager>();
            Debug.Log($"MapManager found: {mapManager != null}");
        }
        
        if (inputHandler == null)
        {
            inputHandler = GetComponent<InputHandler>();
            Debug.Log($"InputHandler found: {inputHandler != null}");
        }

        gameState = new GameState();
        mapManager.Initialize(gameState, board);
        restartButton.onClick.AddListener(RestartGame);
        lastCameraCellPosition = new Vector2Int(int.MinValue, int.MinValue);
        
        Debug.Log("Game initialization completed");
    }

    private void Start()
    {
        Debug.Log("Game Start called");
        NewGame();
    }

    private void Update()
    {
        if (gameState.IsGameOver) return;

        UpdateDynamicMap();
        HandleInput();
    }

    private void NewGame()
    {
        circle.SetActive(false);
        gameState.Reset();
        restartButton.gameObject.SetActive(false);
        Camera.main.transform.position = new Vector3(0, 0, -10f);
        lastCameraCellPosition = new Vector2Int(int.MinValue, int.MinValue);
    }

    private void UpdateDynamicMap()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector2Int currentCellPosition = new Vector2Int(
            Mathf.FloorToInt(cameraPosition.x),
            Mathf.FloorToInt(cameraPosition.y)
        );

        if (currentCellPosition != lastCameraCellPosition)
        {
            Debug.Log($"Loading blocks around position: {currentCellPosition}");
            LoadVisibleBlocks(currentCellPosition);
            lastCameraCellPosition = currentCellPosition;
        }
    }

    private void LoadVisibleBlocks(Vector2Int centerCell)
    {
        int startX = centerCell.x - viewportWidth / 2 - bufferSize;
        int startY = centerCell.y - viewportHeight / 2 - bufferSize;
        int endX = centerCell.x + viewportWidth / 2 + bufferSize;
        int endY = centerCell.y + viewportHeight / 2 + bufferSize;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector2Int blockCoord = new Vector2Int(
                    Mathf.FloorToInt(x / (float)mapManager.BlockSize),
                    Mathf.FloorToInt(y / (float)mapManager.BlockSize)
                );
                mapManager.InitializeBlock(blockCoord);
            }
        }
    }

    private void HandleInput()
    {
        if (!inputHandler.IsTouching) return;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(inputHandler.TouchPosition);
        Vector3Int cellPosition = new Vector3Int(
            Mathf.FloorToInt(worldPosition.x),
            Mathf.FloorToInt(worldPosition.y),
            0
        );

        if (inputHandler.TouchDuration > 0.5f)
        {
            HandleLongPress(cellPosition);
        }
        else if (inputHandler.CurrentSwipeDirection != InputHandler.SwipeDirection.None)
        {
            HandleSwipe(cellPosition);
        }
    }

    private void HandleLongPress(Vector3Int cellPosition)
    {
        if (!gameState.Cells.ContainsKey(cellPosition)) return;

        Cell cell = gameState.Cells[cellPosition];
        if (!cell.revealed)
        {
            if (!cell.flagged && !cell.questioned)
            {
                cell.flagged = true;
                board.DrawCell(cellPosition, cell);
            }
            else if (cell.flagged)
            {
                cell.flagged = false;
                cell.questioned = true;
                board.DrawCell(cellPosition, cell);
            }
            else
            {
                cell.questioned = false;
                board.DrawCell(cellPosition, cell);
            }
        }
    }

    private void HandleSwipe(Vector3Int cellPosition)
    {
        if (!gameState.Cells.ContainsKey(cellPosition)) return;

        Cell cell = gameState.Cells[cellPosition];
        if (cell.revealed && cell.type == Cell.Type.Number)
        {
            RevealAdjacentCells(cellPosition);
        }
    }

    private void RevealAdjacentCells(Vector3Int position)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector3Int neighbor = new Vector3Int(position.x + dx, position.y + dy, 0);
                if (gameState.Cells.TryGetValue(neighbor, out Cell cell) && !cell.revealed)
                {
                    RevealCell(neighbor);
                }
            }
        }
    }

    private void RevealCell(Vector3Int position)
    {
        if (!gameState.Cells.TryGetValue(position, out Cell cell)) return;

        cell.revealed = true;
        gameState.Cells[position] = cell;

        if (cell.type == Cell.Type.Mine)
        {
            GameOver();
            return;
        }

        board.DrawCell(position, cell);

        if (cell.type == Cell.Type.Empty)
        {
            RevealAdjacentCells(position);
        }
    }

    private void GameOver()
    {
        gameState.SetGameOver(true);
        restartButton.gameObject.SetActive(true);
        // 显示所有地雷
        var cells = new List<Cell>(gameState.Cells.Values);
        foreach (var cell in cells)
        {
            if (cell.type == Cell.Type.Mine)
            {
                Cell revealedCell = cell;
                revealedCell.revealed = true;
                gameState.Cells[cell.position] = revealedCell;
                board.DrawCell(cell.position, revealedCell);
            }
        }
    }

    private void RestartGame()
    {
        board.ClearAllTiles();
        NewGame();
    }
}


