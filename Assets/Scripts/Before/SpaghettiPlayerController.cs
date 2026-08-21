using UnityEngine;
using UnityEngine.InputSystem;

public class SpaghettiPlayerController : MonoBehaviour
{
    public float moveCooldown = 0.1f;

    private Vector2Int gridPos;
    private float lastMoveTime;

    private void Start()
    {
        gridPos = GridUtils.WorldToGrid(transform.position);
        transform.position = GridUtils.GridToWorld(gridPos, transform.position.z);
        GridMapManager.Instance.RegisterEntity(gridPos, this);
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

    // 목표 칸이 통행 가능하면 이동하고, GridMapManager의 점유 등록을 함께 갱신
    private void TryMove(Vector2Int dir)
    {
        Vector2Int targetPos = gridPos + dir;

        if (!GridMapManager.Instance.IsWalkable(targetPos))
        {
            Debug.Log("이동 불가");
            return;
        }

        GridMapManager.Instance.UnregisterEntity(gridPos);
        gridPos = targetPos;
        GridMapManager.Instance.RegisterEntity(gridPos, this);

        transform.position = GridUtils.GridToWorld(gridPos, transform.position.z);
    }

    // 트리거 충돌 & 태그 기반 상호작용 처리 -> 의도한 스파게티 코드
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            SpaghettiEnemy enemy = other.GetComponent<SpaghettiEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(10);
            }
        }

        else if (other.CompareTag("Item"))
        {
            SpaghettiItem item = other.GetComponent<SpaghettiItem>();
            if (item != null)
            {
                item.Interact(gameObject);
            }
        }
    }
}
