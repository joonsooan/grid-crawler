using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IGridEntity, IDamageable
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private PlayerDataSO playerData;
    [SerializeField] private float moveTweenDuration = 0.15f;
    [SerializeField] private float attackPunchDuration = 0.3f;
    [SerializeField] private float hitShakeDuration = 0.3f;
    [SerializeField] private float wallBumpDuration = 0.2f;
    public float moveCooldown = 0.1f;

    public Vector2Int GridPos { get; set; }
    public string PlayerName => playerData.playerName;
    public int MoveRange => moveRange;
    public int CurrentHp => hp;
    public int MaxHp => playerData.maxHp;

    public event Action<int, int> OnHealthChanged;

    private int hp;
    private int moveRange;
    private float lastMoveTime;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 시작 위치를 그리드에 맞춰 등록
    private void Start()
    {
        hp = playerData.maxHp;
        moveRange = playerData.moveRange;
        ((IGridEntity)this).RegisterToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
        TurnManager.Instance.InitializePlayerMoves(moveRange);
        OnHealthChanged?.Invoke(hp, playerData.maxHp);
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
        TurnManager.Instance.OnPlayerActionStarted(moveRange);
        TryMove(dir);
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

    private void TryMove(Vector2Int dir)
    {
        Vector2Int nextPos = GridPos + dir;

        if (GridMapManager.Instance.TryGetEntity(nextPos, out MonoBehaviour other))
        {
            if (GridUtils.IsAdjacent(GridPos, nextPos) && other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(playerData.attackPower);
                PlayAttackPunch(other.transform.position);
                return;
            }

            if (other.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(gameObject);
                TurnManager.Instance.ResolvePlayerAction(PlayerActionResult.TurnEnd);
                return;
            }

            Debug.Log("이동 불가");
            PlayBlockedBump(dir);
            TurnManager.Instance.ResolvePlayerAction(PlayerActionResult.Blocked);
            return;
        }

        if (!GridMapManager.Instance.IsWalkable(nextPos))
        {
            Debug.Log("이동 불가");
            PlayBlockedBump(dir);
            TurnManager.Instance.ResolvePlayerAction(PlayerActionResult.Blocked);
            return;
        }

        if (GridMapManager.Instance.TryGetItem(nextPos, out Item item))
        {
            item.Interact(gameObject);
        }

        ((IGridEntity)this).MoveOnGrid(nextPos);

        Vector3 targetWorldPos = GridUtils.GridToWorld(GridPos, 0f);
        transform.DOMove(targetWorldPos, moveTweenDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => TurnManager.Instance.ResolvePlayerAction(PlayerActionResult.Moved));
    }

    private void PlayAttackPunch(Vector3 targetWorldPos)
    {
        Vector3 punch = (targetWorldPos - transform.position).normalized * 0.3f;
        transform.DOPunchPosition(punch, attackPunchDuration, vibrato: 6, elasticity: 0.5f)
            .OnComplete(() => TurnManager.Instance.ResolvePlayerAction(PlayerActionResult.TurnEnd));
    }

    private void PlayBlockedBump(Vector2Int dir)
    {
        transform.DOKill();
        transform.position = GridUtils.GridToWorld(GridPos, 0f);

        Vector3 bump = new Vector3(dir.x, dir.y, 0f) * 0.15f;
        transform.DOPunchPosition(bump, wallBumpDuration, vibrato: 4, elasticity: 0.3f);
    }

    private void PlayHitReaction()
    {
        transform.DOShakePosition(hitShakeDuration, strength: 0.15f, vibrato: 20);

        if (spriteRenderer == null) return;
        spriteRenderer.DOColor(Color.red, 0.05f)
            .OnComplete(() => spriteRenderer.DOColor(Color.white, hitShakeDuration - 0.05f));
    }

    // 이동 거리 증가 아이템 등, 즉시 적용되는 이동 범위 버프에 사용
    public void IncreaseMoveRange(int amount)
    {
        TurnManager.Instance.IncreaseMovesRemaining(amount);
        Debug.Log($"이동 거리: {amount}칸 증가");
    }

    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, playerData.maxHp);
        Debug.Log($"{playerData.playerName} 체력 {amount} 회복");
        OnHealthChanged?.Invoke(hp, playerData.maxHp);
    }

    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        Debug.Log($"{playerData.playerName} 남은 체력: {hp}");
        OnHealthChanged?.Invoke(hp, playerData.maxHp);
        PlayHitReaction();

        if (hp <= 0)
        {
            Debug.Log($"{playerData.playerName} 사망");
        }
    }
}
