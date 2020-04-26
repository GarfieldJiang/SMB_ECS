using Unity.Entities;
using Unity.Jobs;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateBefore(typeof(CollisionSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public class TranslationSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = TimeUtility.GetLimitedDeltaTime();

        var jobHandle = Entities.ForEach((Entity entity, ref Translation translation, ref MovementData movementData) =>
            {
                movementData.LastPosition = translation.Value;
                translation.Value += movementData.Velocity * deltaTime;
            })
            .Schedule(inputDeps);
        jobHandle.Complete();

        return default;
    }
}