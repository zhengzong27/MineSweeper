// ZoneClickDetector.cs
using UnityEngine;

public class ZoneClickDetector : MonoBehaviour
{
    [Header("调试设置")]
    public bool showDebugLog = true;
    public Color zoneGizmoColor = Color.cyan;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectClickedZone();
        }
    }

    public void DetectClickedZone()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int cellPos = new Vector2Int(
            Mathf.FloorToInt(worldPos.x),
            Mathf.FloorToInt(worldPos.y)
        );

        Vector2Int zoneCoord = ZoneManager.Instance.GetZoneCoord(cellPos);
        Debug.Log($"点击位置: {cellPos} → 区块: {zoneCoord}");
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = zoneGizmoColor;
        Vector2Int currentZone = ZoneManager.Instance.GetZoneCoord(
            new Vector2Int(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y)
            )
        );

        Vector3 center = new Vector3(
    currentZone.x * ZoneManager.Instance.ZoneSize + ZoneManager.Instance.ZoneSize / 2f,
    currentZone.y * ZoneManager.Instance.ZoneSize + ZoneManager.Instance.ZoneSize / 2f,0);
        Gizmos.DrawWireCube(center, new Vector3(ZoneManager.Instance.ZoneSize, ZoneManager.Instance.ZoneSize, 0.1f));
    }
}