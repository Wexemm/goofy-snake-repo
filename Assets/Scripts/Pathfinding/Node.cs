using UnityEngine;

/// <summary>
/// A single cell in the A* pathfinding grid.
/// Stores spatial data and the costs used by the A* algorithm.
/// </summary>
public class Node
{
    public bool    walkable;
    public Vector2 worldPosition;
    public int     gridX;
    public int     gridY;

    // A* costs
    public int  gCost;               // Distance from start node
    public int  hCost;               // Heuristic distance to target node
    public int  fCost => gCost + hCost;
    public Node parent;

    public Node(bool walkable, Vector2 worldPosition, int gridX, int gridY)
    {
        this.walkable      = walkable;
        this.worldPosition = worldPosition;
        this.gridX         = gridX;
        this.gridY         = gridY;
    }
}
