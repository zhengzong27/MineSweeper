using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameOn : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 800; // ��ͼ�Ŀ�ȣ���Ԫ��������
    public int mapHeight = 1600; // ��ͼ�ĸ߶ȣ���Ԫ��������

    private Board board; // ���� Board ��

    private void Start()
    {
        // ��ȡ Board ���������
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
                // �� tileUnknown ���Ƶ� Tilemap ��
                /*  Vector3Int cellPosition = new Vector3Int(x, y, 0);
                  board.tilemap.SetTiles();
                  yield return null;*/
                // ��ʱ���� Cell.type ��ֵ����������Ϸ�߼��д���
            }
        }
        board.tilemap.SetTiles(positionlist, tilelist);
        UnityEngine.Debug.Log( sw.ElapsedMilliseconds);
    }
}