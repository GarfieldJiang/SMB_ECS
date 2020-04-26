using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial class EnemyAttackedByPlayerSystem
{
    private class StrategyKoopaTroopaShell : StrategyKoopaTroopaBase
    {
        protected override bool OnStomped(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight)
        {
            var movementData = system.GetComponentDataFromEntity<MovementData>()[enemyEntity];
            var shellData = system.GetComponentDataFromEntity<ShellData>()[enemyEntity];
            if (math.abs(movementData.Velocity.x) > 0)
            {
                movementData.Velocity.x = 0;
                shellData.ReviveTimeUsed = 0;
            }
            else
            {
                movementData.Velocity.x = (attackIsFromRight ? 1 : -1) * shellData.HorizontalSpeedConfig;
                shellData.ReviveTimeUsed = 0;
            }

            system.EntityManager.SetComponentData(enemyEntity, shellData);
            system.EntityManager.SetComponentData(enemyEntity, movementData);
            return true;
        }

        protected override bool OnInvinciblePlayerHit(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight)
        {
            return OnFireBallHit(system, enemyEntity, position, commandBuffer, attackedEnemySpawningQueue, attackIsFromRight);
        }

        protected override bool OnShellHit(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight)
        {
            return OnFireBallHit(system, enemyEntity, position, commandBuffer, attackedEnemySpawningQueue, attackIsFromRight);
        }

        protected override bool OnKicked(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight)
        {
            var movementData = system.GetComponentDataFromEntity<MovementData>()[enemyEntity];
            var shellData = system.GetComponentDataFromEntity<ShellData>()[enemyEntity];
            movementData.Velocity.x = (attackIsFromRight ? -1 : 1) * shellData.HorizontalSpeedConfig;
            shellData.ReviveTimeUsed = 0;

            system.EntityManager.SetComponentData(enemyEntity, shellData);
            system.EntityManager.SetComponentData(enemyEntity, movementData);
            return true;
        }
    }
}