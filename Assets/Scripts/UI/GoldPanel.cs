using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldPanel : MonoBehaviour
{
    public GameObject goldSourcePerfab;
    public GameObject settlementButton;

    public Transform goldSourcePanel;

    public void ResetContent(Dictionary<string,int> goldSource)
    {
        for (int i = goldSourcePanel.childCount - 1; i >= 0; i--)
        {
            Transform child = goldSourcePanel.GetChild(i);
            Destroy(child.gameObject);
        }

        // 根据字典重新生成新的子对象
        foreach (var pair in goldSource)
        {
            AddGoldSource(pair.Key, pair.Value);
        }

        settlementButton.transform.GetChild(0).GetComponent<TMP_Text>().text = LevelManager.Instance.GetTotalMoney().ToString();
    }
    private void AddGoldSource(string source,int gold)
    {
        GameObject newSource = Instantiate(goldSourcePerfab, goldSourcePanel);
        newSource.transform.GetChild(0).GetComponent<TMP_Text>().text = source;
        newSource.transform.GetChild(1).GetComponent<TMP_Text>().text = "$" + gold.ToString();
    }
}
