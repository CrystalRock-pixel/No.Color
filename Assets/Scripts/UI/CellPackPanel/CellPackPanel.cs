using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class CellPackPanel : MonoBehaviour
{
    [SerializeField] private List<LayoutSlotManager> layoutlist = new List<LayoutSlotManager>();
    [SerializeField] private Transform cellGroup;

    private void Awake()
    {
        this.transform.gameObject.SetActive(false);
    }

    public void OnButtonDown()
    {
        if (gameObject.activeSelf)
        {
            Debug.Log("关闭");
            ClearCellGroup();
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("启动");
            gameObject.SetActive(true);
            RefreshPack();
        }
    }

    private void ClearCellGroup()
    {
        for (int i = 0; i < cellGroup.childCount; i++)
        {
            var  cell = cellGroup.GetChild(i);
            Destroy(cell.gameObject);
        }
    }
    private void RefreshPack()
    {
        //if (transform.gameObject.activeSelf) return;
        ResourcesManager resourcesManager = ResourcesManager.Instance;
        var currentDeck = resourcesManager.masterDeck;
        var groupedCells = currentDeck.GroupBy(x => x.colorType);
        foreach (var group in groupedCells)
        {
            // 找到对应的面板
            var targetPanel = layoutlist[ColorType2PanelIndex(group.Key)];

            if (targetPanel != null)
            {
                int count = 0;
                foreach (var cellConfig in group)
                {
                    GameObject go = resourcesManager.BuildCellFromConfig(cellConfig);
                    go.transform.SetParent(cellGroup);
                    go.transform.GetComponent<SortingGroup>().sortingOrder = 2;    
                    ColorCell colorCell=go.GetComponent<ColorCell>();
                    targetPanel.AddMember(colorCell);
                    colorCell.isPackCell = true;
                    count++;
                }
            }
        }
    }

    private int ColorType2PanelIndex(ColorType type)
    {
        switch (type)
        {
            case ColorType.Red:
                return 0;
            case ColorType.Green:
                return 1;
            case ColorType.Blue:
                return 2;
            case ColorType.Yellow:
                return 3;
            case ColorType.None:
                return 4;
            default:
                return 4;
        }
    }
}
