using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(FireBallDestroyingSystem))]
public class FireBallSpawningSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (GetEntityQuery(typeof(FireBallGeneratorData)).CalculateEntityCount() <= 0)
        {
            return;
        }

        var playerInputData = GetSingleton<PlayerInputData>();
        var playerStates = GetSingleton<PlayerStates>();

        if (!playerInputData.FireHit || playerStates.Level != PlayerLevel.Fire)
        {
            return;
        }

        var fireBallConfig = GameEntry.Instance.Config.Global.FireBall;
        if (GetEntityQuery(typeof(PlayerBulletTag)).CalculateEntityCount() >= fireBallConfig.MaxCount)
        {
            return;
        }

        var playerEntity = GetSingletonEntity<PlayerTag>();
        var playerPosition = GetComponentDataFromEntity<Translation>()[playerEntity].Value;
        var fireBallPrefabEntity = GetSingleton<FireBallGeneratorData>().FireBallPrefabEntity;
        var fireBallEntity = EntityManager.Instantiate(fireBallPrefabEntity);
        var originalPosition = GetComponentDataFromEntity<Translation>()[fireBallEntity].Value;
        EntityManager.SetComponentData(fireBallEntity, new Translation
        {
            Value = new float3(
                playerPosition.x + (playerStates.IsFacingLeft ? -1 : 1) * fireBallConfig.SpawnOffset.x,
                playerPosition.y + fireBallConfig.SpawnOffset.y,
                originalPosition.z),
        });
        EntityManager.SetComponentData(fireBallEntity, new MovementData
        {
            Velocity = new float3(
                (playerStates.IsFacingLeft ? -1 : 1) * fireBallConfig.Speed.x,
                -fireBallConfig.Speed.y,
                0),
        });
        GameEntry.Instance.Event.SendEvent(this, GameEntry.Instance.RefPool.GetOrAdd<FireBallSpawnedEventArgs>().Acquire());
    }
}