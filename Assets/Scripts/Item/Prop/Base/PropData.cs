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

public class PropData : ScriptableObject,IOnUse
{
    public string propName;
    public string effectDescription;
    public Sprite propImage;
    public int price;
    public int sellPrice => (int)(price * 0.5f);

    public PropCardType cardType;
    public virtual void OnAciquire()
    {
    }

    public virtual void OnSold()
    {
    }

    public virtual void OnDestory()
    {
    }

    public virtual void OnUse()
    {
        CardHandler.OnUseProp?.Invoke(this);
    }
}

public interface IOnUse
{
    void OnUse();
}
