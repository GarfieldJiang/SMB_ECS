using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class KoopaTroopaShellRevivingSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = TimeUtility.GetLimitedDeltaTime();
        var commandBufferConcurrent = m_CommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var koopaTroopaGeneratorData = GetEntityQuery(typeof(KoopaTroopaGeneratorData))
            .GetSingleton<KoopaTroopaGeneratorData>();
        var spawningQueue = new NativeQueue<float3>(Allocator.TempJob);
        var spawningQueueWriter = spawningQueue.AsParallelWriter();
        var normalPrefabEntity = koopaTroopaGeneratorData.NormalPrefabEntity;
        var normalPrefabEntityScale = MathUtility.MatrixToScale(GetComponentDataFromEntity<CompositeScale>()[normalPrefabEntity].Value);
        var jobHandle = Entities.WithoutBurst().ForEach((Entity entity, int entityInQueryIndex, ref ShellData shellData, in EnemyTag enemyTag,
            in MovementData movementData,
            in Translation translation, in CompositeScale scaleComponent) =>
        {
            if (enemyTag.Type != EnemyType.KoopaTroopaShell)
            {
                return;
            }

            var lastIsMoving = shellData.IsMoving;
            shellData.IsMoving = EnemyUtility.IsMovingShell(enemyTag.Type, movementData);
            if (!lastIsMoving && shellData.IsMoving)
            {
                shellData.ReviveTimeUsed = 0;
            }

            if (lastIsMoving && !shellData.IsMoving)
            {
                shellData.ReviveTimeUsed = 0;
            }

            if (shellData.IsMoving)
            {
                return;
            }

            shellData.ReviveTimeUsed += deltaTime;
            if (shellData.ReviveTimeUsed >= shellData.ReviveTimeConfig)
            {
                commandBufferConcurrent.DestroyEntity(entityInQueryIndex, entity);
                spawningQueueWriter.Enqueue(translation.Value +
                                            new float3(0, normalPrefabEntityScale.y / 2 - MathUtility.MatrixToScale(scaleComponent.Value).y / 2, 0));
            }
        }).Schedule(inputDeps);
        jobHandle.Complete();

        while (spawningQueue.TryDequeue(out float3 position))
        {
            var entity = EntityManager.Instantiate(koopaTroopaGeneratorData.NormalPrefabEntity);
            EntityManager.SetComponentData(entity, new Translation {Value = position});
        }

        spawningQueue.Dispose();
        return jobHandle;
    }
}