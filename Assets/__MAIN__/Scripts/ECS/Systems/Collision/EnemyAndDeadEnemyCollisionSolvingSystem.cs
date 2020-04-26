using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(CollisionSystemGroup))]
[UpdateAfter(typeof(CollisionDataCollectingSystem))]
[UpdateBefore(typeof(PlayerWithEnemyCollisionSolvingSystem))]
public class EnemyAndDeadEnemyCollisionSolvingSystem : CollisionSolvingSystem
{
    public override string CollisionDataListKey => "EnemyAndDeadEnemy";

    private void AttackWithShell(ComponentDataFromEntity<EnemyAttackedByPlayerData> enemyAttackedByPlayerDataEntities, Entity targetEntity, bool isFromRight)
    {
        var attackedData = enemyAttackedByPlayerDataEntities[targetEntity];
        attackedData.PlayerAttackType = PlayerAttackType.Shell;
        attackedData.IsFromRight = isFromRight;
        enemyAttackedByPlayerDataEntities[targetEntity] = attackedData;
    }

    protected override void ProcessCollisionDataListNonEmpty()
    {
        var simpleMovementAbilityEntities = GetComponentDataFromEntity<SimpleMovementAbilityData>();
        var movementDataEntities = GetComponentDataFromEntity<MovementData>();
        var enemyTagEntities = GetComponentDataFromEntity<EnemyTag>();
        var enemyAttackedByPlayerDataEntities = GetComponentDataFromEntity<EnemyAttackedByPlayerData>();

        foreach (var collisionData in m_CollisionDataList)
        {
            var entityA = collisionData.EntityA;
            var entityB = collisionData.EntityB;
            var movementDataA = movementDataEntities.HasComponent(entityA) ? movementDataEntities[entityA] : default;
            var movementDataB = movementDataEntities.HasComponent(entityB) ? movementDataEntities[entityB] : default;

            bool shellAttack = false;
            if (enemyTagEntities.HasComponent(entityA) && EnemyUtility.IsMovingShell(enemyTagEntities[entityA].Type, movementDataA)
                                                       && enemyTagEntities.HasComponent(entityB))
            {
                shellAttack = true;
                AttackWithShell(enemyAttackedByPlayerDataEntities, entityB, collisionData.SignX >= 0);
            }

            if (enemyTagEntities.HasComponent(entityB) && EnemyUtility.IsMovingShell(enemyTagEntities[entityB].Type, movementDataB)
                                                       && enemyTagEntities.HasComponent(entityB))
            {
                shellAttack = true;
                AttackWithShell(enemyAttackedByPlayerDataEntities, entityA, collisionData.SignX <= 0);
            }

            if (shellAttack)
            {
                continue;
            }

            if (simpleMovementAbilityEntities.HasComponent(entityA))
            {
                movementDataA.Velocity.x = collisionData.SignX * math.abs(movementDataA.Velocity.x);
                movementDataEntities[collisionData.EntityA] = movementDataA;
            }

            if (simpleMovementAbilityEntities.HasComponent(entityB))
            {
                movementDataB.Velocity.x = -collisionData.SignX * math.abs(movementDataB.Velocity.x);
                movementDataEntities[collisionData.EntityB] = movementDataB;
            }
        }
    }
}