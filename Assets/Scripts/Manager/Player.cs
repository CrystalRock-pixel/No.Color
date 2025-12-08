using System;
using System.Collections.Generic;
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

    public RectTransform relicCardPanel;
    public RectTransform propCardPanel;

    private ColorCell currentHoveredCell;

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

    public List<UICard> relicCards = new List<UICard>();
    public List<UICard> propCards = new List<UICard>();

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
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            DetectMouseClickPoint();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            DetectMouseRightClick();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            gridManager.GenerateMap();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            gridManager.ResetMap();
        }
        DetectMouseHover();
    }
    private void DetectMouseHover()
    {
        if (IsPointerOverUIObject())
        {
            // 如果鼠标悬停在 UI 上，但之前悬停在 ColorCell 上，需要清除 ColorCell 的状态
            if (currentHoveredCell != null)
            {
                currentHoveredCell = null;
                clearSequenceService.ClearPrediction();
            }
            return; 
        }

        Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePoint, Vector2.zero);

        ColorCell newHoveredCell = null;

        if (hit.collider != null && hit.collider.CompareTag("ColorCell"))
        {
            newHoveredCell = hit.collider.GetComponent<ColorCell>();
        }

        // 仅在新悬停的单元格不同于当前悬停的单元格时更新
        if (newHoveredCell != currentHoveredCell)
        {
            currentHoveredCell?.MouseExit();
            currentHoveredCell = newHoveredCell;
            currentHoveredCell?.MouseEnter();

            if (currentHoveredCell != null)
            {
                clearSequenceService.StartClearReadyFlow(currentHoveredCell.location, currentHoveredCell.colorType);
            }
            else
            {
                // 鼠标移开 ColorCell，清除预测显示
                clearSequenceService.ClearPrediction();
            }
        }
    }

    private void DetectMouseClickPoint()
    {
        if (IsPointerOverUIObject())
        {
            return;
        }
        Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePoint, Vector2.zero);
        if (hit.collider != null)
        {
            if(hit.collider.CompareTag("ColorCell"))
            {
                ColorCell cell = hit.collider.GetComponent<ColorCell>();
                if (cell != null)
                {
                    clearSequenceService.StartClearFlow(cell.location, cell.colorType);
                    levelManager.SubEliminateTimes();
                    //gridManager.ClearConnectedColors(cell.location, cell.colorType);     //消除操作
                }
            }
            //else if(hit.collider.CompareTag("Card"))
            //{
            //    UICard uiCard = hit.collider.GetComponent<UICard>();
            //    if (uiCard != null)
            //    {
            //        if(currentSelectedCard != null && currentSelectedCard != uiCard)  // 选中了与当前不同的卡牌
            //        {
            //            currentSelectedCard.OnClicked();
            //            uiCard.OnClicked();
            //            currentSelectedCard = uiCard;
            //        }
            //        else if(currentSelectedCard == uiCard)  // 选中了当前已选的卡牌
            //        {
            //             uiCard.OnClicked();
            //            currentSelectedCard = null;
            //        }
            //        else  // 当前没有选中的卡牌
            //        {
            //            uiCard.OnClicked();
            //            currentSelectedCard = uiCard;
            //        }
            //    }
            //}
        }
    }

    private void DetectMouseRightClick()
    {
        if (IsPointerOverUIObject())
        {
            return;
        }
        Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePoint, Vector2.zero);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("ColorCell"))
            {
                ColorCell cell = hit.collider.GetComponent<ColorCell>();
                if (cell != null)
                {
                    clearSequenceService.StartSwitchColorFlow(cell);
                }
            }
        }
    }

    public void HandleCardClicked(UICard clickedCard)
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

    public bool BuyCard(UICard uiCard)
    {
        if (Money >= uiCard.GetCardInstance().cardData.price)
        {
            Money -= uiCard.GetCardInstance().cardData.price;
            AddCard(uiCard);
            currentSelectedCard = null;
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
        //uiCard.transform.parent=relicCardPanel;
        if (uiCard.GetCardInstance().cardData is PropCardData)
        {
            propCards.Add(uiCard);
            //ShopManager.RepositionCards(propCardPanel, propCards, 0.5f);
            uiCard.transform.SetParent(propCardPanel, false);
            cardHandler.AddCard(uiCard.GetCardInstance());
        }
        else
        {
            relicCards.Add(uiCard);
            //ShopManager.RepositionCards(relicCardPanel, relicCards, 0.5f);
            uiCard.transform.SetParent(relicCardPanel, false);
            cardHandler.AddCard(uiCard.GetCardInstance());
        }
    }
    public void SellCard(UICard uiCard)
    {
        Money += uiCard.GetCardInstance().cardData.sellPrice;
        RemoveCard(uiCard);
        //ShopManager.RepositionCards(relicCardPanel, relicCards, 0.5f);
        //cardHandler.RemoveCard(uiCard.GetCardInstance());
        //currentSelectedCard = null;
    }
    public void RemoveCard(UICard uiCard)
    {
        if (uiCard.GetCardInstance().cardData is PropCardData)
        {
            propCards.Remove(uiCard);
            cardHandler.RemoveCard(uiCard.GetCardInstance());
            //ShopManager.RepositionCards(propCardPanel, propCards, 0.5f);
        }
        else
        {
            relicCards.Remove(uiCard);
            cardHandler.RemoveCard(uiCard.GetCardInstance());
            //ShopManager.RepositionCards(relicCardPanel, relicCards, 0.5f);
        }
        currentSelectedCard = null;
    }
    public void AddMoney(int money)
    {
        this.Money += money;
    }
}
