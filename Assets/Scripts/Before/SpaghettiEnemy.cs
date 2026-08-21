using System.Collections.Generic;
using UnityEngine;

public class SpaghettiEnemy : MonoBehaviour, IGridEntity, IDamageable
{
    // 적 유닛 스탯
    public int maxHp = 30;
    public int attackPower = 5;
    public int moveRange = 3;

    private int hp;

    public Vector2Int GridPos { get; set; }

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

    // 플레이어와 인접하면 공격, 아니면 moveRange 내에서 BFS로 접근
    public void ExecuteTurn(Vector2Int playerGridPos)
    {
        int distToPlayer = Mathf.Abs(GridPos.x - playerGridPos.x) + Mathf.Abs(GridPos.y - playerGridPos.y);
        if (distToPlayer == 1)
        {
            AttackPlayer(playerGridPos);
            return;
        }

        Dictionary<Vector2Int, int> depth = new Dictionary<Vector2Int, int> { [GridPos] = 0 };
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(GridPos);

        Vector2Int best = GridPos;
        int bestDist = Mathf.Abs(GridPos.x - playerGridPos.x) + Mathf.Abs(GridPos.y - playerGridPos.y);
        Vector2Int destination = GridPos;
        bool found = false;

        while (queue.Count > 0 && !found)
        {
            Vector2Int current = queue.Dequeue();
            int currentDepth = depth[current];
            int dist = Mathf.Abs(current.x - playerGridPos.x) + Mathf.Abs(current.y - playerGridPos.y);

            if (dist < bestDist)
            {
                best = current;
                bestDist = dist;
            }

            if (dist == 1)
            {
                destination = current;
                found = true;
                break;
            }

            if (currentDepth >= moveRange) continue;

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                if (depth.ContainsKey(next)) continue;
                if (!GridMapManager.Instance.IsWalkable(next)) continue;

                depth[next] = currentDepth + 1;
                queue.Enqueue(next);
            }
        }

        if (!found) destination = best;

        if (destination != GridPos)
        {
            ((IGridEntity)this).MoveOnGrid(destination);
            transform.position = GridUtils.GridToWorld(GridPos, transform.position.z);
        }

        // BFS로 도달한 칸이 플레이어 옆 칸이면 바로 공격
        if (found) AttackPlayer(playerGridPos);
    }

    // 플레이어 공격
    private void AttackPlayer(Vector2Int playerGridPos)
    {
        if (GridMapManager.Instance.TryGetEntity(playerGridPos, out MonoBehaviour other)
            && other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(attackPower);
        }
    }
}
