using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameOn : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 800; // 地图的宽度（单元格数量）
    public int mapHeight = 1600; // 地图的高度（单元格数量）

    private Board board; // 引用 Board 类

    private void Start()
    {
        // 获取 Board 组件的引用
        board = FindObjectOfType<Board>();
        InitializeMap();
    }
    private void InitializeMap()
    {
        var sw = new Stopwatch();
        sw.Start();
       var positionlist= new Vector3Int[mapWidth * mapHeight];
        var tilelist = new Tile[mapWidth * mapHeight];
        Array.Fill(tilelist, board.tileUnknown);
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                positionlist[y + x * mapWidth] = new Vector3Int(x, y,y);
                // 将 tileUnknown 绘制到 Tilemap 上
                /*  Vector3Int cellPosition = new Vector3Int(x, y, 0);
                  board.tilemap.SetTiles();
                  yield return null;*/
                // 暂时不对 Cell.type 赋值，后续在游戏逻辑中处理
            }
        }
        board.tilemap.SetTiles(positionlist, tilelist);
        UnityEngine.Debug.Log( sw.ElapsedMilliseconds);
    }
}