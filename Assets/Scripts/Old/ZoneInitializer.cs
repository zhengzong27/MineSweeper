using System;
using System.Collections.Generic;
using UnityEngine;

public class ZoneInitializer
{
    private int CheckpointSize = 8; // �����С
    private int mineCount; // ��������
    private Cell[,] state; // ��Ϸ��ͼ״̬

    public ZoneInitializer(Cell[,] state, int mineCount)
    {
        this.state = state;
        this.mineCount = mineCount;
    }

    public void CreateZone(Vector2Int checkpointCoord, bool IsFirst, Vector2Int Click)
    {
        // ����߽�
        int startX = checkpointCoord.x * CheckpointSize;
        int startY = checkpointCoord.y * CheckpointSize;
        int endX = startX + CheckpointSize;
        int endY = startY + CheckpointSize;

        // ����1: �����Ҫ������ȫ����
        HashSet<Vector2Int> forbiddenArea = CreateForbiddenArea(IsFirst, Click, startX, endX, startY, endY);

        // ����2: ���ɺ�ѡλ�ã����޵�ǰ���㣩
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

        // ����3: �������
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

        // ����4: �������֣����޵�ǰ���㣩
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

    // ������ȫ����
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

    // ������Χ��������
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