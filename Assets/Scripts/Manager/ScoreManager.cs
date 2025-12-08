using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
public enum ComboScale
{
    None,
    Small,              //消除数<=3
    Normal,
    Big,
    Huge,
}
public enum ComboStruct
{
    None,
    Base,
    Complex,
    Line,
    Block,
}
public class ScoreManager : MonoBehaviour
{
    //private static ScoreManager scoreManager;
    public static ScoreManager Instance;
    private CardHandler CardHandler => CardHandler.Instance;

    public int combinationLevel = 0;  //表示当前消除组合类型的等级
    public int baseMagnification=1;//分数倍率
    private float magnification = 1; //当前分数倍率
    public float Magnification
    {
        get
        {
            return magnification;
        }
        set
        {
            magnification = value;
            UIManager.OnMagnificationChanged?.Invoke(magnification);
        }
    }
    public int baseChips = 0;        //基础筹码
    private int chips = 0;   //当前筹码
    public int Chips
    {
        get
        {
            return chips;
        }
        set
        {
            chips = value;
            UIManager.OnChipsChanged?.Invoke(chips);
        }
    }
    public int score = 0;   //当前所得分数   筹码乘以倍率
    public int maxScore = 0;//最高分数
    public int totalScore = 0;  //总分数
    public ClearGeneralParameters paras;//基础消除参数

    // 新增事件，用于通知 UIManager 更新预测 UI
    public static event Action<string, string,bool> OnComboPredicted;
    public static event Action OnPredictionCleared;


    // ComboScale 每个项的等级和基础筹码
    private Dictionary<ComboScale, int> comboScaleLevels = new()
{
    { ComboScale.Small, 1 },
    { ComboScale.Normal, 1 },
    { ComboScale.Big, 1 },
    { ComboScale.Huge, 1 }
};
    private Dictionary<ComboScale, int> comboScaleBaseChips = new()
{
    { ComboScale.Small, 5 },
    { ComboScale.Normal, 10 },
    { ComboScale.Big, 20 },
    { ComboScale.Huge, 40 }
};

    //ComboScale  每级添加的筹码数
    private Dictionary<ComboScale, int> comboScalePerLevelChips = new()
    {
        { ComboScale.Small, 10 },
        { ComboScale.Normal, 15 },
        { ComboScale.Big, 20 },
        {ComboScale.Huge,25 },
    };

    // ComboStruct 每个项的等级和基础倍率
    private Dictionary<ComboStruct, int> comboStructLevels = new()
{
    { ComboStruct.Base, 1 },
    { ComboStruct.Complex, 1 },
    { ComboStruct.Line, 1 },
    { ComboStruct.Block, 1 }
};
    private Dictionary<ComboStruct, int> comboStructBaseMagnification = new()
{
    { ComboStruct.Base, 1 },
    { ComboStruct.Complex, 2 },
    { ComboStruct.Line, 3 },
    { ComboStruct.Block, 4 }
};

    //ComboStruct 每级添加的倍率数
    private Dictionary<ComboStruct, int> comboStructPerLevelMagnifications = new()
    {
        { ComboStruct.Base, 1 },
        { ComboStruct.Complex, 2 },
        { ComboStruct.Line, 3 },
        { ComboStruct.Block, 4 },
    };


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void ResetScore()
    {
        combinationLevel = 0;
        score = 0;
        maxScore = 0;
        totalScore = 0;
    }

    public ClearGeneralParameters InitializeCombo(List<ColorCell> cells)
    {
        ComboScale comboScale=GetComboScale(cells);
        ComboStruct comboStruct = GetComboStruct(cells);
        ColorCell firstCell = cells[0];
        paras = new ClearGeneralParameters(firstCell.colorType, comboScale, comboStruct);

        Magnification = comboStructBaseMagnification[comboStruct];
        Chips = comboScaleBaseChips[comboScale];

        if (LevelManager.Instance.currentEnemy is TypeLimtEnemy)
        {
            TypeLimtEnemy enemy = (TypeLimtEnemy)LevelManager.Instance.currentEnemy;
            if (enemy.MatchCombo(paras))
            {
                Magnification = 0;
                Chips = 0;
            }
        }

        UIManager.OnSingleCaculateScore?.Invoke(Chips,Magnification);
        return paras;
        //CardHandler.BeforeCalculateScore?.Invoke(paras);
    }

