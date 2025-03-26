// ZoneClickDetectorEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ZoneClickDetector))]
public class ZoneClickDetectorEditor : Editor
{
    void OnSceneGUI()
    {
        // ��ȫ���
        if (ZoneManager.Instance == null)
        {
            var manager = FindObjectOfType<ZoneManager>();
            if (manager == null)
            {
                Debug.LogWarning("������δ�ҵ� ZoneManager");
                return;
            }
        }

        // ԭʼ���������...
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

            // ��ӿ����ü��
            if (ZoneManager.Instance != null)
            {
                Vector2Int zoneCoord = ZoneManager.Instance.GetZoneCoord(cellPos);
                Handles.Label(worldPos, $"����: {zoneCoord}",
                    new GUIStyle { normal = { textColor = Color.green } });

                SceneView.RepaintAll();
                e.Use();
            }
        }
    }
}
#endif