using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpaghettiUITurnManager : MonoBehaviour
{
    public static SpaghettiUITurnManager Instance { get; private set; }

    [SerializeField] private float enemyStepInterval = 0.5f;
    [SerializeField] private float enemyToEnemyDelay = 0.3f;
    [SerializeField] private float turnTransitionDelay = 0.5f;

    // UI 컴포넌트를 게임 로직 스크립트가 직접 참조
    [SerializeField] private TMP_Text turnStateText;
    [SerializeField] private TMP_Text movesRemainingText;

    public TurnState CurrentState { get; private set; } = TurnState.WaitingForPlayer;

    private int movesRemaining;
    private int maxMoves;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        turnStateText.text = ">> 플레이어 턴 <<";
        movesRemainingText.text = "남은 이동 : 10 / 10";
    }

    // 플레이어 입력 시점에 호출
    public void OnPlayerActionStarted(int moveRange)
    {
        if (movesRemaining <= 0)
        {
            maxMoves = moveRange;
            movesRemaining = moveRange;
            movesRemainingText.text = $"남은 이동 : {movesRemaining} / {maxMoves}";
        }

        CurrentState = TurnState.ProcessingPlayerTurn;
        turnStateText.text = ">> 플레이어 턴 <<";
    }

    // 턴을 넘길지, 같은 턴을 유지할지 판단
    public void ResolvePlayerAction(PlayerActionResult result)
    {
        if (result == PlayerActionResult.Blocked)
        {
            CurrentState = TurnState.WaitingForPlayer;
            turnStateText.text = ">> 플레이어 턴 <<";
            return;
        }

        if (result == PlayerActionResult.Moved)
        {
            movesRemaining--;
            movesRemainingText.text = $"남은 이동 : {movesRemaining} / {maxMoves}";

            if (movesRemaining > 0)
            {
                CurrentState = TurnState.WaitingForPlayer;
                turnStateText.text = ">> 플레이어 턴 <<";
                return;
            }
        }

        movesRemaining = 0;
        movesRemainingText.text = $"남은 이동 : {movesRemaining} / {maxMoves}";
        OnPlayerActionCompleted();
    }

    private void OnPlayerActionCompleted()
    {
        CurrentState = TurnState.ProcessingEnemyTurn;
        turnStateText.text = ">> 적 턴 <<";
        movesRemainingText.text = "남은 이동 : 3 / 3";
        StartCoroutine(ProcessEnemyTurnsCoroutine());
    }

    private IEnumerator ProcessEnemyTurnsCoroutine()
    {
        yield return new WaitForSeconds(turnTransitionDelay);

        turnStateText.text = ">> 적 턴 <<";

        Queue<MonoBehaviour> turnQueue = new Queue<MonoBehaviour>(GridMapManager.Instance.GetAllEntities());

        while (turnQueue.Count > 0)
        {
            MonoBehaviour entity = turnQueue.Dequeue();

            if (entity != null && entity is ITurnActor actor)
            {
                yield return StartCoroutine(actor.ExecuteTurnCoroutine(enemyStepInterval));

                turnStateText.text = ">> 적 턴 <<";
                movesRemainingText.text = "남은 이동 : 3 / 3";
                yield return new WaitForSeconds(enemyToEnemyDelay);
            }
        }

        yield return new WaitForSeconds(turnTransitionDelay);

        CurrentState = TurnState.TurnResolve;
        // TODO: 턴 종료 후 처리 (상태 효과, 턴 카운트 증가 등)
        CurrentState = TurnState.WaitingForPlayer;
        turnStateText.text = ">> 플레이어 턴 <<";
        movesRemaining = maxMoves;
        movesRemainingText.text = $"남은 이동 : {movesRemaining} / {maxMoves}";
    }
}
