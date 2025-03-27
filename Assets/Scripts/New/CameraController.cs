using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 0.01f;
    private Vector2 touchStartPos;
    private bool isDragging = false;
    private float debugInterval = 1f;
    private float debugTimer = 0f;

    // GameManager��ȡBoard
    private Board board => GameManager.Instance.board;
    public int visibleWidth = 10;
    public int visibleHeight = 10;
    private Vector3Int lastCameraCellPos;

    // �������洢��ǰ�ɼ��ĸ���
    private HashSet<Vector3Int> currentlyVisibleCells = new HashSet<Vector3Int>();

    void Start()
    {
        //�Ƴ��ֶ�����Board
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

    // �޸ģ��Ż���ͼ���£�ж����Ұ��ĸ���
    void UpdateMapAroundCamera()
    {
        Vector3Int cameraCellPos = GetCurrentCameraCellPosition();

        // �����µĿɼ�����
        int startX = cameraCellPos.x - visibleWidth / 2;
        int endX = cameraCellPos.x + visibleWidth / 2;
        int startY = cameraCellPos.y - visibleHeight / 2;
        int endY = cameraCellPos.y + visibleHeight / 2;

        // �洢�µĿɼ�����
        HashSet<Vector3Int> newVisibleCells = new HashSet<Vector3Int>();

        // ��������Ұ�ڵ����и���
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                newVisibleCells.Add(cellPos);
            }
        }

        // �ҳ����ٿɼ��ĸ��ӣ���ж��
        HashSet<Vector3Int> cellsToRemove = new HashSet<Vector3Int>(currentlyVisibleCells);
        cellsToRemove.ExceptWith(newVisibleCells); // ֻ������Ҫ�Ƴ��ĸ���

        foreach (Vector3Int cell in cellsToRemove)
        {
            board.ClearTile(cell); // ���� Board �ķ���ж�ظ���
        }

        // �ҳ������ĸ��ӣ�������
        HashSet<Vector3Int> cellsToAdd = new HashSet<Vector3Int>(newVisibleCells);
        cellsToAdd.ExceptWith(currentlyVisibleCells); // ֻ������Ҫ�����ĸ���

        foreach (Vector3Int cell in cellsToAdd)
        {
            board.GenerateTile(cell); // ���� Board �ķ������ɸ���
        }

        // ���µ�ǰ�ɼ��ĸ���
        currentlyVisibleCells = newVisibleCells;

        Debug.Log($"Updated map. Visible area: X[{startX},{endX}], Y[{startY},{endY}]");
    }
}