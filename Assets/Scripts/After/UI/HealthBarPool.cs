using UnityEngine;
using UnityEngine.Pool;

public class HealthBarPool : MonoBehaviour
{
    public static HealthBarPool Instance { get; private set; }

    [SerializeField] private HealthBarUI healthBarPrefab;
    [SerializeField] private Transform poolParent;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 30;

    private IObjectPool<HealthBarUI> pool;

    private void Awake()
    {
        Instance = this;

        pool = new ObjectPool<HealthBarUI>(
            createFunc: () => Instantiate(healthBarPrefab, poolParent),
            actionOnGet: bar => bar.gameObject.SetActive(true),
            actionOnRelease: bar =>
            {
                bar.Unbind();
                bar.gameObject.SetActive(false);
            },
            actionOnDestroy: bar =>
            {
                if (bar != null) Destroy(bar.gameObject);
            },
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    public HealthBarUI Rent(IHealthBarSource source)
    {
        HealthBarUI bar = pool.Get();
        bar.Bind(source);
        return bar;
    }

    public void Return(HealthBarUI bar)
    {
        pool.Release(bar);
    }
}
