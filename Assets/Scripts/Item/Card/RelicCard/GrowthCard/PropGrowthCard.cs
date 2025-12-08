using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRelicCard", menuName = "Card/RelicCard/GrowthCard/PropGrowth")]
public class PropGrowthCard : RelicCardData,IBeforeCalculateScore,IOnUseProp,IGrowthCard,IDescriptionShowValue
{
    public float perLevelValue;
    public PropCardType detectCardType;
    string addDescription;
    public bool BeforeCalculateScore(ClearGeneralParameters paras)
    {
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

    public bool OnUseProp(PropCardData cardData)
    {
        PropCardType cardType = cardData.cardType;
        Debug.Log(cardName + "触发，value：" + value);
        if (detectCardType == cardType)
        {
            value += perLevelValue;
            UpdateDescription();
            return true;
        }
        return false;
    }

    public void UpdateDescription()
    {
        if (string.IsNullOrEmpty(addDescription))
        {
            switch (cardType)
            {
                case RelicCardType.AddChips:
                    addDescription = "\n 当前 + " + value + "筹码";
                    break;
                case RelicCardType.AddMagnification:
                    addDescription = "\n 当前 + " + value + "倍率";
                    break;
                case RelicCardType.MultiplyMagnification:
                    addDescription = "\n 当前 * " + value + "倍率";
                    break;
            }
        }
        if (!string.IsNullOrEmpty(addDescription))
        {
            effectDescription = effectDescription.Replace(addDescription, "");
        }
        switch (cardType)
        {
            case RelicCardType.AddChips:
                addDescription = "\n 当前 + " + value + "筹码";
                break;
            case RelicCardType.AddMagnification:
                addDescription = "\n 当前 + " + value + "倍率";
                break;
            case RelicCardType.MultiplyMagnification:
                addDescription = "\n 当前 * " + value + "倍率";
                break;
        }
        effectDescription += addDescription;
    }
}
