using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance; // 单例访问

    private Vector3 originalPosition;

    [Header("震动配置")]
    [SerializeField] private float defaultDuration = 0.5f;
    [SerializeField] private float defaultMagnitude = 0.1f;
    private Tween currentShakeTween;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // 存储摄像机的初始位置
        originalPosition = transform.localPosition;
    }

    /// <summary>
    /// 开始摄像机摇晃（协程版本）
    /// </summary>
    /// <param name="duration">摇晃持续时间</param>
    /// <param name="magnitude">最大摇晃幅度</param>
    public void Shake(float duration = 0f, float magnitude = 0f)
    {
        if (duration <= 0) duration = defaultDuration;
        if (magnitude <= 0) magnitude = defaultMagnitude;

        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    /// <summary>
    /// 使用DOTween的摄像机震动（推荐使用）
    /// </summary>
    /// <param name="duration">震动持续时间</param>
    /// <param name="strength">震动强度</param>
    /// <param name="vibrato">震动的频率（数值越大震动越快）</param>
    /// <param name="randomness">随机性（0-180，数值越大越随机）</param>
    public void ShakeDOTween(float duration = 0f, float strength = 0f, int vibrato = 20, float randomness = 90f)
    {
        if (duration <= 0) duration = defaultDuration;
        if (strength <= 0) strength = defaultMagnitude * 2f; // 转换为DOTween的强度单位

        // 停止当前的震动
        if (currentShakeTween != null && currentShakeTween.IsActive())
        {
            currentShakeTween.Kill();
        }

        // 使用DOTween的DOShakePosition
        currentShakeTween = transform.DOShakePosition(
            duration,
            strength,
            vibrato,
            randomness,
            false,
            true
        ).OnComplete(() => {
            transform.localPosition = originalPosition;
            currentShakeTween = null;
        });
    }

    /// <summary>
    /// 强烈震动（用于爆炸等大效果）
    /// </summary>
    public void StrongShake()
    {
        ShakeDOTween(0.6f, 0.25f, 25, 90f);
    }

    /// <summary>
    /// 轻微震动（用于小效果）
    /// </summary>
    public void LightShake()
    {
        ShakeDOTween(0.3f, 0.05f, 15, 90f);
    }

    /// <summary>
    /// 脉冲震动（快速震动一次）
    /// </summary>
    public void PulseShake(float intensity = 0.1f)
    {
        if (currentShakeTween != null && currentShakeTween.IsActive())
        {
            currentShakeTween.Kill();
        }

        // 快速震动后返回
        currentShakeTween = transform.DOShakePosition(
            0.2f,
            intensity,
            10,
            90f,
            false,
            true
        ).OnComplete(() => {
            transform.localPosition = originalPosition;
            currentShakeTween = null;
        });
    }

    /// <summary>
    /// 停止所有震动
    /// </summary>
    public void StopShake()
    {
        if (currentShakeTween != null && currentShakeTween.IsActive())
        {
            currentShakeTween.Kill();
        }

        StopAllCoroutines();
        transform.localPosition = originalPosition;
    }

    // 原来的协程方法保持不变
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            magnitude = Mathf.Lerp(magnitude, 0f, Time.deltaTime * 5f);

            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}