using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private LevelManager levelManager;

    private GameObject dialogPanelPrefab;
    private GameObject cardDialogPrefab;
    private GameObject buySellButtonPrefab;
    private GameObject buttonPanelPrefab;
    private GameObject useCardButtonPrefab;
    public Transform shopBackCanvasTrans;
    public Transform shopFrontCanvasTrans;
    public Transform shopPanelTrans;
    public Button shopNextLevelBtn;

    private Dictionary<UICard,GameObject> currentDialogPanelDicts;
    private Dictionary<UICard,GameObject> currentButtonPanelDicts;

    [Header("信息面板")]
    public TMP_Text baseChipsText;
    public TMP_Text magnificationText;
    public TMP_Text totalScoreText;
    public TMP_Text eliminateText;
    public TMP_Text refreshText;
    public TMP_Text currentLevelText;
    public TMP_Text moneyText;
    public TMP_Text scoreLineText;
    public TMP_Text comboScaleText;
    public TMP_Text comboStructText;
    private bool isUpdateScore;

    [Header("结算面板")]
    public Transform settlementPanelTrans;
    public TMP_Text btnMoney;
    public Button btnSettle;

    [Header("结束面板")]
    public Transform gameOverPanelTrans;

    [Header("等级面板")]
    public Transform comboLevelPanelTrans;
    public TMP_Text comboScaleInfo;
    public TMP_Text comboStructInfo;
    public PieChartController pieChart;
    private bool comboLevelPanelIsOpening=false;

    [Header("敌人面板")]
    public Transform enemyPanelTrans;
    public TMP_Text enemyName;
    public TMP_Text enemyDescription;

    [Header("特殊方块面板")]
    public GameObject cellDescription;
    private GameObject currentCellDescription;

    public float updateTotalScoreDuration = 1f;

    public static Action<int> OnChipsChanged;
    public static Action<float> OnMagnificationChanged;
    public static Action<int,float> OnSingleCaculateScore;
    public static Action<int, int> OnFinalScoreCalculated;

    private void Awake()
    {
        Instance = this;
        isUpdateScore = false;
    }

    private void Start()
    {
        levelManager = LevelManager.Instance;

        dialogPanelPrefab = Resources.Load<GameObject>("Prefabs/UI/CardInfoPanel");
        cardDialogPrefab = Resources.Load<GameObject>("Prefabs/UI/CardDialog");
        buySellButtonPrefab = Resources.Load<GameObject>("Prefabs/UI/BuySellDialog");
        buttonPanelPrefab = Resources.Load<GameObject>("Prefabs/UI/CardButtonPanel");
        useCardButtonPrefab = Resources.Load<GameObject>("Prefabs/UI/UsedDialog");

        currentDialogPanelDicts = new Dictionary<UICard, GameObject>();
        currentButtonPanelDicts= new Dictionary<UICard, GameObject>();

        //OnSingleCaculateScore += UpdateScorePanel;
        OnFinalScoreCalculated += UpdateFinalScore;
        OnChipsChanged += UpdateChipsText;
        OnMagnificationChanged += UpdateMagnificationText;
        ScoreManager.OnComboPredicted += PredictScorePanel;
        ScoreManager.OnPredictionCleared += ClearPredictScorePanel;
        Player.OnMoneyChanged += UpdateLevelInfo;

        Init();
    }

    private void Init()
    {
        baseChipsText.text = "0";
        magnificationText.text = "0";
        totalScoreText.text = "0";
        eliminateText.text = "0";
        refreshText.text = "0";
        currentLevelText.text = "1";
        moneyText.text = "0";
        scoreLineText.text = "0";
        comboScaleText.text=string.Empty;
        comboStructText.text=string.Empty;

        shopPanelTrans.gameObject.SetActive(false);
        shopNextLevelBtn.gameObject.SetActive(false);
        settlementPanelTrans.gameObject.SetActive(false);
        gameOverPanelTrans.gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示卡牌信息对话框
    /// </summary>
    /// <param name="uiCard"></param>
    /// <param name="position"></param>
    public void ShowCardInfoDialog(UICard uiCard,Vector3 position)
    {
        if (currentDialogPanelDicts.ContainsKey(uiCard))
        {
            return;
        }
        GameObject dialogPanel = Instantiate(dialogPanelPrefab, position, Quaternion.identity);
        GameObject cardDialog = Instantiate(cardDialogPrefab, dialogPanel.transform);
        cardDialog.GetComponent<CardDialog>().SetCardInfo(uiCard.GetCardInstance());

        //dialogPanel.transform.parent = shopFrontCanvasTrans;
        dialogPanel.transform.SetParent(shopFrontCanvasTrans, true);
        dialogPanel.transform.localScale = Vector3.one;
        StartCoroutine(ObjectAnimator.Instance.AnimateIn(dialogPanel,startRotationZ:5f));
        currentDialogPanelDicts.Add(uiCard, dialogPanel);
    }

    /// <summary>
    /// 显示卡牌按钮对话框
    /// </summary>
    /// <param name="uiCard"></param>
    /// <param name="position"></param>
    public void ShowCardButtonDialog(UICard uiCard,Vector3 position)
    {
        if (currentButtonPanelDicts.ContainsKey(uiCard))
        {
            return;
        }

        GameObject buttonPanel = Instantiate(buttonPanelPrefab, position, Quaternion.identity);
        GameObject buySellButton = Instantiate(buySellButtonPrefab, buttonPanel.transform);
        buySellButton.GetComponent<BuySellCard>().SetUp(uiCard);
        if (uiCard.isBought && uiCard.GetCardInstance().cardData is PropCardData)
        {
            GameObject useButton = Instantiate(useCardButtonPrefab, buttonPanel.transform);
            useButton.GetComponent<UseCard>().SetUp(uiCard);
        }
        //buttonPanel.transform.parent = shopFrontCanvasTrans;
        buttonPanel.transform.SetParent(shopFrontCanvasTrans,true);
        buttonPanel.transform.localScale = Vector3.one;
        currentButtonPanelDicts.Add(uiCard,buttonPanel);
    }
    public void RemoveCardInfoDialog(UICard uiCard)
    {
        if (currentDialogPanelDicts.ContainsKey(uiCard))
        {
            Destroy(currentDialogPanelDicts[uiCard]);
            currentDialogPanelDicts.Remove(uiCard);
        }
    }
    public void RemoveCardButtonDialog(UICard uiCard)
    {
        if (currentButtonPanelDicts.ContainsKey(uiCard))
        {
            Destroy(currentButtonPanelDicts[uiCard]);
            currentButtonPanelDicts.Remove(uiCard);
        }
    }
    public void CloseCurrentDialog()
    {
        if(currentDialogPanelDicts.Count == 0) return;
        foreach (var dialogPanel in currentDialogPanelDicts.Values)
        {
            Destroy(dialogPanel);
        }
    }
    private void UpdateScorePanel(int chips,float magnification)
    {
        //isUpdateScore = true;
        //int chips = ScoreManager.Instance.chips;
        //int magnification = (int)ScoreManager.Instance.magnification;
        //baseChipsText.text = chips.ToString();
        //magnificationText.text = magnification.ToString();
        //StartCoroutine(UpdateTotalScoreCoroutine(chips, magnification));
        baseChipsText.text=chips.ToString();
        magnificationText.text=magnification.ToString();
    }
    private void UpdateChipsText(int chips)
    {
        baseChipsText.text = chips.ToString();
    }
    private void UpdateMagnificationText(float magnification)
    {
        magnificationText.text = magnification.ToString();
    }
    public void ResetScorePanel()
    {
        baseChipsText.text = "0";
        magnificationText.text="0";
        totalScoreText.text="0";
    }
    public void UpdateFinalScore(int chips,int magnification)
    {
        isUpdateScore = true;
        int currentChips=chips;
        int currentMagnification=magnification;
        baseChipsText.text = chips.ToString();
        magnificationText.text = magnification.ToString();
        StartCoroutine(UpdateTotalScoreCoroutine(currentChips, currentMagnification));
    }
    public void PredictScorePanel(string comboScale,string comboStruct,bool isLimited=false)
    {
        if (isUpdateScore) return;
        var comboScaleDic = ScoreManager.Instance.GetComboScaleInfo();
        comboScaleText.text = "LV"+comboScaleDic[comboScale].x+"  "+comboScale;
        baseChipsText.text = comboScaleDic[comboScale].y.ToString();

        var comboStructDic= ScoreManager.Instance.GetComboStructInfo();
        comboStructText.text = "LV" + comboStructDic[comboStruct].x + "  " + comboStruct;
        magnificationText.text=comboStructDic[comboStruct].y.ToString();

        if (isLimited)
        {
            magnificationText.text = "0";
            baseChipsText.text="0";
        }
    }
    public void ClearPredictScorePanel()
    {
        if (isUpdateScore) return;
        comboScaleText.text = string.Empty;
        comboStructText.text= string.Empty;
        baseChipsText.text= "0";
        magnificationText.text= "0";
    }

    IEnumerator UpdateTotalScoreCoroutine(int chips,int magnification)
    {
        float smallTime = 0.1f;
        float value= (chips * magnification)/(updateTotalScoreDuration/smallTime);
        float smallChips = chips / (updateTotalScoreDuration / smallTime);
        float smallMagnification= magnification/(updateTotalScoreDuration/smallChips);
        float timer = 0f;
        int totalScore = ScoreManager.Instance.totalScore;
        while (timer <= updateTotalScoreDuration)
        {
            timer += smallTime;
            float currentScore = int.Parse(totalScoreText.text);
            float currentChips=float.Parse(baseChipsText.text);
            float currentMagnification=float.Parse(magnificationText.text);

            currentScore += value;
            currentChips-=smallChips;
            currentMagnification-=smallMagnification;

            totalScoreText.text = ((int)currentScore).ToString();
            baseChipsText.text = ((int)currentChips).ToString();
            magnificationText.text = ((int)currentMagnification).ToString();
            yield return smallTime;
        }
        totalScoreText.text=totalScore.ToString();
        baseChipsText.text= chips.ToString();
        magnificationText.text=magnification.ToString();

        isUpdateScore = false;
    }

    public void OpenShop()
    {
        shopPanelTrans.gameObject.SetActive(true);
        shopNextLevelBtn.gameObject.SetActive(true);
    }
    public void CloseShop()
    {
        shopPanelTrans.gameObject.SetActive(false);
        shopNextLevelBtn.gameObject.SetActive(false);
    }

    public void UpdateLevelInfo()
    {
        if(levelManager == null) levelManager=LevelManager.Instance;
        currentLevelText.text = levelManager.currentLevel.ToString();
        scoreLineText.text = levelManager.scoreLine.ToString();
        eliminateText.text = levelManager.EliminateTimes.ToString();
        refreshText.text = levelManager.RefreshTimes.ToString();
        moneyText.text =Player.Instance.Money.ToString();
    }

    public void UpdateEnemyPanel(bool isReset=false)
    {
        if(enemyPanelTrans == null) levelManager = LevelManager.Instance;
        if (isReset)
        {
            enemyName.text = "";
            enemyDescription.text = "";
        }
        else
        {
            enemyName.text = levelManager.currentEnemy.enemyName.ToString();
            enemyDescription.text = levelManager.currentEnemy.description.ToString();
        }
    }

    public void ShowSettlementPanel(Dictionary<string,int> goldSource)
    {
        settlementPanelTrans.gameObject.SetActive(true);
        //btnSettle.gameObject.SetActive(true);
        settlementPanelTrans.GetComponent<GoldPanel>().ResetContent(goldSource);
        StartCoroutine(ObjectAnimator.Instance.MovePopup(settlementPanelTrans.gameObject,settlementPanelTrans.transform.position+new Vector3(0,-9f),duration:0.4f));
    }

    public void ShowGameOverPanel()
    {
        gameOverPanelTrans.gameObject.SetActive(true);
    }
    public void OnBtnMoneyClicked()
    {
        //settlementPanelTrans.gameObject.SetActive(false);
        //btnSettle.gameObject.SetActive(false);
        StartCoroutine(ObjectAnimator.Instance.MovePopup(settlementPanelTrans.gameObject, settlementPanelTrans.transform.position + new Vector3(0, 9f), duration: 0.4f,isHiding:true));
        AudioManager.Instance.PlaySound(AudioManager.AudioType.Gold);
        levelManager.GotoShop();
    }

    /// <summary>
    /// 显示特殊方块描述信息
    /// </summary>
    /// <param name="position"></param>
    /// <param name="description"></param>
    public void ShowCellDescription(Vector2 position,string description)
    {
        if (description == String.Empty)
        {
            return;
        }
        if (currentCellDescription != null)
        {
            Destroy(currentCellDescription);
        }
        Vector2 offset = new Vector2(1f,0f);
        currentCellDescription = Instantiate(cellDescription, position+offset, Quaternion.identity);
        currentCellDescription.GetComponent<CellDescription>().Init(description);
        currentCellDescription.transform.SetParent(shopFrontCanvasTrans, true);
        currentCellDescription.transform.localScale = Vector3.one;
        StartCoroutine(ObjectAnimator.Instance.AnimateIn(currentCellDescription));
    }
    public void HideCellDescription()
    {
        if (currentCellDescription != null)
        {
            Destroy(currentCellDescription);
        }
    }
    //显示规模\结构等级信息，颜色概率信息

    /// <summary>
    /// 显示规模\结构等级信息面板
    /// </summary>
    public void OnShowComboInfoPanel()
    {
        if (comboLevelPanelIsOpening) return;
        ShowComboInfoPanel();
    }
    private void ShowComboInfoPanel()
    {
        //comboLevelBody.gameObject.SetActive(true);
        string comboInfoStr = "";
        Dictionary<String, Vector2> dicts = ScoreManager.Instance.GetComboScaleInfo();
        foreach (var dict in dicts)
        {
            comboInfoStr += dict.Key + " ";
            comboInfoStr += "level " + dict.Value.x;
            comboInfoStr += ": " + dict.Value.y;
            comboInfoStr += "\n";
        }
        comboScaleInfo.text = comboInfoStr;
        comboInfoStr = "";
        dicts = ScoreManager.Instance.GetComboStructInfo();
        foreach (var dict in dicts)
        {
            comboInfoStr += dict.Key + " ";
            comboInfoStr += "level " + dict.Value.x;
            comboInfoStr += ": " + dict.Value.y;
            comboInfoStr += "\n";
        }
        comboStructInfo.text = comboInfoStr;

        pieChart.UpdateChart();


        StartCoroutine(ObjectAnimator.Instance.MovePopup(comboLevelPanelTrans.gameObject, comboLevelPanelTrans.position + new Vector3(0, 7f)));
        comboLevelPanelIsOpening = true;
    }
    public void OnHideComboInfoPanel()
    {
        //comboLevelPanelTrans.gameObject.SetActive(false);
        StartCoroutine(ObjectAnimator.Instance.MovePopup(comboLevelPanelTrans.gameObject, comboLevelPanelTrans.position + new Vector3(0, -7f)));
        comboLevelPanelIsOpening = false;
    }
}
