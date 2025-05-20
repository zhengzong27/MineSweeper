using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FireworkEffect : MonoBehaviour
{
    private ParticleSystem fireworkParticle;

    private void Awake()
    {
        // 获取组件引用
        fireworkParticle = GetComponent<ParticleSystem>();
        if (fireworkParticle == null)
        {
            Debug.LogError("FireworkEffect: ParticleSystem component is missing! Adding one...");
            fireworkParticle = gameObject.AddComponent<ParticleSystem>();
            SetupParticleSystem();
        }
    }

    private void SetupParticleSystem()
    {
        if (fireworkParticle == null) return;

        // 基本设置
        var main = fireworkParticle.main;
        main.duration = 1f;
        main.loop = false;
        main.startLifetime = 1f;
        main.startSpeed = 5f;
        main.startSize = 0.2f;
        main.startColor = new ParticleSystem.MinMaxGradient(Color.red, Color.yellow);
        main.gravityModifier = 0.5f;

        // 发射器形状
        var shape = fireworkParticle.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        shape.arc = 360f;

        // 颜色随时间变化
        var colorOverLifetime = fireworkParticle.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.red, 0.0f),
                new GradientColorKey(Color.yellow, 0.5f),
                new GradientColorKey(Color.blue, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 0.8f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = gradient;

        // 大小随时间变化
        var sizeOverLifetime = fireworkParticle.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.0f);
        curve.AddKey(0.5f, 1.0f);
        curve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
    }

    public void PlayFirework()
    {
        if (fireworkParticle == null)
        {
            Debug.LogError("FireworkEffect: Cannot play firework - ParticleSystem is null!");
            return;
        }

        // 播放粒子效果
        fireworkParticle.Clear();
        fireworkParticle.Play();
    }

    private void OnParticleSystemStopped()
    {
        // 当粒子效果播放完毕后，销毁对象
        Destroy(gameObject);
    }
} 