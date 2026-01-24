using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnifactionCell : ColorCell
{
    public int extraMagnification = 2;
    public override void Init(ColorType colorType, CellType cellType = CellType.Normal)
    {
        base.Init(colorType, cellType);
        cellName= "倍率块";
        description = $"获得{extraMagnification}点额外倍率";
    }
    public override void OnDestoryed()
    {
        base.OnDestoryed();
        ScoreManager.Instance.AddMagnification(extraMagnification);
        EffectManager.Instance.PlayFlowTextEffect(extraMagnification.ToString(), transform.position, FlowTextType.AddMagnifaction);
    }

}
