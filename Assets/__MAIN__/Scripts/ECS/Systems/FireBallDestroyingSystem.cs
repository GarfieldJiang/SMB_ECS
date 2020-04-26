using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public class FireBallDestroyingSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBufferConcurrent = m_CommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var cameraDistanceX = GameEntry.Instance.Config.Global.FireBall.DistanceXThresholdFromCameraToDestroy;
        var mainCameraPosition = GameEntry.Instance.MainCameraTransform.position;
        var jobHandle = Entities.WithAll<PlayerBulletTag>().ForEach((Entity entity, int entityInQueryIndex, in Translation translation) =>
        {
            if (math.abs(translation.Value.x - mainCameraPosition.x) > cameraDistanceX)
            {
                commandBufferConcurrent.DestroyEntity(entityInQueryIndex, entity);
            }
        }).Schedule(inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }
}