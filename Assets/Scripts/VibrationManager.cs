using UnityEngine;

public class VibrationManager : MonoBehaviour
{
    private static AndroidJavaObject vibrationHelper;

    void Start()
    {
        InitializeVibration();
    }

    // ��ʼ���𶯹���
    void InitializeVibration()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        // ��ʼ�� Vibrator
        AndroidJavaClass vibrationClass = new AndroidJavaClass("com.example.vibration.VibrationHelper");
        vibrationClass.CallStatic("Initialize", currentActivity);
    }
    
    // �����Զ���ʱ������
    public static void Vibrate(long milliseconds)
    {
        AndroidJavaClass vibrationClass = new AndroidJavaClass("com.example.vibration.VibrationHelper");
        vibrationClass.CallStatic("Vibrate", milliseconds);
    }
}