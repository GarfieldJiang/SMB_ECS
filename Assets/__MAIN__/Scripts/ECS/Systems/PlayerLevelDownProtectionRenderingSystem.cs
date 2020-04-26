using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;


[UpdateAfter(typeof(PlayerLevelDownProtectionSystem))]
public class PlayerLevelDownProtectionRenderingSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var playerQuery = GetEntityQuery(typeof(PlayerStates));
        var playerEntity = playerQuery.GetSingletonEntity();
        var playerStates = GetComponentDataFromEntity<PlayerStates>()[playerEntity];
        if (!PlayerUtility.IsLevelDownProtection(playerStates))
        {
            return;
        }

        var deltaTime = TimeUtility.GetLimitedDeltaTime();
        var flashRenderingData = GetComponentDataFromEntity<FlashRenderingData>()[playerEntity];
        flashRenderingData.TimeLeftToSwitchRenderer = math.max(0, flashRenderingData.TimeLeftToSwitchRenderer - deltaTime);
        if (flashRenderingData.TimeLeftToSwitchRenderer <= 0f)
        {
            flashRenderingData.TimeLeftToSwitchRenderer = GameEntry.Instance.Config.Global.PlayerLevelDownProtectionRendering.FlashingPeriod;
            // TODO: This doesn't work right now.
            if (GetComponentDataFromEntity<MaterialColor>().HasComponent(playerEntity))
            {
                EntityManager.RemoveComponent(playerEntity, typeof(MaterialColor));
            }
            else
            {
                EntityManager.AddComponent<MaterialColor>(playerEntity);
            }
        }

        EntityManager.SetComponentData(playerEntity, flashRenderingData);
    }
}