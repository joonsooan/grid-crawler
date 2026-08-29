using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.6f, 0f);

    private IHealthBarSource source;
    private Transform followTarget;

    public void Bind(IHealthBarSource newSource)
    {
        source = newSource;
        followTarget = newSource.HealthBarAnchor;
        source.OnHealthChanged += Refresh;
        Refresh(source.CurrentHp, source.MaxHp);

        transform.position = followTarget.position + offset;
    }

    public void Unbind()
    {
        if (source != null) source.OnHealthChanged -= Refresh;
        source = null;
        followTarget = null;
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;
        transform.position = followTarget.position + offset;
    }

    private void Refresh(int current, int max)
    {
        fillImage.fillAmount = max > 0 ? (float)current / max : 0f;
    }
}
