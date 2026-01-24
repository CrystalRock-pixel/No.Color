using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIProp : MonoBehaviour,IGood,IPointerEnterHandler,
    IPointerExitHandler,IPointerClickHandler,IDragHandler,
    IBeginDragHandler,IEndDragHandler,ILayoutMember
{
    [SerializeField] private PropData propData;

    public string PropName => propData.propName;
    public string Description => propData.effectDescription;
    public int Price=> propData.price;
    public int SellPrice => propData.sellPrice;

    public Transform Transform => transform;

    public Transform CurrentSlot { get; set; }
    public bool IsDragging { get => isDragging; set => isDragging=value; }
    public LayoutSlotManager LayoutManager { get; set; }

    private bool isSelected = false;
    private bool isDragging = false;

    private PropVisual propVisual; 

    private RectTransform rectTransform;

    private void Awake()
    {
        isSelected = false;
        rectTransform = GetComponent<RectTransform>();
        propVisual=GetComponentInChildren<PropVisual>();
    }

    public void OnDestroy()
    {
        
    }

    public void SetUp(PropData propdata)
    {
        this.propData = propdata;
        propVisual.gameObject.GetComponent<Image>().sprite = propData.propImage;
    }

    public void OnBuy()
    {
        throw new System.NotImplementedException();
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
        Debug.Log("ÒÆ³ýµÀ¾ß" + PropName);
        Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 dialogPosition = transform.position + new Vector3(2, 0, -1);
        InfoPanelConfig config = new InfoPanelConfig(PropName, Description, false);
        UIManager.Instance.ShowInfoPanel(config, dialogPosition, this.transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.RemoveInfoPanel(this.transform);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
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
        isDragging=false;
        if(LayoutManager != null)
            LayoutManager.FinishDragging(this);
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
}
