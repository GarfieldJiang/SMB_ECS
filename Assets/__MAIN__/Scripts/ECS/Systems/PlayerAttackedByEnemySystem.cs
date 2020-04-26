using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(CollisionSystemGroup))]
[UpdateBefore(typeof(PlayerMotionStatusSystem))]
public class PlayerAttackedByEnemySystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    private struct PlayerConsequence
    {
        public bool LevelDownOrDead;
        public PlayerLevel NewLevel;
        public float3 DeadPosition;
    }

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var deadPlayerGenerator = GetEntityQuery(typeof(DeadPlayerCharacterGeneratorData)).GetSingletonEntity();
        var deadPlayerGeneratorData = GetComponentDataFromEntity<DeadPlayerCharacterGeneratorData>()[deadPlayerGenerator];
        var consequenceQueue = new NativeQueue<PlayerConsequence>(Allocator.Temp);
        var consequenceQueueWriter = consequenceQueue.AsParallelWriter();
        var jobHandle = Entities
            .WithAll<PlayerTag>()
            .ForEach((int entityInQueryIndex, Entity playerEntity,
                ref PlayerAttackedByEnemyData attackData,
                ref PlayerStates playerStates,
                ref MovementData movementData,
                in Translation translation) =>
            {
                if (attackData.EnemyAttackType == EnemyAttackType.None)
                {
                    return;
                }

                if (playerStates.Main == PlayerMainStatus.Dying || PlayerUtility.IsLevelDownProtection(playerStates))
                {
                    attackData.EnemyAttackType = EnemyAttackType.None;
                    return;
                }

                attackData.EnemyAttackType = EnemyAttackType.None;
                if (playerStates.Level != PlayerLevel.Default)
                {
                    playerStates.NextLevel = PlayerLevel.Default;
                    consequenceQueueWriter.Enqueue(new PlayerConsequence {LevelDownOrDead = true, NewLevel = playerStates.NextLevel});
                }
                else
                {
                    consequenceQueueWriter.Enqueue(new PlayerConsequence {LevelDownOrDead = false, DeadPosition = translation.Value});
                    commandBuffer.DestroyEntity(entityInQueryIndex, playerEntity);
                }
            }).Schedule(inputDeps);
        jobHandle.Complete();

        if (consequenceQueue.TryDequeue(out var playerConsequence))
        {
            if (playerConsequence.LevelDownOrDead)
            {
                var eventArgs = GameEntry.Instance.RefPool.GetOrAdd<PlayerLevelChangeEventArgs>().Acquire();
                eventArgs.Level = playerConsequence.NewLevel;
                GameEntry.Instance.Event.SendEvent(this, eventArgs);
            }
            else
            {
                var position = playerConsequence.DeadPosition;
                var deadPlayerEntity = EntityManager.Instantiate(deadPlayerGeneratorData.PrefabEntity);
                var deadPlayerTranslation = GetComponentDataFromEntity<Translation>()[deadPlayerEntity];
                deadPlayerTranslation.Value.x = position.x;
                deadPlayerTranslation.Value.y = position.y;
                EntityManager.SetComponentData(deadPlayerEntity, deadPlayerTranslation);
                GameEntry.Instance.PlayerData.Die();
            }
        }

        consequenceQueue.Dispose();
        return jobHandle;
    }
}