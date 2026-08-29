using UnityEngine;

public class TurnIndicatorIcon : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.9f, 0f);

    private Transform followTarget;

    private void Start()
    {
        TurnManager.Instance.OnEnemyTurnStarted += Show;
        TurnManager.Instance.OnEnemyTurnEnded += Hide;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        TurnManager.Instance.OnEnemyTurnStarted -= Show;
        TurnManager.Instance.OnEnemyTurnEnded -= Hide;
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;
        transform.position = followTarget.position + offset;
    }

    private void Show(MonoBehaviour entity)
    {
        followTarget = entity.transform;
        gameObject.SetActive(true);
        transform.position = followTarget.position + offset;
    }

    private void Hide()
    {
        followTarget = null;
        gameObject.SetActive(false);
    }
}
