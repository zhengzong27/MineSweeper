using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    void Update()
    {
        // ����Ƿ���ϵͳ���ؼ�
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ��ʾ�˳�ȷ�ϵ���
            Application.Quit();
        }
    }


}
