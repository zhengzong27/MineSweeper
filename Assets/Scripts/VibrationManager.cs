using UnityEngine;

public class VibrationManager : MonoBehaviour
{
    private static AndroidJavaObject vibrationHelper;

    void Start()
    {
        InitializeVibration();
    }

    // 初始化震动功能
    void InitializeVibration()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        // 初始化 Vibrator
        AndroidJavaClass vibrationClass = new AndroidJavaClass("com.example.vibration.VibrationHelper");
        vibrationClass.CallStatic("Initialize", currentActivity);
    }
    
    // 触发自定义时长的震动
    public static void Vibrate(long milliseconds)
    {
        AndroidJavaClass vibrationClass = new AndroidJavaClass("com.example.vibration.VibrationHelper");
        vibrationClass.CallStatic("Vibrate", milliseconds);
    }
}