using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public const int ZoneSize = 8; // 每个区块的大小（8x8）

    // 存储所有区块的地雷数据
    private Dictionary<Vector2Int, ZoneData> zoneDataDict = new Dictionary<Vector2Int, ZoneData>();

    // 区块数据类
    public class ZoneData
    {
        public Vector2Int zoneCoord; // 区块坐标（如 (0,0), (1,0) 等）
        public Dictionary<Vector2Int, bool> minePositions = new Dictionary<Vector2Int, bool>(); // 地雷位置
        public bool isLocked; // 是否被锁定（完成或失败）
    }

    private void Awake()
    {
        // 确保单例
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // 单例访问
    public static ZoneManager Instance { get; private set; }

    /// <summary>
    /// 获取单元格所在的区块坐标
    /// </summary>
    public Vector2Int GetZoneCoord(Vector2Int cellPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((float)cellPos.x / ZoneSize),
            Mathf.FloorToInt((float)cellPos.y / ZoneSize)
        );
    }

    /// <summary>
    /// 初始化或获取区块数据
    /// </summary>
    public ZoneData GetOrCreateZone(Vector2Int zoneCoord)
    {
        if (!zoneDataDict.TryGetValue(zoneCoord, out ZoneData zone))
        {
            zone = new ZoneData
            {
                zoneCoord = zoneCoord,
                isLocked = false
            };
            zoneDataDict[zoneCoord] = zone;

            // 首次访问时生成地雷
            GenerateMinesForZone(zone);
        }
        return zone;
    }

    /// <summary>
    /// 为区块生成地雷
    /// </summary>
    private void GenerateMinesForZone(ZoneData zone)
    {
        // 使用区块坐标生成唯一种子
        int seed = (zone.zoneCoord.x * 397) ^ (zone.zoneCoord.y * 991);
        System.Random random = new System.Random(seed);

        int mineCount = CalculateMineCountForZone(zone);

        // 生成地雷位置
        for (int i = 0; i < mineCount; i++)
        {
            Vector2Int minePos;
            int attempts = 0;
            do
            {
                minePos = new Vector2Int(
                    random.Next(0, ZoneSize),
                    random.Next(0, ZoneSize)
                );
                attempts++;
            } while (zone.minePositions.ContainsKey(minePos) && attempts < 100);

            if (attempts < 100)
            {
                zone.minePositions[minePos] = true;
            }
        }
    }

    /// <summary>
    /// 计算每个区块的地雷数量（可自定义规则）
    /// </summary>
    private int CalculateMineCountForZone(ZoneData zone)
    {
        // 示例：每个区块10%的单元格是地雷
        return Mathf.RoundToInt(ZoneSize * ZoneSize * 0.1f);
    }

    /// <summary>
    /// 检查指定单元格是否有地雷
    /// </summary>
    public bool IsMineAt(Vector2Int cellPos)
    {
        Vector2Int zoneCoord = GetZoneCoord(cellPos);
        Vector2Int localPos = GetLocalCellPos(cellPos);

        if (zoneDataDict.TryGetValue(zoneCoord, out ZoneData zone))
        {
            return zone.minePositions.ContainsKey(localPos);
        }
        return false;
    }

    /// <summary>
    /// 获取单元格在区块内的局部坐标
    /// </summary>
    private Vector2Int GetLocalCellPos(Vector2Int cellPos)
    {
        return new Vector2Int(
            (cellPos.x % ZoneSize + ZoneSize) % ZoneSize,
            (cellPos.y % ZoneSize + ZoneSize) % ZoneSize
        );
    }

    /// <summary>
    /// 检查区块是否应该被锁定
    /// </summary>
    public void CheckZoneLockState(Vector2Int cellPos)
    {
        Vector2Int zoneCoord = GetZoneCoord(cellPos);
        if (zoneDataDict.TryGetValue(zoneCoord, out ZoneData zone))
        {
            // 检查是否所有非雷单元格都已揭开
            bool allNonMinesRevealed = true;
            // 检查是否有雷被揭开
            bool mineRevealed = false;

            // 这里需要接入实际的单元格状态检查
            // 伪代码：
            // foreach (所有单元格 in 区块)
            // {
            //     if (是地雷 && 已揭开) mineRevealed = true;
            //     if (!是地雷 && !已揭开) allNonMinesRevealed = false;
            // }

            zone.isLocked = mineRevealed || allNonMinesRevealed;
        }
    }

    /// <summary>
    /// 检查区块是否被锁定
    /// </summary>
    public bool IsZoneLocked(Vector2Int cellPos)
    {
        Vector2Int zoneCoord = GetZoneCoord(cellPos);
        if (zoneDataDict.TryGetValue(zoneCoord, out ZoneData zone))
        {
            return zone.isLocked;
        }
        return false;
    }
}