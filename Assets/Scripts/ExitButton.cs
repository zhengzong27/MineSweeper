using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    void Update()
    {
        // 检测是否按下系统返回键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 显示退出确认弹窗
            Application.Quit();
        }
    }


}
