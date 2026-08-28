using TMPro;
using UnityEngine;

public class MovesRemainingUI : MonoBehaviour
{
    [SerializeField] private TMP_Text movesRemainingText;

    private void OnEnable()
    {
        TurnManager.Instance.OnMovesRemainingChanged += Refresh;
    }

    private void OnDisable()
    {
        TurnManager.Instance.OnMovesRemainingChanged -= Refresh;
    }

    private void Refresh(int current, int max)
    {
        movesRemainingText.text = $"남은 이동 : {current} / {max}";
    }
}
