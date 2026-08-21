using UnityEngine;

public interface IGridEntity
{
    Vector2Int GridPos { get; set; }

    void RegisterToGrid(Vector3 worldPos)
    {
        GridPos = GridUtils.WorldToGrid(worldPos);
        GridMapManager.Instance.RegisterEntity(GridPos, this as MonoBehaviour);
    }

    void UnregisterFromGrid()
    {
        GridMapManager.Instance.UnregisterEntity(GridPos);
    }

    void MoveOnGrid(Vector2Int newPos)
    {
        GridMapManager.Instance.UnregisterEntity(GridPos);
        GridPos = newPos;
        GridMapManager.Instance.RegisterEntity(GridPos, this as MonoBehaviour);
    }
}
