using System.Collections.Generic;
using UnityEngine;

// moveRange 내에서 BFS로 target과 인접한 칸까지의 경로를 계산하는 공용 유틸리티
public static class ChaseAI
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    // start에서 target과 인접한 칸까지 도달 가능한 다음 위치를 반환
    // moveRange 내에 인접한 칸에 닿지 못하면 갈 수 있는 칸 중 target과 가장 가까운 칸을 반환
    public static Vector2Int FindNextPosition(Vector2Int start, Vector2Int target, int moveRange, out bool reachedAdjacent)
    {
        int startDist = Distance(start, target);
        if (startDist == 1)
        {
            reachedAdjacent = true;
            return start;
        }

        Dictionary<Vector2Int, int> depth = new Dictionary<Vector2Int, int> { [start] = 0 };
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);

        Vector2Int best = start;
        int bestDist = startDist;

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

            if (dist == 1)
            {
                reachedAdjacent = true;
                return current;
            }

            if (currentDepth >= moveRange) continue;

            foreach (Vector2Int dir in Directions)
            {
                Vector2Int next = current + dir;
                if (depth.ContainsKey(next)) continue;
                if (!GridMapManager.Instance.IsWalkable(next)) continue;

                depth[next] = currentDepth + 1;
                queue.Enqueue(next);
            }
        }

        reachedAdjacent = false;
        return best;
    }

    private static int Distance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
