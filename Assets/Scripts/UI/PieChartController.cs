using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 颜色出现概率饼图控制器
/// </summary>
public class PieChartController : MonoBehaviour
{
    // 结构体，用于存储每个颜色及其对应的UI Image组件
    [System.Serializable]
    public struct PieSlice
    {
        public ColorType colorName;
        public Image sliceImage;
    }

    [Header("扇形设置")]
    public List<PieSlice> slices = new List<PieSlice>();

    // 用于表示当前的概率数据 (总和必须是 1.0)
    private Dictionary<ColorType, float> currentProbabilities = new Dictionary<ColorType, float>();

    // 核心函数：根据概率更新饼图
    public void UpdateChart()
    {
        Dictionary<ColorType , float> probabilities = ResourcesManager.Instance.CurrentProbabilities;
        UpdateChart(probabilities);
    }
    public void UpdateChart(Dictionary<ColorType, float> newProbabilities)
    {
        currentProbabilities = newProbabilities;

        // 关键变量：用于追踪当前已填充到哪个角度
        float currentFillAmount = 0f;

        // 遍历所有扇形并设置其填充量
        foreach (var slice in slices)
        {
            if (currentProbabilities.ContainsKey(slice.colorName))
            {
                float probability = currentProbabilities[slice.colorName];

                // 1. 设置这个扇形的填充量 (Fill Amount)
                slice.sliceImage.fillAmount = probability;

                // 2. 设置这个扇形的旋转角度 (Rotation)
                float rotationAngle = currentFillAmount * 360f;
                slice.sliceImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -rotationAngle);

                // 3. 更新已填充总量，供下一个扇形使用
                currentFillAmount += probability;
            }
        }
    }
}
