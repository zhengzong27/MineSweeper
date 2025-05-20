using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMinePattern", menuName = "MineSweeper/Mine Pattern")]
public class MinePattern : ScriptableObject
{
    [System.Serializable]
    public class PatternCell
    {
        public Vector2Int position; // 相对于图案左上角的位置
        public bool isMine; // 该位置是否应该是地雷
    }

    [Header("Pattern Settings")]
    public string patternName; // 图案名称
    public Sprite patternPreview; // 图案预览图
    public int patternWidth = 16; // 图案宽度
    public int patternHeight = 16; // 图案高度
    public float spawnChance = 0.1f; // 生成概率 (0-1)
    public int bonusScore = 1000; // 完成图案获得的额外分数

    [Header("Pattern Data")]
    public List<PatternCell> patternCells = new List<PatternCell>(); // 图案单元格数据

    [Header("Pattern Position")]
    public Vector2Int patternOffset; // 图案在游戏中的偏移位置

    // 检查指定位置是否在图案范围内
    public bool IsPositionInPattern(Vector2Int position)
    {
        Vector2Int relativePos = position - patternOffset;
        return relativePos.x >= 0 && relativePos.x < patternWidth &&
               relativePos.y >= 0 && relativePos.y < patternHeight;
    }

    // 获取指定位置是否应该是地雷
    public bool ShouldBeMine(Vector2Int position)
    {
        Vector2Int relativePos = position - patternOffset;
        if (!IsPositionInPattern(position)) return false;

        foreach (var cell in patternCells)
        {
            if (cell.position == relativePos)
            {
                return cell.isMine;
            }
        }
        return false;
    }

    // 检查图案是否完成（所有地雷位置都已插旗）
    public bool IsPatternComplete(Dictionary<Vector2Int, bool> flaggedPositions)
    {
        foreach (var cell in patternCells)
        {
            if (cell.isMine)
            {
                Vector2Int worldPos = cell.position + patternOffset;
                if (!flaggedPositions.ContainsKey(worldPos) || !flaggedPositions[worldPos])
                {
                    return false;
                }
            }
        }
        return true;
    }

    // 获取图案中所有地雷位置
    public List<Vector2Int> GetAllMinePositions()
    {
        List<Vector2Int> minePositions = new List<Vector2Int>();
        foreach (var cell in patternCells)
        {
            if (cell.isMine)
            {
                minePositions.Add(cell.position + patternOffset);
            }
        }
        return minePositions;
    }
} 