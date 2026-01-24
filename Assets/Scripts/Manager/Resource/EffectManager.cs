using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public enum FlowTextType
{
    None,
    AddChips,
    AddMagnifaction,
    Multiply,
    CardLevelUp,
}
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;
    public Canvas UIEffectCanvas;

    [Header("清除特效配置")]
    [SerializeField] private int maxConcurrentEffects = 10;
    [SerializeField] private float defaultEffectScale = 1.5f;

    private Queue<GameObject> activeClearEffects = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClearEffect(Vector2 clearPosition)
    {
        PlayClearEffect(clearPosition, defaultEffectScale, 1.0f);  // <-- 修改为调用重载方法
    }

    // 新增的重载方法 - 增强的清除特效
    /// <summary>
    /// 播放增强的清除特效
    /// </summary>
    /// <param name="clearPosition">特效位置</param>
    /// <param name="scale">特效缩放</param>
    /// <param name="intensity">特效强度</param>
    public void PlayClearEffect(Vector2 clearPosition, float scale, float intensity)  // <-- 新增的重载方法
    {
        // 1. 从ResourceManager获取粒子对象
        GameObject particleObject = ResourcesManager.Instance.GetClearCellEffect();

        if (particleObject == null)
        {
            return;
        }

        // 2. 设置位置和缩放
        particleObject.transform.position = clearPosition;
        particleObject.transform.localScale = Vector3.one * scale;  // <-- 新增缩放设置
        particleObject.transform.rotation = Quaternion.identity;

        ParticleSystem ps = particleObject.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            // 调整粒子系统参数基于强度  // <-- 新增强度调整逻辑
            var main = ps.main;
            var emission = ps.emission;
            var shape = ps.shape;

            // 根据强度调整粒子数量
            emission.rateOverTime = Mathf.RoundToInt(emission.rateOverTime.constant * intensity);

            // 根据强度调整粒子大小
            main.startSize = main.startSize.constant * Mathf.Sqrt(intensity);

            // 激活并播放
            particleObject.SetActive(true);
            ps.Play();

            // 3. 管理活跃特效数量  // <-- 新增队列管理
            activeClearEffects.Enqueue(particleObject);

            // 如果超过最大数量，销毁最旧的特效
            if (activeClearEffects.Count > maxConcurrentEffects)
            {
                GameObject oldestEffect = activeClearEffects.Dequeue();
                if (oldestEffect != null)
                {
                    Destroy(oldestEffect);
                }
            }

            // 4. 计算并调度销毁
            float duration = main.duration + main.startLifetime.constantMax;
            StartCoroutine(DestroyEffectAfterDelay(particleObject, duration));  // <-- 改为使用协程方法
        }
        else
        {
            Destroy(particleObject, 3f);
            Debug.LogError("EffectManager: 实例化的粒子对象缺少 ParticleSystem 组件。");
        }
    }

    // 新增方法 - 带动画的清除特效
    /// <summary>
    /// 播放带动画的清除特效（有缩放和移动效果）
    /// </summary>
    public void PlayAnimatedClearEffect(Vector2 clearPosition, Vector2? targetPosition = null)  // <-- 新增方法
    {
        GameObject particleObject = ResourcesManager.Instance.GetClearCellEffect();

        if (particleObject == null)
        {
            return;
        }

        // 设置初始状态
        particleObject.transform.position = clearPosition;
        particleObject.transform.localScale = Vector3.zero;  // <-- 新增初始缩放为0
        particleObject.transform.rotation = Quaternion.identity;

        ParticleSystem ps = particleObject.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            particleObject.SetActive(true);

            // 使用DOTween添加动画效果  // <-- 新增DOTween动画
            Sequence effectSequence = DOTween.Sequence();

            // 缩放动画
            effectSequence.Append(
                particleObject.transform.DOScale(Vector3.one * defaultEffectScale, 0.2f)
                    .SetEase(Ease.OutBack)
            );

            // 如果有目标位置，添加移动动画
            if (targetPosition.HasValue)
            {
                effectSequence.Append(
                    particleObject.transform.DOMove(targetPosition.Value, 0.4f)
                        .SetEase(Ease.InQuad)
                );
            }

            // 播放粒子系统
            ps.Play();

            // 管理活跃特效
            activeClearEffects.Enqueue(particleObject);

            if (activeClearEffects.Count > maxConcurrentEffects)
            {
                GameObject oldestEffect = activeClearEffects.Dequeue();
                if (oldestEffect != null)
                {
                    Destroy(oldestEffect);
                }
            }

            // 计算并调度销毁
            var main = ps.main;
            float duration = main.duration + main.startLifetime.constantMax + 0.5f;  // <-- 额外增加延迟
            StartCoroutine(DestroyEffectAfterDelay(particleObject, duration));
        }
        else
        {
            Destroy(particleObject, 3f);
        }
    }

    // 新增方法 - 组合特效
    /// <summary>
    /// 播放组合特效（清除特效+文字特效）
    /// </summary>
    public void PlayComboEffect(Vector2 position, string text, FlowTextType textType = FlowTextType.None)  // <-- 新增方法
    {
        // 播放清除特效
        PlayClearEffect(position, 1.8f, 1.5f);

        // 播放文字特效
        if (textType != FlowTextType.None)
        {
            PlayFlowTextEffect(text, position, textType);
        }

        // 摄像机震动
        if (CameraShaker.Instance != null)
        {
            CameraShaker.Instance.ShakeDOTween(0.4f, 0.15f, 25, 90f);
        }
    }

    // 新增方法 - 清空所有活跃特效
    /// <summary>
    /// 清空所有活跃特效
    /// </summary>
    public void ClearAllEffects()  // <-- 新增方法
    {
        foreach (var effect in activeClearEffects)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        activeClearEffects.Clear();
    }

    public void PlayFlowTextEffect(string text,Vector2 position,FlowTextType type)
    {
        GameObject flowTextEffect=ResourcesManager.Instance.GetFlowTextEffect();
        flowTextEffect.transform.SetParent(UIEffectCanvas.transform,false);
        flowTextEffect.transform.localScale = Vector3.one;

        float xOffset = Random.Range(-0.25f, 0.25f);
        float yOffset = Random.Range(-0.25f, 0.25f);

        flowTextEffect.transform.position = position+new Vector2(xOffset,yOffset);

        FlowText flowText= flowTextEffect.GetComponent<FlowText>();
        if (flowText != null)
        {
            string formattedText = GetColorForType(text, type).Item1;  // <-- 改为使用新增的颜色获取方法
            Color textColor = GetColorForType(text,type).Item2;  // <-- 改为使用新增的颜色获取方法
            flowText.Setup(formattedText, textColor);
        }
    }

    // 新增方法 - 根据类型获取颜色
    /// <summary>
    /// 根据类型获取颜色
    /// </summary>
    private (string,Color) GetColorForType(string text,FlowTextType type)  // <-- 新增方法
    {
        return type switch
        {
            FlowTextType.AddChips => ("+"+text, Color.blue),
            FlowTextType.AddMagnifaction =>("+"+text,Color.red),
            FlowTextType.Multiply => ("*"+text, new Color(1f, 0.5f, 0f)), // 橙色
            FlowTextType.CardLevelUp =>("升级", Color.yellow),
            _ => ("",Color.white)
        };
    }

    // 新增协程方法 - 延迟销毁特效
    /// <summary>
    /// 延迟销毁特效
    /// </summary>
    private IEnumerator DestroyEffectAfterDelay(GameObject effect, float delay)  // <-- 新增协程方法
    {
        yield return new WaitForSeconds(delay);

        if (effect != null)
        {
            // 先从队列中移除
            var newQueue = new Queue<GameObject>();
            foreach (var item in activeClearEffects)
            {
                if (item != effect)
                {
                    newQueue.Enqueue(item);
                }
            }
            activeClearEffects = newQueue;

            // 然后销毁
            Destroy(effect);
        }
    }

    public FlowTextType RelicTypeToFlowTextType(RelicCardType type)
    {
        if(type==RelicCardType.AddChips)
        {
            return FlowTextType.AddChips;
        }
        else if(type == RelicCardType.AddMagnification)
        {
            return FlowTextType.AddMagnifaction;
        }
        else if (type == RelicCardType.MultiplyMagnification)
        {
            return FlowTextType.Multiply;
        }
        else return FlowTextType.None;
    }
}
