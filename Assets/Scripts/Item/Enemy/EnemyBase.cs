using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class EnemyBase : ScriptableObject
{
    public string enemyName;
    public string description;
    public float scoreMultiplication;
    public int bonus;

    // 初始化方法：在 ModifierManager 激活时调用
    public virtual void Initialize() { }

    // 核心：订阅游戏事件的方法 (例如，玩家进行消除操作时)
    public abstract void SubscribeToEvents();

    // 核心：解除事件订阅的方法 (关卡结束或切换时调用，防止内存泄漏)
    public abstract void UnsubscribeFromEvents();
}
