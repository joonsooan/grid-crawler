using UnityEngine;

// 몬스터 전용: 플레이어의 행동과 무관하게 자기 Update()의 쿨다운 타이머로만 추적/공격 -> 여러 마리가 동시에, 제멋대로 동작
public class SpaghettiTurnEnemy : MonoBehaviour, IGridEntity, IDamageable
{
    public float moveCooldown = 0.5f;
    public int maxHp = 30;
    public int attackPower = 5;
    public int moveRange = 3;
    public Transform player;

    public Vector2Int GridPos { get; set; }

    private int hp;
    private float lastMoveTime;

    private void Start()
    {
        hp = maxHp;
        ((IGridEntity)this).RegisterToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
    }

    private void OnDestroy()
    {
        ((IGridEntity)this).UnregisterFromGrid();
    }

    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        Debug.Log($"{name} 피격, 남은 체력: {hp}");

        if (hp <= 0)
        {
            Debug.Log($"{name} 사망");
            Destroy(gameObject);
        }
    }

    // 플레이어와 무관하게 타이머가 차면 인접 여부만 보고 공격 또는 이동
    private void Update()
    {
        if (Time.time - lastMoveTime < moveCooldown) return;

        if (player == null) return;

        Vector2Int playerGridPos = GridUtils.WorldToGrid(player.position);
        Vector2Int destination = ChaseAI.FindNextPosition(GridPos, playerGridPos, moveRange, out bool reachedAdjacent);

        if (destination != GridPos)
        {
            ((IGridEntity)this).MoveOnGrid(destination);
            transform.position = GridUtils.GridToWorld(GridPos, 0f);
        }

        if (reachedAdjacent) AttackPlayer(playerGridPos);

        lastMoveTime = Time.time;
    }

    private void AttackPlayer(Vector2Int playerGridPos)
    {
        if (!GridUtils.IsAdjacent(GridPos, playerGridPos)) return;

        if (GridMapManager.Instance.TryGetEntity(playerGridPos, out MonoBehaviour other)
            && other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(attackPower);
        }
    }
}
