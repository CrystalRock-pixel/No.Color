using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardDialog : MonoBehaviour
{
    public TMP_Text cardName;
    public TMP_Text cardDescription;

    public void SetCardInfo(string name, string description)
    {
        cardName.text = name;
        cardDescription.text = description;
    }

    public void SetCardInfo(CardData card)
    {
        cardName.text = card.cardName;
        cardDescription.text = card.effectDescription;
    }
}
