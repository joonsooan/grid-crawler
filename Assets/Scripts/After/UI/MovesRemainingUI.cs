using TMPro;
using UnityEngine;

public class MovesRemainingUI : MonoBehaviour
{
    [SerializeField] private TMP_Text movesRemainingText;

    private void Start()
    {
        TurnManager.Instance.OnMovesRemainingChanged += Refresh;
        Refresh(TurnManager.Instance.MovesRemaining, TurnManager.Instance.MaxMoves);
    }

    private void OnDestroy()
    {
        TurnManager.Instance.OnMovesRemainingChanged -= Refresh;
    }

    private void Refresh(int current, int max)
    {
        if (current == TurnManager.TransitionMoves)
        {
            movesRemainingText.text = "남은 이동 : -";
            return;
        }

        movesRemainingText.text = $"남은 이동 : {current} / {max}";
    }
}
