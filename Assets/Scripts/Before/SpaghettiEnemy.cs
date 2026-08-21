using UnityEngine;

public class SpaghettiEnemy : MonoBehaviour
{
    public int hp = 20;

    private Vector2Int gridPos;

    private void Start()
    {
        gridPos = GridUtils.WorldToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(gridPos, transform.position.z);
        GridMapManager.Instance.RegisterEntity(gridPos, this);
    }

    private void OnDestroy()
    {
        GridMapManager.Instance.UnregisterEntity(gridPos);
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        Debug.Log($"{name} 피격, 남은 체력: {hp}");

        if (hp <= 0)
        {
            Debug.Log($"{name} 사망");
            Destroy(gameObject);
        }
    }
}
