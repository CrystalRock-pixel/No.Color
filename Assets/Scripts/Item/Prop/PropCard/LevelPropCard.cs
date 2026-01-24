using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPropCard", menuName = "Card/PropCard/LevelUpPropCard")]
public class LevelPropCard : PropData
{
    [ConditionalDisplay(nameof(cardType), PropCardType.ScaleLvUp)]
    public ComboScale comboScale;
    [ConditionalDisplay(nameof(cardType), PropCardType.StructLvUp)]
    public ComboStruct comboStruct;
    [ConditionalDisplay(nameof(cardType), PropCardType.ColorWeightUp)]
    public ColorType colorType;
    public override void OnAciquire()
    {
        base.OnAciquire();
    }

    public override void OnSold()
    {
        base.OnSold();
    }

    public override void OnUse()
    {
        base.OnUse();
        if (cardType == PropCardType.ScaleLvUp)
        {
            ScoreManager.Instance.UpgradeComboScale(comboScale);
        }
        else if (cardType == PropCardType.StructLvUp)
        {
            ScoreManager.Instance.UpgradeComboStruct(comboStruct);
        }
        else if (cardType == PropCardType.ColorWeightUp)
        {
            ResourcesManager.Instance.IncreaseColorProbability(colorType, 0.5f);
        }
    }
}
