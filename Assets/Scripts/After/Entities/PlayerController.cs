using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IGridEntity, IDamageable
{
    [SerializeField] private PlayerDataSO playerData;
    public float moveCooldown = 0.1f;

    public Vector2Int GridPos { get; set; }
    public string PlayerName => playerData.playerName;

    private int hp;
    private int moveRange;
    private float lastMoveTime;

    // 시작 위치를 그리드에 맞춰 등록
    private void Start()
    {
        hp = playerData.maxHp;
        moveRange = playerData.moveRange;
        ((IGridEntity)this).RegisterToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
    }

    private void OnDestroy()
    {
        ((IGridEntity)this).UnregisterFromGrid();
    }

    // WASD 입력을 받아 플레이어를 한 칸 이동
    private void Update()
    {
        if (Time.time - lastMoveTime < moveCooldown) return;
        if (Keyboard.current == null) return;

        Vector2Int dir = Vector2Int.zero;
        if (Keyboard.current.wKey.isPressed) dir = Vector2Int.up;
        else if (Keyboard.current.sKey.isPressed) dir = Vector2Int.down;
        else if (Keyboard.current.aKey.isPressed) dir = Vector2Int.left;
        else if (Keyboard.current.dKey.isPressed) dir = Vector2Int.right;

        if (dir == Vector2Int.zero) return;

        TryMove(dir);
        lastMoveTime = Time.time;
    }

    // moveRange만큼 한 방향으로 이어서 진행, 첫 칸에 점유자가 있으면 공격/상호작용을 먼저 판정
    // 이동 중 장애물이나 다른 점유자를 만나면 그 직전 칸까지만 이동
    private void TryMove(Vector2Int dir)
    {
        Vector2Int destination = GridPos;

        for (int step = 0; step < moveRange; step++)
        {
            Vector2Int nextPos = destination + dir;

            if (GridMapManager.Instance.TryGetEntity(nextPos, out MonoBehaviour other))
            {
                if (step > 0) break;

                if (other.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(playerData.attackPower);
                    return;
                }

                if (other.TryGetComponent<IInteractable>(out var interactable))
                {
                    interactable.Interact(gameObject);
                    return;
                }

                Debug.Log("이동 불가");
                return;
            }

            if (!GridMapManager.Instance.IsWalkable(nextPos))
            {
                if (step == 0)
                {
                    Debug.Log("이동 불가");
                    return;
                }

                break;
            }

            destination = nextPos;
        }

        if (destination == GridPos) return;

        ((IGridEntity)this).MoveOnGrid(destination);
        transform.position = GridUtils.GridToWorld(GridPos, transform.position.z);
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
