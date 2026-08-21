using UnityEngine;

public class SpaghettiEnemy : MonoBehaviour
{
    public int hp = 20;

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
