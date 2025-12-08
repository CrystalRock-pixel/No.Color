using System;
using UnityEngine;

// 确保这个属性可以应用于字段（Field）
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ConditionalDisplayAttribute : PropertyAttribute
{
    // 用于检查条件的枚举变量名
    public string ConditionalField { get; private set; }
    // 触发显示的枚举值（是一个字符串，方便处理）
    public string EnumValue { get; private set; }

    /// <summary>
    /// 只有当 ConditionalField 的值等于 EnumValue 时，才显示该字段。
    /// </summary>
    /// <param name="conditionalField">作为条件的枚举变量名 (e.g., "PropCardType")</param>
    /// <param name="enumValue">触发显示的枚举值 (e.g., "ScaleLvUp")</param>
    public ConditionalDisplayAttribute(string conditionalField, object enumValue)
    {
        ConditionalField = conditionalField;
        // 将枚举值转换为字符串，便于 Property Drawer 中进行比较
        EnumValue = enumValue.ToString();
    }
}