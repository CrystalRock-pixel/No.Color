using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnifactionCell : ColorCell
{
    public int extraMagnification = 2;
    public override void Init(ColorType colorType)
    {
        base.Init(colorType);
        cellName= "倍率块";
        description = $"获得{extraMagnification}点额外倍率";
        cellVisual.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite 
            = ResourcesManager.Instance.GetSpecialCellSprite(this.GetType().Name);
    }

    public override void OnCleared()
    {
        ScoreManager.Instance.AddMagnification(extraMagnification);
        EffectManager.Instance.PlayFlowTextEffect(extraMagnification.ToString(), transform.position, FlowTextType.AddMagnifaction);
        base.OnCleared();
    }

}
