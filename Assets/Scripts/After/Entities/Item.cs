using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemDataSO itemData;

    public bool isOpened = false;

    public Vector2Int GridPos { get; private set; }

    private void Start()
    {
        GridPos = GridUtils.WorldToGrid(transform.position);
        GridMapManager.Instance.RegisterItem(GridPos, this);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
    }

    private void OnDestroy()
    {
        GridMapManager.Instance.UnregisterItem(GridPos);
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
