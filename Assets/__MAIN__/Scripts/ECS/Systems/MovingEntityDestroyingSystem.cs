using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public class MovingEntityDestroyingSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer();
        var commandBufferConcurrent = commandBuffer.ToConcurrent();
        var movingEntitiesConfig = GameEntry.Instance.Config.Global.MovingEntities;
        var minPositionY = movingEntitiesConfig.MinPositionY;
        var maxPositionX = movingEntitiesConfig.MaxPositionY;
        var cameraDistanceX = movingEntitiesConfig.DistanceXThresholdFromCameraToDestroy;
        var cameraPosition = GameEntry.Instance.MainCameraTransform.position;
        var jobHandle = Entities.ForEach((int entityInQueryIndex, Entity entity, in MovementData movementData, in Translation translation) =>
        {
            var position = translation.Value;
            if (math.abs(cameraPosition.x - position.x) > cameraDistanceX || position.y > maxPositionX || position.y < minPositionY)
            {
                commandBufferConcurrent.DestroyEntity(entityInQueryIndex, entity);
            }
        }).Schedule(inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }
}