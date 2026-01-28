using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelConfig:IComparable<InfoPanelConfig>
{
    public string name { get;  set; }
    public string description {  get; set; }
    public int buysellButtonState { get; set; } // 0: none, 1: buy, 2: sell
    public int price { get; set; }
    public bool useButtonState { get; set; }

    public InfoPanelConfig(string name, string description,bool useButtonState,int buysellButton=0,int price=0)
    {
        this.name = name;
        this.description = description;
        this.buysellButtonState = buysellButton;
        this.price = price;
        this.useButtonState = useButtonState;
    }

    public int CompareTo(InfoPanelConfig other)
    {
        if (this.name == other.name && description == other.description && buysellButtonState == other.buysellButtonState && price == other.price && useButtonState == other.useButtonState)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
}

public class InfoPanel : MonoBehaviour
{
    public Transform infoObject;
    [SerializeField] private GameObject descriptionPrefab;
    [SerializeField] private GameObject buysellButtonPrefab;
    [SerializeField] private GameObject useButtonPrefab;

    private Vector3 posOffset;
    private InfoPanelConfig config=new InfoPanelConfig(null,null,false);

    private GameObject description;
    private GameObject buysellButton;
    private GameObject useButton;

    private void LateUpdate()
    {
        transform.position = posOffset+infoObject.position;
    }

    public void Init(InfoPanelConfig config, Transform infoObject)
    {
        if(this.config != null && config.CompareTo(this.config) == 0)
        {
            return;
        }
        this.infoObject = infoObject;
        transform.GetChild(0).GetComponent<TMP_Text>().text = config.name;
        if (this.config.description!=config.description&& config.description != string.Empty)
        {
            description = Instantiate(descriptionPrefab, transform);
            description.transform.GetChild(0).GetComponent<TMP_Text>().text = config.description;
        }
        if (this.config.buysellButtonState!=config.buysellButtonState)
        {
            if (buysellButton == null)
            {
                buysellButton = Instantiate(buysellButtonPrefab, transform);
            }
            else
            {
                buysellButton.GetComponent<Button>().onClick.RemoveAllListeners();
            }

            if (config.buysellButtonState == 1)
            {
                buysellButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "购买 ";
                buysellButton.transform.GetChild(1).GetComponent<TMP_Text>().text = config.price.ToString();
                Button button= buysellButton.GetComponent<Button>();
                button.onClick.AddListener(OnBuy);
            }
            else if (config.buysellButtonState == 2)
            {
                buysellButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "出售 ";
                buysellButton.transform.GetChild(1).GetComponent<TMP_Text>().text = config.price.ToString();
                Button button = buysellButton.GetComponent<Button>();
                button.onClick.AddListener(OnSell);
            }
        }
        if (config.useButtonState&&!this.config.useButtonState)
        {
            useButton = Instantiate(useButtonPrefab, transform);
            useButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "使用";
            Button button = useButton.GetComponent<Button>();
            button.onClick.AddListener(OnUse);
        }

        posOffset = transform.position - infoObject.position;
        this.config = config;
    }


    private void OnBuy()
    {
        Debug.Log("Buying card: " + infoObject.name);

        infoObject.GetComponent<IGood>().OnBuy();
    }

    private void OnSell()
    {
        Debug.Log("Selling card: " + infoObject.name);
        infoObject.GetComponent<IGood>().OnSell();
    }

    private void OnUse()
    {
        Debug.Log("Using card: " + infoObject.name);
        UIProp uiProp = infoObject.GetComponent<UIProp>();
        if (uiProp != null)
        {
            uiProp.OnUse();
            AudioManager.Instance.PlaySound(AudioManager.AudioType.CardUse);
        }
        else
        {
            Debug.Log("不是道具，用不了"+transform.name);
        }
    }
}
