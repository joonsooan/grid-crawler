using UnityEngine;

public class Enemy : MonoBehaviour, IGridEntity, IDamageable
{
    [SerializeField] private EnemyDataSO enemyData;

    private int hp;

    public Vector2Int GridPos { get; set; }

    private void Start()
    {
        hp = enemyData.maxHp;
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
        Debug.Log($"{enemyData.enemyName} 피격, 남은 체력: {hp}");

        if (hp <= 0)
        {
            Debug.Log($"{enemyData.enemyName} 사망");
            Destroy(gameObject);
        }
    }

    // ChaseAI로 계산한 칸까지 이동, 플레이어와 인접해졌다면 이어서 공격
    public void ExecuteTurn(Vector2Int playerGridPos)
    {
        Vector2Int destination = ChaseAI.FindNextPosition(GridPos, playerGridPos, enemyData.moveRange, out bool reachedAdjacent);

        if (destination != GridPos)
        {
            ((IGridEntity)this).MoveOnGrid(destination);
            transform.position = GridUtils.GridToWorld(GridPos, transform.position.z);
        }

        if (reachedAdjacent) AttackPlayer(playerGridPos);
    }

    private void AttackPlayer(Vector2Int playerGridPos)
    {
        if (GridMapManager.Instance.TryGetEntity(playerGridPos, out MonoBehaviour other)
            && other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(enemyData.attackPower);

            string playerName = other.TryGetComponent<PlayerController>(out var player) ? player.PlayerName : "플레이어";
            Debug.Log($"{enemyData.enemyName}이(가) {playerName}을(를) 공격, 데미지: {enemyData.attackPower}");
        }
    }
}
