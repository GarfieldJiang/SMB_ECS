using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[AlwaysUpdateSystem]
public class SystemSwitchSystem : ComponentSystem
{
    private ComponentSystemBase[] m_PlayerDrivenComponentSystems;

    protected override void OnCreate()
    {
        m_PlayerDrivenComponentSystems = new ComponentSystemBase[]
        {
            World.GetOrCreateSystem<PlayerInputSystem>(),
            World.GetOrCreateSystem<PlayerFacingSystem>(),
            World.GetOrCreateSystem<PlayerMovementSystem>(),
            World.GetOrCreateSystem<GravitySystem>(),
            World.GetOrCreateSystem<CollisionDataCollectingSystem>(),
            World.GetOrCreateSystem<PlayerMotionStatusSystem>(),
            World.GetOrCreateSystem<TranslationSystem>(),
            World.GetOrCreateSystem<CameraControllingSystem>(),
            World.GetOrCreateSystem<PlayerTranslationLimitSystem>(),
            World.GetOrCreateSystem<PlayerLevelDownProtectionSystem>(),
            World.GetOrCreateSystem<PlayerInvincibleStarmanSystem>(),
            World.GetOrCreateSystem<KoopaTroopaShellRevivingSystem>(),
            World.GetOrCreateSystem<PlayerLevelDownProtectionRenderingSystem>(),
            World.GetOrCreateSystem<PlayerWithItemCollisionSolvingSystem>(),
            World.GetOrCreateSystem<PlayerWithEnemyCollisionSolvingSystem>(),
            World.GetOrCreateSystem<PlayerWithGroundCollisionSolvingSystem>(),
            World.GetOrCreateSystem<FireBallSpawningSystem>(),
            World.GetOrCreateSystem<KoopaTroopaShellRevivingSystem>(),
            World.GetExistingSystem<EnemyAttackedByPlayerSystem>(),
            World.GetExistingSystem<PlayerAttackedByEnemySystem>(),
        };
    }

    protected override void OnUpdate()
    {
        var playerQuery = GetEntityQuery(typeof(PlayerTag), typeof(PlayerStates));
        var systemEnabled = playerQuery.CalculateEntityCount() > 0;
        if (systemEnabled)
        {
            var playerStates = playerQuery.GetSingleton<PlayerStates>();
            if (PlayerUtility.IsChangingLevel(playerStates))
            {
                systemEnabled = false;
            }
        }

        foreach (var system in m_PlayerDrivenComponentSystems)
        {
            system.Enabled = systemEnabled;
        }
    }
}