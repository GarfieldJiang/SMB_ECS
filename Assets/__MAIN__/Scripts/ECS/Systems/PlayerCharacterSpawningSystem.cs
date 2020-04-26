using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(SystemSwitchSystem))]
public class PlayerCharacterSpawningSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        Enabled = false;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (GetEntityQuery(typeof(PlayerTag)).CalculateEntityCount() > 0)
        {
            Enabled = false;
            return inputDeps;
        }

        var generatorEntityQuery = GetEntityQuery(typeof(PlayerCharacterGeneratorData));
        if (generatorEntityQuery.CalculateEntityCount() == 0)
        {
            return default;
        }

        var generatorData = generatorEntityQuery.GetSingleton<PlayerCharacterGeneratorData>();
        var playerEntity = EntityManager.Instantiate(generatorData.PrefabEntity);

        //Log.Info("Spawning a player character.");

        var playerStates = GetComponentDataFromEntity<PlayerStates>()[playerEntity];

        var playerDataManager = GameEntry.Instance.PlayerData;
        var playerConfig = GameEntry.Instance.Config.Global.Player;
        playerStates.Level = playerStates.NextLevel = playerDataManager.Level;
        var scale = playerStates.Level == PlayerLevel.Default
            ? playerConfig.DefaultScale
            : playerConfig.SuperScale;
        var physicsCollider = GetComponentDataFromEntity<PhysicsCollider>()[playerEntity];
        PhysicsUtility.SetBoxColliderSize(physicsCollider, scale);

        EntityManager.SetComponentData(playerEntity, new Translation
        {
            Value = GameEntry.Instance.Scene.SceneData.PlayerSpawningPosition,
        });
        EntityManager.SetComponentData(playerEntity, new CompositeScale
        {
            Value = MathUtility.ScaleToMatrix(scale),
        });
        EntityManager.SetComponentData(playerEntity, playerStates);
        Enabled = false;
        GameEntry.Instance.Event.SendEvent(this, GameEntry.Instance.RefPool.GetOrAdd<PlayerCharacterCreatedEventArgs>().Acquire());
        return inputDeps;
    }
}