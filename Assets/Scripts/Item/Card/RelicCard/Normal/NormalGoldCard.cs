using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalGoldCard : RelicCardData, IAfterCalculateScore
{
    public bool AfterCalculateScore(ClearGeneralParameters paras)
    {
        if (!LevelManager.Instance.settlementGoldCompoent.ContainsKey("利息卡"))
        {
            LevelManager.Instance.settlementGoldCompoent.Add("利息卡", 4);
        }
        return true;
    }
}
