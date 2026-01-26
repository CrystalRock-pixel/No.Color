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
    public List<GameObject> cards = new List<GameObject>();
    public List<GameObject> prop = new List<GameObject>();

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
        foreach (GameObject card in cards)
        {
            Destroy(card);
        }
        cards.Clear();
        foreach(GameObject card in prop)
        {
            Destroy(card);
        }
        prop.Clear();
    }

    private void InitShop()
    {
        for (int i = 0; i < relicCardCount; i++)
        {
            AddCard(resourcesManager.GetOneRandomRelicCard());
        }
        for (int i = 0;i < propCardCount; i++)
        {
            AddProp(resourcesManager.GetOneRandomPropCard());
        }

        InitLayout();
    }

    private void InitLayout()
    {
        CardLayoutManager.Init(cards.Select(card=>card.GetComponent<ILayoutMember>()).ToList());
        PropLayoutManager.Init(prop.Select(prop => prop.GetComponent<ILayoutMember>()).ToList());
    }

    public void RemoveCard(IGood good)
    {
        if (cards.Select(card=>card.GetComponent<IGood>()).Contains(good))
        {
            cards.Remove(good.Transform.gameObject);
        }
        else if (prop.Select(prop => prop.GetComponent<IGood>()).Contains(good))
        {
            prop.Remove(good.Transform.gameObject);
        }
        else
        {
            Debug.Log("错误：商店中没有该物品"+good.Transform.name);
        }
    }

    public void AddGood(IGood good)
    {
        if (good is UICard)
        {
            UICard uiCard=good as UICard;
            AddCard(good.Transform.gameObject);
        }
        else if (good is UIProp)
        {
            AddProp(good.Transform.gameObject);
        }
        else
        {
            Debug.Log("错误：无法添加该物品，未知的商品类型" + good.Transform.name);
        }
    }

    private void AddCard(GameObject newCard)
    {
        cards.Add(newCard);
        newCard.transform.SetParent(goodsGroup);
        newCard.transform.localScale = Vector3.one;
        //RepositionCards(relicCardPanel,relicCards,relicCardPanelPadding);
    }
    private void AddProp(GameObject newPropCard)
    {
        prop.Add(newPropCard);
        newPropCard.transform.SetParent(goodsGroup);
        newPropCard.transform.localScale = Vector3.one;
        //RepositionCards(propCardPanel,propCards,propCardPanelPadding);
    }
}
