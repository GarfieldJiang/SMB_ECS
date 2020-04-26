using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial class EnemyAttackedByPlayerSystem
{
    private class StrategyGoomba : StrategyBase
    {
        private AttackedGoombaGeneratorData GetAttakedEnemyGeneratorData(EnemyAttackedByPlayerSystem system) => system
            .GetEntityQuery(typeof(AttackedGoombaGeneratorData))
            .GetSingleton<AttackedGoombaGeneratorData>();

        protected override bool OnStomped(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight)
        {
            attackedEnemySpawningQueue.Enqueue(new AttackedEnemySpawningData
            {
                PrefabEntity = GetAttakedEnemyGeneratorData(system).StompedGoombaPrefabEntity,
                Position = position,
                AttackedIsFromRight = attackIsFromRight,
            });
            GameEntry.Instance.Event.SendEvent(this, GameEntry.Instance.RefPool.GetOrAdd<PlayerStompingEnemyEventArgs>().Acquire());
            commandBuffer.DestroyEntity(enemyEntity);
            return true;
        }

        protected override bool OnFireBallHit(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight)
        {
            attackedEnemySpawningQueue.Enqueue(new AttackedEnemySpawningData
            {
                PrefabEntity = GetAttakedEnemyGeneratorData(system).DeadGoombaPrefabEntity,
                Position = position,
                AttackedIsFromRight = attackIsFromRight,
            });
            commandBuffer.DestroyEntity(enemyEntity);
            PlayKillSound();
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
            throw new System.NotSupportedException();
        }
    }
}