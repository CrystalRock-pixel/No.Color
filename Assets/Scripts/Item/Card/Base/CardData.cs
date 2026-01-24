using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Relic,
    Prop,
    Effect
}

public enum CardCraftingType
{

}

public class CardData : ScriptableObject
{
    public string cardName;
    public string effectDescription;
    public string loreDescription;
    public CardType type;
    public Sprite cardImage;
    public int price;
    public int sellPrice => (int)(price * 0.5f);
    public virtual void OnAciquire()
    {
        Debug.Log("Acquired " + cardName);
    }

    public virtual void OnSold()
    {
        Debug.Log("Sold " + cardName);
    }

    public virtual void OnDestory()
    {

    }
}
/// <summary>
/// 卡牌触发：计算分数时触发
/// </summary>
public interface IOnCalculateScore
{
    bool OnCalculateScore(ClearGeneralParameters paras);
}

/// <summary>
/// 卡牌触发：计算分数前触发
/// </summary>
public interface IBeforeCalculateScore
{
    bool BeforeCalculateScore(ClearGeneralParameters paras);
}

/// <summary>
/// 卡牌触发：计算分数后触发
/// </summary>

public interface IAfterCalculateScore
{
    bool AfterCalculateScore(ClearGeneralParameters paras);
}

/// <summary>
/// 卡牌触发：使用道具时触发
/// </summary>
public interface IOnUseProp
{
    bool OnUseProp(PropData cardData);
}