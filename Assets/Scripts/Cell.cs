using UnityEngine;

public struct Cell //��Ԫ����
{
    public enum Type//��������
    {
        Empty,
        Mine,
        Number
    }
    //����Ϊ������
    public Vector3Int position;//λ��
    public Type type;//����
    public int Number;//��ʾֵ��
    public bool revealed;//�Ƿ�ҿ�
    public bool flagged;//�Ƿ񱻱��
    public bool exploded;//�Ƿ�ը
    }
