using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnState { WaitingForPlayer, ProcessingPlayerTurn, ProcessingEnemyTurn, TurnResolve }
public enum PlayerActionResult { Blocked, Moved, TurnEnd }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private float enemyStepInterval = 0.5f;
    [SerializeField] private float enemyToEnemyDelay = 0.3f;
    [SerializeField] private float turnTransitionDelay = 0.5f;

    public TurnState CurrentState { get; private set; } = TurnState.WaitingForPlayer;

    private int movesRemaining;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 플레이어 입력 시점에 호출
    public void OnPlayerActionStarted(int moveRange)
    {
        if (movesRemaining <= 0) movesRemaining = moveRange;
        CurrentState = TurnState.ProcessingPlayerTurn;
    }

    // 턴을 넘길지, 같은 턴을 유지할지 판단
    public void ResolvePlayerAction(PlayerActionResult result)
    {
        if (result == PlayerActionResult.Blocked)
        {
            CurrentState = TurnState.WaitingForPlayer;
            return;
        }

        if (result == PlayerActionResult.Moved && --movesRemaining > 0)
        {
            CurrentState = TurnState.WaitingForPlayer;
            return;
        }

        movesRemaining = 0;
        OnPlayerActionCompleted();
    }

    private void OnPlayerActionCompleted()
    {
        CurrentState = TurnState.ProcessingEnemyTurn;
        StartCoroutine(ProcessEnemyTurnsCoroutine());
    }

    private IEnumerator ProcessEnemyTurnsCoroutine()
    {
        yield return new WaitForSeconds(turnTransitionDelay);

        Queue<MonoBehaviour> turnQueue = new Queue<MonoBehaviour>(GridMapManager.Instance.GetAllEntities());

        while (turnQueue.Count > 0)
        {
            MonoBehaviour entity = turnQueue.Dequeue();

            if (entity != null && entity is ITurnActor actor)
            {
                yield return StartCoroutine(actor.ExecuteTurnCoroutine(enemyStepInterval));
                yield return new WaitForSeconds(enemyToEnemyDelay);
            }
        }

        yield return new WaitForSeconds(turnTransitionDelay);

        CurrentState = TurnState.TurnResolve;
        // TODO: 턴 종료 후 처리 (상태 효과, 턴 카운트 증가 등)
        CurrentState = TurnState.WaitingForPlayer;
    }
}
