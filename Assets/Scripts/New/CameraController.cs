using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 0.01f;
    private Vector2 touchStartPos;
    private bool isDragging = false;
    private float debugInterval = 1f;
    private float debugTimer = 0f;

    // GameManager获取Board
    private Board board => GameManager.Instance.board;
    public int visibleWidth = 10;
    public int visibleHeight = 10;
    private Vector3Int lastCameraCellPos;

    // 新增：存储当前可见的格子
    private HashSet<Vector3Int> currentlyVisibleCells = new HashSet<Vector3Int>();

    void Start()
    {
        //移除手动查找Board
        debugTimer = debugInterval;
        lastCameraCellPos = GetCurrentCameraCellPosition();
        UpdateMapAroundCamera();
    }

    void Update()
    {
        HandleTouchInput();
        DebugCameraPosition();
        CheckCameraMovementForMapUpdate();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    isDragging = true;
                    break;
                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 touchDelta = touch.position - touchStartPos;
                        transform.position -= new Vector3(touchDelta.x * moveSpeed, touchDelta.y * moveSpeed, 0);
                        touchStartPos = touch.position;
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
    }

    void DebugCameraPosition()
    {
        debugTimer -= Time.deltaTime;
        if (debugTimer <= 0f)
        {
            debugTimer = debugInterval;
            Debug.Log("Camera position: " + transform.position);
        }
    }

    Vector3Int GetCurrentCameraCellPosition()
    {
        Vector3 worldPos = transform.position;
        return board.tilemap.WorldToCell(worldPos);
    }

    void CheckCameraMovementForMapUpdate()
    {
        Vector3Int currentCellPos = GetCurrentCameraCellPosition();
        if (currentCellPos != lastCameraCellPos)
        {
            UpdateMapAroundCamera();
            lastCameraCellPos = currentCellPos;
        }
    }

    // 修改：优化地图更新，卸载视野外的格子
    void UpdateMapAroundCamera()
    {
        Vector3Int cameraCellPos = GetCurrentCameraCellPosition();

        // 计算新的可见区域
        int startX = cameraCellPos.x - visibleWidth / 2;
        int endX = cameraCellPos.x + visibleWidth / 2;
        int startY = cameraCellPos.y - visibleHeight / 2;
        int endY = cameraCellPos.y + visibleHeight / 2;

        // 存储新的可见格子
        HashSet<Vector3Int> newVisibleCells = new HashSet<Vector3Int>();

        // 遍历新视野内的所有格子
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                newVisibleCells.Add(cellPos);
            }
        }

        // 找出不再可见的格子，并卸载
        HashSet<Vector3Int> cellsToRemove = new HashSet<Vector3Int>(currentlyVisibleCells);
        cellsToRemove.ExceptWith(newVisibleCells); // 只保留需要移除的格子

        foreach (Vector3Int cell in cellsToRemove)
        {
            board.ClearTile(cell); // 调用 Board 的方法卸载格子
        }

        // 找出新增的格子，并加载
        HashSet<Vector3Int> cellsToAdd = new HashSet<Vector3Int>(newVisibleCells);
        cellsToAdd.ExceptWith(currentlyVisibleCells); // 只保留需要新增的格子

        foreach (Vector3Int cell in cellsToAdd)
        {
            board.GenerateTile(cell); // 调用 Board 的方法生成格子
        }

        // 更新当前可见的格子
        currentlyVisibleCells = newVisibleCells;

        Debug.Log($"Updated map. Visible area: X[{startX},{endX}], Y[{startY},{endY}]");
    }
}