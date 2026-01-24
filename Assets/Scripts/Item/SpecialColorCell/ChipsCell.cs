using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipsCell : ColorCell
{
    public int extraChips = 10;
    public override void Init(ColorType colorType, CellType cellType = CellType.Normal)
    {
        base.Init(colorType, cellType);
        baseChips += extraChips;
        cellName= "筹码块";
        description = $"获得{extraChips}点额外筹码";
    }
}
