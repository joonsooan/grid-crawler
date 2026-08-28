using DG.Tweening;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemDataSO itemData;
    [SerializeField] private float hoverHeight = 0.15f;
    [SerializeField] private float hoverDuration = 0.8f;
    [SerializeField] private float spawnPopDuration = 0.3f;

    public bool isOpened = false;

    public Vector2Int GridPos { get; private set; }

    private void Start()
    {
        GridPos = GridUtils.WorldToGrid(transform.position);
        GridMapManager.Instance.RegisterItem(GridPos, this);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);

        PlaySpawnPop();
    }

    private void OnDestroy()
    {
        GridMapManager.Instance.UnregisterItem(GridPos);
        transform.DOKill();
    }

    private void PlaySpawnPop()
    {
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(targetScale, spawnPopDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(PlayHoverLoop);
    }

    private void PlayHoverLoop()
    {
        transform.DOMoveY(transform.position.y + hoverHeight, hoverDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void Interact(GameObject interactor)
    {
        if (isOpened) return;

        isOpened = true;
        Debug.Log($"{itemData.itemName} 획득");

        if (interactor.TryGetComponent<PlayerController>(out var player))
        {
            if (itemData.moveRangeBonus > 0) player.IncreaseMoveRange(itemData.moveRangeBonus);
            if (itemData.healAmount > 0) player.Heal(itemData.healAmount);
        }

        Destroy(gameObject);
    }
}
