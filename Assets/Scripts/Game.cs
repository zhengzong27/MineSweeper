using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    float touchTime = 0f; // ��������ʱ��
    bool isTouching = false;
    public Vector2 TouchPosition;

    public int width = 8;
    public int height = 16;
    public int mineCount = 20;
    private Board board;
    private Cell[,] state;
    private void Awake()
    {
        board = GetComponentInChildren<Board>();
    }
    private void Start()
    {
        NewGame();
    }
    private void NewGame()
    {
        state = new Cell[width, height];
        GenerateCells();
        GenerateMines();
        GenerateNumber();
        Camera.main.transform.position = new Vector3(0, 0, -10f);
        board.Draw(state);
    }
    private void GenerateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;
            }
        }
    }
    private void GenerateMines()
    {
        for (int i = 0; i < mineCount; i++)
        {//�������
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            while (state[x, y].type == Cell.Type.Mine)//�����ǰ�����е��ף���������
            {
                x++;
                if (x >= width)
                {
                    x = 0;
                    y++;
                    if (y >= height) {
                        y = 0;
                    }
                }
            }
            //���õ���
            state[x, y].type = Cell.Type.Mine;
            //����ȫ���������״̬
            //state[x, y].revealed = true;
        }
    }
    private void GenerateNumber()
    { for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                if (cell.type == Cell.Type.Mine)
                {
                    continue;
                }
                cell.Number = CountMines(x, y);
                if (cell.Number > 0)
                {
                    cell.type = Cell.Type.Number;
                }
                state[x, y] = cell;
                //��ʾ����
                //state[x, y].revealed = true;
            }
        }
    }
    private int CountMines(int cellX, int cellY)
    {
        int count = 0;
        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0)
                {
                    continue;
                }
                int x = cellX + adjacentX;
                int y = cellY + adjacentY;
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue;
                }
                if (state[x, y].type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }
        return count;
    }
    void Update()
    {
        if (Input.touchCount > 0) // ����Ƿ��д�����
        {
            Touch touch = Input.GetTouch(0); // ��ȡ��һ��������

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isTouching = true;
                    touchTime = 0f; // ���ü�ʱ��
                    TouchPosition = touch.position;
                    Debug.Log("������ʼ");
                    break;

                case TouchPhase.Stationary:
                    if (isTouching)
                    {
                        touchTime += Time.deltaTime; // �ۼƴ���ʱ��
                        if (touchTime > 0.25f) // ���� 1 ��
                        {
                            Flags();
                            Debug.Log("��������");
                            isTouching = false; // ����״̬
                            return;
                        }
                    }
                    break;

                case TouchPhase.Ended:
                    Reveal();
                    Debug.Log("�ҿ�����");
                    isTouching = false;
                    break;
                case TouchPhase.Canceled:
                    isTouching = false;
                    Debug.Log("��������");
                    break;
            }
        }
    }
    private void Reveal()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);
        if (cell.type==Cell.Type.Invalid||cell.revealed||cell.revealed)
        {
            return;
        }
        if(cell.type== Cell.Type.Empty)
        {
            Flood(cell);
        }
        cell.revealed = true;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);

    }
    private void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;
        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.x));
            Flood(GetCell(cell.position.x, cell.position.y-1));
            Flood(GetCell(cell.position.x, cell.position.y+1));
        }
    }
    private void Flags()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);
        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }
        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);
    }
    private Cell GetCell(int x,int y)
    {
        if (IsValid(x, y))
        {
            return state[x, y];
        }
        else {
            return new Cell();
        }
    }
    private bool IsValid(int x,int y)
    {
        return x >= 0 && x < width && y >= 0 && y <= height;
    }

}


