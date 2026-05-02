using UnityEngine;

/// <summary>
/// Chases whichever player (Red or Blue) is currently closest.
/// Can damage any player it catches.
///
/// Attach to an enemy prefab alongside a Rigidbody2D and CircleCollider2D.
/// </summary>
public class ChaserEnemy : EnemyBase
{
    protected override void RefreshTarget()
    {
        target = FindNearestPlayer();
    }

    // Chaser is colour-blind — it damages everyone.
    protected override bool CanDamage(PlayerIdentity player) => true;
}
