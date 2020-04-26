using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(PlayerInputSystem))]
[AlwaysSynchronizeSystem]
public class PlayerFacingSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entities.WithAll<PlayerTag>().ForEach((ref PlayerStates playerStates, in PlayerInputData inputData) =>
        {
            if (PlayerUtility.IsInAir(playerStates))
            {
                return;
            }

            if (inputData.HorizontalInput > 0)
            {
                playerStates.IsFacingLeft = false;
            }
            else if (inputData.HorizontalInput < 0)
            {
                playerStates.IsFacingLeft = true;
            }
        }).Run();
        return default;
    }
}