using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LayoutGroupManager : MonoBehaviour
{
    public List<LayoutSlotManager> layoutManagers;
    private Transform visualContainer; // 所有物体实际存放的父物体

    private void Awake()
    {
        visualContainer = GetComponent<Transform>();
        RegisterManager();
    }
    private void RegisterManager()
    {
        foreach (var manager in layoutManagers)
        {
            manager.OnLayoutOrderChanged += RefreshAllDepths;
        }
    }
    public void RefreshAllDepths()
    {
        // 1. 先按优先级对 Manager 进行排序

        int currentGlobalIndex = 0;

        foreach (var manager in layoutManagers)
        {
            // 2. 获取该布局内排好序的物体
            var orderedItems = manager.GetOrderedMembers();

            foreach (var item in orderedItems)
            {
                if (item.Transform.parent == visualContainer)
                {
                    item.Transform.SetSiblingIndex(currentGlobalIndex);
                    currentGlobalIndex++;
                }
            }
        }
    }
}
