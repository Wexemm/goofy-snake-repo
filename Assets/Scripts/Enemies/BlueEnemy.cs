using UnityEngine;

/// <summary>
/// Exclusively tracks and damages the Blue player using A* pathfinding.
/// The Red player is completely ignored — neither chased nor harmed.
///
/// Attach to an enemy prefab alongside a Rigidbody2D and CircleCollider2D.
/// Tip: give this prefab a blue tint on its SpriteRenderer for clarity.
/// </summary>
public class BlueEnemy : EnemyBase
{
    protected override void RefreshTarget()
    {
        target = FindPlayerByColor(PlayerColor.Blue);
    }

    protected override bool CanDamage(PlayerIdentity player)
        => player.playerColor == PlayerColor.Blue;
}
