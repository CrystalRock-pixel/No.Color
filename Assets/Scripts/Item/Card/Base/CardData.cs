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

    protected ScoreManager ScoreManager => ScoreManager.Instance;
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

public interface IOnCalculateScore
{
    bool OnCalculateScore(ClearGeneralParameters paras);
}
public interface IBeforeCalculateScore
{
    bool BeforeCalculateScore(ClearGeneralParameters paras);
}

public interface IAfterCalculateScore
{
    bool AfterCalculateScore(ClearGeneralParameters paras);
}
public interface IOnUseCard
{
    void OnUseCard();
}

public interface IOnUseProp
{
    bool OnUseProp(PropCardData cardData);
}