using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClearSequenceService : MonoBehaviour
{
    public static ClearSequenceService Instance;
    private GridManager GridManager => GridManager.Instance;
    private ScoreManager ScoreManager => ScoreManager.Instance;
    private CardHandler CardHandler => CardHandler.Instance;
    private LevelManager LevelManager => LevelManager.Instance;

    private List<ColorCell> currentSelectColorCells=new List<ColorCell>();
    private Coroutine readyClearRoutine;
    private Coroutine clearRoutine;
    private Coroutine switchColorRoutine;

    public bool isClearing => clearRoutine != null;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 对外暴露的启动消除流程的接口。
    /// </summary>
    public void StartClearFlow(Vector2 startLocation, ColorType targetColor)
    {
        if (clearRoutine != null) return;
        // 1. 获取要清除的单元格列表 (调用 GridManager 的只读方法)
        List<ColorCell> cellsToClear = GridManager.GetCellsToClear(startLocation, targetColor);
        if (readyClearRoutine != null)
        {
            StopCoroutine(readyClearRoutine);
            readyClearRoutine = null;
        }
        clearRoutine= StartCoroutine(ClearFlowCoroutine(cellsToClear));
    }

    public void StartClearReadyFlow(Vector2 startLocation, ColorType targetColor)
    {
        if (clearRoutine != null) return;

        List<ColorCell> cellsToClear = GridManager.GetCellsToClear(startLocation, targetColor);
        if (cellsToClear.Count == currentSelectColorCells.Count &&
            cellsToClear.All(currentSelectColorCells.Contains))
        {
            return; // 目标一致，不启动新协程
        }
        if (readyClearRoutine != null)
        {
            StopCoroutine(readyClearRoutine);
            readyClearRoutine = null;
        }
        readyClearRoutine = StartCoroutine(ReadyClearCoroutine(cellsToClear));
    }

    public void ClearPrediction()
    {
        if (clearRoutine != null) return;
        if (readyClearRoutine != null)
        {
            StopCoroutine(readyClearRoutine);
            readyClearRoutine = null;
        }

        List<ColorCell> validCells = currentSelectColorCells
        // 检查 Unity 引用是否有效（这是关键）
        .Where(cell => cell != null)
        .ToList();

        foreach (var cell in validCells)
        {
            cell.DeSelected();
        }
        currentSelectColorCells.Clear();
        ScoreManager.ClearPrediction();
    }

    ///<summary>
    ///第一下点击的预备消除协程
    /// </summary>
    private IEnumerator ReadyClearCoroutine(List<ColorCell> cellsToClear)
    {
        ClearPrediction();
        currentSelectColorCells =cellsToClear;
        ScoreManager.Instance.PredictCombo(cellsToClear);

        foreach (ColorCell cell in cellsToClear.OrderBy(c => c.location.x))
        {
            //调用单元格被选中协程
            yield return cell.BeSelected();
        }
    }



    /// <summary>
    /// 核心消除流程编排。
    /// </summary>
    private IEnumerator ClearFlowCoroutine(List<ColorCell> cellsToDestroy)
    {
        if (currentSelectColorCells.Count > 0)
        {
            ClearPrediction(); 
        }
        // 0. 确定受影响的列
        HashSet<int> affectedColumns = new HashSet<int>();
        foreach (var cell in cellsToDestroy)
        {
            affectedColumns.Add((int)cell.location.x);
        }

        // 1. 初始化分数和组合类型
        ClearGeneralParameters paras = ScoreManager.InitializeCombo(cellsToDestroy); // 调用 ScoreManager
        //ScoreManager.ClearPrediction(); // 清除预测UI

        // 2. 触发道具前置效果 (调用 CardHandler 的异步执行方法)
        yield return CardHandler.ExecuteBeforeCalculateScore(paras); // 道具动画等待

        // 3. 动态消除和单格计分
        int totalChips = 0;
        float cellClearDelay = 0.2f;

        // 按照Y轴顺序消除，视觉上更自然
        foreach (ColorCell cell in cellsToDestroy.OrderBy(c => c.location.x))
        {
            // 计算单个单元格的筹码贡献
            int cellChips = ScoreManager.CalculateSingleCellChips(cell);
            yield return CardHandler.ExecuteCalculateScore(paras);
            totalChips += cellChips;

            // 触发单个单元格的消除动画（由 GridManager 或 Cell 自身处理）
            //yield return cell.PlayClearAnimation(); // 假设 ColorCell.cs 中有此协程
            //EffectManager.Instance.PlayClearEffect(cell.location);
            //CameraShaker.Instance.Shake(0.15f, 0.2f);
            AudioManager.Instance.PlaySound(AudioManager.AudioType.ClearCell);

            //摄像机震动和碎片粒子特效由AnimateObjectBreak调用
            yield return ObjectAnimator.Instance.AnimateObjectBreak(cell.gameObject, duration: cellClearDelay);

            // 从地图中移除和销毁单元格 (GridManager 的职责)
            GridManager.ClearCell(cell);

            //yield return new WaitForSeconds(cellClearDelay);
        }
        // 5. 最终分数结算 (通知 UI 启动滚动)
        ScoreManager.FinalizeScore();
        yield return CardHandler.ExecuteAfterCalculateScore(paras);

        // 6. 块下落和填充流程 (GridManager 的职责)
        if (!GridManager.stopCoroutine && !LevelManager.isPass)
        {
            // GridManager 必须提供一个等待地图处理完成的异步方法
            yield return GridManager.StartRefillMapFlow(affectedColumns);
        }
        clearRoutine = null;
    }

    /// <summary>
    /// 对外暴露的启动颜色切换流程的接口（右键点击）。
    /// </summary>
    public void StartSwitchColorFlow(ColorCell cellToChange)
    {
        if (!LevelManager.Instance.SubRefreshTimes()) return;
        // 确保没有正在进行的消除或切换流程
        if (clearRoutine != null || switchColorRoutine != null) return;

        // 确保清除预测状态
        ClearPrediction();

        switchColorRoutine = StartCoroutine(SwitchColorFlowCoroutine(cellToChange));
    }

    /// <summary>
    /// 颜色切换的核心流程编排。
    /// </summary>
    private IEnumerator SwitchColorFlowCoroutine(ColorCell cell)
    {
        // 1. 获取新的随机颜色
        ColorType newColor = ResourcesManager.Instance.GetRandomColorType();

        // 2. 播放切换动画 (例如：闪烁或旋转)
        //float switchDuration = 0.3f; // 动画持续时间
        float flashInterval = 0.05f; // 每次闪烁间隔

        // 播放多组颜色快速闪烁
        for (int i = 0; i < 3; i++)
        {
            // 切换到临时颜色/状态
            cell.DisplayTempColor(ColorType.Blue);
            yield return new WaitForSeconds(flashInterval);
            cell.DisplayTempColor(ColorType.Yellow);
            yield return new WaitForSeconds(flashInterval);
        }

        // 3. 最终更新单元格的颜色
        cell.UpdateColorType(newColor); // 假设 ColorCell.cs 中有此方法

        // 4. 播放特效和声音
        //EffectManager.Instance.PlayFlowTextEffect(cell.location, "REROLL"); // 假设有播放特效的方法
        //AudioManager.Instance.PlaySwitchColorSound(); // 假设有播放声音的方法

        switchColorRoutine = null;
    }
}
