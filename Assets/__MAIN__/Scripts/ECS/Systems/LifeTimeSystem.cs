using Unity.Entities;
using Unity.Jobs;

public class LifeTimeSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var deltaTime = TimeUtility.GetLimitedDeltaTime();
        var jobHandle = Entities.ForEach((int entityInQueryIndex, Entity entity, ref LifeTime lifeTime) =>
        {
            lifeTime.RemainingLifeTime -= deltaTime;
            if (lifeTime.RemainingLifeTime <= 0)
            {
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }
        }).Schedule(inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }
}