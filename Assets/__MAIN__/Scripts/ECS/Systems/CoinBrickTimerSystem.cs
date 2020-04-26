using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(HeadableBlockTriggerSystem))]
public class CoinBrickTimerSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = TimeUtility.GetLimitedDeltaTime();
        return Entities.ForEach((ref CoinBrickStates states, in CoinBrickConfigData config) =>
        {
            if (!states.TimerStarted || states.CoinLeft <= 1)
            {
                return;
            }

            states.TimeElapsed += deltaTime;
            if (states.TimeElapsed < config.CoinDecrementInterval)
            {
                return;
            }

            states.TimeElapsed = 0;
            states.CoinLeft -= 1;
        }).Schedule(inputDeps);
    }
}