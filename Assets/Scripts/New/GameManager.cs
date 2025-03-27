using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [Header("Seed Settings")]
    #region Singleton
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        _instance = null;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        minesCreate.InitializeRandomSeed();
    }
    #endregion

    #region Game State Variables
    [Header("Game Settings")]
    [SerializeField] private int _mineCount = 10;
    [SerializeField] private int _zoneSize = 8;
    [SerializeField] private bool _useDynamicSeed = true;
    [SerializeField] private int _fixedSeed = 12345;

    [Header("References")]
    public Board board;
    public Sweep sweep;
    public TouchRespond touchRespond;
    public CameraController cameraController;
    public MinesCreate minesCreate;
    public ZoneManager zoneManager;

    private bool _gameOver = false;
    private bool _gameInitialized = false;
    private int _revealedCellsCount = 0;
    private int _flaggedMinesCount = 0;
    public int FixedSeed
    {
        get => _fixedSeed;
        set
        {
            _fixedSeed = value;
            minesCreate?.InitializeRandomSeed(); // 种子变化时重新初始化
        }
    }

    public int MineCount => _mineCount;
    public int ZoneSize => _zoneSize;
    public bool GameOver => _gameOver;
    public bool GameInitialized => _gameInitialized;

    public bool UseDynamicSeed
    {
        get => _useDynamicSeed;
        set
        {
            _useDynamicSeed = value;
            minesCreate?.InitializeRandomSeed(); // 种子变化时重新初始化
        }
    }
    #endregion

    #region Initialization
    private void Initialize()
    {
        if (board == null) board = FindObjectOfType<Board>();
        if (sweep == null) sweep = FindObjectOfType<Sweep>();
        if (touchRespond == null) touchRespond = FindObjectOfType<TouchRespond>();
        if (cameraController == null) cameraController = FindObjectOfType<CameraController>();
        if (minesCreate == null) minesCreate = FindObjectOfType<MinesCreate>();
        if (zoneManager == null) zoneManager = FindObjectOfType<ZoneManager>();

        // Initialize seed settings
        minesCreate.InitializeRandomSeed();

        Debug.Log("GameManager initialized");
    }
    #endregion

    #region Game Flow Control
    public void StartNewGame()
    {
        _gameOver = false;
        _gameInitialized = false;
        _revealedCellsCount = 0;
        _flaggedMinesCount = 0;

        // Clear existing game state
        sweep.CellStates.Clear();

        // Regenerate mines with current seed settings
        minesCreate.RegenerateAllMines();

        // Update UI/board
        board.Draw(sweep.CellStates);

        Debug.Log("New game started");
    }

    public void EndGame(bool win)
    {
        _gameOver = true;

        if (win)
        {
            Debug.Log("You win!");
            // Handle win condition
        }
        else
        {
            Debug.Log("Game Over");
            // Handle lose condition
        }
    }
    #endregion

    #region State Management
    public void CellRevealed(Vector2Int position)
    {
        _revealedCellsCount++;
        CheckWinCondition();
    }

    public void FlagPlaced(Vector2Int position, bool isMine)
    {
        _flaggedMinesCount += isMine ? 1 : -1;
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        // Calculate total safe cells (non-mine cells)
        int totalSafeCells = (zoneManager.GetAllZones().Count * _zoneSize * _zoneSize) - _mineCount;

        if (_revealedCellsCount >= totalSafeCells)
        {
            EndGame(true);
        }
    }

    public void SetGameInitialized(bool initialized)
    {
        _gameInitialized = initialized;
    }
    #endregion

    #region Helper Methods
    public Vector2Int GetZoneCoord(Vector2Int cellPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((float)cellPos.x / _zoneSize),
            Mathf.FloorToInt((float)cellPos.y / _zoneSize)
        );
    }

    public Vector2Int GetLocalCellPos(Vector2Int cellPos)
    {
        Vector2Int zoneCoord = GetZoneCoord(cellPos);
        return new Vector2Int(
            cellPos.x - zoneCoord.x * _zoneSize,
            cellPos.y - zoneCoord.y * _zoneSize
        );
    }
    #endregion
}