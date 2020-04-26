using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial class EnemyAttackedByPlayerSystem
{
    private abstract class StrategyKoopaTroopaBase : StrategyBase
    {
        protected KoopaTroopaGeneratorData GetEntityGeneratorData(EnemyAttackedByPlayerSystem system) => system
            .GetEntityQuery(typeof(KoopaTroopaGeneratorData))
            .GetSingleton<KoopaTroopaGeneratorData>();

        protected override bool OnFireBallHit(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight)
        {
            attackedEnemySpawningQueue.Enqueue(new AttackedEnemySpawningData
            {
                PrefabEntity = GetEntityGeneratorData(system).DeadPrefabEntity,
                Position = position,
                AttackedIsFromRight = attackIsFromRight,
            });
            GameEntry.Instance.Event.SendEvent(this, GameEntry.Instance.RefPool.GetOrAdd<PlayerStompingEnemyEventArgs>().Acquire());
            commandBuffer.DestroyEntity(enemyEntity);
            PlayKillSound();
            return true;
        }
    }
}