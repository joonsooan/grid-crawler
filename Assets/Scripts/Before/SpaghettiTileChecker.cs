using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaghettiTileChecker : MonoBehaviour
{
    public List<Transform> allUnits;
    public Transform player;
    public float moveCooldown = 0.1f;

    private float lastMoveTime;

    // 방향키 입력을 받아 플레이어를 한 칸 이동
    private void Update()
    {
        if (Time.time - lastMoveTime < moveCooldown) return;

        if (Keyboard.current == null) return;

        Vector2Int dir = Vector2Int.zero;
        if (Keyboard.current.wKey.isPressed) dir = Vector2Int.up;
        else if (Keyboard.current.sKey.isPressed) dir = Vector2Int.down;
        else if (Keyboard.current.aKey.isPressed) dir = Vector2Int.left;
        else if (Keyboard.current.dKey.isPressed) dir = Vector2Int.right;

        if (dir == Vector2Int.zero) return;

        Vector2Int currentPos = WorldToGrid(player.position);
        Vector2Int targetPos = currentPos + dir;

        if (!IsTileOccupied(targetPos))
        {
            player.position = new Vector3(targetPos.x + 0.5f, targetPos.y + 0.5f, player.position.z);
            lastMoveTime = Time.time;
        }
        else
        {
            Debug.Log("이동 불가: 대상 칸이 이미 점유되어 있음");
        }
    }

    // 월드 좌표를 그리드 좌표로 변환
    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    // allUnits를 전체 순회하며 해당 칸이 점유되어 있는지 확인
    private bool IsTileOccupied(Vector2Int gridPos)
    {
        foreach (var unit in allUnits)
        {
            Vector2Int unitGridPos = WorldToGrid(unit.position);
            Debug.Log($"{unit.name} 좌표: {unitGridPos}");
            if (unitGridPos == gridPos) return true;
        }
        return false;
    }
}
