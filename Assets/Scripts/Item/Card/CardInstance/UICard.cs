using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICard : MonoBehaviour,
    IDragHandler, IBeginDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler
{
    private CardInstance cardInstance { get;set; }
    private UICardVisual cardVisual;

    [SerializeField]
    private CardData cardData;

    //private SpriteRenderer spriteRenderer;
    private Image image;

    GameObject cardDialogPrefab;
    GameObject cardDialog;
    public string CardName => cardData.cardName;
    public CardType Type => cardData.type;
    public Sprite CardImage => cardData.cardImage;
    public int Price => cardData.price;
    public int SellPrice => cardData.sellPrice;

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

    private void Awake()
    {
        //spriteObject = transform.GetChild(0);
        //spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        cardVisual = GetComponentInChildren<UICardVisual>();
        SetSelected(false);
    }
    private void Start()
    {
        cardDialogPrefab = Resources.Load<GameObject>("Prefabs/UI/CardDialog");
    }
    public void SetUp(CardInstance cardInstance)
    {
        this.cardInstance = cardInstance;
        this.cardData = cardInstance.cardData;
        image.sprite=cardData.cardImage;
        //spriteRenderer.sprite = cardData.cardImage;
        //defaultRotation = transform.rotation;
        //defaultScale = transform.localScale;
    }

    //public void OnClicked()
    //{
    //    if (!isSelected)
    //    {
    //        transform.position+=new Vector3(0, 0.3f, 0);
    //        isSelected = true;
    //        Vector3 dialogPosition = this.transform.position + new Vector3(1, 0, -1);
    //        UIManager.Instance.ShowCardButtonDialog(this, dialogPosition);
    //    }
    //    else if(isSelected)
    //    {
    //        DeSelected();
    //    }
    //}

    //public void DeSelected()
    //{
    //    transform.position -= new Vector3(0, 0.3f, 0);
    //    isSelected = false;
    //    UIManager.Instance.RemoveCardInfoDialog(this);
    //    UIManager.Instance.RemoveCardButtonDialog(this);
    //}

    //public void OnMouseEnter()
    //{
    //    transform.localScale *= 1.1f;
    //    Vector3 dialogPosition = this.transform.position+new Vector3(2,0,-1);
    //    UIManager.Instance.ShowCardInfoDialog(this, dialogPosition);
    //}
    //public void OnMouseExit()
    //{
    //    transform.localScale /= 1.1f;
    //    //UIManager.Instance.RemoveCardInfoDialog(this);
    //    if (isSelected) return;
    //    UIManager.Instance.RemoveCardInfoDialog(this);
    //    UIManager.Instance.RemoveCardButtonDialog(this);
    //}

    ///// <summary>
    ///// 当设置新的Sprite时调用此方法来统一大小。
    ///// </summary>
    ///// <param name="newSprite">新的Sprite对象。</param>
    //public void SetAndScaleSprite(Sprite newSprite)
    //{
    //    if (spriteRenderer == null) return;

    //    // 1. 设置新的Sprite
    //    spriteRenderer.sprite = newSprite;

    //    if (newSprite == null)
    //    {
    //        // 如果Sprite为空，重置缩放
    //        transform.localScale = Vector3.one;
    //        return;
    //    }

    //    // 2. 获取当前Sprite在世界空间的尺寸 (bounds)
    //    // 注意：bounds.size 基于 Sprite 的 Pixels Per Unit (PPU)
    //    Vector3 currentSize = newSprite.bounds.size; // (width, height, depth)

    //    // 3. 计算缩放比例
    //    float scaleX = targetWidth / currentSize.x;
    //    float scaleY = targetHeight / currentSize.y;

    //    if (maintainAspectRatio)
    //    {
    //        // 保持纵横比，选择较小的缩放比例 (Fit) 或较大的缩放比例 (Fill)
    //        // 这里我们选择较小的比例 (Fit) 以确保整个Sprite都可见
    //        float uniformScale = Mathf.Min(scaleX, scaleY);

    //        spriteObject.localScale = new Vector3(uniformScale, uniformScale, 1f);
    //    }
    //    else
    //    {
    //        // 不保持纵横比，直接拉伸
    //        spriteObject.localScale = new Vector3(scaleX, scaleY, 1f);
    //    }
    //}

    public CardInstance GetCardInstance()
    {
        return cardInstance;
    }

    public void OnBuy()
    {
        if (Player.Instance.BuyCard(this))
        {
            isBought = true;
            ShopManager.Instance.RemoveCard(this);
            isSelected = false;
            cardInstance.cardData.OnAciquire();
            cardVisual.ResetVisuals();
            UIManager.Instance.RemoveCardInfoDialog(this);
            UIManager.Instance.RemoveCardButtonDialog(this);
        }
        else
        {
            Debug.Log("你买不起");
            isBought = false;
        }
    }
    public void OnSell()
    {
        Player.Instance.SellCard(this);
        cardInstance.cardData.OnSold();
        UIManager.Instance.RemoveCardInfoDialog(this);
        UIManager.Instance.RemoveCardButtonDialog(this);
        Debug.Log("移除卡牌" + CardName);
        Destroy(gameObject);
    }

    public void OnDestroyed()
    {
        Player.Instance.RemoveCard(this);
        cardInstance.cardData.OnDestory();

        UIManager.Instance.RemoveCardInfoDialog(this);
        UIManager.Instance.RemoveCardButtonDialog(this);
        Debug.Log("移除卡牌" + CardName);
        Destroy(gameObject);
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
        UIManager.Instance.ShowCardInfoDialog(this, dialogPosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //if (isDragging) return; // 拖拽中不触发离开
        UIManager.Instance.RemoveCardInfoDialog(this);
        //UIManager.Instance.RemoveCardButtonDialog(this);

        PointerExitEvent.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return; // 拖拽结束后不触发点击

        // 触发 Player 单例中的选择逻辑
        // 假设 Player 单例有一个方法来处理卡牌选择/取消选择
        Player.Instance.HandleCardClicked(this);
    }

    // 拖拽逻辑
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        BeginDragEvent.Invoke(this);
        // 在这里设置卡牌在拖拽时的层级（由 CardVisual.cs 处理）
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        EndDragEvent.Invoke(this);
    }

    // 新增方法：由 Player 单例调用来设置卡牌的选中状态
    public void SetSelected(bool selected)
    {
        if (this.isSelected == selected) return;

        this.isSelected = selected;

        if (selected)
        {
            Vector3 dialogPosition = rectTransform.position + new Vector3(0, -1, -1);
            UIManager.Instance.ShowCardButtonDialog(this, dialogPosition);
        }
        else
        {
            UIManager.Instance.RemoveCardInfoDialog(this);
            UIManager.Instance.RemoveCardButtonDialog(this);
        }

        SelectEvent.Invoke(this, selected);
    }

    private void OnDestroy()
    {
        cardVisual.ResetVisuals();
        CardData cardData = cardInstance.cardData;
        UnityEngine.Object.Destroy(cardData);
        cardInstance.cardData = null;
    }
}
