using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Enemy : MonoBehaviour, IGridEntity, IDamageable, ITurnActor
{
    [SerializeField] private EnemyDataSO enemyData;
    [SerializeField] private float attackPunchDuration = 0.3f;
    [SerializeField] private float hitShakeDuration = 0.3f;
    [SerializeField] private float deathDuration = 0.25f;

    private int hp;
    private SpriteRenderer spriteRenderer;

    public Vector2Int GridPos { get; set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

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
        PlayHitReaction();

        if (hp <= 0)
        {
            Debug.Log($"{enemyData.enemyName} 사망");
            StartCoroutine(DieAfterHitReaction());
        }
    }

    private IEnumerator DieAfterHitReaction()
    {
        yield return new WaitForSeconds(hitShakeDuration);

        Sequence deathSequence = DOTween.Sequence();
        deathSequence.Join(transform.DOScale(Vector3.zero, deathDuration).SetEase(Ease.InBack));
        if (spriteRenderer != null) deathSequence.Join(spriteRenderer.DOFade(0f, deathDuration));
        yield return deathSequence.WaitForCompletion();

        Destroy(gameObject);
    }

    private void PlayHitReaction()
    {
        transform.DOShakePosition(hitShakeDuration, strength: 0.15f, vibrato: 20);

        if (spriteRenderer == null) return;
        spriteRenderer.DOColor(Color.red, 0.05f)
            .OnComplete(() => spriteRenderer.DOColor(Color.white, hitShakeDuration - 0.05f));
    }

    // 계산한 경로를 한 칸씩 이동, 플레이어와 인접해졌다면 공격
    public IEnumerator ExecuteTurnCoroutine(float stepInterval, Action<int, int> onMovesRemainingChanged)
    {
        List<Vector2Int> path = ChaseAI.FindPath(GridPos, PlayerController.Instance.GridPos, enemyData.moveRange, out bool reachedAdjacent);

        int stepsRemaining = path.Count;
        onMovesRemainingChanged?.Invoke(stepsRemaining, enemyData.moveRange);

        foreach (Vector2Int step in path)
        {
            ((IGridEntity)this).MoveOnGrid(step);

            Vector3 targetWorldPos = GridUtils.GridToWorld(GridPos, 0f);
            yield return transform.DOMove(targetWorldPos, stepInterval)
                .SetEase(Ease.OutQuad)
                .WaitForCompletion();

            stepsRemaining--;
            onMovesRemainingChanged?.Invoke(stepsRemaining, enemyData.moveRange);
        }

        if (reachedAdjacent) yield return AttackPlayerCoroutine();
    }

    private IEnumerator AttackPlayerCoroutine()
    {
        if (!GridUtils.IsAdjacent(GridPos, PlayerController.Instance.GridPos)) yield break;

        PlayerController.Instance.TakeDamage(enemyData.attackPower);
        Debug.Log($"{enemyData.enemyName}이(가) {PlayerController.Instance.PlayerName}을(를) 공격, 데미지: {enemyData.attackPower}");

        Vector3 punch = (PlayerController.Instance.transform.position - transform.position).normalized * 0.3f;
        yield return transform.DOPunchPosition(punch, attackPunchDuration, vibrato: 6, elasticity: 0.5f).WaitForCompletion();
    }
}
