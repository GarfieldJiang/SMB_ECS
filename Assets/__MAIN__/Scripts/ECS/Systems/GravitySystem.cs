using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(TranslationSystem))]
public class GravitySystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = TimeUtility.GetLimitedDeltaTime();
        var verticalSpeedLimit = GameEntry.Instance.Config.Global.Gravity.VerticalSpeedLimit;
        var jobHandle = Entities.ForEach((ref MovementData movementData, in GravityAbility gravityAbility) =>
        {
            var vel = movementData.Velocity;
            vel.y -= gravityAbility.GravityAcc * deltaTime;
            if (vel.y < -verticalSpeedLimit)
            {
                vel.y = -verticalSpeedLimit;
            }

            movementData.Velocity = vel;
        }).Schedule(inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }
}