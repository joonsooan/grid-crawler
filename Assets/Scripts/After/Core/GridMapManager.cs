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
    private Dictionary<Vector2Int, Item> itemMap = new Dictionary<Vector2Int, Item>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        RegisterTilemap(wallTilemap, TileType.Wall);
        RegisterTilemap(waterTilemap, TileType.Water);
    }

    // 타일이 존재하는 칸을 순회하며 tileMap에 등록
    private void RegisterTilemap(Tilemap tilemap, TileType type)
    {
        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cell)) continue;
            tileMap[(Vector2Int)cell] = type;
        }
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

    // 그리드에 등록된 모든 엔티티 조회
    public IEnumerable<MonoBehaviour> GetAllEntities()
    {
        return entityMap.Values;
    }

    // 해당 칸에 아이템을 등록 (이동 차단 없이 상호작용만 가능)
    public void RegisterItem(Vector2Int pos, Item item)
    {
        itemMap[pos] = item;
    }

    // 해당 칸의 아이템 등록을 해제
    public void UnregisterItem(Vector2Int pos)
    {
        if (itemMap.ContainsKey(pos)) itemMap.Remove(pos);
    }

    // 특정 칸의 아이템을 O(1)로 조회
    public bool TryGetItem(Vector2Int pos, out Item item)
    {
        return itemMap.TryGetValue(pos, out item);
    }
}
