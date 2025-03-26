// ZoneClickDetectorEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ZoneClickDetector))]
public class ZoneClickDetectorEditor : Editor
{
    void OnSceneGUI()
    {
        // 安全检测
        if (ZoneManager.Instance == null)
        {
            var manager = FindObjectOfType<ZoneManager>();
            if (manager == null)
            {
                Debug.LogWarning("场景中未找到 ZoneManager");
                return;
            }
        }

        // 原始点击检测代码...
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 worldPos = ray.origin;
            worldPos.z = 0;

            Vector2Int cellPos = new Vector2Int(
                Mathf.FloorToInt(worldPos.x),
                Mathf.FloorToInt(worldPos.y)
            );

            // 添加空引用检查
            if (ZoneManager.Instance != null)
            {
                Vector2Int zoneCoord = ZoneManager.Instance.GetZoneCoord(cellPos);
                Handles.Label(worldPos, $"区块: {zoneCoord}",
                    new GUIStyle { normal = { textColor = Color.green } });

                SceneView.RepaintAll();
                e.Use();
            }
        }
    }
}
#endif