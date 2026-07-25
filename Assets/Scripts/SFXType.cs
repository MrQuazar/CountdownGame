public enum SFXType
{
    Move,               // player footsteps / running
    Jump,
    Dash,
    JumpPadLaunch,
    Attack,             // player attack
    Collectible,
    TakeDamage,         // player taking damage
    SimpleEnemyMove,    // BasicPatrolEnemy movement
    ChasingEnemyMove,   // AggressiveEnemy movement (patrol + chase)
    ChasingEnemyAttack, // AggressiveEnemy attack
    BoxPush,            // Pushable being pushed
    VerticalFan,        // VerticalFan ambient hum
    MovingPlatform      // MovingPlatform in motion
}
