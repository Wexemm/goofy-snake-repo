using UnityEngine;

/// <summary>
/// Exclusively tracks and damages the Red player using A* pathfinding.
/// The Blue player is completely ignored — neither chased nor harmed.
///
/// Attach to an enemy prefab alongside a Rigidbody2D and CircleCollider2D.
/// Tip: give this prefab a red tint on its SpriteRenderer for clarity.
/// </summary>
public class RedEnemy : EnemyBase
{
    protected override void RefreshTarget()
    {
        target = FindPlayerByColor(PlayerColor.Red);
    }

    protected override bool CanDamage(PlayerIdentity player)
        => player.playerColor == PlayerColor.Red;
}
