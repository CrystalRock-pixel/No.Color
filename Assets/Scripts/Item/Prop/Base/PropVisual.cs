using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PropVisual : SmoothFollowVisual
{
    private UIProp parentCard;

    private Image image;
    private Material oriMat;
    protected override void Awake()
    {
        base.Awake();
        image = GetComponent<Image>();
        parentCard = GetComponentInParent<UIProp>();
        oriMat = image.material;

        parentCard.PointerEnterEvent.AddListener(HandlePointerEnter);
        parentCard.PointerExitEvent.AddListener(HandlePointerExit);
        parentCard.BeginDragEvent.AddListener(HandleBeginDrag);
        parentCard.EndDragEvent.AddListener(HandleEndDrag);
    }



    public void ResetVisuals()
    {
        image.material = oriMat;
        transform.DOKill();

        transform.localRotation = Quaternion.identity;
        base.transform.localPosition = Vector3.zero;
    }

    private void HandlePointerEnter(UIProp prop)
    {
        image.material = ResourcesManager.Instance.GetMaterial("Outline");
    }

    private void HandlePointerExit(UIProp prop)
    {
        image.material = oriMat;
    }
    private void HandleBeginDrag(UIProp prop)
    {
    }

    private void HandleEndDrag(UIProp prop)
    {
    }
}
