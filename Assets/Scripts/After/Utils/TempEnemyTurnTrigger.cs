using UnityEngine;
using UnityEngine.InputSystem;

// 임시 테스트용: 스페이스바를 누르면 모든 Enemy의 턴을 한 번에 수동 실행
public class TempEnemyTurnTrigger : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;

        foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            enemy.ExecuteTurn(player.GridPos);
        }
    }
}
