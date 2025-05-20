using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MinePatternManager", menuName = "MineSweeper/Mine Pattern Manager")]
public class MinePatternManager : ScriptableObject
{
    [Header("Pattern Collection")]
    public List<MinePattern> availablePatterns = new List<MinePattern>();

    private Dictionary<Vector2Int, MinePattern> activePatterns = new Dictionary<Vector2Int, MinePattern>();
    private Dictionary<Vector2Int, bool> flaggedPositions = new Dictionary<Vector2Int, bool>();

    // 初始化图案管理器
    public void Initialize()
    {
        activePatterns.Clear();
        flaggedPositions.Clear();
    }

    // 尝试在指定位置生成图案
    public bool TrySpawnPattern(Vector2Int position)
    {
        foreach (var pattern in availablePatterns)
        {
            if (Random.value <= pattern.spawnChance)
            {
                // 设置图案偏移
                pattern.patternOffset = position;
                
                // 检查该位置是否已经有图案
                if (!HasOverlappingPattern(pattern))
                {
                    activePatterns[position] = pattern;
                    return true;
                }
            }
        }
        return false;
    }

    // 检查是否有重叠的图案
    private bool HasOverlappingPattern(MinePattern newPattern)
    {
        foreach (var existingPattern in activePatterns.Values)
        {
            // 检查两个图案的范围是否重叠
            if (DoPatternsOverlap(newPattern, existingPattern))
            {
                return true;
            }
        }
        return false;
    }

    // 检查两个图案是否重叠
    private bool DoPatternsOverlap(MinePattern pattern1, MinePattern pattern2)
    {
        Vector2Int pattern1Min = pattern1.patternOffset;
        Vector2Int pattern1Max = pattern1.patternOffset + new Vector2Int(pattern1.patternWidth, pattern1.patternHeight);
        Vector2Int pattern2Min = pattern2.patternOffset;
        Vector2Int pattern2Max = pattern2.patternOffset + new Vector2Int(pattern2.patternWidth, pattern2.patternHeight);

        return !(pattern1Max.x < pattern2Min.x || pattern1Min.x > pattern2Max.x ||
                pattern1Max.y < pattern2Min.y || pattern1Min.y > pattern2Max.y);
    }

    // 更新插旗状态
    public void UpdateFlagPosition(Vector2Int position, bool isFlagged)
    {
        flaggedPositions[position] = isFlagged;
        CheckPatternCompletion();
    }

    // 检查图案完成状态
    private void CheckPatternCompletion()
    {
        List<Vector2Int> completedPatterns = new List<Vector2Int>();

        foreach (var kvp in activePatterns)
        {
            if (kvp.Value.IsPatternComplete(flaggedPositions))
            {
                completedPatterns.Add(kvp.Key);
                // 触发图案完成事件
                OnPatternCompleted(kvp.Value);
            }
        }

        // 移除已完成的图案
        foreach (var position in completedPatterns)
        {
            activePatterns.Remove(position);
        }
    }

    // 图案完成事件
    private void OnPatternCompleted(MinePattern pattern)
    {
        // 这里可以触发UI显示、音效等
        Debug.Log($"图案 {pattern.patternName} 已完成！获得 {pattern.bonusScore} 分！");
    }

    // 获取指定位置是否应该是地雷（考虑图案）
    public bool ShouldBeMine(Vector2Int position)
    {
        foreach (var pattern in activePatterns.Values)
        {
            // 计算相对于图案原点的位置
            Vector2Int relativePos = position - pattern.patternOffset;
            
            // 旋转180度
            Vector2Int rotatedPos = new Vector2Int(
                pattern.patternWidth - 1 - relativePos.x,
                pattern.patternHeight - 1 - relativePos.y
            );

            // 检查旋转后的位置是否在图案范围内
            if (rotatedPos.x >= 0 && rotatedPos.x < pattern.patternWidth &&
                rotatedPos.y >= 0 && rotatedPos.y < pattern.patternHeight)
            {
                // 检查该位置是否应该是地雷
                foreach (var cell in pattern.patternCells)
                {
                    if (cell.position == rotatedPos && cell.isMine)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // 获取指定位置的图案（如果有）
    public MinePattern GetPatternAtPosition(Vector2Int position)
    {
        foreach (var pattern in activePatterns.Values)
        {
            if (pattern.IsPositionInPattern(position))
            {
                return pattern;
            }
        }
        return null;
    }
} 