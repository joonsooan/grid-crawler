using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaghettiUIPlayerController : MonoBehaviour, IGridEntity, IDamageable
{
    public static SpaghettiUIPlayerController Instance { get; private set; }

    [SerializeField] private PlayerDataSO playerData;
    public float moveCooldown = 0.1f;
    // UI 컴포넌트를 게임 로직 스크립트가 직접 참조
    [SerializeField] private TMP_Text hpText;

    public Vector2Int GridPos { get; set; }
    public string PlayerName => playerData.playerName;

    private int hp;
    private int moveRange;
    private float lastMoveTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        hp = playerData.maxHp;
        moveRange = playerData.moveRange;
        ((IGridEntity)this).RegisterToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
        hpText.text = $"체력 : {hp} / {playerData.maxHp}";
    }

    private void OnDestroy()
    {
        ((IGridEntity)this).UnregisterFromGrid();
    }

    private void Update()
    {
        if (Time.time - lastMoveTime < moveCooldown) return;
        if (SpaghettiUITurnManager.Instance.CurrentState != TurnState.WaitingForPlayer) return;

        Vector2Int dir = GetInputDirection();
        if (dir == Vector2Int.zero) return;

        lastMoveTime = Time.time;
        SpaghettiUITurnManager.Instance.OnPlayerActionStarted(moveRange);
        SpaghettiUITurnManager.Instance.ResolvePlayerAction(TryMove(dir));
    }

    private static Vector2Int GetInputDirection()
    {
        if (Keyboard.current == null) return Vector2Int.zero;

        if (Keyboard.current.wKey.isPressed) return Vector2Int.up;
        if (Keyboard.current.sKey.isPressed) return Vector2Int.down;
        if (Keyboard.current.aKey.isPressed) return Vector2Int.left;
        if (Keyboard.current.dKey.isPressed) return Vector2Int.right;

        return Vector2Int.zero;
    }

    private PlayerActionResult TryMove(Vector2Int dir)
    {
        Vector2Int nextPos = GridPos + dir;

        if (GridMapManager.Instance.TryGetEntity(nextPos, out MonoBehaviour other))
        {
            if (GridUtils.IsAdjacent(GridPos, nextPos) && other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(playerData.attackPower);
                return PlayerActionResult.TurnEnd;
            }

            if (other.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(gameObject);
                return PlayerActionResult.TurnEnd;
            }

            Debug.Log("이동 불가");
            return PlayerActionResult.Blocked;
        }

        if (!GridMapManager.Instance.IsWalkable(nextPos))
        {
            Debug.Log("이동 불가");
            return PlayerActionResult.Blocked;
        }

        if (GridMapManager.Instance.TryGetItem(nextPos, out Item item))
        {
            item.Interact(gameObject);
        }

        ((IGridEntity)this).MoveOnGrid(nextPos);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
        return PlayerActionResult.Moved;
    }

    public void IncreaseMoveRange(int amount)
    {
        moveRange += amount;
        Debug.Log($"이동 거리 증가: {moveRange}칸");
    }

    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        hpText.text = $"체력 : {hp} / {playerData.maxHp}";

        if (hp <= 0)
        {
            Debug.Log($"{playerData.playerName} 사망");
        }
    }
}
