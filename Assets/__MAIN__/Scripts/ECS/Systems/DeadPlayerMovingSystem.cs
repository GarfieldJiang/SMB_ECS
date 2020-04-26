using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class DeadPlayerMovingSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;
    private EntityQuery m_DeadPlayerQuery;

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_DeadPlayerQuery = GetEntityQuery(typeof(DeadPlayerTag));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (m_DeadPlayerQuery.CalculateEntityCount() == 0)
        {
            return default;
        }

        var deadPlayer = GetSingletonEntity<DeadPlayerTag>();
        var deltaTime = TimeUtility.GetLimitedDeltaTime();
        var movingAbilityData = GetComponentDataFromEntity<DeadPlayerMovingAbilityData>()[deadPlayer];
        var movingStates = GetComponentDataFromEntity<DeadPlayerMovingStates>()[deadPlayer];
        var translation = GetComponentDataFromEntity<Translation>()[deadPlayer];
        var movingAmount = deltaTime * movingAbilityData.Speed;
        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer();
        switch (movingStates.Status)
        {
            case DeadPlayerMovingStatus.Init:
                movingStates.InitTime += deltaTime;
                if (movingStates.InitTime > movingAbilityData.InitDuration)
                {
                    movingStates.Status = DeadPlayerMovingStatus.GoingUp;
                }

                break;
            case DeadPlayerMovingStatus.GoingUp:
                movingStates.UpDistanceCovered += movingAmount;
                if (movingStates.UpDistanceCovered > movingAbilityData.UpDistance)
                {
                    movingAmount -= movingStates.UpDistanceCovered - movingAbilityData.UpDistance;
                    movingStates.Status = DeadPlayerMovingStatus.StayingAtSummit;
                }

                translation.Value.y += movingAmount;
                break;
            case DeadPlayerMovingStatus.StayingAtSummit:
                movingStates.StayAtSummitTime += deltaTime;
                if (movingStates.StayAtSummitTime > movingAbilityData.StayAtSummitDuration)
                {
                    movingStates.Status = DeadPlayerMovingStatus.GoingDown;
                }
                break;
            case DeadPlayerMovingStatus.GoingDown:
                movingStates.DownDistanceCovered += movingAmount;
                if (movingStates.DownDistanceCovered > movingAbilityData.DownDistance)
                {
                    commandBuffer.DestroyEntity(deadPlayer);
                }

                translation.Value.y -= movingAmount;
                break;
        }

        EntityManager.SetComponentData(deadPlayer, movingStates);
        EntityManager.SetComponentData(deadPlayer, translation);
        return inputDeps;
    }
}