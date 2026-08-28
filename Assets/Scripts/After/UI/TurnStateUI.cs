using TMPro;
using UnityEngine;

public class TurnStateUI : MonoBehaviour
{
    [SerializeField] private TMP_Text turnStateText;

    private void Start()
    {
        TurnManager.Instance.OnTurnStateChanged += Refresh;
        Refresh(TurnManager.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        TurnManager.Instance.OnTurnStateChanged -= Refresh;
    }

    private void Refresh(TurnState state)
    {
        turnStateText.text = state == TurnState.ProcessingEnemyTurn ? ">> 적 턴 <<" : ">> 플레이어 턴 <<";
    }
}
