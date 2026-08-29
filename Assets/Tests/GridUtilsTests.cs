using NUnit.Framework;
using UnityEngine;

public class GridUtilsTests
{
    // 가로로 한 칸 떨어진 두 좌표는 인접한 것으로 판정
    [Test]
    public void IsAdjacent_OneTileApartHorizontally_ReturnsTrue()
    {
        Vector2Int a = new Vector2Int(2, 2);
        Vector2Int b = new Vector2Int(3, 2);

        Assert.IsTrue(GridUtils.IsAdjacent(a, b));
    }

    // 세로로 한 칸 떨어진 두 좌표는 인접한 것으로 판정
    [Test]
    public void IsAdjacent_OneTileApartVertically_ReturnsTrue()
    {
        Vector2Int a = new Vector2Int(2, 2);
        Vector2Int b = new Vector2Int(2, 3);

        Assert.IsTrue(GridUtils.IsAdjacent(a, b));
    }

    // 대각선으로 떨어진 두 좌표는 인접 X
    [Test]
    public void IsAdjacent_Diagonal_ReturnsFalse()
    {
        Vector2Int a = new Vector2Int(2, 2);
        Vector2Int b = new Vector2Int(3, 3);

        Assert.IsFalse(GridUtils.IsAdjacent(a, b));
    }

    // 같은 칸끼리는 인접 X
    [Test]
    public void IsAdjacent_SameTile_ReturnsFalse()
    {
        Vector2Int a = new Vector2Int(2, 2);

        Assert.IsFalse(GridUtils.IsAdjacent(a, a));
    }

    // 두 칸 이상 떨어진 좌표는 인접 X
    [Test]
    public void IsAdjacent_TwoTilesApart_ReturnsFalse()
    {
        Vector2Int a = new Vector2Int(2, 2);
        Vector2Int b = new Vector2Int(4, 2);

        Assert.IsFalse(GridUtils.IsAdjacent(a, b));
    }

    // 소수점이 있는 월드 좌표는 내림 처리되어 그리드 좌표로 변환
    [Test]
    public void WorldToGrid_FractionalPosition_FloorsToGridCell()
    {
        Vector3 worldPos = new Vector3(2.7f, 3.2f, 0f);

        Vector2Int result = GridUtils.WorldToGrid(worldPos);

        Assert.AreEqual(new Vector2Int(2, 3), result);
    }

    // 음수 소수점 월드 좌표도 음의 무한대 방향으로 내림 처리
    [Test]
    public void WorldToGrid_NegativeFractionalPosition_FloorsTowardNegativeInfinity()
    {
        Vector3 worldPos = new Vector3(-0.5f, -1.5f, 0f);

        Vector2Int result = GridUtils.WorldToGrid(worldPos);

        Assert.AreEqual(new Vector2Int(-1, -2), result);
    }

    // 그리드 좌표는 타일의 정중앙 월드 좌표로 변환
    [Test]
    public void GridToWorld_ReturnsTileCenterPosition()
    {
        Vector2Int gridPos = new Vector2Int(2, 3);

        Vector3 result = GridUtils.GridToWorld(gridPos);

        Assert.AreEqual(new Vector3(2.5f, 3.5f, 0f), result);
    }

    // 그리드 좌표를 월드 좌표로 변환했다가 다시 그리드 좌표로 되돌려도 원래 값과 동일
    [Test]
    public void GridToWorld_RoundTripsWithWorldToGrid()
    {
        Vector2Int original = new Vector2Int(5, -3);

        Vector3 worldPos = GridUtils.GridToWorld(original);
        Vector2Int roundTripped = GridUtils.WorldToGrid(worldPos);

        Assert.AreEqual(original, roundTripped);
    }
}
