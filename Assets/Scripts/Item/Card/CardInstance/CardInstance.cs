using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInstance
{
    public CardData cardData;
    public UICard uiCard;
    public string descriptionStr;
    public CardInstance(CardData cardData, UICard uiCard)
    {
        this.cardData = cardData;
        this.uiCard = uiCard;
        descriptionStr=cardData.effectDescription;
    }
}