    public int CalculateSingleCellChips(ColorCell cell)
    {
        int initialChips = (int)cell.GetChips();
        Chips += initialChips;

        // 触发卡牌计分前的效果 (同步效果)
        //CardHandler.OnCalculateScore?.Invoke(paras);
        UIManager.OnSingleCaculateScore?.Invoke(Chips, Magnification);

        // 返回该单元格的总筹码贡献（不乘倍率）
        return initialChips;
    }

    public void FinalizeScore()
    {
        totalScore += Chips * (int)Magnification;
        if (score > maxScore)
        {
            maxScore = score;
        }

        // 通知 UIManager 启动总分数滚动动画
        // 传递原始的筹码和倍率，让 UI 去滚动
        //CardHandler.AfterCalculateScore?.Invoke(paras);
        UIManager.OnFinalScoreCalculated?.Invoke(Chips, (int)Magnification);
        LevelManager.OnCaculateScore?.Invoke();
    }

    // 新增：预测消除组合类型并发送事件
    public void PredictCombo(List<ColorCell> cells)
    {
        if (cells == null) // 假设至少需要3个才能消除
        {
            ClearPrediction();
            return;
        }

        ComboScale comboScale = GetComboScale(cells);
        ComboStruct comboStruct = GetComboStruct(cells);
        ColorType color = cells[0].colorType;
        ClearGeneralParameters paras=new ClearGeneralParameters(color, comboScale,comboStruct);

        string scaleName = GetComboScaleName(comboScale);
        string structName = GetComboStructName(comboStruct);

        // 触发事件，将预测结果发送给 UIManager
        bool isLimited=LevelManager.Instance.currentEnemy is TypeLimtEnemy &&((TypeLimtEnemy)LevelManager.Instance.currentEnemy).MatchCombo(paras);
        OnComboPredicted?.Invoke(scaleName, structName, isLimited);
    }

    // 新增：清除预测显示
    public void ClearPrediction()
    {
        OnPredictionCleared?.Invoke();
    }

    public void UpgradeComboScale(ComboScale scale)
    {
        comboScaleLevels[scale]++;
        comboScaleBaseChips[scale] += comboScalePerLevelChips[scale]; // 升级时基础筹码+2，可根据实际调整
    }
    public void UpgradeComboStruct(ComboStruct comboStruct)
    {
        comboStructLevels[comboStruct]++;
        comboStructBaseMagnification[comboStruct] += comboStructPerLevelMagnifications[comboStruct]; // 升级时基础倍率+1，可根据实际调整
    }

    private ComboScale GetComboScale(List<ColorCell> cells)
    {
        int count = cells.Count;
        if (count <= 3) return ComboScale.Small;
        if (count <= 5) return ComboScale.Normal;
        if (count <= 8) return ComboScale.Big;
        return ComboScale.Huge;
    }
    private ComboStruct GetComboStruct(List<ColorCell> cells)
    {
        // --- 预处理：将格子的位置映射到 HashSet，并确定边界 ---
        // 使用 HashSet 存储所有实际存在的格子位置 (x, y)，用于 O(1) 查找
        var cellLocations = new HashSet<(int, int)>();
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var cell in cells)
        {
            // 确保坐标为整数
            int x = Mathf.RoundToInt(cell.location.x);
            int y = Mathf.RoundToInt(cell.location.y);

            cellLocations.Add((x, y));

            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }

