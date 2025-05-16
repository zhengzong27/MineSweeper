package com.unity3d.player;

import android.content.Context;
import android.os.Vibrator;
import android.os.VibrationEffect;
import android.os.Build;
import com.unity3d.player.UnityPlayer;

public class VibrationHelper {
    private static Vibrator vibrator;

    // 初始化 Vibrator
    public static void Initialize() {
        if (UnityPlayer.currentActivity != null) {
            vibrator = (Vibrator) UnityPlayer.currentActivity.getSystemService(Context.VIBRATOR_SERVICE);
        }
    }

    // 自定义震动时长
    public static void Vibrate(long milliseconds) {
        if (vibrator == null) {
            Initialize();
        }
        
        if (vibrator != null && vibrator.hasVibrator()) {
            try {
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                    // Android 8.0 (API 26) 及以上版本
                    VibrationEffect vibrationEffect = VibrationEffect.createOneShot(milliseconds, VibrationEffect.DEFAULT_AMPLITUDE);
                    vibrator.vibrate(vibrationEffect);
                } else {
                    // Android 8.0 以下版本
                    vibrator.vibrate(milliseconds);
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }
}