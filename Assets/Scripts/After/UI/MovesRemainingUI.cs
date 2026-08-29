using DG.Tweening;
using TMPro;
using UnityEngine;

public class MovesRemainingUI : MonoBehaviour
{
    [SerializeField] private TMP_Text movesRemainingText;
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private float punchScaleAmount = 0.3f;

    private void Start()
    {
        TurnManager.Instance.OnMovesRemainingChanged += Refresh;
        TurnManager.Instance.OnMovesIncreasedByItem += PlayPunch;
        Refresh(TurnManager.Instance.MovesRemaining, TurnManager.Instance.MaxMoves);
    }

    private void OnDestroy()
    {
        TurnManager.Instance.OnMovesRemainingChanged -= Refresh;
        TurnManager.Instance.OnMovesIncreasedByItem -= PlayPunch;
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

    private void PlayPunch()
    {
        movesRemainingText.transform.DOPunchScale(Vector3.one * -punchScaleAmount, punchDuration, vibrato: 8);
    }
}
