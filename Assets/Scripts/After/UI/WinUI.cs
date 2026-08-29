using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinUI : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private CanvasGroup winCanvasGroup;
    [SerializeField] private Button restartButton;
    [SerializeField] private float showDelay = 1f;
    [SerializeField] private float fadeDuration = 0.6f;

    private void Start()
    {
        Enemy.OnAllEnemiesDefeated += Show;
        winPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        Enemy.OnAllEnemiesDefeated -= Show;
    }

    private void Show()
    {
        Invoke(nameof(ShowPanel), showDelay);
    }

    private void ShowPanel()
    {
        winPanel.SetActive(true);

        winCanvasGroup.alpha = 0f;
        winCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutSine);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
