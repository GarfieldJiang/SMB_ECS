using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(CollisionSystemGroup))]
public class ItemBearingSystem : JobComponentSystem
{
    private Random m_Random;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_Random = new Random((uint)(System.DateTime.Now.Ticks % uint.MaxValue));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var gravityAcc = GameEntry.Instance.Config.Global.Item.ItemGravityAcc;
        var deltaTime = TimeUtility.GetLimitedDeltaTime();
        var random = m_Random;
        var jobHandle = Entities.WithoutBurst().ForEach((
            ref Translation translation,
            ref SimpleMovementAbilityData movementAbilityData,
            ref GravityAbility gravityAbility,
            ref MovementData movementData,
            ref ItemStates states,
            in ItemTag itemTag) =>
        {
            if (states.Status != ItemStatus.BeingBorn) return;
            var moveAmount = states.UpSpeed * deltaTime;
            if (moveAmount > states.UpToGo)
            {
                states.Status = ItemStatus.Normal;
                moveAmount = states.UpToGo;
                gravityAbility.GravityAcc = gravityAcc;
                movementAbilityData.InteractsWithGround = true;
                movementData.Velocity.x = movementAbilityData.HorizontalSpeed;
                movementData.Velocity.y = random.NextFloat(itemTag.InitialMinVerticalSpeed, itemTag.InitialMaxVerticalSpeed);
            }

            states.UpToGo -= moveAmount;

            var position = translation.Value;
            position.y += moveAmount;
            translation.Value = position;
        }).Schedule(inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }
}