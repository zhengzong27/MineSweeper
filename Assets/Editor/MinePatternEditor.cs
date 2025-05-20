using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MinePattern))]
public class MinePatternEditor : Editor
{
    private bool[,] patternGrid;
    private Vector2 scrollPosition;
    private float cellSize = 20f;
    private Color gridColor = new Color(0.3f, 0.3f, 0.3f);
    private Color mineColor = new Color(1f, 0.3f, 0.3f);
    private Color hoverColor = new Color(0.7f, 0.7f, 0.7f);
    private Vector2Int hoveredCell = new Vector2Int(-1, -1);

    private void OnEnable()
    {
        MinePattern pattern = (MinePattern)target;
        patternGrid = new bool[pattern.patternWidth, pattern.patternHeight];
        
        // 从现有数据初始化网格
        foreach (var cell in pattern.patternCells)
        {
            if (cell.position.x >= 0 && cell.position.x < pattern.patternWidth &&
                cell.position.y >= 0 && cell.position.y < pattern.patternHeight)
            {
                patternGrid[cell.position.x, cell.position.y] = cell.isMine;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        MinePattern pattern = (MinePattern)target;

        // 绘制默认的Inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("图案编辑器", EditorStyles.boldLabel);

        // 绘制网格
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // 计算网格区域
        float gridWidth = pattern.patternWidth * cellSize;
        float gridHeight = pattern.patternHeight * cellSize;
        Rect gridRect = GUILayoutUtility.GetRect(gridWidth, gridHeight);

        // 绘制背景
        EditorGUI.DrawRect(gridRect, Color.white);

        // 绘制网格线
        for (int x = 0; x <= pattern.patternWidth; x++)
        {
            float xPos = gridRect.x + x * cellSize;
            EditorGUI.DrawRect(new Rect(xPos, gridRect.y, 1, gridHeight), gridColor);
        }
        for (int y = 0; y <= pattern.patternHeight; y++)
        {
            float yPos = gridRect.y + y * cellSize;
            EditorGUI.DrawRect(new Rect(gridRect.x, yPos, gridWidth, 1), gridColor);
        }

        // 处理鼠标事件
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && gridRect.Contains(e.mousePosition))
        {
            int x = Mathf.FloorToInt((e.mousePosition.x - gridRect.x) / cellSize);
            int y = Mathf.FloorToInt((e.mousePosition.y - gridRect.y) / cellSize);
            
            if (x >= 0 && x < pattern.patternWidth && y >= 0 && y < pattern.patternHeight)
            {
                patternGrid[x, y] = !patternGrid[x, y];
                UpdatePatternCells(pattern);
                e.Use();
            }
        }

        // 更新悬停的单元格
        if (gridRect.Contains(e.mousePosition))
        {
            int x = Mathf.FloorToInt((e.mousePosition.x - gridRect.x) / cellSize);
            int y = Mathf.FloorToInt((e.mousePosition.y - gridRect.y) / cellSize);
            
            if (x >= 0 && x < pattern.patternWidth && y >= 0 && y < pattern.patternHeight)
            {
                hoveredCell = new Vector2Int(x, y);
            }
            else
            {
                hoveredCell = new Vector2Int(-1, -1);
            }
            Repaint();
        }

        // 绘制地雷和悬停效果
        for (int x = 0; x < pattern.patternWidth; x++)
        {
            for (int y = 0; y < pattern.patternHeight; y++)
            {
                Rect cellRect = new Rect(
                    gridRect.x + x * cellSize,
                    gridRect.y + y * cellSize,
                    cellSize,
                    cellSize
                );

                if (patternGrid[x, y])
                {
                    EditorGUI.DrawRect(cellRect, mineColor);
                }
                else if (hoveredCell.x == x && hoveredCell.y == y)
                {
                    EditorGUI.DrawRect(cellRect, hoverColor);
                }
            }
        }

        EditorGUILayout.EndScrollView();

        // 添加清除按钮
        if (GUILayout.Button("清除图案"))
        {
            patternGrid = new bool[pattern.patternWidth, pattern.patternHeight];
            UpdatePatternCells(pattern);
        }
    }

    private void UpdatePatternCells(MinePattern pattern)
    {
        pattern.patternCells.Clear();
        for (int x = 0; x < pattern.patternWidth; x++)
        {
            for (int y = 0; y < pattern.patternHeight; y++)
            {
                if (patternGrid[x, y])
                {
                    pattern.patternCells.Add(new MinePattern.PatternCell
                    {
                        position = new Vector2Int(x, y),
                        isMine = true
                    });
                }
            }
        }
        EditorUtility.SetDirty(pattern);
    }
} 