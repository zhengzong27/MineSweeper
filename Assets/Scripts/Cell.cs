using UnityEngine;

public struct Cell //单元格类
{
    public enum Type//三种类型
    {
        Empty,
        Mine,
        Number
    }
    //以下为其属性
    public Vector3Int position;//位置
    public Type type;//类型
    public int Number;//提示值量
    public bool revealed;//是否揭开
    public bool flagged;//是否被标记
    public bool exploded;//是否爆炸
    }
