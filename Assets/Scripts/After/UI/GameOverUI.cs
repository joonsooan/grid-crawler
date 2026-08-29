using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private Button restartButton;
    [SerializeField] private float showDelay = 1f;
    [SerializeField] private float fadeDuration = 0.6f;

    private void Start()
    {
        PlayerController.Instance.OnPlayerDied += Show;
        gameOverPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnPlayerDied -= Show;
    }

    private void Show()
    {
        Invoke(nameof(ShowPanel), showDelay);
    }

    private void ShowPanel()
    {
        gameOverPanel.SetActive(true);

        gameOverCanvasGroup.alpha = 0f;
        gameOverCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutSine);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
