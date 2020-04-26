using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;

[AlwaysSynchronizeSystem]
public class PlayerLevelDownProtectionSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var playerQuery = GetEntityQuery(typeof(PlayerStates));
        var playerEntity = playerQuery.GetSingletonEntity();
        var playerStates = GetComponentDataFromEntity<PlayerStates>()[playerEntity];
        if (playerStates.Main != PlayerMainStatus.Alive)
        {
            return default;
        }

        if (PlayerUtility.IsLevelDownProtection(playerStates))
        {
            playerStates.LevelDownProtectionTime = math.max(0f, playerStates.LevelDownProtectionTime - TimeUtility.GetLimitedDeltaTime());
            EntityManager.SetComponentData(playerEntity, playerStates);
        }

        return inputDeps;
    }
}