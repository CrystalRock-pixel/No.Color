using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PropCardType
{
    None,
    ScaleLvUp,
    StructLvUp,
    ColorWeightUp,
}

public class PropCardData : CardData,IOnUseCard
{
    public PropCardType cardType;
    public override void OnAciquire()
    {
        base.OnAciquire();
    }

    public override void OnSold()
    {
        base.OnSold();
    }

    public virtual void OnUseCard()
    {
        CardHandler.OnUseProp?.Invoke(this);
    }
}
