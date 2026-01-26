using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static ColorCell;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    private CardHandler cardHandler;
    private PropHandler propHandler;


    private GridManager gridManager;
    private ScoreManager scoreManager;
    private ShopManager shopManager;
    private LevelManager levelManager;
    private ClearSequenceService clearSequenceService;

    private UICard currentSelectedCard;
    private UIProp currentSelectedProp;

    public RectTransform relicCardPanel;
    public RectTransform propCardPanel;
    public Transform GoodsGroup;

    private ColorCell currentHoveredCell;

    private LayoutSlotManager cardSlotManager;

    private int money = 10;
    public int Money
    {
        get { return money; }
        set
        {
            money = value;
            OnMoneyChanged?.Invoke();
        }
    }
    public static event Action OnMoneyChanged;
    public int eliminateTimes=>LevelManager.Instance.EliminateTimes;
    public int refreshTimes => LevelManager.Instance.RefreshTimes;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        cardHandler = GetComponent<CardHandler>();
        propHandler=GetComponent<PropHandler>();


        gridManager = GridManager.Instance;
        scoreManager = ScoreManager.Instance;
        shopManager = ShopManager.Instance;
        levelManager = LevelManager.Instance;
        clearSequenceService= ClearSequenceService.Instance;

        cardSlotManager=relicCardPanel.GetComponent<LayoutSlotManager>();
        cardSlotManager.OnLayoutOrderChanged += UpdateCardOrder;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            gridManager.GenerateMap();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            gridManager.ResetMap();
        }
    }
    public void OnMouseCellHover(ColorCell cell)
    {
        if (currentHoveredCell != cell||currentHoveredCell==null)
        {
            currentHoveredCell = null;
            clearSequenceService.ClearPrediction();
        }
        currentHoveredCell = cell;
        if (currentHoveredCell != null)
        {
            clearSequenceService.StartClearReadyFlow(currentHoveredCell.location, currentHoveredCell.colorType);
        }
    }

    public void OnMouseClickCell(ColorCell cell)
    {
         clearSequenceService.StartClearFlow(cell.location, cell.colorType);
         levelManager.SubEliminateTimes();
    }

    public void OnMouseRightClickCell(ColorCell cell)
    {
         clearSequenceService.StartSwitchColorFlow(cell);
    }

    public void HandleCardClicked(Transform clickedTransform)
    {
        UICard clickedCard = clickedTransform.GetComponent<UICard>();
        if (clickedCard != null)
        {
            if (currentSelectedCard != null && currentSelectedCard != clickedCard)
            {
                // 选中了与当前不同的卡牌：先取消旧卡选中，再选中新卡
                currentSelectedCard.SetSelected(false);
                clickedCard.SetSelected(true);
                currentSelectedCard = clickedCard;
            }
            else if (currentSelectedCard == clickedCard)
            {
                // 选中了当前已选的卡牌：取消选中
                clickedCard.SetSelected(false);
                currentSelectedCard = null;
            }
            else // 当前没有选中的卡牌：选中新卡
            {
                clickedCard.SetSelected(true);
                currentSelectedCard = clickedCard;
            }
        }
        UIProp clickedProp = clickedTransform.GetComponent<UIProp>();
        if(clickedProp != null)
        {
            if (currentSelectedProp != null && currentSelectedProp != clickedProp)
            {
                // 选中了与当前不同的卡牌：先取消旧卡选中，再选中新卡
                currentSelectedProp.SetSelected(false);
                clickedProp.SetSelected(true);
                currentSelectedProp = clickedProp;
            }
            else if (currentSelectedProp == clickedProp)
            {
                // 选中了当前已选的卡牌：取消选中
                clickedProp.SetSelected(false);
                currentSelectedProp = null;
            }
            else // 当前没有选中的卡牌：选中新卡
            {
                clickedCard.SetSelected(true);
                currentSelectedProp = clickedProp;
            }
        }
    }

    public bool Buy(IGood good)
    {
        if (Money >= good.Price)
        {
            Money -= good.Price;
            UICard uiCard = good.Transform.GetComponent<UICard>();
            if (uiCard != null)
            {
                AddGood(uiCard);
                currentSelectedCard = null;
            }
            return true;
        }
        else
        {
            Debug.Log("买不起别摸");
            return false;
        }
    }

    public void AddGood(IGood good)
    {
        if (good is UICard uiCard)
        {
            uiCard.transform.SetParent(GoodsGroup, false);
            uiCard.ChangeLayoutManager(cardSlotManager);
            cardHandler.AddCard(uiCard);
        }
        else if(good is UIProp uiProp)
        {
            propHandler.AddProp(uiProp);
        }
    }
    public void Sell(IGood good)
    {
        Money += good.SellPrice;
        UICard uiCard = good.Transform.GetComponent<UICard>();
        if (uiCard != null)
        {
            RemoveCard(uiCard);
        }
    }
    public void RemoveCard(UICard uiCard)
    {
        cardHandler.RemoveCard(uiCard);
        currentSelectedCard = null;
    }
    public void AddMoney(int money)
    {
        this.Money += money;
    }

    private void UpdateCardOrder()
    {
        List<ILayoutMember> members = cardSlotManager.GetOrderedMembers();
        List<UICard> uiCards = cardHandler.cards;
        var orderedCards = members
            .Select(member => member as UICard)
            .Where(card => card != null && uiCards.Contains(card))
            .ToList();
        cardHandler.cards = orderedCards;
    }
}
