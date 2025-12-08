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
        if (this.uiCard != null&&uiCard.GetCardInstance().cardData is PropCardData)
        {
            PropCardData cardData =(PropCardData) uiCard.GetCardInstance().cardData;
            AudioManager.Instance.PlaySound(AudioManager.AudioType.CardUse);
            cardData.OnUseCard();
            uiCard.OnDestroyed();
        }
    }
}
