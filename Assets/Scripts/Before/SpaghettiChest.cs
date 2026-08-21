using UnityEngine;

public class SpaghettiChest : MonoBehaviour
{
    public bool isOpened = false;

    public void Open()
    {
        if (isOpened) return;

        isOpened = true;
        Debug.Log($"아이템 획득");
        Destroy(gameObject);
    }
}
