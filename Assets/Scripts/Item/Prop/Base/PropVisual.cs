using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropVisual : SmoothFollowVisual
{
    private UIProp parentCard;

    protected override void Awake()
    {
        base.Awake();
        parentCard = GetComponent<UIProp>();
    }
}
