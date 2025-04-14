using UnityEngine;
using System.Collections.Generic;

public class GameState
{
    public bool IsInitialized { get; private set; }
    public bool IsGameOver { get; private set; }
    public Dictionary<Vector3Int, Cell> Cells { get; private set; }
    public HashSet<Vector2Int> SafeZone { get; private set; }
    public Dictionary<Vector2Int, bool> InitializedBlocks { get; private set; }
    public Dictionary<Vector2Int, HashSet<Vector2Int>> BlockMinePositions { get; private set; }

    public GameState()
    {
        Reset();
    }

    public void Reset()
    {
        IsInitialized = false;
        IsGameOver = false;
        Cells = new Dictionary<Vector3Int, Cell>();
        SafeZone = new HashSet<Vector2Int>();
        InitializedBlocks = new Dictionary<Vector2Int, bool>();
        BlockMinePositions = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    }

    public void SetInitialized(bool value) => IsInitialized = value;
    public void SetGameOver(bool value) => IsGameOver = value;
} 