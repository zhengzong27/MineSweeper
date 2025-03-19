using UnityEngine;
using UnityEngine.UI;

public class Circle : MonoBehaviour
{
    public GameObject circle; // ��ק Circle �� GameObject ������
    public float longPressDuration = 2.5f; // ����ʱ����ֵ
    private Vector2 pressPosition; // ��ѹλ��

  /*  private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // ����Ǵ����豸
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                StartPress(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                EndPress();
            }
        }

        // ������ڰ�ѹ��ʱ�䳬����ֵ����ʾ Circle
        if (isPressing && Time.time - pressStartTime >= longPressDuration)
        {
            ShowCircle();
        }
    }*/

    private void StartPress(Vector2 position)
    {

        pressPosition = position;
    }

    private void EndPress()
    {

        HideCircle();
    }

    private void ShowCircle()
    {
        if (circle != null)
        {
            // �� Circle ����Ϊ��ʾ
            circle.SetActive(true);

            // �� Circle ��λ������Ϊ��ѹλ��
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                circle.GetComponent<RectTransform>().parent as RectTransform,
                pressPosition,
                null,
                out Vector2 localPoint
            );
            circle.GetComponent<RectTransform>().localPosition = localPoint;
        }
    }

    private void HideCircle()
    {
        if (circle != null)
        {
            // �� Circle ����Ϊ����
            circle.SetActive(false);
        }
    }
}