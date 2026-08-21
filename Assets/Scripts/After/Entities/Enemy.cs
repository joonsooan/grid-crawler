using UnityEngine;

public class Enemy : MonoBehaviour, IGridEntity, IDamageable
{
    public int hp = 20;

    public Vector2Int GridPos { get; set; }

    private void Start()
    {
        ((IGridEntity)this).RegisterToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
    }

    private void OnDestroy()
    {
        ((IGridEntity)this).UnregisterFromGrid();
    }

    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        Debug.Log($"{name} 피격, 남은 체력: {hp}");

        if (hp <= 0)
        {
            Debug.Log($"{name} 사망");
            Destroy(gameObject);
        }
    }
}
