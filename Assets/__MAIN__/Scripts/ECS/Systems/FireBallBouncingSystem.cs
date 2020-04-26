using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(TransformSystemGroup))]
public class FireBallBouncingSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var bounceHeight = GameEntry.Instance.Config.Global.FireBall.BounceHeight;
        var jobHandle = Entities.WithAll<PlayerBulletTag>()
            .ForEach((ref FireBallStates fireBallStates, ref MovementData movementData, in Translation translation) =>
            {
                if (translation.Value.y - movementData.LastPosition.y > 0)
                {
                    fireBallStates.BouncedHeight += translation.Value.y - movementData.LastPosition.y;

                    if (fireBallStates.BouncedHeight > bounceHeight)
                    {
                        movementData.Velocity.y = -math.abs(movementData.Velocity.y);
                    }
                }
                else
                {
                    fireBallStates.BouncedHeight = 0;
                }
            }).Schedule(inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }
}