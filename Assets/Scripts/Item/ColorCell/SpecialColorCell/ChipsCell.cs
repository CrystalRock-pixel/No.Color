using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipsCell : ColorCell
{
    public int extraChips = 10;
    public override void Init(ColorType colorType)
    {
        base.Init(colorType);
        baseChips += extraChips;
        cellName= "筹码块";
        description = $"获得{extraChips}点额外筹码";
        cellVisual.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite 
            = ResourcesManager.Instance.GetSpecialCellSprite(this.GetType().Name);
    }
}
