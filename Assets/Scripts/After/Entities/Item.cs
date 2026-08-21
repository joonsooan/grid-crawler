using UnityEngine;

public class Item : MonoBehaviour, IGridEntity, IInteractable
{
    [SerializeField] private ItemDataSO itemData;

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
        Debug.Log($"{itemData.itemName} 획득");

        if (itemData.moveRangeBonus > 0 && interactor.TryGetComponent<PlayerController>(out var player))
        {
            player.IncreaseMoveRange(itemData.moveRangeBonus);
        }

        Destroy(gameObject);
    }
}
