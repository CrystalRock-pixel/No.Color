using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICard : MonoBehaviour,
    IDragHandler, IBeginDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler,ILayoutMember,IGood
{
    private CardInstance cardInstance { get;set; }
    private UICardVisual cardVisual;

    [SerializeField]
    private CardData cardData;
    public string CardName => cardData.cardName;
    public string Description => cardData.effectDescription;
    public CardType Type => cardData.type;
    public Sprite CardImage => cardData.cardImage;
    public int Price => cardData.price;
    public int SellPrice => cardData.sellPrice;

    public Transform Transform => transform;
    public Transform CurrentSlot { get; set; }
    public bool IsDragging { get => isDragging; set => isDragging = value; }
    public LayoutSlotManager LayoutManager { get; set; }

    //private Coroutine wiggleCoroutine = null;

    [Header("对话框参数")]
    public Transform dialogCanvas;

    [Header("贴图参数")]
    // 你期望SpriteRenderer显示的统一宽度 (Unity Units)
    public float targetWidth = 1f;
    // 你期望SpriteRenderer显示的统一高度 (Unity Units)
    public float targetHeight = 1f;
    // 你是否希望按比例缩放 (即保持原始纵横比)
    public bool maintainAspectRatio = true;
    private Transform spriteObject;

    [Header("事件")]
    [HideInInspector] public UnityEvent<UICard> PointerEnterEvent = new UnityEvent<UICard>();
    [HideInInspector] public UnityEvent<UICard> PointerExitEvent = new UnityEvent<UICard>();
    [HideInInspector] public UnityEvent<UICard> BeginDragEvent = new UnityEvent<UICard>();
    [HideInInspector] public UnityEvent<UICard> EndDragEvent = new UnityEvent<UICard>();
    [HideInInspector] public UnityEvent<UICard, bool> SelectEvent = new UnityEvent<UICard, bool>(); // bool: isSelected

    [Header("状态参数")]
    public bool isSelected = false;
    public bool isDragging = false; // 新增拖拽状态
    public bool isBought = false;

    private RectTransform rectTransform; // 用于UI操作

    private GameObject shadow;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cardVisual = GetComponentInChildren<UICardVisual>();
        SetSelected(false);
    }
    private void Start()
    {
    }
    private void LateUpdate()
    {
        if (!isDragging && CurrentSlot != null)
        {
            transform.position=CurrentSlot.position;
        }
        if(isDragging&&shadow != null)
        {
            shadow.transform.localPosition=cardVisual.transform.localPosition+new Vector3(50f,-50f);
        }
    }

    public void SetUp(CardInstance cardInstance)
    {
        this.cardInstance = cardInstance;
        this.cardData = cardInstance.cardData;
        cardVisual.gameObject.GetComponent<Image>().sprite=cardData.cardImage;
        cardVisual.gameObject.GetComponent<Image>().SetNativeSize();
        //spriteRenderer.sprite = cardData.cardImage;
        //defaultRotation = transform.rotation;
        //defaultScale = transform.localScale;
    }
    public CardData GetCardData()
    {
        return cardData;
    }

    public void OnBuy()
    {
        if (Player.Instance.Buy(this))
        {
            isBought = true;
            ShopManager.Instance.RemoveCard(this);
            isSelected = false;
            cardInstance.cardData.OnAciquire();
            cardVisual.ResetVisuals();

            UIManager.Instance.RemoveInfoPanel(this.transform);
        }
        else
        {
            Debug.Log("你买不起");
            isBought = false;
        }
    }
    public void OnSell()
    {
        Player.Instance.Sell(this);
        cardInstance.cardData.OnSold();
        Debug.Log("出售卡牌" + CardName);
        OnDestroyed();
    }

    public void OnDestroyed()
    {
        Player.Instance.RemoveCard(this);
        cardInstance.cardData.OnDestory();

        UIManager.Instance.RemoveInfoPanel(this.transform);
        LayoutManager.RemoveMember(this);
        Debug.Log("移除卡牌" + CardName);
        Destroy(gameObject);
    }

    public void ChangeLayoutManager(LayoutSlotManager manager)
    {
        LayoutManager.RemoveMember(this);
        LayoutManager = manager;
        LayoutManager.AddMember(this);
    }

    public IEnumerator TryPlayAnimation(bool isBlocking,bool isLevelUp=false)
    {
        Debug.Log(this.gameObject.name+"播放触发动画");
        RelicCardData relicCardData = cardInstance.cardData as RelicCardData;

        if (relicCardData != null)
        {
            Vector3 offset=new Vector3(0,-1,0);
            if (isLevelUp)
            {
                EffectManager.Instance.PlayFlowTextEffect("", transform.position+offset, FlowTextType.CardLevelUp);
            }
            else
            {
                FlowTextType flowTextType = EffectManager.Instance.RelicTypeToFlowTextType(relicCardData.cardType);
                EffectManager.Instance.PlayFlowTextEffect(relicCardData.value.ToString(), transform.position+offset, flowTextType);
            }
        }
        return cardVisual.TryPlayWiggleAnimation(isBlocking);
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);

        Vector3 dialogPosition = rectTransform.position+new Vector3(2,0,-1);
        InfoPanelConfig config=new InfoPanelConfig(CardName, Description, false);
        UIManager.Instance.ShowInfoPanel(config,dialogPosition,this.transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return; // 拖拽中不触发离开
        if (!isSelected)
        {
            UIManager.Instance.RemoveInfoPanel(this.transform);
        }

        PointerExitEvent.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return; // 拖拽结束后不触发点击

        // 触发 Player 单例中的选择逻辑
        // 假设 Player 单例有一个方法来处理卡牌选择/取消选择
        Player.Instance.HandleCardClicked(this.transform);
    }

    // 拖拽逻辑
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isSelected)
        {
            isSelected = false;
        }
        isDragging = true;
        BeginDragEvent.Invoke(this);

        ShowShadow();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mousePosition=Camera.main.ScreenToWorldPoint(eventData.position);
        transform.position= new Vector3(mousePosition.x,mousePosition.y);
        LayoutManager.UpdateShadowPosition(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        LayoutManager.FinishDragging(this);
        EndDragEvent.Invoke(this);

        if (shadow != null)
        {
            shadow.gameObject.SetActive(false);
        }
    }

    // 新增方法：由 Player 单例调用来设置卡牌的选中状态
    public void SetSelected(bool selected)
    {
        if (this.isSelected == selected) return;

        this.isSelected = selected;

        if (selected)
        {
            Vector3 dialogPosition = rectTransform.position + new Vector3(2, 0, -1);
            InfoPanelConfig config;
            if (!isBought)
            {
                config = new InfoPanelConfig(CardName, Description, false, 1, Price);
            }
            else
            {
                config = new InfoPanelConfig(CardName, Description, false,2,SellPrice);
            }
            UIManager.Instance.ShowInfoPanel(config, dialogPosition, this.transform);
        }
        else
        {
            UIManager.Instance.RemoveInfoPanel(this.transform);
        }

        SelectEvent.Invoke(this, selected);
    }

    private void OnDestroy()
    {
        cardVisual.ResetVisuals();
        CardData cardData = cardInstance.cardData;
        UnityEngine.Object.Destroy(cardData);
        cardInstance.cardData = null;
        UIManager.Instance.RemoveInfoPanel(this.transform);
    }

    private void ShowShadow()
    {
        if (shadow == null)
        {
            shadow = ResourcesManager.Instance.GetShadow(transform, cardVisual.transform.GetComponent<Image>().sprite);
            shadow.transform.SetSiblingIndex(0);
            shadow.transform.localPosition = new Vector3(50f, -50f);
        }
        else
        {
            shadow.gameObject.SetActive(true);
        }
    }
}
