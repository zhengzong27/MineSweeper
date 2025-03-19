using UnityEngine;

public class Circle : MonoBehaviour
{
    public GameObject circle; // ��ק Circle �� GameObject ������
    private float touchStartTime = 0f; // ��¼������ʼʱ��
    private bool isTouching = false; // �Ƿ����ڴ���
    private Vector2 touchStartPosition; // ��¼������ʼλ��
    private bool isCircleActive = false; // Circle �Ƿ��Ѽ���

    private void Update()
    {
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        // ��ⴥ������
        if (Input.touchCount > 0) // ����д�������
        {
            Touch touch = Input.GetTouch(0); // ��ȡ��һ��������

            switch (touch.phase)
            {
                case TouchPhase.Began: // ������ʼ
                    isTouching = true;
                    touchStartTime = Time.time; // ��¼������ʼʱ��
                    touchStartPosition = touch.position; // ��¼������ʼλ��
                    Debug.Log("������ʼ��λ��: " + touchStartPosition);
                    break;

                case TouchPhase.Stationary: // ��������
                    if (isTouching && Time.time - touchStartTime >= 2.5f) // ����ʱ����ڵ��� 2.5 ��
                    {
                        // ���� Circle ��λ��
                        SetCirclePosition(touchStartPosition);
                        // ���� Circle
                        circle.SetActive(true);
                        isCircleActive = true;
                        Debug.Log("Circle �Ѽ���");
                    }
                    break;

                case TouchPhase.Moved: // �����ƶ�
                    if (isCircleActive) // ��� Circle �Ѽ���
                    {
                        // ��⻬������
                        DetectSwipe(touch.position);
                    }
                    break;

                case TouchPhase.Ended: // ��������
                case TouchPhase.Canceled: // ����ȡ��
                    isTouching = false;
                    isCircleActive = false;
                    // ��ָ�뿪��Ļʱ������ Circle
                    circle.SetActive(false);
                    Debug.Log("����������Circle ������");
                    break;
            }
        }
    }

    private void SetCirclePosition(Vector2 screenPosition)
    {
        if (circle != null)
        {
            Debug.Log("��Ļ����: " + screenPosition);
            // ����Ļ����ת��Ϊ��������
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                circle.GetComponent<RectTransform>().parent as RectTransform,
                screenPosition,
                null,
                out Vector2 localPosition
            );
            Debug.Log("ת����ľֲ�����: " + localPosition);

            // ���� Circle ��λ��
            circle.GetComponent<RectTransform>().localPosition = localPosition;
        }
    }

    private void DetectSwipe(Vector2 currentTouchPosition)
    {
        // ���㻬������
        float swipeDistance = currentTouchPosition.y - touchStartPosition.y;

        // ����������ֵ������ 50 ���أ�
        float swipeThreshold = 50f;

        if (Mathf.Abs(swipeDistance) > swipeThreshold)
        {
            if (swipeDistance > 0) // ���ϻ���
            {
                Debug.Log("���ϻ�����ִ�� Flags ����");
                Flags();
            }
            else // ���»���
            {
                Debug.Log("���»�����ִ�� Question ����");
                Question();
            }

            // ���ô�����ʼλ�ã������ظ�����
            touchStartPosition = currentTouchPosition;
        }
    }

    private void Flags()
    {
        // TODO: ʵ�� Flags ����
        Debug.Log("Flags ����������");
    }

    private void Question()
    {
        // TODO: ʵ�� Question ����
        Debug.Log("Question ����������");
    }
}