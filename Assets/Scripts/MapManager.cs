using UnityEngine;
using System.Collections.Generic;
public class MapManager : MonoBehaviour
{
    private const int CheckpointSize = 8; // 检查点大小
    private const int ChunkSize = 40; // 块大小
    private const int CheckpointsPerChunk = ChunkSize / CheckpointSize; // 每个块包含的检查点数量

    private Cell[,] state; // 游戏地图状态
    private int mineCount = 10; // 地雷数量
    private ZoneInitializer zoneInitializer; // 区块初始化器

    private void Start()
    {
        // 初始化地图状态
        state = new Cell[100, 100]; // 示例地图大小
        zoneInitializer = new ZoneInitializer(state, mineCount);
    }

    // 主方法：检查并初始化区块
    public void CheckAndInitializeZone(Vector2Int cellPosition, bool IsFirst)
    {
        // 获取单元格所在的检查点坐标
        Vector2Int checkpointCoord = GetCheckpointCoord(cellPosition);

        // 获取当前检查点所在的区块范围
        Vector2Int[] checkpointsInChunk = GetCheckpointsInChunk(checkpointCoord);

        // 检查区块中的所有检查点是否已全部初始化
        bool needInitialization = false;
        foreach (var checkpoint in checkpointsInChunk)
        {
            if (!IsCheckpointInitialized(checkpoint))
            {
                needInitialization = true;
                break;
            }
        }

        // 如果需要初始化，则初始化区块中所有未初始化的检查点
        if (needInitialization)
        {
            foreach (var checkpoint in checkpointsInChunk)
            {
                if (!IsCheckpointInitialized(checkpoint))
                {
                    zoneInitializer.CreateZone(checkpoint, IsFirst, cellPosition);
                }
            }
        }
    }

    // 获取单元格所在的检查点坐标
    private Vector2Int GetCheckpointCoord(Vector2Int cellPosition)
    {
        return new Vector2Int(
            cellPosition.x / CheckpointSize,
            cellPosition.y / CheckpointSize
        );
    }

    // 获取当前检查点所在的区块中的所有检查点坐标
    private Vector2Int[] GetCheckpointsInChunk(Vector2Int checkpointCoord)
    {
        // 区块的起始检查点坐标
        int startX = (checkpointCoord.x / CheckpointsPerChunk) * CheckpointsPerChunk;
        int startY = (checkpointCoord.y / CheckpointsPerChunk) * CheckpointsPerChunk;

        // 区块中的所有检查点坐标
        List<Vector2Int> checkpoints = new List<Vector2Int>();
        for (int x = startX; x < startX + CheckpointsPerChunk; x++)
        {
            for (int y = startY; y < startY + CheckpointsPerChunk; y++)
            {
                checkpoints.Add(new Vector2Int(x, y));
            }
        }

        return checkpoints.ToArray();
    }

    // 检查检查点是否已初始化
    private bool IsCheckpointInitialized(Vector2Int checkpointCoord)
    {
        // 检查点边界
        int startX = checkpointCoord.x * CheckpointSize;
        int startY = checkpointCoord.y * CheckpointSize;
        int endX = startX + CheckpointSize;
        int endY = startY + CheckpointSize;

        // 遍历检查点内的所有单元格
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (state[x, y].type != Cell.Type.Empty)
                {
                    return true; // 如果发现非空单元格，说明已初始化
                }
            }
        }

        return false; // 所有单元格都是空的，说明未初始化
    }
}