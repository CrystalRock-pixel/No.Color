using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRelicCard", menuName = "Card/RelicCard/ChipsMoneyCard")]
public class ChipsMoneyCard :NormalBeforeCaculateCard
{
    public override bool BeforeCalculateScore(ClearGeneralParameters paras)
    {
        value=Player.Instance.Money*2;
        bool tof= base.BeforeCalculateScore(paras);
        return tof;
    }
}
