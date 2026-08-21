using UnityEngine;
using UnityEngine.InputSystem;

// 플레이어 전용: 자기 Update()의 쿨다운 타이머로만 이동
public class SpaghettiTurnPlayer : MonoBehaviour, IGridEntity, IDamageable
{
    public float moveCooldown = 0.1f;
    public int attackPower = 10;
    public int maxHp = 100;

    public Vector2Int GridPos { get; set; }

    private int hp;
    private float lastMoveTime;

    private void Start()
    {
        hp = maxHp;
        ((IGridEntity)this).RegisterToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
    }

    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        Debug.Log($"플레이어 피격, 남은 체력: {hp}");

        if (hp <= 0)
        {
            Debug.Log("플레이어 사망");
        }
    }

    private void OnDestroy()
    {
        ((IGridEntity)this).UnregisterFromGrid();
    }

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

        Vector2Int nextPos = GridPos + dir;

        if (GridMapManager.Instance.TryGetEntity(nextPos, out MonoBehaviour other)
            && other.TryGetComponent<IDamageable>(out var damageable))
        {
            if (GridUtils.IsAdjacent(GridPos, nextPos))
            {
                damageable.TakeDamage(attackPower);
            }
            lastMoveTime = Time.time;
            return;
        }

        if (!GridMapManager.Instance.IsWalkable(nextPos)) return;

        if (GridMapManager.Instance.TryGetItem(nextPos, out Item item))
        {
            item.Interact(gameObject); // 아이템 효과는 적용 안됨
        }

        ((IGridEntity)this).MoveOnGrid(nextPos);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
        lastMoveTime = Time.time;
    }
}
