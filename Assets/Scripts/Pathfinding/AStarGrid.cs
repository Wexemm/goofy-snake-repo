using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds and owns the walkability grid used by A* pathfinding.
/// Place this on a single GameObject in the scene and configure the inspector fields.
///
/// Setup:
///   1. Create a "Wall" layer in Unity (Edit → Project Settings → Tags and Layers).
///   2. Assign that layer to all wall prefabs.
///   3. Set <see cref="unwalkableMask"/> to the Wall layer in the inspector.
///   4. Set <see cref="gridWorldSize"/> large enough to cover the entire level.
///   5. Leave <see cref="nodeRadius"/> at 0.25 to match the default cell size of 0.5.
/// </summary>
public class AStarGrid : MonoBehaviour
{
    [Header("Obstacle Detection")]
    [Tooltip("Layer(s) treated as obstacles (assign the Wall layer here).")]
    public LayerMask unwalkableMask;

    [Tooltip("Layer assigned to gate objects. Closed gates on this layer block enemies; open gates (disabled collider) do not.")]
    public LayerMask gateObstacleMask;

    [Header("Grid Dimensions")]
    [Tooltip("World-space size of the scanned area. Must cover the entire level.")]
    public Vector2 gridWorldSize = new Vector2(20f, 20f);

    [Tooltip("Half the size of one grid cell. Use 0.25 for the default cell size of 0.5.")]
    public float nodeRadius = 0.25f;

    public static AStarGrid Instance { get; private set; }

    /// <summary>True after <see cref="BuildGrid"/> has completed at least once.</summary>
    public bool IsReady => grid != null;

    Node[,] grid;
    float   nodeDiameter;
    int     gridSizeX;
    int     gridSizeY;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    IEnumerator Start()
    {
        // Wait one frame so LevelGenerator.Start() can spawn all wall objects first.
        yield return null;
        BuildGrid();
    }

    // ── Grid Construction ─────────────────────────────────────────────────────

    /// <summary>
    /// Scans the world and marks each node as walkable or not.
    /// Called automatically after the first frame, but can also be called manually
    /// (e.g. after dynamically adding/removing walls).
    /// </summary>
    public void BuildGrid()
    {
        nodeDiameter = nodeRadius * 2f;
        gridSizeX    = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY    = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        grid = new Node[gridSizeX, gridSizeY];

        Vector2 bottomLeft = (Vector2)transform.position
            - Vector2.right * gridWorldSize.x * 0.5f
            - Vector2.up    * gridWorldSize.y * 0.5f;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = bottomLeft
                    + Vector2.right * (x * nodeDiameter + nodeRadius)
                    + Vector2.up    * (y * nodeDiameter + nodeRadius);

                bool walkable = Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.9f, unwalkableMask | gateObstacleMask) == null;
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    // ── Public Queries ────────────────────────────────────────────────────────

    /// <summary>Returns the grid node closest to the given world position.</summary>
    public Node NodeFromWorldPoint(Vector2 worldPos)
    {
        if (grid == null)
        {
            Debug.LogWarning("AStarGrid: grid not yet built. Call BuildGrid() first.");
            return null;
        }

        Vector2 bottomLeft = (Vector2)transform.position
            - Vector2.right * gridWorldSize.x * 0.5f
            - Vector2.up    * gridWorldSize.y * 0.5f;

        float pctX = Mathf.Clamp01((worldPos.x - bottomLeft.x) / gridWorldSize.x);
        float pctY = Mathf.Clamp01((worldPos.y - bottomLeft.y) / gridWorldSize.y);

        int x = Mathf.RoundToInt((gridSizeX - 1) * pctX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * pctY);
        return grid[x, y];
    }

    /// <summary>Returns all valid 8-directional neighbours of the given node.</summary>
    public List<Node> GetNeighbors(Node node)
    {
        var neighbors = new List<Node>(8);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int cx = node.gridX + dx;
                int cy = node.gridY + dy;

                if (cx >= 0 && cx < gridSizeX && cy >= 0 && cy < gridSizeY)
                    neighbors.Add(grid[cx, cy]);
            }
        }

        return neighbors;
    }

    // ── Debug Visualization ───────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position,
            new Vector3(gridWorldSize.x, gridWorldSize.y, 0.1f));

        if (grid == null) return;

        float cellSize = nodeDiameter * 0.88f;
        foreach (Node n in grid)
        {
            Gizmos.color = n.walkable
                ? new Color(1f, 1f, 1f, 0.08f)
                : new Color(1f, 0f, 0f, 0.30f);

            Gizmos.DrawCube(
                new Vector3(n.worldPosition.x, n.worldPosition.y, 0f),
                new Vector3(cellSize, cellSize, 0.01f));
        }
    }
}
