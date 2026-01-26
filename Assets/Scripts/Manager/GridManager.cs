using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    //private static GridManager gridManager;
    public static GridManager Instance;
    public GameObject colorGridPrefab;
    public List<ColorCell> colorCellList;
    public Vector2 size;

    private ScoreManager scoreManager;

    public bool stopCoroutine;
    private float fallSpeed = 5f;
    //private bool isProcessingMap = false;

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
        colorCellList = new List<ColorCell>();
        colorGridPrefab = Resources.Load<GameObject>("Prefabs/ColorGrid");
    }

    private void Start()
    {
        scoreManager = ScoreManager.Instance;
    }
    private void Update()
    {
    }
    public void GenerateMap()
    {
        //for (int x = 0; x < size.x; x++)
        //{
        //    for (int y = 0; y < size.y; y++)
        //    {
        //        CreateGridCell(x,y);
        //    }
        //}
        float offsetX = -(size.x - 1) * 0.5f;  // (5-1)/2 = 2
        float offsetY = -(size.y - 1) * 0.5f;  // (5-1)/2 = 2

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                CreateGridCell(x, y, offsetX, offsetY);
            }
        }
    }

    private ColorCell CreateGridCell(int x,int y)
    {
        GameObject gridCell = ResourcesManager.Instance.GetOneRandomColorCell();
        gridCell.transform.position=new Vector3(x,y,0);
        gridCell.transform.parent = this.transform;
        gridCell.GetComponent<ColorCell>().Init(new Vector2(x, y), this);
        colorCellList.Add(gridCell.GetComponent<ColorCell>());
        return gridCell.GetComponent<ColorCell>();
    }

    private ColorCell CreateGridCell(int x, int y, float offsetX, float offsetY)
    {
        GameObject gridCell = ResourcesManager.Instance.GetOneRandomColorCell();

        // 计算位置：脚本物体位置 + 偏移后的网格坐标
        Vector3 position = this.transform.position + new Vector3(
            offsetX + x,  // 例如：-2 + 0 = -2, -2 + 1 = -1, ..., -2 + 4 = 2
            offsetY + y,  // 同样
            0
        );

        gridCell.transform.position = position;
        gridCell.transform.parent = this.transform;
        gridCell.GetComponent<ColorCell>().Init(new Vector2(x, y), this);
        colorCellList.Add(gridCell.GetComponent<ColorCell>());
        return gridCell.GetComponent<ColorCell>();
    }

    /// <summary>
    /// 获取指定位置的 ColorGrid 引用。
    /// </summary>
    private ColorCell GetGridAt(Vector2 loc)
    {
        // 使用 Linq 查找指定位置的网格
        return colorCellList.FirstOrDefault(g => g.location == loc);
    }

    public void ClearMap()
    {
        foreach (ColorCell cell in colorCellList)
        {
            cell.OnDestoryed();
            Destroy(cell.gameObject);
        }
        colorCellList.Clear();
    }

    public void ResetMap()
    {
        stopCoroutine=false;
        ClearMap();
        GenerateMap();
    }

    /// <summary>
    /// 使用 BFS 查找并清除所有相邻的同色块。
    /// </summary>
    /// <param name="startLocation">点击的网格位置。</param>
    /// <param name="targetColor">要清除的目标颜色。</param>
    public void ClearConnectedColors(Vector2 startLocation, ColorType targetColor)
    {
        // 使用 Queue 实现广度优先搜索 (BFS)
        Queue<Vector2> checkQueue = new Queue<Vector2>();
        // 使用 HashSet 跟踪已访问的位置，防止重复和死循环
        HashSet<Vector2> visited = new HashSet<Vector2>();
        // 存储所有待销毁的网格对象
        List<ColorCell> cellsToDestroy = new List<ColorCell>();

        HashSet<int> affectedColumns = new HashSet<int>(); // 新增：记录被消除的列

        checkQueue.Enqueue(startLocation);
        visited.Add(startLocation);

        while (checkQueue.Count > 0)
        {
            Vector2 currentLoc = checkQueue.Dequeue();
            // 确保网格存在且颜色匹配
            ColorCell currentGrid = GetGridAt(currentLoc);

            if (currentGrid != null && currentGrid.colorType == targetColor)
            {
                cellsToDestroy.Add(currentGrid);
                affectedColumns.Add((int)currentLoc.x); // 记录该列

                // 定义四个相邻方向：上、下、左、右
                Vector2[] neighbors = new Vector2[]
                {
                    new Vector2(currentLoc.x, currentLoc.y + 1),
                    new Vector2(currentLoc.x, currentLoc.y - 1),
                    new Vector2(currentLoc.x - 1, currentLoc.y),
                    new Vector2(currentLoc.x + 1, currentLoc.y)
                };

                foreach (Vector2 neighborLoc in neighbors)
                {
                    // 1. 检查是否超出地图边界
                    if (neighborLoc.x >= 0 && neighborLoc.x < size.x &&
                        neighborLoc.y >= 0 && neighborLoc.y < size.y)
                    {
                        // 2. 检查是否已被访问
                        if (!visited.Contains(neighborLoc))
                        {
                            visited.Add(neighborLoc);
                            ColorCell neighborGrid = GetGridAt(neighborLoc);

                            // 3. 检查邻居网格是否存在且颜色是否匹配目标颜色
                            if (neighborGrid != null && neighborGrid.colorType == targetColor)
                            {
                                checkQueue.Enqueue(neighborLoc);
                            }
                        }
                    }
                }
            }
        }

        if(!stopCoroutine)
             StartCoroutine(RefillMap(affectedColumns));
    }

    public List<ColorCell> GetCellsToClear(Vector2 startLocation,ColorType targetColor)
    {
        Queue<Vector2> checkQueue = new Queue<Vector2>();
        HashSet<Vector2> visited = new HashSet<Vector2>();
        List<ColorCell> cellsToDestroy = new List<ColorCell>();
        checkQueue.Enqueue(startLocation);
        visited.Add(startLocation);

        while (checkQueue.Count > 0)
        {
            Vector2 currentLoc = checkQueue.Dequeue();
            // 确保网格存在且颜色匹配
            ColorCell currentGrid = GetGridAt(currentLoc);

            if (currentGrid != null && currentGrid.colorType == targetColor)
            {
                cellsToDestroy.Add(currentGrid);
                // 定义四个相邻方向：上、下、左、右
                Vector2[] neighbors = new Vector2[]
                {
                    new Vector2(currentLoc.x, currentLoc.y + 1),
                    new Vector2(currentLoc.x, currentLoc.y - 1),
                    new Vector2(currentLoc.x - 1, currentLoc.y),
                    new Vector2(currentLoc.x + 1, currentLoc.y)
                };

                foreach (Vector2 neighborLoc in neighbors)
                {
                    // 1. 检查是否超出地图边界
                    if (neighborLoc.x >= 0 && neighborLoc.x < size.x &&
                        neighborLoc.y >= 0 && neighborLoc.y < size.y)
                    {
                        // 2. 检查是否已被访问
                        if (!visited.Contains(neighborLoc))
                        {
                            visited.Add(neighborLoc);
                            ColorCell neighborGrid = GetGridAt(neighborLoc);

                            // 3. 检查邻居网格是否存在且颜色是否匹配目标颜色
                            if (neighborGrid != null && neighborGrid.colorType == targetColor)
                            {
                                checkQueue.Enqueue(neighborLoc);
                            }
                        }
                    }
                }
            }
        }
        return cellsToDestroy;
    }

    public void ClearCell(ColorCell cell)
    {
        colorCellList.Remove(cell);
        cell.OnCleared();
        Destroy(cell.gameObject);
    }

    // 【新增/修改】将原有的 RefillMap 包装为公共的异步方法
    public IEnumerator StartRefillMapFlow(HashSet<int> columnsToProcess)
    {
        // 1. 块下落（重力）
        yield return StartCoroutine(ApplyGravity(columnsToProcess));

        // 2. 填充新块
        yield return StartCoroutine(FillEmptySpaces(columnsToProcess));

        // 可以在这里添加一个事件通知 RefillMap 完成
        // LevelManager.OnMapRefilled?.Invoke();
    }


    /// <summary>
    /// 处理消除后的地图：1. 下落 -> 2. 填充。
    /// </summary>
    private IEnumerator RefillMap(HashSet<int> columnsToProcess)
    {
        // 1. 块下落（重力）
        yield return StartCoroutine(ApplyGravity(columnsToProcess));

        // 2. 填充新块
        yield return StartCoroutine(FillEmptySpaces(columnsToProcess));

        //isProcessingMap = false; // 地图处理完毕
    }

    /// <summary>
    /// 将每一列的网格块向下移动到最低的空位。
    /// </summary>
    private IEnumerator ApplyGravity(HashSet<int> columnsToProcess)
    {
        float maxFallTime = 0f; // 记录最长的下落时间，以便等待所有块到位

        // 使用List存储所有动画，便于统一等待
        List<Tween> fallTweens = new List<Tween>();

        foreach (int x in columnsToProcess)
        {
            int emptySpaces = 0;
            // 从底部 (y=0) 向上扫描
            for (int y = 0; y < size.y; y++)
            {
                // 注意：GetGridAt 接受 Vector2，但我们用 (x, y) 来查询
                ColorCell grid = GetGridAt(new Vector2(x, y));

                if (grid == null)
                {
                    // 找到一个空位
                    emptySpaces++;
                }
                else if (emptySpaces > 0)
                {
                    // 找到一个块，且下方有空位，需要下落
                    int newY = y - emptySpaces;
                    Vector2 newLocation = new Vector2(x, newY);

                    // 更新块的逻辑位置
                    grid.location = newLocation;

                    // 计算下落时间 (距离 / 速度)
                    float fallDuration = emptySpaces / fallSpeed;
                    maxFallTime = Mathf.Max(maxFallTime, fallDuration); // 更新最长等待时间

                    // 启动平滑移动的协程
                    //StartCoroutine(MoveGridToPosition(grid.transform, new Vector3(x, newY, 0), fallDuration));
                    Vector3 targetWorldPos = GetWorldPosition(x, newY); // 使用新的世界坐标计算方法
                    Tween fallTween = grid.transform.DOMove(targetWorldPos, fallDuration)
                        .SetEase(Ease.OutQuad); // 添加缓动效果增强动画感

                    fallTweens.Add(fallTween);
                }
            }
        }

        // 等待最长时间，确保所有块都到达目标位置
        //yield return new WaitForSeconds(maxFallTime);
        if (fallTweens.Count > 0)
        {
            // 创建序列来等待所有动画
            yield return DOTween.Sequence()
                .AppendInterval(maxFallTime + 0.1f) // 额外等待一点时间确保所有动画完成
                .WaitForCompletion();
        }
        else
        {
            yield break; // 没有需要下落的块
        }
    }

    /// <summary>
    /// 平滑移动网格块到目标位置。
    /// </summary>
    //private IEnumerator MoveGridToPosition(Transform gridTransform, Vector3 targetPos, float duration)
    //{
    //    float time = 0;
    //    Vector3 startPos = gridTransform.position;
    //    // 如果持续时间太短（例如移动距离为0），直接跳到终点
    //    if (duration <= 0.01f)
    //    {
    //        gridTransform.position = targetPos;
    //        yield break;
    //    }

    //    while (time < duration)
    //    {
    //        // 使用 Lerp 进行平滑插值移动
    //        gridTransform.position = Vector3.Lerp(startPos, targetPos, time / duration);
    //        time += Time.deltaTime;
    //        yield return null;
    //    }
    //    gridTransform.position = targetPos; // 确保最终位置精确
    //}

    /// <summary>
    /// 在每一列的顶部生成新的网格块来填补空缺。
    /// </summary>
    private IEnumerator FillEmptySpaces(HashSet<int> columnsToProcess)
    {
        float maxCreationTime = 0f;

        // 使用List存储所有动画
        List<Tween> creationTweens = new List<Tween>();

        foreach (int x in columnsToProcess)
        {
            int emptyCount = 0;
            // 找出当前列有多少空位 (从底部向上查找)
            for (int y = 0; y < size.y; y++)
            {
                if (GetGridAt(new Vector2(x, y)) == null)
                {
                    emptyCount++;
                }
            }

            // 从顶部开始，为每个空位生成新块
            if (emptyCount > 0)
            {
                for (int i = 0; i < emptyCount; i++)
                {
                    // 目标 Y 坐标是 (size.y - emptyCount) 加上当前新块的索引 i
                    int targetY = (int)size.y - emptyCount + i;

                    // 1. 创建新块，并设置其初始位置在屏幕上方
                    ColorCell newGrid = CreateGridCell(x, targetY);

                    // 使用新的世界坐标计算方法设置初始位置和最终位置
                    Vector3 startWorldPos = GetWorldPosition(x, (int)size.y + i); // 起始位置在棋盘上方
                    Vector3 targetWorldPos = GetWorldPosition(x, targetY); // 目标位置

                    // 初始位置：比网格顶部更高，这样它有距离可以下落
                    newGrid.transform.position = startWorldPos;
                    newGrid.location = new Vector2(x, targetY); // 设置正确的最终逻辑位置

                    float fallDistance = (float)size.y - targetY+i;
                    float fallDuration = fallDistance / fallSpeed;

                    maxCreationTime = Mathf.Max(maxCreationTime, fallDuration);

                    // 将新块从上方移动到正确的逻辑位置
                    //StartCoroutine(MoveGridToPosition(newGrid.transform, new Vector3(x, targetY, 0), fallDuration));
                    Tween creationTween = newGrid.transform.DOMove(targetWorldPos, fallDuration)
                    .SetEase(Ease.OutBack) // 使用OutBack有回弹效果，更有趣味性
                    .OnStart(() => {
                        // 可以在这里添加一些粒子效果或声音
                        // 例如：newGrid.PlaySpawnEffect();
                    });
                }
            }
        }

        // 等待所有新块下落完毕
        //yield return new WaitForSeconds(maxCreationTime);
        if (creationTweens.Count > 0)
        {
            yield return DOTween.Sequence()
                .AppendInterval(maxCreationTime + 0.1f) // 额外等待一点时间
                .WaitForCompletion();
        }
        else
        {
            yield break; // 没有需要创建的新块
        }
    }

    /// <summary>
    /// 【新增】根据网格坐标计算世界坐标
    /// </summary>
    private Vector3 GetWorldPosition(int x, int y)
    {
        // 计算偏移量，使棋盘中心与脚本物体位置对齐
        float offsetX = -(size.x - 1) * 0.5f;
        float offsetY = -(size.y - 1) * 0.5f;

        // 计算世界位置：脚本物体位置 + 偏移后的网格坐标
        return this.transform.position + new Vector3(offsetX + x, offsetY + y, 0);
    }
}
