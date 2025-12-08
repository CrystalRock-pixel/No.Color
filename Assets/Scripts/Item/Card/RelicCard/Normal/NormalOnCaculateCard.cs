using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRelicCard", menuName = "Card/RelicCard/NormalOCCard")]
public class NormalOnCaculateCard : NormalRelicCard, IOnCalculateScore
{
    public virtual bool OnCalculateScore(ClearGeneralParameters paras)
    {
        this.paras=paras;

        if (isUnrestricted || (!isUnrestricted && constraintsMet))
        {
            // 统一的卡牌效果执行逻辑
            switch (cardType)
            {
                case RelicCardType.AddChips:
                    ScoreManager.Instance.AddChips((int)value);
                    Debug.Log(cardName + "触发");
                    return true;
                case RelicCardType.AddMagnification:
                    ScoreManager.Instance.AddMagnification((int)value);
                    Debug.Log(cardName + "触发");
                    return true;
                case RelicCardType.MultiplyMagnification:
                    ScoreManager.Instance.MultiplyMagnification(value);
                    Debug.Log(cardName + "触发");
                    return true;
                default:
                    return false;
            }
        }

        return false;
    }
}
