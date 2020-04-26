using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(CollisionSystemGroup))]
public class FireBallHitSystem : ComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        if (GetEntityQuery(typeof(FireBallGeneratorData)).CalculateEntityCount() <= 0)
        {
            return;
        }

        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer();
        var fireBallGeneratorData = GetSingleton<FireBallGeneratorData>();
        while (GameEntry.Instance.SharedData.FireBallsOnHit.TryDequeue(out var fireBallOnHitData))
        {
            var fireBallEntity = fireBallOnHitData.FireBallEntity;
            var position = GetComponentDataFromEntity<Translation>()[fireBallEntity].Value;
            var fireBallHitEntity = EntityManager.Instantiate(fireBallGeneratorData.FireBallHitPrefabEntity);
            var originalPosition = GetComponentDataFromEntity<Translation>()[fireBallHitEntity].Value;
            EntityManager.SetComponentData(fireBallHitEntity, new Translation
            {
                Value = new float3(position.x, position.y, originalPosition.z)
            });
            commandBuffer.DestroyEntity(fireBallEntity);
            if (fireBallOnHitData.IsHittingGround)
            {
                GameEntry.Instance.Audio.PlaySoundEffect("smb_bump");
            }
        }
    }
}