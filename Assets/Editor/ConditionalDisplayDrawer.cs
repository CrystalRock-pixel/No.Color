using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 告诉 Unity，当遇到 ConditionalDisplayAttribute 时，使用这个绘制器
[CustomPropertyDrawer(typeof(ConditionalDisplayAttribute))]
public class ConditionalDisplayDrawer : PropertyDrawer
{
    // 用于确定字段是否应该显示的标记
    private bool shouldBeDrawn = true;

    // 绘制检视面板GUI时调用
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 1. 获取自定义属性
        ConditionalDisplayAttribute condAttr = attribute as ConditionalDisplayAttribute;

        // 2. 找到作为条件的属性 (PropCardType)
        // property.serializedObject.FindProperty(string) 用于查找同一对象上的另一个属性
        SerializedProperty conditionalProp = property.serializedObject.FindProperty(condAttr.ConditionalField);

        if (conditionalProp != null)
        {
            // 3. 检查条件
            // 只有当条件属性的类型是枚举（Enum）时，我们才执行检查
            if (conditionalProp.propertyType == SerializedPropertyType.Enum)
            {
                // 获取当前枚举值的字符串名称
                string currentEnumName = conditionalProp.enumNames[conditionalProp.enumValueIndex];

                // 比较当前枚举值和我们在属性中设置的期望值
                shouldBeDrawn = (currentEnumName == condAttr.EnumValue);
            }
            else
            {
                // 如果条件属性不是枚举，则显示警告
                Debug.LogWarning($"ConditionalDisplayAttribute applied to '{property.name}' but '{condAttr.ConditionalField}' is not an Enum.");
                shouldBeDrawn = true; // 确保字段可见，方便调试
            }
        }
        else
        {
            Debug.LogError($"Conditional field '{condAttr.ConditionalField}' not found for property '{property.name}'.");
            shouldBeDrawn = true;
        }

        // 4. 根据条件绘制或跳过
        if (shouldBeDrawn)
        {
            // 如果条件满足，正常绘制该字段
            EditorGUI.PropertyField(position, property, label, true);
        }
        else
        {
            // 如果条件不满足，不绘制任何东西，但保留空间（虽然我们下面将空间设为0）
        }
    }

    // 计算字段所需的高度
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 如果字段应该显示，返回默认高度
        if (shouldBeDrawn)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        // 如果字段不应该显示，返回 0，使其不可见且不占用空间
        else
        {
            return 0f;
        }
    }
}