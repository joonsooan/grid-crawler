using UnityEngine;

public class SpaghettiChest : MonoBehaviour
{
    public bool isOpened = false;

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

    public void Open()
    {
        if (isOpened) return;

        isOpened = true;
        Debug.Log($"아이템 획득");
        Destroy(gameObject);
    }
}
