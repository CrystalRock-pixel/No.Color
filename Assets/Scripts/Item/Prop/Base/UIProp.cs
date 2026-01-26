using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIProp : MonoBehaviour, IGood, IPointerEnterHandler,
    IPointerExitHandler, IPointerClickHandler, IDragHandler,
    IBeginDragHandler, IEndDragHandler, ILayoutMember
{
    [SerializeField] private PropData propData;

    public string PropName => propData.propName;
    public string Description => propData.effectDescription;
    public int Price => propData.price;
    public int SellPrice => propData.sellPrice;

    public Transform Transform => transform;

    public Transform CurrentSlot { get; set; }
    public bool IsDragging { get => isDragging; set => isDragging = value; }
    public LayoutSlotManager LayoutManager { get; set; }

    [Header("状态")]
    private bool isBought = false;
    private bool isSelected = false;
    private bool isDragging = false;


    [Header("事件")]
    [HideInInspector] public UnityEvent<UIProp> PointerEnterEvent = new UnityEvent<UIProp>();
    [HideInInspector] public UnityEvent<UIProp> PointerExitEvent = new UnityEvent<UIProp>();
    [HideInInspector] public UnityEvent<UIProp> BeginDragEvent = new UnityEvent<UIProp>();
    [HideInInspector] public UnityEvent<UIProp> EndDragEvent = new UnityEvent<UIProp>();
    [HideInInspector] public UnityEvent<UIProp, bool> SelectEvent = new UnityEvent<UIProp, bool>(); // bool: isSelected

    private PropVisual propVisual;
    private RectTransform rectTransform;


    private GameObject shadow; 

    private void Awake()
    {
        isSelected = false;
        rectTransform = GetComponent<RectTransform>();
        propVisual = GetComponentInChildren<PropVisual>();
    }

    private void OnDestroy()
    {
        UnityEngine.Object.Destroy(propData);
        UIManager.Instance.RemoveInfoPanel(this.transform);
    }

    public void LateUpdate()
    {
        if (CurrentSlot != null && !isDragging)
        {
            transform.position = CurrentSlot.position;
        }
        if (isDragging && shadow != null)
        {
            shadow.transform.localPosition = propVisual.transform.localPosition + new Vector3(50f, -50f);
        }
    }

    public void SetUp(PropData propdata)
    {
        this.propData = propdata;
        Image visualImage = propVisual.gameObject.GetComponent<Image>();
        visualImage.sprite = propData.propImage;
        visualImage.SetNativeSize();
    }

    public void OnBuy()
    {
        if (Player.Instance.Buy(this))
        {
            isBought = true;
            ShopManager.Instance.RemoveCard(this);
            isSelected = false;
            propData.OnAciquire();
            propVisual.ResetVisuals();

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
        throw new System.NotImplementedException();
    }

    public void OnUse()
    {
        propData.OnUse();

        propData.OnDestory();
    }

    public void OnDestroyed()
    {
        propData.OnDestory();

        UIManager.Instance.RemoveInfoPanel(this.transform);
        Debug.Log("移除道具" + PropName);
        Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        Vector3 dialogPosition = transform.position + new Vector3(2, 0, -1);
        InfoPanelConfig config = new InfoPanelConfig(PropName, Description, false);

        UIManager.Instance.ShowInfoPanel(config, dialogPosition, this.transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;
        PointerExitEvent.Invoke(this);
        UIManager.Instance.RemoveInfoPanel(this.transform);
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        ShowShadow();
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
        transform.position = new Vector3(mousePosition.x, mousePosition.y);
        if (LayoutManager != null)
            LayoutManager.UpdateShadowPosition(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);
        isDragging = false;
        if (LayoutManager != null)
            LayoutManager.FinishDragging(this);

        if (shadow != null)
        {
            shadow.gameObject.SetActive(false);
        }
    }
    public void SetSelected(bool selected)
    {
        if (this.isSelected == selected) return;

        this.isSelected = selected;

        if (selected)
        {
            Vector3 dialogPosition = rectTransform.position + new Vector3(2, 0, -1);
            InfoPanelConfig config = new InfoPanelConfig(PropName, Description, false, 1, Price);
            UIManager.Instance.ShowInfoPanel(config, dialogPosition, this.transform);
        }
        else
        {
            UIManager.Instance.RemoveInfoPanel(this.transform);
        }

        //SelectEvent.Invoke(this, selected);
    }

    private void ShowShadow()
    {
        if (shadow == null)
        {
            shadow = ResourcesManager.Instance.GetShadow(transform, propVisual.transform.GetComponent<Image>().sprite);
            shadow.transform.SetSiblingIndex(0);
            shadow.transform.localPosition = new Vector3(50f, -50f);
        }
        else
        {
            shadow.gameObject.SetActive(true);
        }
    }
}
