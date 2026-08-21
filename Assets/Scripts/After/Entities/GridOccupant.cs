using UnityEngine;

// 고정 유닛 테스트용 컴포넌트
public class GridOccupant : MonoBehaviour, IGridEntity
{
    public Vector2Int GridPos { get; set; }

    private void Start()
    {
        ((IGridEntity)this).RegisterToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
    }

    private void OnDestroy()
    {
        ((IGridEntity)this).UnregisterFromGrid();
    }
}
