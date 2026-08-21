using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IGridEntity, IDamageable
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private PlayerDataSO playerData;
    public float moveCooldown = 0.1f;

    public Vector2Int GridPos { get; set; }
    public string PlayerName => playerData.playerName;

    private int hp;
    private int moveRange;
    private int movesRemaining;
    private float lastMoveTime;
    private enum ActionResult { Blocked, Moved, TurnEnd }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 시작 위치를 그리드에 맞춰 등록
    private void Start()
    {
        hp = playerData.maxHp;
        moveRange = playerData.moveRange;
        movesRemaining = moveRange;
        ((IGridEntity)this).RegisterToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
    }

    private void OnDestroy()
    {
        ((IGridEntity)this).UnregisterFromGrid();
    }

    // TurnManager가 WaitingForPlayer 상태일 때만 WASD 입력을 받아 플레이어를 한 칸 이동
    private void Update()
    {
        if (Time.time - lastMoveTime < moveCooldown) return;
        if (TurnManager.Instance.CurrentState != TurnState.WaitingForPlayer) return;

        Vector2Int dir = GetInputDirection();
        if (dir == Vector2Int.zero) return;

        lastMoveTime = Time.time;
        ResolveAction(TryMove(dir));
    }

    // 공격/상호작용은 즉시 턴 종료, 이동은 movesRemaining이 남아있는 동안 턴 유지
    private void ResolveAction(ActionResult result)
    {
        if (result == ActionResult.Blocked) return;
        if (result == ActionResult.Moved && --movesRemaining > 0) return;

        movesRemaining = moveRange;
        TurnManager.Instance.OnPlayerActionCompleted();
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


    private ActionResult TryMove(Vector2Int dir)
    {
        Vector2Int nextPos = GridPos + dir;

        if (GridMapManager.Instance.TryGetEntity(nextPos, out MonoBehaviour other))
        {
            if (GridUtils.IsAdjacent(GridPos, nextPos) && other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(playerData.attackPower);
                return ActionResult.TurnEnd;
            }

            if (other.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(gameObject);
                return ActionResult.TurnEnd;
            }

            Debug.Log("이동 불가");
            return ActionResult.Blocked;
        }

        if (!GridMapManager.Instance.IsWalkable(nextPos))
        {
            Debug.Log("이동 불가");
            return ActionResult.Blocked;
        }

        if (GridMapManager.Instance.TryGetItem(nextPos, out Item item))
        {
            item.Interact(gameObject);
        }

        ((IGridEntity)this).MoveOnGrid(nextPos);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
        return ActionResult.Moved;
    }

    // 이동 거리 증가 아이템 등, 즉시 적용되는 이동 범위 버프에 사용
    public void IncreaseMoveRange(int amount)
    {
        moveRange += amount;
        Debug.Log($"이동 거리 증가: {moveRange}칸");
    }

    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        Debug.Log($"{playerData.playerName} 남은 체력: {hp}");

        if (hp <= 0)
        {
            Debug.Log($"{playerData.playerName} 사망");
        }
    }
}
