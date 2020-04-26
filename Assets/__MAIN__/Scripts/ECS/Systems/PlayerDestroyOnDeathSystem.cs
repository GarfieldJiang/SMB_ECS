using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(PlayerFallingToDeathSystem))]
[UpdateAfter(typeof(PlayerAttackedByEnemySystem))]
[AlwaysSynchronizeSystem]
public class PlayerDestroyOnDeathSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var playerQuery = GetEntityQuery(typeof(PlayerTag));
        if (playerQuery.CalculateEntityCount() == 0)
        {
            return default;
        }

        var playerEntity = playerQuery.GetSingletonEntity();
        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer();
        var playerStates = GetComponentDataFromEntity<PlayerStates>()[playerEntity];
        if (playerStates.Main == PlayerMainStatus.Dying)
        {
            commandBuffer.DestroyEntity(playerEntity);
        }

        return inputDeps;
    }
}