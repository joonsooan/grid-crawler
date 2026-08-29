using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChaseAITests
{
    private GameObject gridManagerObject;
    private GridMapManager gridMapManager;

    [SetUp]
    public void SetUp()
    {
        gridManagerObject = new GameObject("GridMapManager");
        gridManagerObject.SetActive(false);

        gridMapManager = gridManagerObject.AddComponent<GridMapManager>();

        Tilemap wallTilemap = new GameObject("WallTilemap").AddComponent<Tilemap>();
        Tilemap waterTilemap = new GameObject("WaterTilemap").AddComponent<Tilemap>();
        SetPrivateField("wallTilemap", wallTilemap);
        SetPrivateField("waterTilemap", waterTilemap);

        gridManagerObject.SetActive(true);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gridManagerObject);
    }

    private void SetPrivateField(string fieldName, object value)
    {
        typeof(GridMapManager)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(gridMapManager, value);
    }

    private void SetWalls(IEnumerable<Vector2Int> wallPositions)
    {
        FieldInfo tileMapField = typeof(GridMapManager).GetField("tileMap", BindingFlags.NonPublic | BindingFlags.Instance);
        var tileMap = (Dictionary<Vector2Int, TileType>)tileMapField.GetValue(gridMapManager);
        foreach (Vector2Int pos in wallPositions) tileMap[pos] = TileType.Wall;
    }

    // 시작 지점이 이미 target과 인접하면 빈 경로와 reachedAdjacent=true 반환
    [Test]
    public void FindPath_AlreadyAdjacent_ReturnsEmptyPathAndReachedAdjacentTrue()
    {
        List<Vector2Int> path = ChaseAI.FindPath(new Vector2Int(0, 0), new Vector2Int(1, 0), moveRange: 5, out bool reachedAdjacent);

        Assert.IsTrue(reachedAdjacent);
        Assert.IsEmpty(path);
    }

    // 직선 경로가 벽으로 막혀도 moveRange가 충분하면 우회해서 target과 인접한 칸까지 도달
    [Test]
    public void FindPath_StraightLineBlocked_RoutesAroundWallToReachAdjacent()
    {
        // x=1 열을 y=-2..2 구간에서 완전히 막아 (0,0) -> (3,0) 직선 경로를 차단
        SetWalls(new[]
        {
            new Vector2Int(1, -2), new Vector2Int(1, -1), new Vector2Int(1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, 2)
        });

        List<Vector2Int> path = ChaseAI.FindPath(new Vector2Int(0, 0), new Vector2Int(3, 0), moveRange: 10, out bool reachedAdjacent);

        Assert.IsTrue(reachedAdjacent);
        Assert.IsNotEmpty(path);
        Assert.IsTrue(GridUtils.IsAdjacent(path[path.Count - 1], new Vector2Int(3, 0)));
    }

    // [회귀 테스트] 벽을 돌아가려면 처음 몇 걸음은 target에서 오히려 멀어져야 하는 상황
    [Test]
    public void FindPath_DetourRequiresMovingAwayFirst_StillMakesProgressWithinMoveRange()
    {
        SetWalls(new[]
        {
            new Vector2Int(1, -2), new Vector2Int(1, -1), new Vector2Int(1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, 2)
        });

        // moveRange=3: 우회를 위해 (0,0)->(0,1)->(0,2)->(0,3)으로 올라가는 3칸 모두
        // target(3,0)과의 맨해튼 거리가 시작 지점(3)보다 커지는 구간이라 예전 로직은 경로를 못 찾음
        List<Vector2Int> path = ChaseAI.FindPath(new Vector2Int(0, 0), new Vector2Int(3, 0), moveRange: 3, out bool reachedAdjacent);

        Assert.IsFalse(reachedAdjacent); // 이번 턴엔 인접 칸까지 못 감
        Assert.AreEqual(3, path.Count); // 그래도 멈추지 않고 moveRange만큼은 전진해야 함
        Assert.AreEqual(new Vector2Int(0, 1), path[0]);
        Assert.AreEqual(new Vector2Int(0, 3), path[2]);
    }

    // 전체 최단 경로가 moveRange보다 길면 경로 앞부분만 잘라서 반환
    [Test]
    public void FindPath_PathLongerThanMoveRange_TruncatesButKeepsMoving()
    {
        List<Vector2Int> fullPath = ChaseAI.FindPath(new Vector2Int(0, 0), new Vector2Int(10, 0), moveRange: 100, out bool fullyReached);
        Assert.IsTrue(fullyReached);

        List<Vector2Int> truncated = ChaseAI.FindPath(new Vector2Int(0, 0), new Vector2Int(10, 0), moveRange: 3, out bool reachedAdjacent);

        Assert.IsFalse(reachedAdjacent);
        Assert.AreEqual(3, truncated.Count);
        CollectionAssert.AreEqual(fullPath.GetRange(0, 3), truncated);
    }

    // target 주변이 벽으로 완전히 밀폐되어 있으면 도달 불가능 - 경로 없이 제자리
    [Test]
    [Timeout(5000)]
    public void FindPath_TargetFullyEnclosedInOpenSpace_TerminatesAndReturnsEmpty()
    {
        Vector2Int target = new Vector2Int(5, 5);
        SetWalls(new[]
        {
            new Vector2Int(4, 4), new Vector2Int(5, 4), new Vector2Int(6, 4),
            new Vector2Int(4, 5), new Vector2Int(6, 5),
            new Vector2Int(4, 6), new Vector2Int(5, 6), new Vector2Int(6, 6)
        });

        List<Vector2Int> path = ChaseAI.FindPath(new Vector2Int(0, 0), target, moveRange: 50, out bool reachedAdjacent);

        Assert.IsFalse(reachedAdjacent);
        Assert.IsEmpty(path);
    }

    // 다른 엔티티가 점유한 칸도 장애물로 취급되어 경로 우회
    [Test]
    public void FindPath_CellOccupiedByEntity_TreatsCellAsBlocked()
    {
        Vector2Int blockedCell = new Vector2Int(1, 0);
        gridMapManager.RegisterEntity(blockedCell, gridManagerObject.AddComponent<GridOccupant>());

        List<Vector2Int> path = ChaseAI.FindPath(new Vector2Int(0, 0), new Vector2Int(2, 0), moveRange: 10, out bool reachedAdjacent);

        Assert.IsTrue(reachedAdjacent);
        CollectionAssert.DoesNotContain(path, blockedCell);
    }
}
