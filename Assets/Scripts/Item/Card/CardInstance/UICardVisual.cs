using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UICardVisual : MonoBehaviour
{
    private UICard parentCard;
    private RectTransform visualRect;
    private Canvas canvas; // 用于层级管理

    [Header("位置参数")]
    [Tooltip("选中状态下卡牌向上抬升的距离")]
    [SerializeField] private float selectionOffset = 50f;
    [SerializeField] private float positionTransitionDuration = 0.25f;

    [Header("缩放/悬停参数")]
    [Tooltip("悬停时的放大倍数")]
    [SerializeField] private float scaleOnHover = 1.15f;
    [Tooltip("过渡动画时长")]
    [SerializeField] private float scaleTransitionDuration = 0.15f;
    [SerializeField] private Ease scaleEase = Ease.OutQuad; // 使用平滑的缓动函数

    [Header("动画参数 (从 UICard 迁移)")]
    [Tooltip("摇摆/冲击动画的总时长")]
    [SerializeField] private float wiggleDuration = 0.3f;
    private Vector3 defaultScale;
    private Coroutine wiggleCoroutine = null;

    private void Awake()
    {
        // 确保它挂载在子对象上，父对象上有 UICard 脚本
        parentCard = GetComponentInParent<UICard>();
        visualRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // --- 订阅事件 (核心解耦) ---
        parentCard.PointerEnterEvent.AddListener(HandlePointerEnter);
        parentCard.PointerExitEvent.AddListener(HandlePointerExit);
        parentCard.BeginDragEvent.AddListener(HandleBeginDrag);
        parentCard.EndDragEvent.AddListener(HandleEndDrag);
        parentCard.SelectEvent.AddListener(HandleSelect);

        defaultScale = transform.localScale;    
    }

    private void Update()
    {
        // 实现平滑跟随：让 Visual 对象的 RectTransform 平滑地追随父级 Card 的位置
        // visualRect.position = Vector3.Lerp(visualRect.position, parentCard.transform.position, followSpeed * Time.deltaTime);
        // visualRect.rotation = Quaternion.Slerp(visualRect.rotation, parentCard.transform.rotation, followSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 悬停进入事件的响应。执行放大动画。
    /// </summary>
    private void HandlePointerEnter(UICard card)
    {
        // 停止任何正在进行的缩放动画，并执行放大
        visualRect.DOKill(true);
        visualRect.DOScale(defaultScale * scaleOnHover, scaleTransitionDuration).SetEase(scaleEase);
    }

    /// <summary>
    /// 悬停离开事件的响应。如果卡牌未被选中，则恢复原始大小。
    /// </summary>
    private void HandlePointerExit(UICard card)
    {
        // 只有在卡牌未被选中的情况下才恢复原始缩放
        if (!card.isSelected)
        {
            visualRect.DOKill(true);
            visualRect.DOScale(defaultScale, scaleTransitionDuration).SetEase(scaleEase);
        }
    }

    /// <summary>
    /// 选中/取消选中事件的响应。执行位置偏移和 Wiggle 动画。
    /// </summary>
    private void HandleSelect(UICard card, bool isSelected)
    {
        // 执行位置偏移动画
        if (isSelected)
        {
            // 选中时向上偏移，并播放冲击动画
            visualRect.DOLocalMoveY(selectionOffset, positionTransitionDuration).SetEase(Ease.OutBack);
            visualRect.DOPunchRotation(new Vector3(0, 0, 10), 0.8f, 15, 0.8f);
        }
        else
        {
            // 取消选中时恢复到本地坐标 (0, 0, 0)
            visualRect.DOLocalMoveY(0f, positionTransitionDuration).SetEase(Ease.OutQuad);

            // 如果未被拖拽，并且不在悬停状态，恢复缩放
            if (!card.isDragging && !card.GetComponent<Image>().IsActive())
            {
                visualRect.DOScale(defaultScale, scaleTransitionDuration).SetEase(scaleEase);
            }
        }
    }

    /// <summary>
    /// 开始拖拽事件的响应。调整层级，避免被其他卡牌遮挡。
    /// </summary>
    private void HandleBeginDrag(UICard card)
    {
        // 1. 放大一点点
        visualRect.DOScale(defaultScale * 1.2f, scaleTransitionDuration).SetEase(scaleEase);

        // 2. 利用 Canvas 的 overrideSorting 确保被拖拽的卡牌在最上层
        if (canvas != null)
        {
            // 确保 UICard (父对象) 上的 CanvasGroup 禁用了 Raycast 阻止，以便 Raycast 可以穿透。
            // 或者，直接操作 Canvas 的 Sorting Order
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100; // 设置一个较高的排序值
        }
    }

    /// <summary>
    /// 结束拖拽事件的响应。恢复层级。
    /// </summary>
    private void HandleEndDrag(UICard card)
    {
        // 1. 恢复到悬停或默认大小（取决于是否选中）
        if (!card.isSelected)
        {
            visualRect.DOScale(defaultScale, scaleTransitionDuration).SetEase(scaleEase);
        }

        // 2. 恢复 Canvas Sorting
        if (canvas != null)
        {
            canvas.overrideSorting = false;
            canvas.sortingOrder = 0; // 恢复默认排序
        }
    }
    /// <summary>
    /// 尝试播放卡牌 Wiggle 冲击动画。
    /// </summary>
    /// <param name="isBlocking">是否同步触发</param>
    public IEnumerator TryPlayWiggleAnimation(bool isBlocking)
    {
        if (wiggleCoroutine != null)
        {
            StopCoroutine(wiggleCoroutine);
            ResetVisuals();
            wiggleCoroutine = null;
        }
        if (isBlocking)
        {
            //返回协程，由 CardHandler 启动并等待
            return PlayWiggleAnimation();
        }
        else
        {
            //直接启动动画，不等待  
            wiggleCoroutine = StartCoroutine(PlayWiggleAnimation());
            return null; // 返回 null 表示不需要外部等待
        }
    }

    /// <summary>
    /// Wiggle 动画的具体实现（从 UICard 迁移过来的代码）
    /// </summary>
    private IEnumerator PlayWiggleAnimation(float duration = 0.5f, float magnitude = 30f, float rotationFrequency = 60f)
    {
        float timer = 0f;
        // 随机一个初始方向：-1 或 1
        float initialAngleOffset = (Random.Range(0, 2) * 2 - 1);

        // --- 缩放关键点的时间定义 (占总时长的比例) ---
        // 确保所有阶段总和不超过 duration
        float scalePhase1End = duration * 0.25f; // 1.0 -> 0.8 (挤压)
        float scalePhase2End = duration * 0.5f;  // 0.8 -> 1.2 (回弹/冲击)
        float scalePhase3End = duration;       // 1.2 -> 1.0 (归位)


        while (timer < duration)
        {
            timer += Time.deltaTime;
            float timeRatio = timer / duration;
            // A. 冲击旋转 (高频衰减振荡)

            // 线性阻尼因子：在 duration 内从1衰减到0
            float dampening = 1f - timeRatio;

            // 振荡因子：高频正弦波
            // 振荡频率使用预设的 rotationFrequency，使其摇晃速度更快
            float oscillation = Mathf.Sin(timer * rotationFrequency);

            // 最终角度： magnitude * 衰减 * 振荡 * 随机方向
            float zAngle = magnitude * dampening * oscillation * initialAngleOffset;

            // 围绕自身的Z轴旋转
            transform.localRotation = Quaternion.Euler(0, 0, zAngle);

            // B. 缩放冲击
            float currentScale = 1f;

            if (timer <= scalePhase1End)
            {
                // Phase 1: 挤压 (1.0 -> 0.8)
                float progress = timer / scalePhase1End;
                currentScale = Mathf.Lerp(1.0f, 0.8f, progress);
            }
            else if (timer <= scalePhase2End)
            {
                // Phase 2: 强力回弹/冲击 (0.8 -> 1.2)
                float progress = (timer - scalePhase1End) / (scalePhase2End - scalePhase1End);
                // 使用 Mathf.SmoothStep 让回弹更柔和
                currentScale = Mathf.Lerp(0.8f, 1.2f, Mathf.SmoothStep(0f, 1f, progress));
            }
            else
            {
                // Phase 3: 归位 (1.2 -> 1.0)
                float progress = (timer - scalePhase2End) / (scalePhase3End - scalePhase2End);
                currentScale = Mathf.Lerp(1.2f, 1.0f, progress);
            }

            transform.localScale = defaultScale * currentScale;

            yield return null;
        }
        // 确保结束时回到初始状态
        ResetVisuals();
        if (wiggleCoroutine != null)
        {
            wiggleCoroutine = null;
        }
    }

    /// <summary>
    /// 强制将卡牌的视觉效果恢复到默认状态（缩放、旋转）
    /// </summary>
    public void ResetVisuals()
    {
        if (wiggleCoroutine != null)
        {
            StopCoroutine(wiggleCoroutine);
            wiggleCoroutine = null;
        }
        // 停止所有 DOTween 动画
        visualRect.DOKill();

        visualRect.localScale = defaultScale;
        visualRect.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
    }
}
