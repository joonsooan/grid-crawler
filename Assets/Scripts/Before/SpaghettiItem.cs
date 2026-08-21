using UnityEngine;

public class SpaghettiItem : MonoBehaviour, IGridEntity, IInteractable
{
    // 아이템 스탯
    public string itemName = "휘발유";
    public int healAmount = 0;

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
        Debug.Log($"{itemName} 획득");
        Destroy(gameObject);
    }
}