        // --- 1. Block判断 (查找 2x2 子矩形) ---
        // 遍历所有可能的 2x2 矩形的左下角 (i, j)
        if (cells.Count >= 4)
        {
            for (int i = minX; i <= maxX - 1; i++) // 遍历 X 轴，至少需要 i 和 i+1
            {
                for (int j = minY; j <= maxY - 1; j++) // 遍历 Y 轴，至少需要 j 和 j+1
                {
                    // 检查这个 2x2 区域的四个格子是否都被选中：
                    if (cellLocations.Contains((i, j)) &&  // 左下 (i, j)
                        cellLocations.Contains((i + 1, j)) &&  // 右下 (i+1, j)
                        cellLocations.Contains((i, j + 1)) &&  // 左上 (i, j+1)
                        cellLocations.Contains((i + 1, j + 1)))    // 右上 (i+1, j+1)
                    {
                        // 找到了一个 2x2 的子区域，满足 Block 条件
                        return ComboStruct.Block;
                    }
                }
            }
        }
    // --- 2. Line判断（横排或竖排，至少3个） ---
        // 横排判断
        var yGroups = new Dictionary<int, int>();
        foreach (var (x, y) in cellLocations)
        {
            if (!yGroups.ContainsKey(y)) yGroups[y] = 0;
            yGroups[y]++;
        }
        foreach (var kv in yGroups)
        {
            if (kv.Value >= 3) return ComboStruct.Line;
        }

        // 竖排判断
        var xGroups = new Dictionary<int, int>();
        foreach (var (x, y) in cellLocations)
        {
            if (!xGroups.ContainsKey(x)) xGroups[x] = 0;
            xGroups[x]++;
        }
        foreach (var kv in xGroups)
        {
            if (kv.Value >= 3) return ComboStruct.Line;
        }

        // --- 3. Complex判定 ---
        // 注意：如果满足 Block 或 Line，则不会执行到这里。
        if (cells.Count > 4) return ComboStruct.Complex;

        // --- 4. Base ---
        return ComboStruct.Base;
    }

    public Dictionary<string,Vector2> GetComboScaleInfo()       //x是等级    y是数值
    {
        Dictionary<string,Vector2> keyValuePairs = new Dictionary<string,Vector2>();
        foreach(var kv in comboScaleLevels)
        {
            Vector2 vector2=new Vector2(kv.Value, comboScaleBaseChips[kv.Key]);
            string name;
            name=GetComboScaleName(kv.Key);
            keyValuePairs[name] = vector2;
        }
        return keyValuePairs;
    }
    public Dictionary<string, Vector2> GetComboStructInfo()
    {
        Dictionary<string, Vector2> keyValuePairs = new Dictionary<string, Vector2>();
        foreach (var kv in comboStructLevels)
        {
            Vector2 vector2 = new Vector2(kv.Value, comboStructBaseMagnification[kv.Key]);
            string name;
            name=GetComboStructName(kv.Key);
            keyValuePairs[name] = vector2;
        }
        return keyValuePairs;
    }

    public string GetComboScaleName(ComboScale comboScale)
    {
        string name;
        if (comboScale == ComboScale.Small)
        {
            name = "微粒";
        }
        else if (comboScale == ComboScale.Normal)
        {
            name = "聚合";
        }
        else if (comboScale == ComboScale.Big)
        {
            name = "巨石";
        }
        else
        {
            name = "奇迹";
        }
        return name;
    }
    public string GetComboStructName(ComboStruct comboStruct)
    {
        string name;
        if (comboStruct == ComboStruct.Base)
        {
            name = "散块";
        }
        else if (comboStruct == ComboStruct.Complex)
        {
            name = "复杂";
        }
        else if (comboStruct == ComboStruct.Line)
        {
            name = "直线";
        }
        else
        {
            name = "板块";
        }
        return name;
    }

    public void AddChips(int _chips)
    {
        Chips += _chips;
    }
    public void AddMagnification(int _magnification)
    {
        Magnification += _magnification;
    }
    public void MultiplyMagnification(float _magnification)
    {
        Magnification *= _magnification;
    }
}
