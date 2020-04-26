using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(SystemSwitchSystem))]
public class EntitySpawnSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    struct SpawnData
    {
        public SpawnPointData SpawnPointData;
        public float3 Position;
    }

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var cameraPosition = GameEntry.Instance.MainCameraTransform.position;
        var cameraDistanceX = GameEntry.Instance.Config.Global.MovingEntities.DistanceXThresholdFromCameraToCreate;
        var spawnQueue = new NativeQueue<SpawnData>(Allocator.TempJob);
        var spawnQueueWriter = spawnQueue.AsParallelWriter();
        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var jobHandle = Entities.ForEach((int entityInQueryIndex, Entity entity, in SpawnPointData spawnPointData, in Translation translation) =>
        {
            if (math.abs(translation.Value.x - cameraPosition.x) < cameraDistanceX)
            {
                spawnQueueWriter.Enqueue(new SpawnData {SpawnPointData = spawnPointData, Position = translation.Value});
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }
        }).Schedule(inputDeps);
        jobHandle.Complete();
        while (spawnQueue.TryDequeue(out SpawnData spawnData))
        {
            var newEntity = EntityManager.Instantiate(spawnData.SpawnPointData.PrefabEntity);
            EntityManager.SetComponentData(newEntity, new Translation {Value = spawnData.Position});
        }

        spawnQueue.Dispose();
        return jobHandle;
    }
}