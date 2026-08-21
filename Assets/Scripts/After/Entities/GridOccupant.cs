using UnityEngine;

// 고정 유닛 테스트용 컴포넌트
public class GridOccupant : MonoBehaviour
{
    private Vector2Int gridPos;

    private void Start()
    {
        gridPos = GridUtils.WorldToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(gridPos, transform.position.z);
        GridMapManager.Instance.RegisterEntity(gridPos, this);
    }

    private void OnDestroy()
    {
        GridMapManager.Instance.UnregisterEntity(gridPos);
    }
}
