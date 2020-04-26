using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(CollisionSystemGroup))]
[UpdateAfter(typeof(CollisionDataCollectingSystem))]
[UpdateBefore(typeof(CollisionDataClearingSystem))]
public class PlayerWithEnemyCollisionSolvingSystem : CollisionSolvingSystem
{
    public override string CollisionDataListKey => "PlayerWithEnemy";

    private NativeHashMap<Entity, bool> m_LastFramePlayerInteractedEnemies;
    private NativeHashMap<Entity, bool> m_CurrentFramePlayerInteractedEnemies;

    protected override void ProcessCollisionDataListNonEmpty()
    {
        var enemyTagEntities = GetComponentDataFromEntity<EnemyTag>();
        var playerStatesEntities = GetComponentDataFromEntity<PlayerStates>();
        var enemyAttackedByPlayerDataEntities = GetComponentDataFromEntity<EnemyAttackedByPlayerData>();
        var playerAttackedByEnemyDataEntities = GetComponentDataFromEntity<PlayerAttackedByEnemyData>();
        var movementDataEntities = GetComponentDataFromEntity<MovementData>();
        foreach (var collisionData in m_CollisionDataList)
        {
            var enemyTag = enemyTagEntities[collisionData.EntityB];
            var entityPlayer = collisionData.EntityA;
            var entityEnemy = collisionData.EntityB;
            var movementDataEnemy = movementDataEntities.HasComponent(entityEnemy) ? movementDataEntities[entityEnemy] : default;
            if (!m_LastFramePlayerInteractedEnemies.ContainsKey(entityEnemy))
            {
                if (playerStatesEntities[entityPlayer].InvincibleRemainingTime > 0)
                {
                    var attackedData = enemyAttackedByPlayerDataEntities[entityEnemy];
                    attackedData.PlayerAttackType = PlayerAttackType.InvincibleStarman;
                    attackedData.IsFromRight = collisionData.SignX >= 0;
                    enemyAttackedByPlayerDataEntities[entityEnemy] = attackedData;
                }
                else if (collisionData.ColliderBoundsB.Max.y < collisionData.ColliderBoundsA.Center.y
                         && collisionData.OverlapX > .2f * collisionData.ColliderBoundsA.Size.x && EnemyUtility.IsStomptable(enemyTag.Type)
                )
                {
                    var attackedData = enemyAttackedByPlayerDataEntities[entityEnemy];
                    attackedData.PlayerAttackType = PlayerAttackType.Stomping;
                    attackedData.IsFromRight = collisionData.SignX >= 0;
                    enemyAttackedByPlayerDataEntities[entityEnemy] = attackedData;
                }
                else if (EnemyUtility.IsStaticShell(enemyTag.Type, movementDataEnemy))
                {
                    var attackedData = enemyAttackedByPlayerDataEntities[entityEnemy];
                    attackedData.PlayerAttackType = PlayerAttackType.Kicking;
                    attackedData.IsFromRight = collisionData.SignX >= 0;
                    enemyAttackedByPlayerDataEntities[entityEnemy] = attackedData;
                }
                else
                {
                    var attackedData = playerAttackedByEnemyDataEntities[entityPlayer];
                    attackedData.EnemyAttackType = EnemyAttackType.NormalHit;
                    playerAttackedByEnemyDataEntities[entityPlayer] = attackedData;
                }
            }

            m_CurrentFramePlayerInteractedEnemies.TryAdd(entityEnemy, true);
        }
    }

    protected override void TearDownFrame()
    {
        var tmp = m_LastFramePlayerInteractedEnemies;
        m_LastFramePlayerInteractedEnemies = m_CurrentFramePlayerInteractedEnemies;
        m_CurrentFramePlayerInteractedEnemies = tmp;
        m_CurrentFramePlayerInteractedEnemies.Clear();
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        m_LastFramePlayerInteractedEnemies = new NativeHashMap<Entity, bool>(4, Allocator.Persistent);
        m_CurrentFramePlayerInteractedEnemies = new NativeHashMap<Entity, bool>(4, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        m_LastFramePlayerInteractedEnemies.Dispose();
        m_CurrentFramePlayerInteractedEnemies.Dispose();
        base.OnDestroy();
    }
}