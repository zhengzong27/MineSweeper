using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public const int ZoneSize = 8; // ÿ������Ĵ�С��8x8��

    // �洢��������ĵ�������
    private Dictionary<Vector2Int, ZoneData> zoneDataDict = new Dictionary<Vector2Int, ZoneData>();

    // ����������
    public class ZoneData
    {
        public Vector2Int zoneCoord; // �������꣨�� (0,0), (1,0) �ȣ�
        public Dictionary<Vector2Int, bool> minePositions = new Dictionary<Vector2Int, bool>(); // ����λ��
        public bool isLocked; // �Ƿ���������ɻ�ʧ�ܣ�
    }

    private void Awake()
    {
        // ȷ������
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // ��������
    public static ZoneManager Instance { get; private set; }

    /// <summary>
    /// ��ȡ��Ԫ�����ڵ���������
    /// </summary>
    public Vector2Int GetZoneCoord(Vector2Int cellPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((float)cellPos.x / ZoneSize),
            Mathf.FloorToInt((float)cellPos.y / ZoneSize)
        );
    }

    /// <summary>
    /// ��ʼ�����ȡ��������
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

            // �״η���ʱ���ɵ���
            GenerateMinesForZone(zone);
        }
        return zone;
    }

    /// <summary>
    /// Ϊ�������ɵ���
    /// </summary>
    private void GenerateMinesForZone(ZoneData zone)
    {
        // ʹ��������������Ψһ����
        int seed = (zone.zoneCoord.x * 397) ^ (zone.zoneCoord.y * 991);
        System.Random random = new System.Random(seed);

        int mineCount = CalculateMineCountForZone(zone);

        // ���ɵ���λ��
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
    /// ����ÿ������ĵ������������Զ������
    /// </summary>
    private int CalculateMineCountForZone(ZoneData zone)
    {
        // ʾ����ÿ������10%�ĵ�Ԫ���ǵ���
        return Mathf.RoundToInt(ZoneSize * ZoneSize * 0.1f);
    }

    /// <summary>
    /// ���ָ����Ԫ���Ƿ��е���
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
    /// ��ȡ��Ԫ���������ڵľֲ�����
    /// </summary>
    private Vector2Int GetLocalCellPos(Vector2Int cellPos)
    {
        return new Vector2Int(
            (cellPos.x % ZoneSize + ZoneSize) % ZoneSize,
            (cellPos.y % ZoneSize + ZoneSize) % ZoneSize
        );
    }

    /// <summary>
    /// ��������Ƿ�Ӧ�ñ�����
    /// </summary>
    public void CheckZoneLockState(Vector2Int cellPos)
    {
        Vector2Int zoneCoord = GetZoneCoord(cellPos);
        if (zoneDataDict.TryGetValue(zoneCoord, out ZoneData zone))
        {
            // ����Ƿ����з��׵�Ԫ���ѽҿ�
            bool allNonMinesRevealed = true;
            // ����Ƿ����ױ��ҿ�
            bool mineRevealed = false;

            // ������Ҫ����ʵ�ʵĵ�Ԫ��״̬���
            // α���룺
            // foreach (���е�Ԫ�� in ����)
            // {
            //     if (�ǵ��� && �ѽҿ�) mineRevealed = true;
            //     if (!�ǵ��� && !�ѽҿ�) allNonMinesRevealed = false;
            // }

            zone.isLocked = mineRevealed || allNonMinesRevealed;
        }
    }

    /// <summary>
    /// ��������Ƿ�����
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