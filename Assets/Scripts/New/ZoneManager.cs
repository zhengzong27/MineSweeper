using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public int ZoneSize => GameManager.Instance.ZoneSize;

    [Header("Debug Settings")]
    public bool drawZoneGizmos = true;
    public Color zoneGizmoColor = new Color(0, 1, 1, 0.3f);

    // 存储所有区块的地雷数据
    private Dictionary<Vector2Int, ZoneData> zoneDataDict = new Dictionary<Vector2Int, ZoneData>();

    #region Singleton
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        Instance = null;
    }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            Debug.Log("ZoneManager 实例已创建");
        }
    }

    public static ZoneManager Instance { get; private set; }
    #endregion

    #region Zone Data Structure
    [System.Serializable]
    public class ZoneData
    {
        public Vector2Int zoneCoord;
        public Dictionary<Vector2Int, bool> minePositions = new Dictionary<Vector2Int, bool>();
        public bool isLocked;

        // 添加更多辅助方法
        public bool ContainsMine(Vector2Int localPos)
        {
            return minePositions.ContainsKey(localPos);
        }
    }
    #endregion

    #region Public API
    public Dictionary<Vector2Int, ZoneData> GetAllZones()
    {
        return new Dictionary<Vector2Int, ZoneData>(zoneDataDict);
    }

    public Vector2Int GetZoneCoord(Vector2Int cellPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((float)cellPos.x / ZoneSize),
            Mathf.FloorToInt((float)cellPos.y / ZoneSize)
        );
    }

    public ZoneData GetZoneData(Vector2Int zoneCoord)
    {
        zoneDataDict.TryGetValue(zoneCoord, out ZoneData zone);
        return zone;
    }

    public bool IsMineAt(Vector2Int cellPos)
    {
        Vector2Int zoneCoord = GetZoneCoord(cellPos);
        Vector2Int localPos = GetLocalCellPos(cellPos);

        var zone = GetZoneData(zoneCoord);
        return zone?.ContainsMine(localPos) ?? false;
    }
    #endregion

    #region Zone Generation
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
            GenerateMinesForZone(zone);
        }
        return zone;
    }

    private void GenerateMinesForZone(ZoneData zone)
    {
        int seed = (zone.zoneCoord.x * 397) ^ (zone.zoneCoord.y * 991);
        System.Random random = new System.Random(seed);

        int mineCount = CalculateMineCountForZone(zone);

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

    private int CalculateMineCountForZone(ZoneData zone)
    {
        return Mathf.RoundToInt(ZoneSize * ZoneSize * 0.1f);
    }
    #endregion

    #region Helper Methods
    private Vector2Int GetLocalCellPos(Vector2Int cellPos)
    {
        return new Vector2Int(
            (cellPos.x % ZoneSize + ZoneSize) % ZoneSize,
            (cellPos.y % ZoneSize + ZoneSize) % ZoneSize
        );
    }
    #endregion

    #region Gizmos Drawing
    void OnDrawGizmos()
    {
        if (!drawZoneGizmos || zoneDataDict == null) return;

        foreach (var zone in zoneDataDict.Keys)
        {
            DrawZoneGizmo(zone);
        }
    }

    private void DrawZoneGizmo(Vector2Int zoneCoord)
    {
        Vector3 center = new Vector3(
            zoneCoord.x * ZoneSize + ZoneSize / 2f,
            zoneCoord.y * ZoneSize + ZoneSize / 2f,
            0
        );

        // 绘制填充区域
        Gizmos.color = zoneGizmoColor;
        Gizmos.DrawCube(center, new Vector3(ZoneSize, ZoneSize, 0.1f));

        // 绘制边框
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(center, new Vector3(ZoneSize, ZoneSize, 0.1f));

        // 显示区块坐标
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            center - new Vector3(ZoneSize / 2f, ZoneSize / 2f, 0),
            $"Zone {zoneCoord.x},{zoneCoord.y}",
            new GUIStyle()
            {
                normal = { textColor = Color.black },
                fontSize = 10,
                alignment = TextAnchor.UpperLeft
            }
        );
#endif
    }
    #endregion
}