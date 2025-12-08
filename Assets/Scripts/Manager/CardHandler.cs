using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ClearGeneralParameters   //遗物卡检测的一般参数
{
    public ColorType colorType;
    public ComboScale comboScale;
    public ComboStruct comboStruct;
    public ClearGeneralParameters(ColorType colorType, ComboScale comboScale, ComboStruct comboStruct)
    {
        this.colorType = colorType;
        this.comboScale = comboScale;
        this.comboStruct = comboStruct;
    }
}
public class CardHandler : MonoBehaviour
{
    //private static RelicHandler _instance;
    public static CardHandler Instance;

    public List<CardInstance> cards = new List<CardInstance>();
    public List<CardInstance> props = new List<CardInstance>();
    //public static Action<ClearGeneralParameters> OnCalculateScore;
    //public static Action<ClearGeneralParameters> BeforeCalculateScore;
    //public static Action<ClearGeneralParameters> AfterCalculateScore;
    public static Action<PropCardData> OnUseProp;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void AddCard(CardInstance cardInstance)
    {
        if(cardInstance.cardData is PropCardData)
        {
            props.Add(cardInstance);
        }
        else
        {
            cards.Add(cardInstance);
        }
    }
    public void RemoveCard(CardInstance cardInstance)
    {
        if (cardInstance.cardData is PropCardData)
        {
            props.Remove(cardInstance);
        }
        else
        {
            cards.Remove(cardInstance);
        }
    }
    public void SoldCard(CardInstance cardInstance)
    {
        cards.Remove(cardInstance);
        cardInstance.cardData.OnSold();
    }


    private void Start()
    {
        //OnCalculateScore += HandleCalculateScore;
        //AfterCalculateScore += HandleAfterCalculateScore;
        //BeforeCalculateScore += HandleBeforeCalculateScore;
        OnUseProp += HandleOnUseProp;
    }

    private void HandleCalculateScore(ClearGeneralParameters cells)
    {
        foreach (var cardInstance in cards)
        {
            if (cardInstance.cardData is IOnCalculateScore caculateScoreCard)
            {
                if (caculateScoreCard.OnCalculateScore(cells))
                {
                    cardInstance.uiCard.TryPlayAnimation(false);
                }
            }
        }
    }
    private void HandleAfterCalculateScore(ClearGeneralParameters cells)
    {
        foreach (var cardInstance in cards)
        {
            if (cardInstance.cardData is IAfterCalculateScore destoryCellsCard)
            {
                if(destoryCellsCard.AfterCalculateScore(cells))
                    cardInstance.uiCard.TryPlayAnimation(false);
            }
        }
    }
    private void HandleBeforeCalculateScore(ClearGeneralParameters cells)
    {
        foreach (var cardInstance in cards)
        {
            if (cardInstance.cardData is IBeforeCalculateScore beforeCalculateScoreCard)
            {
                if(beforeCalculateScoreCard.BeforeCalculateScore(cells))
                       cardInstance.uiCard.TryPlayAnimation(false);
            }
        }
    }

    public IEnumerator ExecuteCalculateScore(ClearGeneralParameters cells)
    {
        foreach (var cardInstance in cards)
        {
            if (cardInstance.cardData is IOnCalculateScore caculateScoreCard)
            {
                if (caculateScoreCard.OnCalculateScore(cells))
                {
                    AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                    yield return cardInstance.uiCard.TryPlayAnimation(true);
                }
            }
        }
    }
    public IEnumerator ExecuteAfterCalculateScore(ClearGeneralParameters cells)
    {
        foreach (var cardInstance in cards)
        {
            if (cardInstance.cardData is IAfterCalculateScore destoryCellsCard)
            {
                if (destoryCellsCard.AfterCalculateScore(cells))
                {
                    if (cardInstance.cardData is NormalGrowthCard)
                    {
                        AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                        cardInstance.uiCard.TryPlayAnimation(false, true);
                    }
                    else
                    {
                        AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                        yield return cardInstance.uiCard.TryPlayAnimation(true);
                    }
                }
            }
        }
    }

    public IEnumerator ExecuteBeforeCalculateScore(ClearGeneralParameters cells)
    {
        foreach (var cardInstance in cards)
        {
            if (cardInstance.cardData is IBeforeCalculateScore beforeCalculateScoreCard)
            {
                if (beforeCalculateScoreCard.BeforeCalculateScore(cells))
                {
                    AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                    yield return cardInstance.uiCard.TryPlayAnimation(true);
                }
            }
        }
    }
    private void HandleOnUseProp(PropCardData cardData)
    {
        foreach(var cardInstance in cards)
        {
            if(cardInstance.cardData is IOnUseProp onUseProp)
            {
                if (onUseProp.OnUseProp(cardData))
                {
                    if (cardInstance.cardData is PropGrowthCard)
                    {
                        AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                        cardInstance.uiCard.TryPlayAnimation(false, true);
                    }
                    else
                    {
                        AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                        cardInstance.uiCard.TryPlayAnimation(false);
                    }
                }
            }
        }
    }

}
