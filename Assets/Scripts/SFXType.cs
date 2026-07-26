// One entry per distinct sound in the game. Add new entries here first,
// then give them clips in the AudioManager's SFX Library in the Inspector.
public enum SFXType
{
    Move,               // player footsteps / running
    Jump,
    Dash,
    JumpPadLaunch,
    Attack,             // player attack
    Collectible,
    TakeDamage,         // player taking damage
    ScaleUp,            // player scales up a stage
    ScaleDown,          // player scales down a stage
    SimpleEnemyMove,    // BasicPatrolEnemy movement
    ChasingEnemyPatrolMove, // AggressiveEnemy movement while patrolling
    ChasingEnemyChaseMove,  // AggressiveEnemy movement while actively chasing
    ChasingEnemyAttack, // AggressiveEnemy attack
    BoxPush,            // Pushable being pushed
    VerticalFan,        // VerticalFan ambient hum
    MovingPlatform ,     // MovingPlatform in motion
    Win,
    Lose
}
