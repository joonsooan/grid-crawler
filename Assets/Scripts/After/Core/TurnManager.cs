using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnState { WaitingForPlayer, ProcessingPlayerTurn, ProcessingEnemyTurn, TurnResolve }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private float enemyStepInterval = 0.5f;
    [SerializeField] private float turnTransitionDelay = 0.5f;

    public TurnState CurrentState { get; private set; } = TurnState.WaitingForPlayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnPlayerActionCompleted()
    {
        CurrentState = TurnState.ProcessingEnemyTurn;
        StartCoroutine(ProcessEnemyTurnsCoroutine());
    }

    private IEnumerator ProcessEnemyTurnsCoroutine()
    {
        yield return new WaitForSeconds(turnTransitionDelay);

        Queue<MonoBehaviour> turnQueue = new Queue<MonoBehaviour>(FindObjectsByType<Enemy>(FindObjectsSortMode.None));

        while (turnQueue.Count > 0)
        {
            MonoBehaviour entity = turnQueue.Dequeue();

            if (entity != null && entity is Enemy enemy)
            {
                yield return StartCoroutine(enemy.ExecuteTurnCoroutine(enemyStepInterval));
            }
        }

        yield return new WaitForSeconds(turnTransitionDelay);

        CurrentState = TurnState.TurnResolve;
        // TODO: 턴 종료 후 처리 (상태 효과, 턴 카운트 증가 등)
        CurrentState = TurnState.WaitingForPlayer;
    }
}
