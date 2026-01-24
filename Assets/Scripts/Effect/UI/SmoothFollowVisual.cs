using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有需要“身处子级但要平滑跟随父级”的视觉组件基类
/// </summary>
public abstract class SmoothFollowVisual : MonoBehaviour
{
    [Header("基础跟随设置")]
    [SerializeField] protected float followSpeed = 15f;

    protected Vector3 lastParentPosition;
    protected Transform parentTransform;

    protected virtual void Awake()
    {
        parentTransform = transform.parent;
    }

    protected virtual void Start()
    {
        if (parentTransform != null)
            lastParentPosition = parentTransform.position;
    }

    // 定义为 virtual，方便子类 override
    protected virtual void LateUpdate()
    {
        if (parentTransform == null) return;

        // --- 统一处理位移跟随 ---
        Vector3 parentDelta = parentTransform.position - lastParentPosition;
        transform.position -= parentDelta;
        transform.position = Vector3.Lerp(transform.position, parentTransform.position, followSpeed * Time.deltaTime);

        // 记录位置供下一帧或子类使用
        lastParentPosition = parentTransform.position;
    }
}