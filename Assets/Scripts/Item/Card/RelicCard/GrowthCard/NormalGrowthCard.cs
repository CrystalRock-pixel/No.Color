using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRelicCard", menuName = "Card/RelicCard/GrowthCard/NormalGrowth")]
public class NormalGrowthCard : NormalRelicCard, IBeforeCalculateScore,IAfterCalculateScore,IGrowthCard,IDescriptionShowValue
{
    public int perLevelValue;
    string addDescription;
    public bool BeforeCalculateScore(ClearGeneralParameters paras)
    {
        switch (cardType)
        {
            case RelicCardType.AddChips:
                ScoreManager.Instance.AddChips((int)value);
                Debug.Log(cardName + "触发");
                return true ;
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

    public bool AfterCalculateScore (ClearGeneralParameters paras)
    {
        this.paras = paras;
        if (isUnrestricted || (!isUnrestricted && constraintsMet))
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

public interface IGrowthCard
{
}

public interface IDescriptionShowValue
{
    void UpdateDescription();
}
