using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class FlowText : MonoBehaviour
{
    [Header("文本组件")]
    public TMP_Text textComponent;

    [Header("整体动画参数")]
    [Tooltip("文本总的生命周期")]
    public float duration = 1.0f;
    [Tooltip("渐隐开始的时间（duration * fadeStartRatio）")]
    [Range(0.1f, 0.9f)]
    public float fadeStartRatio = 0.4f;

    [Header("缩放冲击参数")]
    [Tooltip("文本在动画中达到的最大缩放倍数（例如：1.5）")]
    public float maxScale = 1.5f;
    [Tooltip("从0缩放到最大尺寸所需时间（越短冲击力越强，例如：0.2s）")]
    public float scaleUpDuration = 0.2f;

    [Header("旋转冲击参数")]
    [Tooltip("初始最大旋转角度（例如：45度）")]
    public float rotationMagnitude = 45f;
    [Tooltip("旋转摆动的持续时间（例如：0.4s）")]
    public float rotationDuration = 0.4f;
    [Tooltip("旋转摆动的频率（例如：25f）")]
    public float rotationFrequency = 25f;

    [Header("位置动画参数")]
    [Tooltip("向上移动的距离")]
    public float moveUpDistance = 1.5f;
    [Tooltip("位置动画的缓动类型")]
    public Ease moveEase = Ease.OutCubic;

    private float initialAngleOffset;
    private Sequence animationSequence;

    private void OnDestroy()
    {
        // 确保动画序列被正确清理
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }
    }

    /// <summary>
    /// 初始化文本内容、颜色和位置，并启动动画协程。
    /// </summary>
    public void Setup(string text, Color color)
    {
        textComponent = GetComponentInChildren<TMP_Text>();
        if (textComponent == null)
        {
            Debug.LogError("PopupText: Text Component is missing!");
            return;
        }

        textComponent.text = text;

        // 确保颜色完全不透明开始
        Color startColor = color;
        startColor.a = 1f;
        textComponent.color = startColor;

        // 重置文本初始状态
        transform.localScale = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // 随机一个正负号，让文本随机向左或向右旋转
        initialAngleOffset = Random.Range(0, 2) * 2 - 1;

        // 启动动画
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        // 清理之前的动画序列
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }

        // 创建新的动画序列
        animationSequence = DOTween.Sequence();

        // --- 1. 缩放动画 ---
        // 快速缩放效果
        animationSequence.Append(
            transform.DOScale(Vector3.one * maxScale, scaleUpDuration)
                .SetEase(Ease.OutBack)
        );

        // 保持在最大尺寸一段时间，然后缓慢回弹到正常尺寸
        animationSequence.AppendInterval(0.1f); // 短暂停顿

        animationSequence.Append(
            transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBounce)
        );

        // --- 2. 旋转动画 ---
        // 使用DOPunchRotation实现震动旋转效果
        animationSequence.Join(
            transform.DOPunchRotation(
                new Vector3(0, 0, rotationMagnitude * initialAngleOffset),
                rotationDuration,
                Mathf.RoundToInt(rotationFrequency),
                0.5f
            ).SetEase(Ease.OutQuad)
        );

        // --- 3. 位置移动动画 ---
        // 向上移动
        animationSequence.Join(
            transform.DOMoveY(transform.position.y + moveUpDistance, duration)
                .SetEase(moveEase)
        );

        // 可选：轻微的水平移动
        float horizontalMove = Random.Range(-0.3f, 0.3f);
        animationSequence.Join(
            transform.DOMoveX(transform.position.x + horizontalMove, duration)
                .SetEase(Ease.InOutSine)
        );

        // --- 4. 渐隐动画 ---
        // 计算渐隐开始时间
        float fadeStartTime = duration * fadeStartRatio;
        float fadeDuration = duration - fadeStartTime;

        // 在指定时间开始渐隐
        animationSequence.Insert(
            fadeStartTime,
            textComponent.DOFade(0f, fadeDuration)
                .SetEase(Ease.InQuad)
        );

        // 渐隐时同时缩小
        animationSequence.Insert(
            fadeStartTime,
            transform.DOScale(Vector3.one * 0.8f, fadeDuration)
                .SetEase(Ease.InBack)
        );

        // --- 5. 动画完成后的处理 ---
        animationSequence.OnComplete(() =>
        {
            // 确保完全透明
            Color finalColor = textComponent.color;
            finalColor.a = 0f;
            textComponent.color = finalColor;

            // 销毁对象
            Destroy(gameObject);
        });

        // 设置自动销毁保护
        animationSequence.SetTarget(gameObject);

        // 开始播放动画
        animationSequence.Play();
    }

    /// <summary>
    /// 提前停止动画并销毁
    /// </summary>
    public void StopAndDestroy()
    {
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// 重新设置文本内容（用于更新数值）
    /// </summary>
    public void UpdateText(string newText)
    {
        if (textComponent != null)
        {
            textComponent.text = newText;
        }
    }

    /// <summary>
    /// 重新设置文本颜色
    /// </summary>
    public void UpdateColor(Color newColor)
    {
        if (textComponent != null)
        {
            newColor.a = textComponent.color.a; // 保持当前的透明度
            textComponent.color = newColor;
        }
    }
}
