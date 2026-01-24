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
    public Player player => Player.Instance;

    public List<UICard> cards =new List<UICard>();
    public List<UICard> props =new List<UICard>();
    public static Action<PropData> OnUseProp;

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

    public void AddCard(UICard cardInstance)
    {
        if(cardInstance.GetCardData() is PropData)
        {
            props.Add(cardInstance);
        }
        else
        {
            cards.Add(cardInstance);
        }
    }
    public void RemoveCard(UICard cardInstance)
    {
        if (cardInstance.GetCardData() is PropData)
        {
            props.Remove(cardInstance);
        }
        else
        {
            cards.Remove(cardInstance);
        }
    }
    public void SoldCard(UICard cardInstance)
    {
        cards.Remove(cardInstance);
        cardInstance.GetCardData().OnSold();
    }


    private void Start()
    {
        OnUseProp += HandleOnUseProp;
    }

    public IEnumerator ExecuteCalculateScore(ClearGeneralParameters cells)
    {
        foreach (var cardInstance in cards)
        {
            if (cardInstance.GetCardData() is IOnCalculateScore caculateScoreCard)
            {
                if (caculateScoreCard.OnCalculateScore(cells))
                {
                    AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                    yield return cardInstance.TryPlayAnimation(true);
                }
            }
        }
    }
    public IEnumerator ExecuteAfterCalculateScore(ClearGeneralParameters cells)
    {
        foreach (var cardInstance in cards)
        {
            if (cardInstance.GetCardData() is IAfterCalculateScore destoryCellsCard)
            {
                if (destoryCellsCard.AfterCalculateScore(cells))
                {
                    if (cardInstance.GetCardData() is NormalGrowthCard)
                    {
                        AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                        cardInstance.TryPlayAnimation(false, true);
                    }
                    else
                    {
                        AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                        yield return cardInstance.TryPlayAnimation(true);
                    }
                }
            }
        }
    }
    public IEnumerator ExecuteBeforeCalculateScore(ClearGeneralParameters cells)
    {
        foreach (var cardInstance in cards)
        {
            if (cardInstance.GetCardData() is IBeforeCalculateScore beforeCalculateScoreCard)
            {
                if (beforeCalculateScoreCard.BeforeCalculateScore(cells))
                {
                    AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                    yield return cardInstance.TryPlayAnimation(true);
                }
            }
        }
    }
    private void HandleOnUseProp(PropData cardData)
    {
        foreach(var cardInstance in cards)
        {
            if(cardInstance.GetCardData() is IOnUseProp onUseProp)
            {
                if (onUseProp.OnUseProp(cardData))
                {
                    if (cardInstance.GetCardData() is PropGrowthCard)
                    {
                        AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                        cardInstance.TryPlayAnimation(false, true);
                    }
                    else
                    {
                        AudioManager.Instance.PlaySound(AudioManager.AudioType.CardEffect);
                        cardInstance.TryPlayAnimation(false);
                    }
                }
            }
        }
    }

}
