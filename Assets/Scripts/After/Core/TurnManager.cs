using System;
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

    public event Action<TurnState> OnTurnStateChanged;
    public event Action<int, int> OnMovesRemainingChanged;

    public int MovesRemaining => movesRemaining;
    public int MaxMoves => maxMoves;

    public const int TransitionMoves = -1;

    private int movesRemaining;
    private int maxMoves;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void InitializePlayerMoves(int moveRange)
    {
        maxMoves = moveRange;
        movesRemaining = moveRange;
        OnMovesRemainingChanged?.Invoke(movesRemaining, maxMoves);
    }

    // 플레이어 입력 시점에 호출
    public void OnPlayerActionStarted(int moveRange)
    {
        if (movesRemaining <= 0)
        {
            maxMoves = moveRange;
            movesRemaining = moveRange;
            OnMovesRemainingChanged?.Invoke(movesRemaining, maxMoves);
        }

        SetState(TurnState.ProcessingPlayerTurn);
    }

    // 턴을 넘길지, 같은 턴을 유지할지 판단
    public void ResolvePlayerAction(PlayerActionResult result)
    {
        if (result == PlayerActionResult.Blocked)
        {
            SetState(TurnState.WaitingForPlayer);
            return;
        }

        if (result == PlayerActionResult.Moved)
        {
            movesRemaining--;
            OnMovesRemainingChanged?.Invoke(movesRemaining, maxMoves);

            if (movesRemaining > 0)
            {
                SetState(TurnState.WaitingForPlayer);
                return;
            }
        }

        movesRemaining = 0;
        OnMovesRemainingChanged?.Invoke(movesRemaining, maxMoves);
        OnPlayerActionCompleted();
    }

    private void OnPlayerActionCompleted()
    {
        SetState(TurnState.ProcessingEnemyTurn);
        StartCoroutine(ProcessEnemyTurnsCoroutine());
    }

    private IEnumerator ProcessEnemyTurnsCoroutine()
    {
        OnMovesRemainingChanged?.Invoke(TransitionMoves, TransitionMoves);
        yield return new WaitForSeconds(turnTransitionDelay);

        Queue<MonoBehaviour> turnQueue = new Queue<MonoBehaviour>(GridMapManager.Instance.GetAllEntities());

        while (turnQueue.Count > 0)
        {
            MonoBehaviour entity = turnQueue.Dequeue();

            if (entity != null && entity is ITurnActor actor)
            {
                yield return StartCoroutine(actor.ExecuteTurnCoroutine(enemyStepInterval, RelayMovesRemaining));
                yield return new WaitForSeconds(enemyToEnemyDelay);
            }
        }

        OnMovesRemainingChanged?.Invoke(TransitionMoves, TransitionMoves);
        yield return new WaitForSeconds(turnTransitionDelay);

        SetState(TurnState.TurnResolve);
        // TODO: 턴 종료 후 처리 (추가 상태 효과 등)
        SetState(TurnState.WaitingForPlayer);

        movesRemaining = maxMoves;
        OnMovesRemainingChanged?.Invoke(movesRemaining, maxMoves);
    }

    private void RelayMovesRemaining(int current, int max)
    {
        OnMovesRemainingChanged?.Invoke(current, max);
    }

    private void SetState(TurnState state)
    {
        CurrentState = state;
        OnTurnStateChanged?.Invoke(state);
    }
}
