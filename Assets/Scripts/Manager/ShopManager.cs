using System.Collections;
using System.Collections.Generic;
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
    public float relicCardPanelPadding = 0.5f;
    public float propCardPanelPadding = 0.5f;
    public List<GameObject> relicCards = new List<GameObject>();
    public List<GameObject> propCards = new List<GameObject>();

    private void Awake()
    {
        if(Instance == null) Instance = this;
    }

    private void Start()
    {
        resourcesManager = ResourcesManager.Instance;
        //relicCardPanel.gameObject.SetActive(false);
        //propCardPanel.gameObject.SetActive(false);
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
    }

    public void RemoveCard(UICard card)
    {
        CardType cardType = card.GetCardInstance().cardData.type;
        if (cardType == CardType.Relic)
        {
            relicCards.Remove(card.gameObject);
        }
        else if (cardType == CardType.Prop)
        {
            propCards.Remove(card.gameObject);
        }
    }
    public void RepositionCards(RectTransform cardPanel, List<GameObject> cards, float panelPdadding)
    {
        int cardCount = cards.Count;
        if (cardCount == 0) return;

        // --- 1. 获取 UI 货架在世界空间中的边界 ---
        // corners 数组顺序：
        // 0: 左下角 (Bottom-Left)
        // 1: 左上角 (Top-Left)
        // 2: 右上角 (Top-Right)
        // 3: 右下角 (Bottom-Right)
        Vector3[] corners = new Vector3[4];
        cardPanel.GetWorldCorners(corners);

        // 确定左右边界（X 坐标）
        float leftWorldX = corners[0].x;
        float rightWorldX = corners[3].x;

        // 确定用于定位的 Y 坐标 (例如，使用货架底部和顶部中间的 Y 值)
        float targetY = (corners[0].y + corners[1].y) / 2f;

        // --- 2. 应用世界单位的内边距，确定可用范围 ---
        float availableLeftX = leftWorldX + panelPdadding;
        float availableRightX = rightWorldX - panelPdadding;
        float availableWidth = availableRightX - availableLeftX;

        // --- 3. 循环计算每个商品的世界坐标 X ---
        for (int i = 1; i < cardCount+1; i++)
        {
            GameObject card = cards[i-1];

            // 计算标准化位置 t (0.0 到 1.0)
            float t = 0f;
            if (cardCount >= 1)
            {
                // N个物品，N-1个间隔，t = i / (N-1) 均匀分布
                t = (float)i / (cardCount + 1);
            }
            //else if (cardCount == 1)
            //{
            //    t = 0.5f;
            //}
            // 根据 t 值在可用范围内进行插值 (Lerp)
            // t = 0 -> availableLeftX
            // t = 1 -> availableRightX
            float xWorldPos = availableLeftX + (t * availableWidth);

            // 4. 设置卡牌的世界位置
            Vector3 newPos = new Vector3(xWorldPos, targetY,-t);
            card.transform.position = newPos;
        }
    }
    public static void RepositionCards(RectTransform cardPanel,List<UICard> cards,float panelPdadding)
    {
        int cardCount = cards.Count;
        if (cardCount == 0) return;

        Vector3[] corners = new Vector3[4];
        cardPanel.GetWorldCorners(corners);

        float leftWorldX = corners[0].x;
        float rightWorldX = corners[3].x;

        float targetY = (corners[0].y + corners[1].y) / 2f;

        float availableLeftX = leftWorldX + panelPdadding;
        float availableRightX = rightWorldX - panelPdadding;
        float availableWidth = availableRightX - availableLeftX;

        for (int i = 1; i < cardCount + 1; i++)
        {
            UICard card = cards[i - 1];

            float t = 0f;
            if (cardCount >= 1)
            {
                t = (float)i / (cardCount + 1);
            }

            float xWorldPos = availableLeftX + (t * availableWidth);

            Vector3 newPos = new Vector3(xWorldPos, targetY, -t);
            card.transform.position = newPos;
        }
    }

    public void AddRelicCard(GameObject newCard)
    {
        relicCards.Add(newCard);
        newCard.transform.SetParent(relicCardPanel);
        newCard.transform.localScale = Vector3.one;
        //RepositionCards(relicCardPanel,relicCards,relicCardPanelPadding);
    }
    public void AddPropCard(GameObject newPropCard)
    {
        propCards.Add(newPropCard);
        newPropCard.transform.SetParent(propCardPanel);
        newPropCard.transform.localScale = Vector3.one;
        //RepositionCards(propCardPanel,propCards,propCardPanelPadding);
    }
}
