using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IGridEntity
{
    public float moveCooldown = 0.1f;
    public int attackPower = 10;

    public Vector2Int GridPos { get; set; }

    private float lastMoveTime;

    // 시작 위치를 그리드에 맞춰 등록
    private void Start()
    {
        ((IGridEntity)this).RegisterToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(GridPos, 0f);
    }

    private void OnDestroy()
    {
        ((IGridEntity)this).UnregisterFromGrid();
    }

    // WASD 입력을 받아 플레이어를 한 칸 이동
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

        TryMove(dir);
        lastMoveTime = Time.time;
    }

    // 목표 칸의 점유자를 조회해 공격/상호작용을 먼저 판정, 비어 있으면 이동
    private void TryMove(Vector2Int dir)
    {
        Vector2Int targetPos = GridPos + dir;

        if (GridMapManager.Instance.TryGetEntity(targetPos, out MonoBehaviour other))
        {
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attackPower);
                return;
            }

            if (other.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(gameObject);
                return;
            }

            Debug.Log("이동 불가");
            return;
        }

        if (!GridMapManager.Instance.IsWalkable(targetPos))
        {
            Debug.Log("이동 불가");
            return;
        }

        ((IGridEntity)this).MoveOnGrid(targetPos);
        transform.position = GridUtils.GridToWorld(GridPos, transform.position.z);
    }
}
