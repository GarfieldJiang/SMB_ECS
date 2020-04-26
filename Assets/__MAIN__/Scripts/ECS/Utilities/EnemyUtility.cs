using Unity.Mathematics;

public static class EnemyUtility
{
    /// <summary>
    /// Check whether an enemy can be stomped by the player.
    /// </summary>
    /// <returns></returns>
    public static bool IsStomptable(EnemyType enemyType) => enemyType == EnemyType.Goomba || enemyType == EnemyType.KoopaTroopa;

    public static bool IsStaticShell(EnemyType enemyType, MovementData movementData)
    {
        return enemyType == EnemyType.KoopaTroopaShell && math.abs(movementData.Velocity.x) < float.Epsilon;
    }

    public static bool IsMovingShell(EnemyType enemyType, MovementData movementData)
    {
        return enemyType == EnemyType.KoopaTroopaShell && math.abs(movementData.Velocity.x) > 0;
    }
}