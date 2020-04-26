using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateAfter(typeof(EnemyInitializationSystem))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
[UpdateBefore(typeof(StepPhysicsWorld))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
public class HeadedBlockMovingSystem : JobComponentSystem
{
    private float m_VerticalRange;
    private float m_VerticalSpeed;

    protected override void OnCreate()
    {
        m_VerticalRange = GameEntry.Instance.Config.Global.MovableBlock.VerticalRange;
        m_VerticalSpeed = GameEntry.Instance.Config.Global.MovableBlock.VerticalSpeed;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var verticalRange = m_VerticalRange;
        var verticalSpeed = m_VerticalSpeed;
        var deltaTime = TimeUtility.GetLimitedDeltaTime();
        return Entities
            .ForEach((ref MovableBlockStates movableBlockStates, ref Translation translation) =>
            {
                if (movableBlockStates.Status == MovableBlockStatus.None)
                {
                    return;
                }

                switch (movableBlockStates.Status)
                {
                    case MovableBlockStatus.MovingUp:
                    {
                        var position = translation.Value + deltaTime * new float3(0, verticalSpeed, 0);
                        if (position.y - movableBlockStates.OriginalY > verticalRange)
                        {
                            position.y = movableBlockStates.OriginalY + verticalRange;
                            movableBlockStates.Status = MovableBlockStatus.MovingDown;
                        }

                        translation.Value = position;
                        break;
                    }
                    case MovableBlockStatus.MovingDown:
                    {
                        var position = translation.Value - deltaTime * new float3(0, verticalSpeed, 0);
                        if (position.y < movableBlockStates.OriginalY)
                        {
                            position.y = movableBlockStates.OriginalY;
                            movableBlockStates.Status = MovableBlockStatus.None;
                        }

                        translation.Value = position;
                        break;
                    }
                }
            }).Schedule(inputDeps);
    }
}