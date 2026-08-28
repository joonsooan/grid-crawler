using DG.Tweening;
using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private float punchScaleAmount = 0.3f;
    [SerializeField] private float flashDuration = 0.2f;

    private int previousHp = -1;

    private void Start()
    {
        PlayerController.Instance.OnHealthChanged += Refresh;
        Refresh(PlayerController.Instance.CurrentHp, PlayerController.Instance.MaxHp);
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnHealthChanged -= Refresh;
    }

    private void Refresh(int current, int max)
    {
        healthText.text = $"체력 : {current} / {max}";

        if (previousHp >= 0 && current < previousHp)
        {
            healthText.transform.DOPunchScale(Vector3.one * punchScaleAmount, punchDuration, vibrato: 8);
            healthText.DOColor(Color.red, flashDuration * 0.5f)
                .OnComplete(() => healthText.DOColor(Color.white, flashDuration * 0.5f));
        }
        else if (previousHp >= 0 && current > previousHp)
        {
            healthText.DOColor(Color.green, flashDuration * 0.5f)
                .OnComplete(() => healthText.DOColor(Color.white, flashDuration * 0.5f));
        }

        previousHp = current;
    }
}
