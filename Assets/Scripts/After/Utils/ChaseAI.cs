using System.Collections.Generic;
using UnityEngine;

// BFS로 target과 인접한 칸까지의 경로를 계산하는 공용 유틸리티
public static class ChaseAI
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private const int MaxSearchDepth = 200;

    // start에서 target과 인접한 칸까지의 실제 최단 경로를 구한 뒤
    // moveRange만큼만 앞부분을 잘라 반환
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, int moveRange, out bool reachedAdjacent)
    {
        if (GridUtils.IsAdjacent(start, target))
        {
            reachedAdjacent = true;
            return new List<Vector2Int>();
        }

        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> depth = new Dictionary<Vector2Int, int> { [start] = 0 };
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);

        Vector2Int adjacentCell = start;
        bool foundAdjacent = false;

        while (queue.Count > 0 && !foundAdjacent)
        {
            Vector2Int current = queue.Dequeue();
            int currentDepth = depth[current];
            if (currentDepth >= MaxSearchDepth) continue;

            foreach (Vector2Int dir in Directions)
            {
                Vector2Int next = current + dir;
                if (depth.ContainsKey(next)) continue;
                if (!GridMapManager.Instance.IsWalkable(next)) continue;

                depth[next] = currentDepth + 1;
                parent[next] = current;

                if (GridUtils.IsAdjacent(next, target))
                {
                    adjacentCell = next;
                    foundAdjacent = true;
                    break;
                }

                queue.Enqueue(next);
            }
        }

        if (!foundAdjacent)
        {
            reachedAdjacent = false;
            return new List<Vector2Int>();
        }

        List<Vector2Int> fullPath = BuildPath(parent, start, adjacentCell);
        reachedAdjacent = fullPath.Count <= moveRange;

        return fullPath.Count <= moveRange ? fullPath : fullPath.GetRange(0, moveRange);
    }

    private static List<Vector2Int> BuildPath(Dictionary<Vector2Int, Vector2Int> parent, Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = end;

        while (current != start)
        {
            path.Add(current);
            current = parent[current];
        }

        path.Reverse();
        return path;
    }
}
