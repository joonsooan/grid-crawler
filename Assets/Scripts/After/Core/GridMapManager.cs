using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Floor, Wall, Water }

public class GridMapManager : MonoBehaviour
{
    public static GridMapManager Instance { get; private set; }

    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap waterTilemap;

    // O(1) 정적 타일 및 동적 엔티티 점유 상태 관리
    private Dictionary<Vector2Int, TileType> tileMap = new Dictionary<Vector2Int, TileType>();
    private Dictionary<Vector2Int, MonoBehaviour> entityMap = new Dictionary<Vector2Int, MonoBehaviour>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // TODO: wallTilemap, waterTilemap을 순회하며 tileMap에 TileType(Wall/Water) 등록
    }

    // 해당 칸이 통행 가능한지 판정
    public bool IsWalkable(Vector2Int pos)
    {
        if (tileMap.TryGetValue(pos, out TileType type))
        {
            if (type != TileType.Floor) return false;
        }
        return !entityMap.ContainsKey(pos);
    }

    // 해당 칸에 엔티티를 점유 등록
    public void RegisterEntity(Vector2Int pos, MonoBehaviour entity)
    {
        entityMap[pos] = entity;
    }

    // 해당 칸의 엔티티 점유를 해제
    public void UnregisterEntity(Vector2Int pos)
    {
        if (entityMap.ContainsKey(pos)) entityMap.Remove(pos);
    }

    // 특정 칸을 점유한 엔티티를 O(1)로 조회
    public bool TryGetEntity(Vector2Int pos, out MonoBehaviour entity)
    {
        return entityMap.TryGetValue(pos, out entity);
    }
}
