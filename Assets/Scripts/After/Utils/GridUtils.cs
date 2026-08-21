using UnityEngine;

public static class GridUtils
{
    // 월드 좌표를 그리드 좌표로 변환
    public static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    // 그리드 좌표를 타일 중심 월드 좌표로 변환
    public static Vector3 GridToWorld(Vector2Int gridPos, float z = 0f)
    {
        return new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, z);
    }
}
