using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RelicCardType
{
    AddChips,
    AddMagnification,
    MultiplyMagnification,
    Other,
}
public class RelicCardData : CardData
{
    public RelicCardType cardType;
    public float value;
    public override void OnAciquire()
    {
        base.OnAciquire();
    }

    public override void OnSold()
    {
        base.OnSold();
    }
}
