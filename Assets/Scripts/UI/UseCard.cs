using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseCard : MonoBehaviour
{
    private UICard uiCard;
    public void SetUp(UICard uiCard)
    {
        this.uiCard = uiCard;
    }

    public void OnUse()
    {
        //if (this.uiCard != null&&uiCard.GetCardData() is PropData)
        //{
        //    PropData cardData =(PropData) uiCard.GetCardData();
        //    AudioManager.Instance.PlaySound(AudioManager.AudioType.CardUse);
        //    cardData.OnUse();
        //    uiCard.OnDestroyed();
        //}
    }
}
