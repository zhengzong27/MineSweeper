using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 0.01f; // ����ͷ�ƶ��ٶ�
    private Vector2 touchStartPos; // ������ʼλ��
    private bool isDragging = false; // �Ƿ������϶�
    private float debugInterval = 1f; // ��������ʱ����
    private float debugTimer = 0f; // ��ʱ��

    void Start()
    {
        // ��ʼ����ʱ��
        debugTimer = debugInterval;
    }

    void Update()
    {
        HandleTouchInput();
        DebugCameraPosition();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // ��¼������ʼλ��
                    touchStartPos = touch.position;
                    isDragging = true;
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        // ���㴥���ƶ���ƫ����
                        Vector2 touchDelta = touch.position - touchStartPos;

                        // ����ƫ�����ƶ�����ͷ
                        MoveCamera(touchDelta);

                        // ���´�����ʼλ��
                        touchStartPos = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // �����϶�
                    isDragging = false;
                    break;
            }
        }
    }

    void MoveCamera(Vector2 delta)
    {
        // ����ƫ�����ƶ�����ͷ
        Vector3 moveDirection = new Vector3(-delta.x, -delta.y, 0) * moveSpeed;
        transform.Translate(moveDirection, Space.World);
    }

    void DebugCameraPosition()
    {
        // ��ʱ������
        debugTimer -= Time.deltaTime;

        // �����ʱ��С�ڵ���0���������ͷ���겢���ü�ʱ��
        if (debugTimer <= 0f)
        {
            Debug.Log("Camera Position: " + transform.position);
            debugTimer = debugInterval;
        }
    }
}