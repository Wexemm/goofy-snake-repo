using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stateless A* pathfinding utility.
/// Requires an <see cref="AStarGrid"/> singleton to be present in the scene.
///
/// Movement costs: 10 for cardinal directions, 14 for diagonals
/// (integer approximation of 1 and √2 scaled ×10).
/// </summary>
public static class AStarPathfinder
{
    /// <summary>
    /// Computes the shortest walkable path between two world-space positions.
    /// </summary>
    /// <returns>
    /// An ordered list of world-space waypoints leading from
    /// <paramref name="startPos"/> to <paramref name="targetPos"/>,
    /// or <c>null</c> if no path exists.
    /// </returns>
    public static List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        AStarGrid grid = AStarGrid.Instance;
        if (grid == null) return null;

        Node startNode  = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);
        if (startNode == null || targetNode == null) return null;

        // If start or target land inside a wall, reroute to the nearest open cell.
        if (!startNode.walkable)
            startNode = FindNearestWalkable(startNode, grid);
        if (!targetNode.walkable)
            targetNode = FindNearestWalkable(targetNode, grid);

        if (startNode == targetNode)
            return new List<Vector2>();

        // ----- A* -------------------------------------------------------
        var openSet   = new List<Node>();
        var closedSet = new HashSet<Node>();
        // Track every node whose state we touch so we can reset it afterwards.
        // This prevents stale gCost / parent values from corrupting future calls.
        var touched   = new List<Node>();

        startNode.gCost  = 0;
        startNode.hCost  = GetDistance(startNode, targetNode);
        startNode.parent = null;
        openSet.Add(startNode);
        touched.Add(startNode);

        List<Vector2> result = null;

        while (openSet.Count > 0)
        {
            Node current = GetLowestFCost(openSet);

            if (current == targetNode)
            {
                result = RetracePath(startNode, targetNode);
                break;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Node neighbor in grid.GetNeighbors(current))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newG = current.gCost + GetDistance(current, neighbor);
                if (newG < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost  = newG;
                    neighbor.hCost  = GetDistance(neighbor, targetNode);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                        touched.Add(neighbor);
                    }
                }
            }
        }

        // Reset every touched node so their gCost / hCost / parent are clean
        // for the next FindPath call (prevents stale-state path corruption).
        foreach (Node n in touched)
        {
            n.gCost  = 0;
            n.hCost  = 0;
            n.parent = null;
        }

        return result;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    static Node GetLowestFCost(List<Node> list)
    {
        Node best = list[0];
        for (int i = 1; i < list.Count; i++)
        {
            Node n = list[i];
            if (n.fCost < best.fCost ||
                (n.fCost == best.fCost && n.hCost < best.hCost))
                best = n;
        }
        return best;
    }

    static List<Vector2> RetracePath(Node start, Node end)
    {
        var path    = new List<Vector2>();
        var seen    = new HashSet<Node>();
        Node current = end;

        while (current != start)
        {
            // Null parent or cycle in the parent chain means the path is corrupt.
            if (current == null || !seen.Add(current))
                return null;

            path.Add(current.worldPosition);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Octile distance heuristic — exact for 8-directional grids.
    /// </summary>
    static int GetDistance(Node a, Node b)
    {
        int dX = Mathf.Abs(a.gridX - b.gridX);
        int dY = Mathf.Abs(a.gridY - b.gridY);
        return dX > dY
            ? 14 * dY + 10 * (dX - dY)
            : 14 * dX + 10 * (dY - dX);
    }

    /// <summary>
    /// BFS outward from <paramref name="origin"/> to find the closest walkable node.
    /// Used when the target position maps into an unwalkable cell (e.g. inside a wall).
    /// </summary>
    static Node FindNearestWalkable(Node origin, AStarGrid grid)
    {
        var queue   = new Queue<Node>();
        var visited = new HashSet<Node> { origin };
        queue.Enqueue(origin);

        while (queue.Count > 0)
        {
            Node n = queue.Dequeue();
            if (n.walkable) return n;

            foreach (Node neighbor in grid.GetNeighbors(n))
            {
                if (visited.Add(neighbor))
                    queue.Enqueue(neighbor);
            }
        }

        return origin; // Fallback: return origin if everything is blocked
    }
}
