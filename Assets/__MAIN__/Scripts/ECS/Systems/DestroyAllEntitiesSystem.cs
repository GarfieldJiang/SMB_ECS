using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public class DestroyAllEntitiesSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        Enabled = false;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Enabled = false;
        var commandBufferConcurrent = m_CommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var jobHandle = Entities.ForEach((Entity entity, int entityInQueryIndex) => { commandBufferConcurrent.DestroyEntity(entityInQueryIndex, entity); })
            .Schedule(inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }
}