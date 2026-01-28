using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesManager : MonoBehaviour
{
    [Serializable]
    public struct SpecialColorCell
    {
        public string name;
        public GameObject colorCell;
    }
    public static ResourcesManager Instance;
    public List<CardData> cardsList;
    public List<PropData> propsList;
    public List<EnemyBase> enemyList;
    public GameObject clearCellEffect;
    public GameObject flowTextEffect;
    public Dictionary<string, CardData> cardsDict = new Dictionary<string, CardData>();
    public Dictionary<string, PropData> propsDict = new Dictionary<string, PropData>();
    public Dictionary<string, EnemyBase> enemyDict = new Dictionary<string, EnemyBase>();

    private GameObject shadowPrefab;
    private GameObject shadowUIPrefab;
    private GameObject uiCardPrefab;
    private GameObject uiPropPrefab;
    private GameObject colorGridPrefab;

    [Header("场景物体集合")]
    public Transform cardGroups;

    [Header("基本格子贴图")]
    public Sprite redCellSprite;
    public Sprite greenCellSprite;
    public Sprite blueCellSprite;
    public Sprite yellowCellSprite;

    [Header(" 材质")]
    public Material outlineMat;

    [Header("特殊格子模块")]
    public float baseSpecialCellsProbabilities = 0.1f;
    public List<SpecialColorCell> specialColorCells = new List<SpecialColorCell>();
    //存储每个特殊格子及其权重，用于加权随机选择
    private Dictionary<GameObject, float> specialCellWeights = new Dictionary<GameObject, float>();
    // 存储归一化后的特殊格子概率
    public Dictionary<GameObject, float> CurrentSpecialCellProbabilities { get; private set; } = new Dictionary<GameObject, float>();

    [Header("颜色模块")]
    // 存储每个事件（颜色）的权重。默认初始权重为 1.0。
    private Dictionary<ColorType, float> colorWeights = new()
    {
        {ColorType.Red,1 },
        {ColorType.Green,1 },
        {ColorType.Blue,1 },
        {ColorType.Yellow,1 },
    };

    // 获取归一化后的实际概率 (只读属性)
    public Dictionary<ColorType, float> CurrentProbabilities { get; private set; } = new Dictionary<ColorType, float>();

    [Header("方块背包")]
    [SerializeField] private CellAssemblyConfig cellAssemblyConfig;
    [SerializeField] private GameObject baseCellPrefab;
    public List<CellConfig> masterDeck = new List<CellConfig>();
    public List<CellConfig> currentRunDeck = new List<CellConfig>();

    private void Awake()
    {
        Instance = this;
        cardsList = Resources.LoadAll<CardData>("SOAssets/Card").ToList();
        propsList = Resources.LoadAll<PropData>("SOAssets/Prop").ToList();
        enemyList = Resources.LoadAll<EnemyBase>("EnemyAssets").ToList();
        uiCardPrefab = Resources.Load<GameObject>("Prefabs/Card/NewUICard");
        uiPropPrefab = Resources.Load<GameObject>("Prefabs/Prop/NewUIProp");
        colorGridPrefab = Resources.Load<GameObject>("Prefabs/ColorCell/ColorCell");
        shadowPrefab = Resources.Load<GameObject>("Prefabs/Other/Shadow");
        shadowUIPrefab = Resources.Load<GameObject>("Prefabs/Other/Shadow(UI)");
    }

    private void Start()
    {
        InitCardsDict();
        InitCellDeck();

        RecalculateColorProbabilities();
        InitSpecialCellWeights();
        RecalculateSpecialCellProbabilities();
    }

    private void InitCardsDict()
    {
        foreach (CardData card in cardsList)
        {
            cardsDict[card.cardName] = card;
        }
        foreach (PropData prop in propsList)
        {
            propsDict[prop.propName] = prop;
        }
        foreach (EnemyBase enemy in enemyList)
        {
            enemyDict[enemy.enemyName] = enemy;
        }
    }

    #region 方块背包模块

    private void InitCellDeck()
    {
        for(int i = 0; i < 10; i++)
        {
            masterDeck.Add(new CellConfig(ColorType.Red, "ColorCell"));
            masterDeck.Add(new CellConfig(ColorType.Yellow, "ColorCell"));
            masterDeck.Add(new CellConfig(ColorType.Blue, "ColorCell"));
            masterDeck.Add(new CellConfig(ColorType.Green, "ColorCell"));
        }
        for(int i = 0; i < 5; i++)
        {
            masterDeck.Add(new CellConfig(ColorType.Red, "ChipsCell"));
            masterDeck.Add(new CellConfig(ColorType.Yellow, "ChipsCell"));
            masterDeck.Add(new CellConfig(ColorType.Blue, "ChipsCell"));
            masterDeck.Add(new CellConfig(ColorType.Green, "ChipsCell"));
        }
        for (int i = 0; i < 5; i++)
        {
            masterDeck.Add(new CellConfig(ColorType.Red, "MagnifactionCell"));
            masterDeck.Add(new CellConfig(ColorType.Yellow, "MagnifactionCell"));
            masterDeck.Add(new CellConfig(ColorType.Blue, "MagnifactionCell"));
            masterDeck.Add(new CellConfig(ColorType.Green, "MagnifactionCell"));
        }
        ResetCellDeck();
    }

    /// <summary>
    /// 每一关开始时调用，将当前牌组恢复到初始状态
    /// </summary>
    public void ResetCellDeck()
    {
        currentRunDeck = new List<CellConfig>(masterDeck);
    }

    /// <summary>
    /// 从背包中随机抽取并“消耗”一个方块
    /// </summary>
    public GameObject GetCellFromDeck()
    {
        if (currentRunDeck.Count == 0)
        {
            Debug.LogWarning("背包已空！");
        }

        // 随机选一个索引
        int randomIndex = UnityEngine.Random.Range(0, currentRunDeck.Count);
        CellConfig config = currentRunDeck[randomIndex];

        // 从当前可用列表中移除
        currentRunDeck.RemoveAt(randomIndex);
        return BuildCellFromConfig(config);
    }

    public GameObject BuildCellFromConfig(CellConfig config)
    {
        // 1. 实例化通用的“壳”
        GameObject go = Instantiate(baseCellPrefab);

        // 2. 根据字符串反射添加对应的脚本组件
        // 如果 scriptTypeName 是 "ColorCell"，就添加 ColorCell 组件
        System.Type type = System.Type.GetType(config.scriptTypeName);
        ColorCell cellComponent = go.AddComponent(type) as ColorCell;

        // 3. 调用 Init 配置
        cellComponent.Init(config.colorType);

        return go;
    }

    /// <summary>
    /// 向全局背包添加新方块。
    /// </summary>
    public void AddCellToMasterDeck(GameObject cellObject)
    {
        ColorCell cellScript = cellObject.GetComponent<ColorCell>();
        if (cellScript != null)
        {
            // 使用构造函数 2，自动提取脚本类型和颜色信息
            CellConfig newConfig = new CellConfig(cellScript);
            masterDeck.Add(newConfig);
        }
    }

    /// <summary>
    /// 向当前关卡背包添加新方块
    /// </summary>
    public void AddCellToCurrentDeck(GameObject cellObject)
    {
        ColorCell cellScript = cellObject.GetComponent<ColorCell>();
        if (cellScript != null)
        {
            currentRunDeck.Add(new CellConfig(cellScript));
        }
    }

    public Sprite GetCellSprite(ColorType color)
    {
        return cellAssemblyConfig.GetColorSprite(color);
    }

    public Sprite GetSpecialCellSprite(string typeName)
    {
        return cellAssemblyConfig.GetTypeSprite(typeName);
    }

    #endregion

    private void InitSpecialCellWeights()
    {
        foreach (var specialCell in specialColorCells)
        {
            // 默认初始权重为 1.0f
            specialCellWeights.Add(specialCell.colorCell, 1.0f);
        }
    }

    public GameObject GetOneRandomColorCell()
    {
        GameObject gridCellPrefab;

        // 1. 判断生成普通格子还是特殊格子
        // 随机数小于 baseSpecialCellsProbabilities 则生成特殊格子
        if (UnityEngine.Random.value < baseSpecialCellsProbabilities &&
            CurrentSpecialCellProbabilities.Count > 0)
        {
            // 2.生成特殊格子 (进行加权随机选择)
            gridCellPrefab = GetRandomSpecialCellPrefab();
        }
        else
        {
            // 3. 生成普通 ColorCell
            gridCellPrefab = colorGridPrefab;
        }

        // 实例化选中的预制件
        GameObject gridCell = Instantiate(gridCellPrefab);

        // 初始化 ColorCell 组件
        ColorCell colorCell = gridCell.GetComponent<ColorCell>();
        ColorType colorType = GetRandomColorType();
        colorCell.Init(colorType);

        return gridCell;
    }

    //进行特殊格子的加权随机选择
    private GameObject GetRandomSpecialCellPrefab()
    {
        float randomNumber = UnityEngine.Random.value;
        float cumulativeProbability = 0f;

        foreach (var kvp in CurrentSpecialCellProbabilities)
        {
            GameObject specialCell = kvp.Key;
            float probability = kvp.Value;

            cumulativeProbability += probability;

            if (randomNumber <= cumulativeProbability)
            {
                return specialCell; // 返回被抽中的特殊格子预制体
            }
        }

        // 回退机制：如果计算有问题，返回列表中的第一个特殊格子
        return CurrentSpecialCellProbabilities.Keys.First();
    }
    public GameObject GetOneRandomRelicCard()
    {
        GameObject card = Instantiate(uiCardPrefab);
        UICard uiCard = card.GetComponent<UICard>();
        CardData prefab = cardsList[UnityEngine.Random.Range(0, cardsList.Count)];
        CardData clone = prefab.Clone();
        uiCard.SetUp(new CardInstance(clone, uiCard));
        uiCard.transform.parent = cardGroups;
        return card;
    }
    public GameObject GetOneRandomPropCard()
    {
        GameObject prop = Instantiate(uiPropPrefab);
        UIProp uiProp = prop.GetComponent<UIProp>();
        PropData prefab = propsList[UnityEngine.Random.Range(0, propsList.Count)];
        PropData clone = prefab.Clone();
        uiProp.SetUp(clone);
        uiProp.transform.parent = cardGroups;
        return prop;
    }
    public EnemyBase GetOneRandomEnemy()
    {
        EnemyBase enemy = enemyList[UnityEngine.Random.Range(0, enemyList.Count)];
        EnemyBase clone = enemy.Clone();
        clone.Initialize();
        clone.SubscribeToEvents();
        return clone;
    }
    public void DestoryEnemy(EnemyBase enemy)
    {
        enemy.UnsubscribeFromEvents();
        //Destory
    }
    public GameObject GetShadow(Transform transform,Sprite shadowSprite,bool isUI=true)
    {
        GameObject prefab = shadowUIPrefab;
        if(!isUI)
        {
            prefab = shadowPrefab;
        }
        GameObject shadow=Instantiate(prefab, transform);
        if (!isUI)
        {
            shadow.GetComponent<SpriteRenderer>().sprite = shadowSprite;
            return shadow;
        }
        Image image= shadow.GetComponent<Image>();
        image.sprite = shadowSprite;
        image.SetNativeSize();
        return shadow;
    }

    public GameObject GetClearCellEffect()
    {
        GameObject particleObject = Instantiate(clearCellEffect);
        particleObject.name = clearCellEffect.name + "_Instance";
        return particleObject;
    }
    public GameObject GetFlowTextEffect()
    {
        GameObject flowTextObject = Instantiate(flowTextEffect);
        flowTextObject.name = flowTextEffect.name + "_Instance";
        return flowTextObject;
    }

    public Material GetMaterial(string materialName)
    {
        if (materialName == "Outline")
        {
            return outlineMat;
        }
        return null;
    }


    /// <summary>
    /// 核心计算方法：根据当前权重计算归一化后的实际概率。
    /// </summary>
    private void RecalculateColorProbabilities()
    {
        // 1. 计算总权重 (S)
        float totalWeight = colorWeights.Values.Sum();

        // 2. 计算每个颜色的归一化概率
        CurrentProbabilities.Clear();
        foreach (var kvp in colorWeights)
        {
            ColorType color = kvp.Key;
            float weight = kvp.Value;

            // 归一化公式: P_i = w_i / S
            float probability = weight / totalWeight;
            CurrentProbabilities.Add(color, probability);
        }
    }

    // 核心计算方法：根据当前权重计算特殊格子归一化后的实际概率。
    private void RecalculateSpecialCellProbabilities()
    {
        // 1. 计算特殊格子的总权重 (S)
        float totalWeight = specialCellWeights.Values.Sum();

        // 如果没有特殊格子或总权重为零，则不进行计算
        if (totalWeight <= 0)
        {
            CurrentSpecialCellProbabilities.Clear();
            return;
        }

        // 2. 计算每个特殊格子的归一化概率
        CurrentSpecialCellProbabilities.Clear();
        foreach (var kvp in specialCellWeights)
        {
            GameObject specialCellPrefab = kvp.Key;
            float weight = kvp.Value;

            // 归一化公式: P_i = w_i / S
            float probability = weight / totalWeight;
            CurrentSpecialCellProbabilities.Add(specialCellPrefab, probability);
        }
    }

    /// <summary>
    /// 对指定颜色的概率进行相对提升的方法。
    /// </summary>
    /// <param name="colorName">要提升概率的颜色名称。</param>
    /// <param name="increasePercentage">提升的百分比（如 0.10 代表 10%）。</param>
    /// <returns>操作是否成功。</returns>
    public bool IncreaseColorProbability(ColorType colorName, float increaseAmount)
    {
        if (!colorWeights.ContainsKey(colorName))
        {
            Debug.LogError($"Color '{colorName}' not found in the manager.");
            return false;
        }

        // 计算提升后的新权重
        float currentWeight = colorWeights[colorName];

        // 提升 X% 意味着新的权重是旧权重的 (1 + X%) 倍。
        // 例如：1.0 提升 10% (0.1) -> 1.0 * 1.1 = 1.1
        //float newWeight = currentWeight * (1.0f + increasePercentage);
        float newWeight = currentWeight + increaseAmount;
        // 更新权重
        colorWeights[colorName] = newWeight;

        // 重新计算并归一化所有概率
        RecalculateColorProbabilities();

        return true;
    }

    /// <summary>
    /// 增加生成特殊格子的基础概率 (相对于普通格子的总概率)。
    /// </summary>
    /// <param name="increasePercentage">提升的百分比（如 0.10 代表 10%）。</param>
    public void IncreaseBaseSpecialCellProbability(float increasePercentage)
    {
        // 确保概率不超过 1.0f (100%)
        baseSpecialCellsProbabilities = Mathf.Clamp01(baseSpecialCellsProbabilities * (1.0f + increasePercentage));
    }

    /// <summary>
    /// 对指定特殊格子的相对生成概率进行提升。
    /// </summary>
    /// <param name="specialCellPrefab">要提升概率的特殊格子预制体。</param>
    /// <param name="increasePercentage">提升的百分比（如 0.10 代表 10%）。</param>
    /// <returns>操作是否成功。</returns>
    public bool IncreaseSpecialCellTypeProbability(string specialCellname, float increaseAmount)
    {
        GameObject specialCellPrefab = specialColorCells.Find(cell => cell.name == specialCellname).colorCell;
        if (!specialCellWeights.ContainsKey(specialCellPrefab))
        {
            Debug.LogError($"Special Cell Prefab '{specialCellPrefab.name}' not found in the manager.");
            return false;
        }

        // 计算提升后的新权重
        float currentWeight = specialCellWeights[specialCellPrefab];

        //// 提升 X% 意味着新的权重是旧权重的 (1 + X%) 倍。
        //float newWeight = currentWeight * (1.0f + increasePercentage);
        // 【修改】直接增加权重，而不是乘以百分比
        float newWeight = currentWeight + increaseAmount;

        // 权重不能为负
        specialCellWeights[specialCellPrefab] = Mathf.Max(0f, newWeight);

        // 更新权重
        specialCellWeights[specialCellPrefab] = newWeight;

        // 重新计算并归一化所有特殊格子的相对概率
        RecalculateSpecialCellProbabilities();

        return true;
    }

    public ColorType GetRandomColorType()
    {
        // 1. 生成 0 到 1 之间的随机数
        float randomNumber = UnityEngine.Random.value;

        // 2. 初始化累积概率
        float cumulativeProbability = 0f;

        // 3. 遍历当前概率字典，判断随机数落点
        // 注意：遍历顺序是确定的，因为 Dictionary<TKey, TValue> 的顺序是固定的（至少在 C# 现代版本中，它保持了插入顺序）
        foreach (var kvp in CurrentProbabilities)
        {
            ColorType color = kvp.Key;
            float probability = kvp.Value;

            // 累加当前颜色概率
            cumulativeProbability += probability;

            // 4. 判断：如果随机数小于或等于累积概率，则命中
            if (randomNumber <= cumulativeProbability)
            {
                return color; // 返回被抽中的颜色
            }
        }

        // 理论上，如果所有概率的总和为 1.0，代码不应该到达这里。
        // 作为安全回退，如果出现浮点数误差等问题，返回列表中的第一个颜色。
        return CurrentProbabilities.Keys.First();
    }
}
public static class ScriptableObjectExtensions
{
    /// <summary>
    /// 对 ScriptableObject 实例进行深拷贝，创建一个全新的、可修改的运行时副本。
    /// </summary>
    public static T Clone<T>(this T scriptableObject) where T : ScriptableObject
    {
        if (scriptableObject == null)
        {
            Debug.LogError("ScriptableObject to clone is null.");
            return null;
        }

        // 使用 Object.Instantiate 方法创建 SO 的运行时副本。
        // 这是在运行时克隆 SO 的标准方法。
        T clone = UnityEngine.Object.Instantiate(scriptableObject);

        // 可选：重命名克隆体，以便在 Hierarchy 或 Debugging 中区分
        clone.name = scriptableObject.name + " (Cloned)";

        return clone;
    }
}
