using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuySellCard : MonoBehaviour
{
    public Button buyButton;
    public Button sellButton;
    public TMP_Text priceText;

    public UICard uiCard;

    private void SetUp(CardData cardInstance, bool isBought)
    {
        if (isBought)
        {
            priceText.text = "$" + cardInstance.sellPrice.ToString();
            buyButton.gameObject.SetActive(false);
            sellButton.gameObject.SetActive(true);
        }
        else
        {
            priceText.text = "$" + cardInstance.price.ToString();
            buyButton.gameObject.SetActive(true);
            sellButton.gameObject.SetActive(false);
        }
    }
    public void SetUp(UICard uiCard)
    {
        SetUp(uiCard.GetCardData(), uiCard.isBought);
        this.uiCard = uiCard;
    }

    public void OnBuy()
    {
        Debug.Log("Buying card: " + uiCard.CardName);
        
        uiCard.OnBuy();
    }
    public void OnSell()
    {
        Debug.Log("Selling card: " + uiCard.CardName);

        uiCard.OnSell();
    }
}
