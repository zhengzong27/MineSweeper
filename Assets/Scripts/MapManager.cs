using UnityEngine;
using System.Collections.Generic;
public class MapManager : MonoBehaviour
{
    private const int CheckpointSize = 8; // �����С
    private const int ChunkSize = 40; // ���С
    private const int CheckpointsPerChunk = ChunkSize / CheckpointSize; // ÿ��������ļ�������

    private Cell[,] state; // ��Ϸ��ͼ״̬
    private int mineCount = 10; // ��������
    private ZoneInitializer zoneInitializer; // �����ʼ����

    private void Start()
    {
        // ��ʼ����ͼ״̬
        state = new Cell[100, 100]; // ʾ����ͼ��С
        zoneInitializer = new ZoneInitializer(state, mineCount);
    }

    // ����������鲢��ʼ������
    public void CheckAndInitializeZone(Vector2Int cellPosition, bool IsFirst)
    {
        // ��ȡ��Ԫ�����ڵļ�������
        Vector2Int checkpointCoord = GetCheckpointCoord(cellPosition);

        // ��ȡ��ǰ�������ڵ����鷶Χ
        Vector2Int[] checkpointsInChunk = GetCheckpointsInChunk(checkpointCoord);

        // ��������е����м����Ƿ���ȫ����ʼ��
        bool needInitialization = false;
        foreach (var checkpoint in checkpointsInChunk)
        {
            if (!IsCheckpointInitialized(checkpoint))
            {
                needInitialization = true;
                break;
            }
        }

        // �����Ҫ��ʼ�������ʼ������������δ��ʼ���ļ���
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

    // ��ȡ��Ԫ�����ڵļ�������
    private Vector2Int GetCheckpointCoord(Vector2Int cellPosition)
    {
        return new Vector2Int(
            cellPosition.x / CheckpointSize,
            cellPosition.y / CheckpointSize
        );
    }

    // ��ȡ��ǰ�������ڵ������е����м�������
    private Vector2Int[] GetCheckpointsInChunk(Vector2Int checkpointCoord)
    {
        // �������ʼ��������
        int startX = (checkpointCoord.x / CheckpointsPerChunk) * CheckpointsPerChunk;
        int startY = (checkpointCoord.y / CheckpointsPerChunk) * CheckpointsPerChunk;

        // �����е����м�������
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

    // �������Ƿ��ѳ�ʼ��
    private bool IsCheckpointInitialized(Vector2Int checkpointCoord)
    {
        // ����߽�
        int startX = checkpointCoord.x * CheckpointSize;
        int startY = checkpointCoord.y * CheckpointSize;
        int endX = startX + CheckpointSize;
        int endY = startY + CheckpointSize;

        // ���������ڵ����е�Ԫ��
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (state[x, y].type != Cell.Type.Empty)
                {
                    return true; // ������ַǿյ�Ԫ��˵���ѳ�ʼ��
                }
            }
        }

        return false; // ���е�Ԫ���ǿյģ�˵��δ��ʼ��
    }
}