using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IGridEntity, IDamageable, ITurnActor
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

    // 계산한 경로를 한 칸씩 이동, 플레이어와 인접해졌다면 공격
    public IEnumerator ExecuteTurnCoroutine(float stepInterval, Action<int, int> onMovesRemainingChanged)
    {
        List<Vector2Int> path = ChaseAI.FindPath(GridPos, PlayerController.Instance.GridPos, enemyData.moveRange, out bool reachedAdjacent);

        int stepsRemaining = path.Count;
        onMovesRemainingChanged?.Invoke(stepsRemaining, enemyData.moveRange);

        foreach (Vector2Int step in path)
        {
            yield return new WaitForSeconds(stepInterval);

            ((IGridEntity)this).MoveOnGrid(step);
            transform.position = GridUtils.GridToWorld(GridPos, 0f);
            stepsRemaining--;
            onMovesRemainingChanged?.Invoke(stepsRemaining, enemyData.moveRange);
        }

        if (reachedAdjacent) AttackPlayer();
    }

    private void AttackPlayer()
    {
        if (!GridUtils.IsAdjacent(GridPos, PlayerController.Instance.GridPos)) return;

        PlayerController.Instance.TakeDamage(enemyData.attackPower);
        Debug.Log($"{enemyData.enemyName}이(가) {PlayerController.Instance.PlayerName}을(를) 공격, 데미지: {enemyData.attackPower}");
    }
}
