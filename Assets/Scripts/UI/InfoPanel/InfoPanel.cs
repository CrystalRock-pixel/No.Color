using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelConfig
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
}

public class InfoPanel : MonoBehaviour
{
    public Transform infoObject;
    [SerializeField] private GameObject descriptionPrefab;
    [SerializeField] private GameObject buysellButtonPrefab;
    [SerializeField] private GameObject useButtonPrefab;

    private Vector3 posOffset;

    private void LateUpdate()
    {
        transform.position = posOffset+infoObject.position;
    }

    public void Init(InfoPanelConfig config, Transform infoObject)
    {
        this.infoObject = infoObject;
        transform.GetChild(0).GetComponent<TMP_Text>().text = config.name;
        if (config.description != string.Empty)
        {
            GameObject descriptionObject = Instantiate(descriptionPrefab, transform);
            descriptionObject.transform.GetChild(0).GetComponent<TMP_Text>().text = config.description;
        }
        if (config.buysellButtonState != 0)
        {
            if (config.buysellButtonState == 1)
            {
                GameObject buyButtonObject = Instantiate(buysellButtonPrefab, transform);
                buyButtonObject.transform.GetChild(0).GetComponent<TMP_Text>().text = "购买 ";
                buyButtonObject.transform.GetChild(1).GetComponent<TMP_Text>().text = config.price.ToString();
                Button button=buyButtonObject.GetComponent<Button>();
                button.onClick.AddListener(OnBuy);
            }
            else if (config.buysellButtonState == 2)
            {
                GameObject sellButtonObject = Instantiate(buysellButtonPrefab, transform);
                sellButtonObject.transform.GetChild(0).GetComponent<TMP_Text>().text = "出售 ";
                sellButtonObject.transform.GetChild(1).GetComponent<TMP_Text>().text = config.price.ToString();
                Button button = sellButtonObject.GetComponent<Button>();
                button.onClick.AddListener(OnSell);
            }
        }
        if (config.useButtonState)
        {
            GameObject useButtonObject = Instantiate(useButtonPrefab, transform);
            useButtonObject.transform.GetChild(0).GetComponent<TMP_Text>().text = "使用";
            Button button = useButtonObject.GetComponent<Button>();
            button.onClick.AddListener(OnUse);
        }

        posOffset = transform.position - infoObject.position;
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
