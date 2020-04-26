using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(PlayerMovementSystem))]
[UpdateBefore(typeof(TranslationSystem))]
public class EnemyInitializationSystem : JobComponentSystem
{
    private EntityQuery m_PlayerQuery;

    protected override void OnCreate()
    {
        m_PlayerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<Translation>());
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (m_PlayerQuery.CalculateEntityCount() == 0)
        {
            return default;
        }

        var playerEntity = m_PlayerQuery.GetSingletonEntity();
        float3 playerPosition = GetComponentDataFromEntity<Translation>(true)[playerEntity].Value;
        return Entities
            .ForEach((ref MovementData movementData,
                ref EnemyTag enemyTag,
                in SimpleMovementAbilityData movementAbilityData,
                in Translation translation) =>
            {
                if (enemyTag.Inited)
                {
                    return;
                }

                enemyTag.Inited = true;
                var sign = math.sign(playerPosition.x - translation.Value.x);
                if (math.abs(sign) < float.Epsilon)
                {
                    sign = -1;
                }

                var vel = movementData.Velocity;
                vel.x = sign * movementAbilityData.HorizontalSpeed;
                movementData.Velocity = vel;
            }).Schedule(inputDeps);
    }
}