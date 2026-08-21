using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player Data")]
public class PlayerDataSO : ScriptableObject
{
    public string playerName;
    public int maxHp = 50;
    public int attackPower = 10;
    public int moveRange = 1;
}
