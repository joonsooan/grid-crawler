using UnityEngine;

public class Chest : MonoBehaviour, IGridEntity, IInteractable
{
    public bool isOpened = false;

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

    public void Interact(GameObject interactor)
    {
        if (isOpened) return;

        isOpened = true;
        Debug.Log($"아이템 획득");
        Destroy(gameObject);
    }
}
