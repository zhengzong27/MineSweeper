using System;
using System.Collections.Generic;
using UnityEngine;

public class MinesCreate : MonoBehaviour
{
    // 单例模式
    //​Fisher-Yates洗牌算法
    private static MinesCreate _instance;// 存储唯一实例的变量
    public static MinesCreate Instance => _instance;// 让其他代码可以访问这个实例

    [Header("地雷配置")]
    [SerializeField] private int mineCount = 10;// 每个区域放10颗地雷
    [SerializeField] private int regionSize = 8;// 每个区域是8x8的格子

    [Header("种子设置")]
    [SerializeField] private bool useDynamicSeed = true; // 是否使用动态种子
    private int _currentSeed;     // 固定种子（useDynamicSeed=false时生效）

    private Dictionary<Vector2Int, List<Vector2Int>> minePositions = new Dictionary<Vector2Int, List<Vector2Int>>();
    private System.Random random;

    private void Awake()
    {
        // 单例初始化
        if (_instance != null && _instance != this)
        {
            Destroy(this); // 只销毁组件而不是整个游戏对象
            return;
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // 如果需要持久化
        }

        InitializeRandomSeed();
    }

    // 初始化随机种子（动态或固定）
    public void InitializeRandomSeed()
    {
        if (useDynamicSeed)
        {
            // 动态种子：结合系统时间、硬件标识和Unity随机数
            int timeSeed = DateTime.Now.Millisecond;
            int deviceSeed = SystemInfo.deviceUniqueIdentifier.GetHashCode();
            int unityRandomSeed = UnityEngine.Random.Range(0, int.MaxValue);
            _currentSeed = timeSeed ^ deviceSeed ^ unityRandomSeed;
        }
        else
        {
            // 从GameManager获取固定种子
            _currentSeed = GameManager.Instance.GetFixedSeed();
        }

        random = new System.Random(_currentSeed);
        Debug.Log($"当前游戏种子: {_currentSeed} ({(useDynamicSeed ? "动态生成" : "固定")})");
    }

    // 重新生成所有地雷（切换种子时调用）
    public void RegenerateAllMines()
    {
        minePositions.Clear();// 清空所有记录的地雷位置
        InitializeRandomSeed(); // 重新初始化种子
    }

    // 为指定区域生成地雷（Fisher-Yates洗牌算法优化版）
    private void GenerateMinesForRegion(Vector2Int regionCoord)
    {
        int mineCount = GameManager.Instance.MineCount;
        int regionSize = GameManager.Instance.ZoneSize;
        // 1. 生成区域唯一种子（混合全局种子和区域坐标）
        int regionSeed = (regionCoord.x * 1000 + regionCoord.y) ^ _currentSeed;
        var regionRandom = new System.Random(regionSeed);

        // 2. 预生成所有候选位置
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int i = 0; i < regionSize * regionSize; i++)
            candidates.Add(new Vector2Int(i % regionSize, i / regionSize));

        // 3. 随机抽取mineCount个位置
        List<Vector2Int> mines = new List<Vector2Int>();
        for (int i = 0; i < mineCount && i < candidates.Count; i++)
        {
            int j = regionRandom.Next(i, candidates.Count);//随机选一个位置
            mines.Add(candidates[j]);//记录此位置会放雷
            candidates[j] = candidates[i]; // 交换位置，避免重复选择
        }

        minePositions[regionCoord] = mines;//记录雷的位置
    }
    public bool IsMineAt(Vector2Int cellPos)
    {
        return ZoneManager.Instance.IsMineAt(cellPos);
    }
    // 其他原有方法（IsMineAt、GetAdjacentMineCount等）保持不变...
}