using DG.Tweening;
using TMPro;
using UnityEngine;

public class TurnStateUI : MonoBehaviour
{
    [SerializeField] private TMP_Text turnStateText;
    [SerializeField] private float slideDistance = 100f;
    [SerializeField] private float slideDuration = 0.3f;

    private RectTransform rectTransform;
    private Vector2 basePos;
    private bool isEnemyTurnDisplayed;

    private void Awake()
    {
        rectTransform = turnStateText.rectTransform;
    }

    private void Start()
    {
        basePos = rectTransform.anchoredPosition;

        isEnemyTurnDisplayed = TurnManager.Instance.CurrentState == TurnState.ProcessingEnemyTurn;
        turnStateText.text = isEnemyTurnDisplayed ? ">> 적 턴 <<" : ">> 플레이어 턴 <<";

        TurnManager.Instance.OnTurnStateChanged += Refresh;
    }

    private void OnDestroy()
    {
        TurnManager.Instance.OnTurnStateChanged -= Refresh;
        rectTransform.DOKill();
    }

    private void Refresh(TurnState state)
    {
        bool isEnemyTurn = state == TurnState.ProcessingEnemyTurn;
        if (isEnemyTurn == isEnemyTurnDisplayed) return;
        isEnemyTurnDisplayed = isEnemyTurn;

        rectTransform.DOKill();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOAnchorPosX(basePos.x - slideDistance, slideDuration * 0.5f).SetEase(Ease.InCubic));
        sequence.AppendCallback(() =>
        {
            turnStateText.text = isEnemyTurn ? ">> 적 턴 <<" : ">> 플레이어 턴 <<";
            rectTransform.anchoredPosition = new Vector2(basePos.x + slideDistance, basePos.y);
        });
        sequence.Append(rectTransform.DOAnchorPosX(basePos.x, slideDuration * 0.5f).SetEase(Ease.OutCubic));
    }
}
