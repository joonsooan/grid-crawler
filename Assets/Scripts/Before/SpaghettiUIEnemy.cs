using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpaghettiUIEnemy : MonoBehaviour, IGridEntity, IDamageable, ITurnActor
{
    [SerializeField] private EnemyDataSO enemyData;
    // UI 컴포넌트를 게임 로직 스크립트가 직접 참조
    [SerializeField] private TMP_Text movesRemainingText;

    private int hp;

    public Vector2Int GridPos { get; set; }

    private void Start()
    {
        hp = enemyData.maxHp;
        ((IGridEntity)this).RegisterToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
        movesRemainingText.text = "남은 이동 : 3 / 3";
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
    public IEnumerator ExecuteTurnCoroutine(float stepInterval)
    {
        List<Vector2Int> path = ChaseAI.FindPath(GridPos, SpaghettiUIPlayerController.Instance.GridPos, enemyData.moveRange, out bool reachedAdjacent);

        int totalSteps = path.Count;
        int stepsRemaining = totalSteps;
        movesRemainingText.text = $"남은 이동 : {stepsRemaining} / {totalSteps}";

        foreach (Vector2Int step in path)
        {
            ((IGridEntity)this).MoveOnGrid(step);
            transform.position = GridUtils.GridToWorld(GridPos, 0f);
            stepsRemaining--;
            movesRemainingText.text = $"남은 이동 : {stepsRemaining} / {totalSteps}";
            yield return new WaitForSeconds(stepInterval);
        }

        if (reachedAdjacent) AttackPlayer();
    }

    private void AttackPlayer()
    {
        if (!GridUtils.IsAdjacent(GridPos, SpaghettiUIPlayerController.Instance.GridPos)) return;

        SpaghettiUIPlayerController.Instance.TakeDamage(enemyData.attackPower);
        Debug.Log($"{enemyData.enemyName}이(가) {SpaghettiUIPlayerController.Instance.PlayerName}을(를) 공격, 데미지: {enemyData.attackPower}");
    }
}
