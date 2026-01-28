using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public EnemyBase currentEnemy;
    public int currentLevel = 0;
    private int baseScoreLine = 240;
    public int scoreLine = 0;
    private int eliminateTimes = 4;

    // 结算时的金币存储
    public Dictionary<string, int> settlementGoldCompoent = new Dictionary<string, int>
    {
        {"剩余消除次数", 0 },
        {"基础奖金", 0 },
    };
    public int EliminateTimes
    {
        get
        {
            return eliminateTimes;
        }
        set
        {
            eliminateTimes = value;
            UIManager.Instance.UpdateLevelInfo();
        }
    }
    private int refreshTimes = 4;
    public int RefreshTimes
    {
        get { return refreshTimes; }
        set
        {
            refreshTimes = value;
            UIManager.Instance.UpdateLevelInfo();
        }
    }


    private GridManager gridManager;
    private ShopManager shopManager;
    private ScoreManager scoreManager;

    private Button shopNextLevelBtn;

    public static Action OnCaculateScore;

    public bool isPass=> scoreManager.totalScore >= scoreLine;

    public bool isGameOver;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gridManager = GridManager.Instance;
        shopManager = ShopManager.Instance;
        scoreManager = ScoreManager.Instance;

        shopNextLevelBtn = UIManager.Instance.shopNextLevelBtn;
        shopNextLevelBtn.onClick.AddListener(AdvanceLevel);
        shopNextLevelBtn.gameObject.SetActive(false);

        AdvanceLevel();
        OnCaculateScore += CheckScore;
        isGameOver = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            AdvanceLevel();
        }
    }
    public void AdvanceLevel()
    {
        currentLevel++;
        if (currentEnemy != null)
        {
            ResourcesManager.Instance.DestoryEnemy(currentEnemy);
        }
        currentEnemy=ResourcesManager.Instance.GetOneRandomEnemy();
        scoreLine =(int)(CalculateLevelScoreLine(currentLevel, baseScoreLine)*currentEnemy.scoreMultiplication);
        EliminateTimes = 5;
        RefreshTimes = 3;
        shopManager.CloseShop();
        gridManager.ResetMap();
        UIManager.Instance.UpdateEnemyPanel();
        UIManager.Instance.UpdateLevelInfo();
        UIManager.Instance.ResetScorePanel();
        ResourcesManager.Instance.ResetCellDeck();
        //关卡刷新：过关分数刷新，信息刷新，过关事件触发
    }

    public void CheckScore()
    {
        if (isPass)
        {
            Debug.Log("过关，进入商店");
            gridManager.ClearMap();
            gridManager.stopCoroutine = true;
            RefreshSettlementGoldDicts();
            ScoreManager.Instance.ResetScore();
            UIManager.Instance.ShowSettlementPanel(settlementGoldCompoent);
        }
        else if (!isPass&&EliminateTimes <= 0)
        {
            isGameOver = true;
            UIManager.Instance.ShowGameOverPanel();
        }
    }

    public void GotoShop()
    {
        SettleBonus();
        shopManager.OpenShop();
        UIManager.Instance.ResetScorePanel();
        UIManager.Instance.UpdateEnemyPanel(true);
    }

    public void SubEliminateTimes()
    {
        EliminateTimes--;
        UIManager.Instance.UpdateLevelInfo();
    }

    public bool SubRefreshTimes()
    {
        if (RefreshTimes <= 0)
        {
            return false;
        }
        RefreshTimes--;
        return true;
    }

    public void SettleBonus()
    {
        int totalGold = GetTotalMoney();
        Player.Instance.AddMoney(totalGold);
    }
    public int GetTotalMoney()
    {
        int totalGold = 0;
        foreach (var item in settlementGoldCompoent.Values)
        {
            totalGold += item;
        }
        return totalGold;
    }
    private void RefreshSettlementGoldDicts()
    {
        settlementGoldCompoent["剩余消除次数"] = EliminateTimes;
        settlementGoldCompoent["基础奖金"] = currentEnemy.bonus;
    }

    /// <summary>
    /// 根据关卡数计算整数最终分数。
    /// 分数增长为指数函数（每3关翻倍），组内使用整数步进平滑过渡。
    /// </summary>
    /// <param name="level">当前关卡数 (L >= 1)。</param>
    /// <param name="baseScoreConstant">基础分数常数，必须是 3 的倍数 (例如 300, 3000)。</param>
    /// <returns>当前关卡的最终整数分数。</returns>
    public static int CalculateLevelScoreLine(int level, int baseScoreConstant = 300)
    {
        // 安全检查：确保 baseScoreConstant 是 3 的倍数
        if (baseScoreConstant % 3 != 0)
        {
            Debug.LogError("Base Score Constant must be a multiple of 3 to guarantee integer steps.");
            // 尝试使用最接近的 3 的倍数
            baseScoreConstant = (baseScoreConstant / 3) * 3;
            if (baseScoreConstant == 0) baseScoreConstant = 3;
        }

        if (level <= 0) return 0;

        // 1. 确定关卡组 n
        int n = Mathf.CeilToInt((float)level / 3.0f);

        // 2. 计算组的结束分数 S_end (L=3n)
        // S_end = B_base * 2^(n-1)
        // 使用 int 强制转换，因为 n-1 >= 0，Mathf.Pow 的结果应是精确的 2 的幂。
        int S_end = baseScoreConstant * (int)Mathf.Pow(2.0f, n - 1);

        // 3. 计算组的起始分数 S_start (L=3n-3)
        int S_start;
        if (n == 1)
        {
            // 第一组 (L=1, 2, 3) 从 0 开始计算，实现平滑起步
            S_start = 0;
        }
        else
        {
            // S_start = B_base * 2^(n-2)
            S_start = baseScoreConstant * (int)Mathf.Pow(2.0f, n - 2);
        }

        // 4. 计算整数步进值 StepSize
        int S_diff = S_end - S_start;
        // 保证 S_diff 是 3 的倍数，所以 StepSize 是整数
        int stepSize = S_diff / 3;

        // 5. 根据关卡在组内的偏移量确定最终分数
        int offset = level % 3;

        if (offset == 0)
        {
            // L = 3n (例如 3, 6, 9)
            return S_end;
        }
        else if (offset == 1)
        {
            // L = 3n-2 (例如 1, 4, 7)
            return S_start + stepSize;
        }
        else // offset == 2
        {
            // L = 3n-1 (例如 2, 5, 8)
            return S_start + 2 * stepSize;
        }
    }
}
