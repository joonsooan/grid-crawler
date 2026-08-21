using System.Collections.Generic;
using UnityEngine;

// moveRange 내에서 BFS로 target과 인접한 칸까지의 경로를 계산하는 공용 유틸리티
public static class ChaseAI
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    // start에서 target과 인접한 칸까지 도달 가능한 경로 반환
    // moveRange 내에 인접한 칸에 닿지 못하면 갈 수 있는 칸 중 target과 가장 가까운 칸까지의 경로를 반환
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

        Vector2Int best = start;
        int bestDist = Distance(start, target);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDepth = depth[current];
            int dist = Distance(current, target);

            if (dist < bestDist)
            {
                best = current;
                bestDist = dist;
            }

            if (GridUtils.IsAdjacent(current, target))
            {
                reachedAdjacent = true;
                return BuildPath(parent, start, current);
            }

            if (currentDepth >= moveRange) continue;

            foreach (Vector2Int dir in Directions)
            {
                Vector2Int next = current + dir;
                if (depth.ContainsKey(next)) continue;
                if (!GridMapManager.Instance.IsWalkable(next)) continue;

                depth[next] = currentDepth + 1;
                parent[next] = current;
                queue.Enqueue(next);
            }
        }

        reachedAdjacent = false;
        return BuildPath(parent, start, best);
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

    private static int Distance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
