using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldPanel : MonoBehaviour
{
    public GameObject goldSourcePerfab;
    public GameObject settlementButton;

    public void ResetContent(Dictionary<string,int> goldSource)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            // 检查是否是需要保留的结算按钮
            if (child.gameObject != settlementButton)
            {
                // 立即销毁 GameObject
                Destroy(child.gameObject);
                // 如果是在编辑器模式下，使用 DestroyImmediate(child.gameObject);
            }
        }

        // 根据字典重新生成新的子对象
        foreach (var pair in goldSource)
        {
            AddGoldSource(pair.Key, pair.Value);
        }
    }
    private void AddGoldSource(string source,int gold)
    {
        GameObject newSource = Instantiate(goldSourcePerfab, transform);
        newSource.transform.GetChild(0).GetComponent<TMP_Text>().text = source;
        newSource.transform.GetChild(1).GetComponent<TMP_Text>().text = "$" + gold.ToString();
        if (settlementButton != null)
        {
            int bSiblingIndex = settlementButton.transform.GetSiblingIndex();
            // 将新的子对象设置到该索引位置，使其位于结算按钮的上方
            newSource.transform.SetSiblingIndex(bSiblingIndex);
        }
        else
        {
            Debug.LogWarning("Settlement Button 引用为空，新的子对象将位于列表末尾。");
        }
    }
}
