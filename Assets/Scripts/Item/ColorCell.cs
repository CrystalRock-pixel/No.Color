using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ColorType
{
    None,
    Red,
    Green,
    Blue,
    Yellow
}

public enum CellType
{
    Normal,
    Effect,
}
public class ColorCell : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    public string cellName;
    public ColorType colorType;
    public Vector2 location;
    public GridManager gridManager;
    public string description;

    public float baseChips = 3f;
    public CellType cellType = CellType.Normal;

    // 控制方块视觉下落的速度
    public float fallSpeed = 5f;

    public void Init(Vector2 location,GridManager gridManager)
    {
        this.location = location;
        this.gridManager=gridManager;
    }
    public virtual void Init(ColorType colorType,CellType cellType = CellType.Normal)
    {
        this.colorType = colorType;
        SpriteRenderer cellRenderer = GetComponent<SpriteRenderer>();
        //cellRenderer.material.color=ColorType2Color(colorType);
        cellRenderer.sprite = ResourcesManager.Instance.GetCellSprite(colorType);
        this.cellType = cellType;
        description = string.Empty;
    }

    public static Color ColorType2Color(ColorType colorType)
    {
        if (colorType == ColorType.Red) { return Color.red; }
        else if(colorType == ColorType.Green) { return Color.green; }
        else if(colorType == ColorType.Blue) { return Color.blue; }
        else return Color.yellow;
    }


    public int GetChips()
    {
        return Mathf.RoundToInt(baseChips);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Player.Instance.OnMouseCellHover(this);
        MouseEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Player.Instance.OnMouseCellHover(null);
        MouseExit();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button==PointerEventData.InputButton.Left)
             Player.Instance.OnMouseClickCell(this);
        else if(eventData.button==PointerEventData.InputButton.Right)
            Player.Instance.OnMouseRightClickCell(this);
    }

    public void MouseEnter()
    {
        InfoPanelConfig config = new InfoPanelConfig(cellName, description, false);
        Vector3 position = transform.position + new Vector3(2, 0, 0);
        UIManager.Instance.ShowInfoPanel(config,position,this.transform);
    }
    public void MouseExit()
    {
        UIManager.Instance.RemoveInfoPanel(this.transform);
    }

    public IEnumerator BeSelected()
    {
        float beSelectedDelay = 0.05f;
        float timer = 0f;
        while (timer < beSelectedDelay)
        {
            timer += Time.deltaTime;
            float t = timer / beSelectedDelay;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, t);
            yield return null;
        }
    }
    public void DeSelected()
    {
        transform.localScale = Vector3.one*0.9f;
    }

    public virtual void OnDestoryed()
    {
        EffectManager.Instance.PlayFlowTextEffect(GetChips().ToString(), transform.position, FlowTextType.AddChips);
        UIManager.Instance.RemoveInfoPanel(this.transform);
    }

    public void DisplayTempColor(ColorType colorType)
    {
        Renderer cellRenderer = GetComponent<Renderer>();
        cellRenderer.material.color = ColorType2Color(colorType);
    }
    public void UpdateColorType(ColorType colorType)
    {
        this.colorType=colorType;
        Renderer cellRenderer = GetComponent<Renderer>();
        cellRenderer.material.color = ColorType2Color(colorType);
    }

    public virtual void TriggerEffectOnClear(ClearGeneralParameters clearParas, ClearSequenceService clearService)
    {
    }

    public virtual void TriggerEffectBeforeClear(ClearGeneralParameters clearParas, ClearSequenceService clearService)
    {
    }
}