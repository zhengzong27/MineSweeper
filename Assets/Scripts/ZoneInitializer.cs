using System;
using System.Collections.Generic;
using UnityEngine;

public class ZoneInitializer
{
    private int CheckpointSize = 8; // 检查点大小
    private int mineCount; // 地雷数量
    private Cell[,] state; // 游戏地图状态

    public ZoneInitializer(Cell[,] state, int mineCount)
    {
        this.state = state;
        this.mineCount = mineCount;
    }

    public void CreateZone(Vector2Int checkpointCoord, bool IsFirst, Vector2Int Click)
    {
        // 检查点边界
        int startX = checkpointCoord.x * CheckpointSize;
        int startY = checkpointCoord.y * CheckpointSize;
        int endX = startX + CheckpointSize;
        int endY = startY + CheckpointSize;

        // 步骤1: 如果需要创建安全区域
        HashSet<Vector2Int> forbiddenArea = CreateForbiddenArea(IsFirst, Click, startX, endX, startY, endY);

        // 步骤2: 生成候选位置（仅限当前检查点）
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (!IsFirst || !forbiddenArea.Contains(new Vector2Int(x, y)))
                {
                    candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        // 步骤3: 随机布雷
        int mineCount = Mathf.Min(this.mineCount, candidates.Count);
        System.Random rng = new System.Random();
        for (int i = 0; i < mineCount; i++)
        {
            int index = rng.Next(i, candidates.Count);
            Vector2Int temp = candidates[i];
            candidates[i] = candidates[index];
            candidates[index] = temp;

            Vector2Int pos = candidates[i];
            state[pos.x, pos.y].type = Cell.Type.Mine;
        }

        // 步骤4: 计算数字（仅限当前检查点）
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (state[x, y].type != Cell.Type.Mine)
                {
                    int count = CountMines(x, y);
                    state[x, y].Number = count;
                    state[x, y].type = count > 0 ? Cell.Type.Number : Cell.Type.Empty;
                }
            }
        }
    }

    // 创建安全区域
    private HashSet<Vector2Int> CreateForbiddenArea(bool IsFirst, Vector2Int Click, int startX, int endX, int startY, int endY)
    {
        HashSet<Vector2Int> forbiddenArea = new HashSet<Vector2Int>();
        if (IsFirst)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int x = Mathf.Clamp(Click.x + dx, startX, endX - 1);
                    int y = Mathf.Clamp(Click.y + dy, startY, endY - 1);
                    forbiddenArea.Add(new Vector2Int(x, y));
                }
            }
        }
        return forbiddenArea;
    }

    // 计算周围地雷数量
    private int CountMines(int x, int y)
    {
        int mineCount = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < state.GetLength(0) && ny >= 0 && ny < state.GetLength(1))
                {
                    if (state[nx, ny].type == Cell.Type.Mine)
                    {
                        mineCount++;
                    }
                }
            }
        }
        return mineCount;
    }
}