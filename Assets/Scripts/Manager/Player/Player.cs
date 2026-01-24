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
        cardHandler = CardHandler.Instance;
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
        //if (IsPointerOverUIObject())
        //{
        //    // 如果鼠标悬停在 UI 上，但之前悬停在 ColorCell 上，需要清除 ColorCell 的状态
        if (currentHoveredCell != cell||currentHoveredCell==null)
        {
            currentHoveredCell = null;
            clearSequenceService.ClearPrediction();
        }
        //    return; 
        //}

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

    /// <summary>
    /// 检查当前鼠标位置是否悬停在任何 Unity UI (UGUI) 元素上。
    /// </summary>
    private bool IsPointerOverUIObject()
    {
        // 获取当前 EventSystem 实例
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        // 设置事件数据，使用当前鼠标位置
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        // 创建一个列表来存储射线检测结果
        List<RaycastResult> results = new List<RaycastResult>();

        // 使用 EventSystem 的 Raycaster 对 UI 进行射线检测
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        // 如果 results 列表不为空，说明射线击中了 UI 元素
        return results.Count > 0;
    }

    public bool Buy(IGood good)
    {
        if (Money >= good.Price)
        {
            Money -= good.Price;
            UICard uiCard = good.Transform.GetComponent<UICard>();
            if (uiCard != null)
            {
                AddCard(uiCard);
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

    public void AddCard(UICard uiCard)
    {
        uiCard.transform.SetParent(GoodsGroup, false);
        uiCard.ChangeLayoutManager(cardSlotManager);
        cardHandler.AddCard(uiCard);
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
