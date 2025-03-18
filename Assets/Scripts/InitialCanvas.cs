using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitialCanvas : MonoBehaviour
{
    private int screenWidth;
    private int screenHeight;
    // Start is called before the first frame update
    void Start()
    {
        screenHeight = UnityEngine.Screen.height;
        GetComponent<CanvasScaler>().scaleFactor = screenHeight / (float)1080;
    }
}
