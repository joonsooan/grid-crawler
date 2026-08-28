using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;

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
    }
}
