using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all enemy types.
/// Handles A* path following, automatic path recalculation, and contact damage.
///
/// Subclasses must implement:
///   <see cref="RefreshTarget"/>  — set <see cref="target"/> to the desired player.
///   <see cref="CanDamage"/>      — return true if this enemy may damage the given player.
///
/// Prefab setup:
///   • Add a Rigidbody2D  (Gravity Scale = 0, Freeze Rotation = true).
///   • Add a CircleCollider2D for physical wall collision (not a trigger).
///   • Add a SpriteRenderer with your enemy sprite.
///   • Assign this (or a subclass) as a component.
///   • Ensure wall objects are on the layer assigned to AStarGrid.unwalkableMask.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("World-units per second.")]
    public float moveSpeed = 3f;

    [Tooltip("Seconds between path recalculations. Lower = more responsive, higher = better performance.")]
    public float pathRecalculateRate = 0.25f;

    [Tooltip("Distance (world units) at which a waypoint is considered reached.")]
    public float waypointTolerance = 0.1f;

    [Header("Combat")]
    [Tooltip("Damage dealt per hit.")]
    public float damage = 20f;

    [Tooltip("Minimum seconds between damage ticks.")]
    public float damageInterval = 1f;

    [Tooltip("Distance at which the enemy can deal damage (should roughly match sprite radius).")]
    public float damageRange = 0.35f;

    [Header("Button / Lever Interaction")]
    [Tooltip("Distance at which the enemy activates buttons and levers.")]
    public float interactRange = 0.4f;

    [Tooltip("A new target only replaces the current one if it is at least this many units closer. Prevents oscillating between two equidistant players.")]
    public float targetSwitchHysteresis = 1.5f;

    [Tooltip("Multiplier on the collision radius used for wall-clearance checks during path shortcutting. " +
             "Values below 1.0 let the enemy cut corners more aggressively but risk clipping walls. " +
             "1.0 = exact collision radius, 1.1 = 10 % safety margin (recommended).")]
    public float cornerClearanceFactor = 1.2f;

    // ── State ─────────────────────────────────────────────────────────────────

    protected Rigidbody2D rb;
    protected Transform   target;

    List<Vector2> path;
    int   pathIndex;
    float pathTimer;
    float damageTimer;
    float losRadius; // cached from CircleCollider2D for string-pulling casts

    readonly HashSet<ColorButton> activeButtons = new();
    readonly HashSet<ColorLever>  activeLevers  = new();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.freezeRotation = true;
        rb.linearDamping = 0f;

        var col = GetComponent<CircleCollider2D>();
        losRadius = col != null ? col.radius * cornerClearanceFactor : 0.18f;
    }

    protected virtual void Start()
    {
        // Only refresh target if grid is ready; otherwise let Update() handle it
        if (AStarGrid.Instance != null && AStarGrid.Instance.IsReady)
        {
            RefreshTarget();
            pathTimer = 0f; // Force immediate path calc on first Update
        }
        else
        {
            pathTimer = 0f; // Still ensure Update() will check immediately once grid is ready
        }
    }

    void Update()
    {
        pathTimer -= Time.deltaTime;
        if (pathTimer <= 0f && AStarGrid.Instance != null && AStarGrid.Instance.IsReady)
        {
            pathTimer = pathRecalculateRate;
            RefreshTarget();
            RecalculatePath();
        }

        HandleDamage();
        HandleInteractables();
    }

    void FixedUpdate()
    {
        FollowPath();
    }

    // ── Abstract interface ────────────────────────────────────────────────────

    /// <summary>
    /// Called every <see cref="pathRecalculateRate"/> seconds.
    /// Implementations should assign <see cref="target"/> to the desired player Transform.
    /// </summary>
    protected abstract void RefreshTarget();

    /// <summary>
    /// Returns true if this enemy is allowed to damage <paramref name="player"/>.
    /// </summary>
    protected abstract bool CanDamage(PlayerIdentity player);

    // ── Pathfinding ───────────────────────────────────────────────────────────

    void RecalculatePath()
    {
        if (target == null) { path = null; return; }
        path      = AStarPathfinder.FindPath(transform.position, target.position);
        pathIndex = 0;
    }

    void FollowPath()
    {
        if (path == null || pathIndex >= path.Count)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 15f);
            return;
        }

        // String-pulling: skip ahead to the furthest waypoint we can reach
        // without hitting a wall, so we don't hug corners unnecessarily.
        LayerMask blockMask = AStarGrid.Instance != null
            ? AStarGrid.Instance.unwalkableMask | AStarGrid.Instance.gateObstacleMask
            : 0;

        while (pathIndex < path.Count - 1)
        {
            Vector2 toNext = path[pathIndex + 1] - (Vector2)transform.position;
            float   dist   = toNext.magnitude;
            if (Physics2D.CircleCast(transform.position, losRadius, toNext / dist, dist, blockMask))
                break; // wall in the way → stay on current waypoint
            pathIndex++;
        }

        // Overshoot check: if the current waypoint is now behind us along the
        // path segment, advance immediately instead of doubling back.
        if (pathIndex > 0 && pathIndex < path.Count)
        {
            Vector2 segDir = (path[pathIndex] - path[pathIndex - 1]).normalized;
            Vector2 toWp   = path[pathIndex] - (Vector2)transform.position;
            if (Vector2.Dot(segDir, toWp) < 0f)
                pathIndex++;
        }

        if (pathIndex >= path.Count)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 15f);
            return;
        }

        Vector2 dir = (path[pathIndex] - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (Vector2.Distance(transform.position, path[pathIndex]) < waypointTolerance)
            pathIndex++;
    }

    // ── Damage ────────────────────────────────────────────────────────────────

    void HandleDamage()
    {
        if (target == null) return;
        if (Vector2.Distance(transform.position, target.position) > damageRange) return;

        damageTimer -= Time.deltaTime;
        if (damageTimer > 0f) return;
        damageTimer = damageInterval;

        PlayerIdentity identity = target.GetComponent<PlayerIdentity>();
        if (identity == null || !CanDamage(identity)) return;

        PlayerHealth health = target.GetComponent<PlayerHealth>();
        health?.TakeDamage(damage);
    }

    // ── Interactables (buttons / levers) ─────────────────────────────────────

    void HandleInteractables()
    {
        // Buttons
        foreach (var btn in FindObjectsByType<ColorButton>(FindObjectsInactive.Exclude))
        {
            bool inRange = Vector2.Distance(transform.position, btn.transform.position) <= interactRange;
            if (inRange && activeButtons.Add(btn))   btn.NotifyEnemyEnter();
            if (!inRange && activeButtons.Remove(btn)) btn.NotifyEnemyExit();
        }

        // Levers
        foreach (var lev in FindObjectsByType<ColorLever>(FindObjectsInactive.Exclude))
        {
            bool inRange = Vector2.Distance(transform.position, lev.transform.position) <= interactRange;
            if (inRange && activeLevers.Add(lev))   lev.NotifyEnemyEnter();
            if (!inRange && activeLevers.Remove(lev)) lev.NotifyEnemyExit();
        }
    }

    void OnDestroy()
    {
        foreach (var btn in activeButtons) btn.NotifyEnemyExit();
        foreach (var lev in activeLevers)  lev.NotifyEnemyExit();
    }

    // ── Shared player finders (used by subclasses) ────────────────────────────

    protected Transform FindNearestPlayer()
    {
        PlayerIdentity[] players = FindObjectsByType<PlayerIdentity>(FindObjectsSortMode.None);
        Transform nearest  = null;
        float     minDist  = float.MaxValue;

        foreach (PlayerIdentity p in players)
        {
            float d = GetPathLength(p.transform.position);
            if (d < minDist) { minDist = d; nearest = p.transform; }
        }

        // Only switch if the new candidate is meaningfully closer along the actual path.
        if (target != null && nearest != target)
        {
            float currentDist = GetPathLength(target.position);
            if (minDist >= currentDist - targetSwitchHysteresis)
                return target;
        }

        return nearest;
    }

    // Returns the total length of the A* path from this enemy to worldPos.
    // Falls back to Euclidean distance if no path exists.
    float GetPathLength(Vector2 worldPos)
    {
        List<Vector2> p = AStarPathfinder.FindPath(transform.position, worldPos);
        if (p == null || p.Count == 0)
            return Vector2.Distance(transform.position, worldPos);

        float total = Vector2.Distance(transform.position, p[0]);
        for (int i = 1; i < p.Count; i++)
            total += Vector2.Distance(p[i - 1], p[i]);
        return total;
    }

    protected Transform FindPlayerByColor(PlayerColor color)
    {
        PlayerIdentity[] players = FindObjectsByType<PlayerIdentity>();
        foreach (PlayerIdentity p in players)
        {
            if (p.playerColor == color) return p.transform;
        }
        return null;
    }
}
