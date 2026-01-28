using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface ILayoutMember
{
    // 物体自身的 Transform
    Transform Transform { get; }
    // 该物体当前占用的插槽（由管理器分配）
    Transform CurrentSlot { get; set; }
    // 是否正在被拖拽
    bool IsDragging { get; set; }
    LayoutSlotManager LayoutManager { get; set; }
}

public class LayoutSlotManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public GameObject shadowSlotPrefab;
    private GameObject shadowSlot;
    private RectTransform shadowSlotRectTransform;

    // 事件：当布局顺序发生变化时通知外部
    public Action OnLayoutOrderChanged;

    private List<ILayoutMember> members = new List<ILayoutMember>();
    private void Awake()
    {
        shadowSlot = Instantiate(shadowSlotPrefab,this.transform);
        shadowSlot.gameObject.SetActive(false);
        shadowSlotRectTransform = shadowSlot.GetComponent<RectTransform>();
    }
    public void Init(List<ILayoutMember> _members)
    {
        if (members != null)
        {
            foreach (var member in members)
            {
                if (member.CurrentSlot != null)
                    Destroy(member.CurrentSlot.gameObject);
                member.CurrentSlot = null;
            }
            members.Clear();
        }
        foreach (var member in _members)
        {
            AddMember(member);
        }
    }
    public void AddMember(ILayoutMember member)
    {
        if (members.Contains(member)) return;

        GameObject newSlot = Instantiate(slotPrefab, transform);
        member.CurrentSlot = newSlot.GetComponent<RectTransform>();
        member.LayoutManager = this;
        members.Add(member);

        OnLayoutOrderChanged?.Invoke();
    }

    public void RemoveMember(ILayoutMember member)
    {
        if (members.Remove(member))
        {
            if (member.CurrentSlot != null) Destroy(member.CurrentSlot.gameObject);
            member.CurrentSlot = null;
            OnLayoutOrderChanged?.Invoke();
        }
    }
    // 更新影子位置：仅操作插槽层级
    public void UpdateShadowPosition(ILayoutMember draggingMember)
    {
        draggingMember.CurrentSlot.gameObject.SetActive(false);
        if (!shadowSlotRectTransform.gameObject.activeSelf)
        {
            shadowSlotRectTransform.gameObject.SetActive(true);
            shadowSlotRectTransform.SetSiblingIndex(draggingMember.CurrentSlot.GetSiblingIndex());
        }

        int targetIndex = CalculateIndex(draggingMember.Transform.position);
        if (shadowSlotRectTransform.GetSiblingIndex() != targetIndex)
        {
            shadowSlotRectTransform.SetSiblingIndex(targetIndex);
            // 影子挪位了，通知同步层级
            OnLayoutOrderChanged?.Invoke();
        }
    }

    public void FinishDragging(ILayoutMember member)
    {
        member.CurrentSlot.gameObject.SetActive(true);
        member.CurrentSlot.SetSiblingIndex(shadowSlotRectTransform.GetSiblingIndex());
        shadowSlotRectTransform.gameObject.SetActive(false);
        OnLayoutOrderChanged?.Invoke();
    }

    private int CalculateIndex(Vector3 pos)
    {
        int index = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child == shadowSlotRectTransform) continue;
            if (pos.x > child.position.x) index = i + 1;
        }
        return index;
    }
    public List<ILayoutMember> GetOrderedMembers()
    {
        members.Sort((a, b) => a.CurrentSlot.GetSiblingIndex().CompareTo(b.CurrentSlot.GetSiblingIndex()));
        return members;
    }
}
