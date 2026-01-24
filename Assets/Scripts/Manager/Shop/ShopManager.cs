using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public ResourcesManager resourcesManager;
    [Header("商店参数")]
    public int relicCardCount = 3;
    public int propCardCount = 3;

    [Header("面板参数")]
    public GameObject shopPanel;
    public GameObject shopShadow;
    public RectTransform relicCardPanel;
    public RectTransform propCardPanel;
    public Transform goodsGroup;
    public float relicCardPanelPadding = 0.5f;
    public float propCardPanelPadding = 0.5f;
    public List<GameObject> relicCards = new List<GameObject>();
    public List<GameObject> propCards = new List<GameObject>();

    public LayoutSlotManager CardLayoutManager;
    public LayoutSlotManager PropLayoutManager;

    private void Awake()
    {
        if(Instance == null) Instance = this;
    }

    private void Start()
    {
        resourcesManager = ResourcesManager.Instance;
    }

    private void Update()
    {
    }

    public void OpenShop()
    {
        UIManager.Instance.OpenShop();
        InitShop();
        StartCoroutine(ObjectAnimator.Instance.AnimateDropFromSky(shopPanel, shopShadow, shopPanel.transform.position,duration:2f));
    }
    public void CloseShop()
    {
        UIManager.Instance.CloseShop();
        foreach (GameObject card in relicCards)
        {
            Destroy(card);
        }
        relicCards.Clear();
        foreach(GameObject card in propCards)
        {
            Destroy(card);
        }
        propCards.Clear();
    }

    private void InitShop()
    {
        for (int i = 0; i < relicCardCount; i++)
        {
            AddRelicCard(resourcesManager.GetOneRandomRelicCard());
        }
        for (int i = 0;i < propCardCount; i++)
        {
            AddPropCard(resourcesManager.GetOneRandomPropCard());
        }

        InitLayout();
    }

    private void InitLayout()
    {
        CardLayoutManager.Init(relicCards.Select(card=>card.GetComponent<ILayoutMember>()).ToList());
        PropLayoutManager.Init(propCards.Select(prop => prop.GetComponent<ILayoutMember>()).ToList());
    }

    public void RemoveCard(UICard card)
    {
        CardType cardType = card.GetCardData().type;
        if (cardType == CardType.Relic)
        {
            relicCards.Remove(card.gameObject);
        }
        else if (cardType == CardType.Prop)
        {
            propCards.Remove(card.gameObject);
        }
    }

    public void AddRelicCard(GameObject newCard)
    {
        relicCards.Add(newCard);
        newCard.transform.SetParent(goodsGroup);
        newCard.transform.localScale = Vector3.one;
        //RepositionCards(relicCardPanel,relicCards,relicCardPanelPadding);
    }
    public void AddPropCard(GameObject newPropCard)
    {
        propCards.Add(newPropCard);
        newPropCard.transform.SetParent(goodsGroup);
        newPropCard.transform.localScale = Vector3.one;
        //RepositionCards(propCardPanel,propCards,propCardPanelPadding);
    }
}
