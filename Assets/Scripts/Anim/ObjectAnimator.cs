using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ObjectAnimator : MonoBehaviour
{
    public static ObjectAnimator Instance;
    private void Awake()
    {
        Instance = this;
    }
    // 字典用于存储正在运行的Sequence，键是执行动画的GameObject实例。
    // 注意：我们主要依赖 DOTween 的 SetTarget 和目标销毁时的自动清理，
    // 此字典主要用于在动画再次启动时清理旧动画，以及作为手动检查的备用。
    private Dictionary<GameObject, Sequence> activeAnimations = new Dictionary<GameObject, Sequence>();

    // --- 动画管理和清理的核心方法 ---

    /// <summary>
    /// 在动画开始前或对象销毁时调用，用于停止并移除字典中对应的动画。
    /// </summary>
    /// <param name="target">需要清理动画的 GameObject。</param>
    private void ClearActiveAnimation(GameObject target)
    {
        if (target != null && activeAnimations.ContainsKey(target))
        {
            Sequence sequenceToKill = activeAnimations[target];
            if (sequenceToKill != null && sequenceToKill.IsActive())
            {
                // 停止并销毁 DOTween 实例
                sequenceToKill.Kill();
            }
            activeAnimations.Remove(target);
        }
    }

    // --- 公开核心方法 1: 通用动感出现动画 ---

    public IEnumerator AnimateIn(
        GameObject target,
        float duration = 0.5f,
        Vector3? startPosition = null,
        float startRotationZ = 0f,
        float delay = 0f)
    {
        // 核心保护：如果目标在协程启动时已被销毁，立即退出。
        if (target == null) yield break;
        Transform targetTransform = target.transform;

        Vector3 initialPosition = targetTransform.position;
        float initialRotationZ = targetTransform.localEulerAngles.z;

        targetTransform.localScale = Vector3.zero;
        if (startPosition.HasValue) targetTransform.position = startPosition.Value;

        Vector3 startRotation = targetTransform.localEulerAngles;
        startRotation.z = startRotationZ;
        targetTransform.localEulerAngles = startRotation;

        target.SetActive(true);

        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
            // 协程保护 1: 延迟后检查
            if (target == null) { ClearActiveAnimation(target); yield break; }
        }

        // --- 1. 创建和注册 DOTween Sequence ---
        Sequence sequence = DOTween.Sequence();

        // 注册动画：先清理旧动画，再添加新动画
        ClearActiveAnimation(target);
        activeAnimations.Add(target, sequence);

        // 如果 target.transform 被销毁，Sequence 会自动清理自身，防止内存泄漏。
        sequence.SetTarget(targetTransform);

        // 动感缩放 (0 -> 1.2 -> 1)
        sequence.Append(targetTransform.DOScale(1.2f, duration * 0.7f).SetEase(Ease.OutQuad));
        sequence.Append(targetTransform.DOScale(Vector3.one, duration * 0.3f).SetEase(Ease.OutBack));

        // 位置移动
        if (startPosition.HasValue) sequence.Insert(0, targetTransform.DOMove(initialPosition, duration).SetEase(Ease.OutBack));

        // 摆正旋转
        if (startRotationZ != initialRotationZ)
        {
            Vector3 targetRotation = targetTransform.localEulerAngles;
            targetRotation.z = initialRotationZ;
            sequence.Insert(0, targetTransform.DOLocalRotate(targetRotation, duration).SetEase(Ease.InOutBack));
        }

        // 快速摇摆效果
        Tween punchTween = targetTransform.DOPunchRotation(new Vector3(0f, 0f, 5f), duration * 0.5f, 15, 1f).SetTarget(target);
        sequence.Insert(0, punchTween);


        // --- 2. 等待动画完成并清理 ---
        yield return sequence.WaitForCompletion();

        // 协程保护 2: 动画完成后检查
        if (target == null) { ClearActiveAnimation(target); yield break; }

        // 动画完成后，确保 position 和 rotation 最终定格在目标值
        targetTransform.position = initialPosition;
        targetTransform.localEulerAngles = new Vector3(targetTransform.localEulerAngles.x, targetTransform.localEulerAngles.y, initialRotationZ);

        // 成功完成动画后，从字典中移除
        ClearActiveAnimation(target);
        Debug.Log($"Object '{target.name}' 入场动画完成并清理。");
    }

    // --- 公开核心方法 2: 通用 position 切换动画 ---

    public IEnumerator ToggleObjectPosition(
        GameObject target,
        float duration,
        Vector3 inPosition,
        Vector3 outPosition,
        bool isShowing)
    {
        if (target == null) yield break;

        Vector3 targetPosition = isShowing ? outPosition : inPosition;

        // --- 1. 创建和注册 DOTween Sequence ---
        Sequence sequence = DOTween.Sequence();

        ClearActiveAnimation(target);
        activeAnimations.Add(target, sequence);

        // 关键设置：将 Sequence 关联到 target.transform
        sequence.SetTarget(target.transform);

        // 执行移动动画
        Tween moveTween = target.transform.DOMove(targetPosition, duration).SetEase(Ease.InOutQuad);
        sequence.Append(moveTween);

        // --- 2. 等待动画完成并清理 ---
        yield return sequence.WaitForCompletion();

        // 协程保护：动画完成后检查
        if (target == null) { ClearActiveAnimation(target); yield break; }

        // 动画完成后，清理
        ClearActiveAnimation(target);

        if (!isShowing)
        {
            target.SetActive(false);
            Debug.Log($"Object '{target.name}' 离开屏幕");
        }
        else
        {
            Debug.Log($"Object '{target.name}' 进入屏幕。");
        }
    }

    public IEnumerator MovePopup(
        GameObject target,
        Vector3 movePosition,
        float duration = 0.3f,
        bool isHiding=false,
        GameObject hideObject=null
        )
    {
        if (target == null) yield break;
        ClearActiveAnimation(target);
        Sequence sequence = DOTween.Sequence();
        activeAnimations.Add(target, sequence);

        // 关键设置：将 Sequence 关联到 target.transform
        sequence.SetTarget(target.transform);

        // 执行移动动画
        Tween moveTween = target.transform.DOMove(movePosition, duration).SetEase(Ease.InOutQuad);
        sequence.Append(moveTween);

        // --- 2. 等待动画完成并清理 ---
        yield return sequence.WaitForCompletion();

        // 协程保护：动画完成后检查
        if (target == null) { ClearActiveAnimation(target); yield break; }

        // 动画完成后，清理
        ClearActiveAnimation(target);

        if (isHiding)
        {
            if (hideObject != null)
            {
                hideObject.SetActive(false);
                Debug.Log($"Object '{hideObject.name}' 隐藏");
            }
            else
            {
                target.SetActive(false);
                Debug.Log($"Object '{target.name}' 隐藏");
            }
        }
    }

    /// <summary>
    /// 模拟物体从天而降的动画，结合阴影放大效果。
    /// </summary>
    /// <param name="target">要掉落的物体本体。</param>
    /// <param name="shadow">物体轮廓的全黑阴影 GameObject。</param>
    /// <param name="finalPosition">物体最终要停下的位置。</param>
    /// <param name="duration">整个动画的总时长。</param>
    /// <param name="delay">动画开始前的延迟。</param>
    public IEnumerator AnimateDropFromSky(
        GameObject target,
        GameObject shadow,
        Vector3 finalPosition,
        float duration = 1.0f,
        float delay = 0f)
    {
        // 核心保护：如果目标或阴影在协程启动时已被销毁，立即退出。
        if (target == null || shadow == null) yield break;

        // 确保 target 和 shadow 都在同一层级或有相同的 scale 基础。
        Transform targetTransform = target.transform;
        Transform shadowTransform = shadow.transform;

        // 动画开始前，确保目标物体是隐藏的
        target.SetActive(false);
        // 确保阴影是可见的，但初始缩放为 0
        shadow.SetActive(true);
        shadowTransform.position = finalPosition; // 阴影始终在最终落点
        shadowTransform.localScale = Vector3.zero;

        // 清理旧动画
        ClearActiveAnimation(target);
        ClearActiveAnimation(shadow); // 额外清理阴影对象上的动画

        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
            // 协程保护 1: 延迟后检查
            if (target == null || shadow == null) { ClearActiveAnimation(target); ClearActiveAnimation(shadow); yield break; }
        }

        // --- 1. 阴影放大阶段 (Sequence 1) ---

        // 阴影放大动画的持续时间，占总时长的约 70%
        float shadowGrowDuration = duration * 0.7f;

        Sequence shadowSequence = DOTween.Sequence();
        activeAnimations.Add(shadow, shadowSequence);
        shadowSequence.SetTarget(shadowTransform);

        shadowSequence.Append(
            shadowTransform.DOScale(Vector3.one * 0.8f, shadowGrowDuration)
                .SetEase(Ease.InQuad) // 使用 Ease.InQuad 来模拟加速
        );

        yield return shadowSequence.WaitForCompletion();

        // 协程保护 2: 阴影动画完成后检查
        if (target == null || shadow == null) { ClearActiveAnimation(target); ClearActiveAnimation(shadow); yield break; }

        // 动画完成后，清理阴影 Sequence
        ClearActiveAnimation(shadow);

        // --- 2. 物体出现和定格阶段 (Sequence 2) ---

        // 物体从天而降到最终定格的持续时间，占总时长的约 30%
        float dropDuration = duration * 0.3f;

        // A. 瞬时变化：让物体充满整个屏幕（大爆炸/闪烁效果）
        targetTransform.position = finalPosition;
        // 假设 Vector3.one * 20f 是一个足够大的尺寸来“充满屏幕”
        targetTransform.localScale = Vector3.one * 2f;
        target.SetActive(true); // 物体突然出现

        // B. 隐藏阴影
        shadow.SetActive(false);

        Sequence dropSequence = DOTween.Sequence();
        activeAnimations.Add(target, dropSequence);
        dropSequence.SetTarget(targetTransform);

        // 物体从巨大的尺寸快速缩小到最终尺寸 (Vector3.one)
        dropSequence.Append(
            targetTransform.DOScale(Vector3.one, dropDuration)
                .SetEase(Ease.OutBack) // 使用 OutBack 带有回弹效果，增加动感
        );

        CameraShaker.Instance.StrongShake(); // 落地时轻微震动摄像机
        AudioManager.Instance.PlaySound(AudioManager.AudioType.ShopDrop); // 播放落地音效
        // 增加一个微小的震动效果，模拟落地冲击
        //dropSequence.Join(
        //    targetTransform.DOPunchScale(Vector3.one, dropDuration * 0.5f, 10, 0.5f)
        //);

        yield return dropSequence.WaitForCompletion();

        // 协程保护 3: 最终动画完成后检查
        if (target == null) { ClearActiveAnimation(target); yield break; }

        // 确保最终状态准确
        targetTransform.position = finalPosition;
        targetTransform.localScale = Vector3.one;

        // 成功完成动画后，从字典中移除
        ClearActiveAnimation(target);

        Debug.Log($"Object '{target.name}' 从天而降动画完成并清理。");
    }

    // 在ObjectAnimator类中添加这个方法
    /// <summary>
    /// 物体破裂动画，模拟物体受到冲击后破裂的效果（整合特效管理器和摄像机震动）
    /// </summary>
    /// <param name="target">要执行破裂动画的物体</param>
    /// <param name="duration">总动画时长（默认0.8秒）</param>
    /// <param name="explosionForce">爆炸力度，影响动画强度（默认1.0）</param>
    /// <param name="playClearEffect">是否播放清除特效（默认true）</param>
    /// <param name="shakeCamera">是否震动摄像机（默认true）</param>
    /// <param name="delay">动画开始前的延迟（默认0秒）</param>
    /// <returns></returns>
    public IEnumerator AnimateObjectBreak(
        GameObject target,
        float duration = 0.8f,
        float explosionForce = 1.0f,
        bool playClearEffect = true,
        bool shakeCamera = true,
        float delay = 0f)
    {
        // 核心保护：如果目标在协程启动时已被销毁，立即退出
        if (target == null) yield break;

        Transform targetTransform = target.transform;
        Vector3 originalScale = targetTransform.localScale;
        Vector3 originalPosition = targetTransform.position;
        Quaternion originalRotation = targetTransform.rotation;

        // 清理旧动画
        ClearActiveAnimation(target);

        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
            // 协程保护：延迟后检查
            if (target == null) { ClearActiveAnimation(target); yield break; }
        }

        // --- 第一阶段：收缩和震动 ---
        float shrinkDuration = duration * 0.15f;

        Sequence phase1 = DOTween.Sequence();
        activeAnimations.Add(target, phase1);
        phase1.SetTarget(targetTransform);

        // 收缩动画：1 -> 0.8
        phase1.Append(
            targetTransform.DOScale(originalScale * 0.8f, shrinkDuration)
                .SetEase(Ease.InQuad)
        );

        // 快速震动效果
        phase1.Join(
            targetTransform.DOPunchRotation(new Vector3(0, 0, 15f), shrinkDuration, 5, 0.5f)
        );

        yield return phase1.WaitForCompletion();

        // 协程保护：第一阶段完成后检查
        if (target == null) { ClearActiveAnimation(target); yield break; }
        ClearActiveAnimation(target);

        // --- 第二阶段：膨胀和旋转 ---
        float expandDuration = duration * 0.15f;

        Sequence phase2 = DOTween.Sequence();
        activeAnimations.Add(target, phase2);
        phase2.SetTarget(targetTransform);

        // 膨胀动画：0.8 -> 1.2
        phase2.Append(
            targetTransform.DOScale(originalScale * 1.2f, expandDuration)
                .SetEase(Ease.OutQuad)
        );

        // 快速旋转
        phase2.Join(
            targetTransform.DORotate(new Vector3(0, 0, originalRotation.eulerAngles.z + 180f), expandDuration)
                .SetEase(Ease.InOutQuad)
        );

        // 轻微的位置抖动
        phase2.Join(
            targetTransform.DOShakePosition(expandDuration, 0.1f * explosionForce, 10, 90f, false, false)
        );

        yield return phase2.WaitForCompletion();

        // 协程保护：第二阶段完成后检查
        if (target == null) { ClearActiveAnimation(target); yield break; }
        ClearActiveAnimation(target);

        // --- 第三阶段：爆炸破裂 ---
        float explosionDuration = duration * 0.4f;

        // 播放清除特效（使用EffectManager）
        if (playClearEffect && EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayClearEffect(originalPosition);
        }

        // 摄像机震动（使用CameraShaker）
        if (shakeCamera && CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(explosionDuration * 0.6f, 0.1f * explosionForce);
        }

        // 原物体爆炸动画（缩放消失）
        Sequence phase3 = DOTween.Sequence();
        activeAnimations.Add(target, phase3);
        phase3.SetTarget(targetTransform);

        // 爆炸式缩放：1.2 -> 0
        phase3.Append(
            targetTransform.DOScale(Vector3.zero, explosionDuration)
                .SetEase(Ease.OutBack)
        );

        // 爆炸旋转
        phase3.Join(
            targetTransform.DORotate(new Vector3(0, 0, originalRotation.eulerAngles.z + 720f), explosionDuration)
                .SetEase(Ease.OutQuad)
        );

        yield return phase3.WaitForCompletion();

        // 协程保护：最终检查
        if (target == null)
        {
            ClearActiveAnimation(target);
            yield break;
        }

        // 清理原物体动画
        ClearActiveAnimation(target);

        // 最终清理：如果原物体仍存在，确保其为隐藏状态
        if (target != null && target.activeSelf)
        {
            target.SetActive(false);
        }

        Debug.Log($"Object '{target.name}' 破裂动画完成。");
    }
}
