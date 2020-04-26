using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateAfter(typeof(TranslationSystem))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public class PlayerLevelSystem : JobComponentSystem
{
    private float m_PlayerLevelChangeTimeConfig;
    private float m_PlayerLevelDownProtectionTimeConfig;

    protected override void OnCreate()
    {
        var playerConfig = GameEntry.Instance.Config.Global.Player;
        m_PlayerLevelChangeTimeConfig = playerConfig.LevelChangeTime;
        m_PlayerLevelDownProtectionTimeConfig = playerConfig.LevelDownProtectionTime;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = TimeUtility.GetLimitedDeltaTime();
        var playerLevelChangeTimeConfig = m_PlayerLevelChangeTimeConfig;
        var playerLevelDownProtectionTimeConfig = m_PlayerLevelDownProtectionTimeConfig;
        var playerConfig = GameEntry.Instance.Config.Global.Player;
        var defaultScale = playerConfig.DefaultScale;
        var superScale = playerConfig.SuperScale;

        var jobHandle = Entities
            .ForEach((ref PlayerStates playerStates, ref Translation translation, ref CompositeScale scaleComponent, ref PhysicsCollider physicsCollider) =>
            {
                if (!PlayerUtility.IsChangingLevel(playerStates))
                {
                    return;
                }

                if (math.abs(playerStates.LevelChangeTimeUsed) <= float.Epsilon)
                {
                    playerStates.LevelDownProtectionTime = 0;
                    var oldScale = MathUtility.MatrixToScale(scaleComponent.Value);
                    var newScale = playerStates.NextLevel == PlayerLevel.Default ? defaultScale : superScale;
                    scaleComponent.Value = MathUtility.ScaleToMatrix(newScale);
                    translation.Value.y += newScale.y / 2 - oldScale.y / 2;
                    PhysicsUtility.SetBoxColliderSize(physicsCollider, newScale);
                }

                if (playerStates.LevelChangeTimeUsed >= playerLevelChangeTimeConfig)
                {
                    playerStates = CompleteLevelChange(playerStates, playerLevelDownProtectionTimeConfig);
                }
                else
                {
                    playerStates.LevelChangeTimeUsed += deltaTime;
                }
            }).Schedule(inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }

    private static PlayerStates CompleteLevelChange(PlayerStates playerStates, float playerLevelDownProtectionTimeConfig)
    {
        if (PlayerUtility.IsLevelDown(playerStates))
        {
            playerStates.LevelDownProtectionTime = playerLevelDownProtectionTimeConfig;
        }

        playerStates.Level = playerStates.NextLevel;
        playerStates.LevelChangeTimeUsed = 0;
        return playerStates;
    }
}