using UnityEngine;

public enum ItemType { Consumable, Equipment }

[CreateAssetMenu(fileName = "ItemData", menuName = "Item Data")]
public class ItemDataSO : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public Sprite icon;
    [TextArea] public string description;

    public int healAmount;

    // 획득 즉시 플레이어의 이동 거리를 늘리는 아이템(신발류 등)에 사용
    public int moveRangeBonus;
}
