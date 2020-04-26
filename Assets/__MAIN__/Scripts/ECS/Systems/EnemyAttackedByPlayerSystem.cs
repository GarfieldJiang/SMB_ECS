using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(CollisionSystemGroup))]
public partial class EnemyAttackedByPlayerSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;
    private EntityQuery m_PlayerQuery;
    private NativeQueue<AttackedEnemySpawningData> m_AttackedEnemySpawningQueue;
    private Dictionary<EnemyType, StrategyBase> m_Strategies;

    private struct AttackedEnemySpawningData
    {
        public Entity PrefabEntity;
        public float3 Position;
        public bool AttackedIsFromRight;
    }

    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_PlayerQuery = GetEntityQuery(typeof(PlayerTag));
        m_AttackedEnemySpawningQueue = new NativeQueue<AttackedEnemySpawningData>(Allocator.Persistent);
        m_Strategies = new Dictionary<EnemyType, StrategyBase>
        {
            {EnemyType.Goomba, new StrategyGoomba()},
            {EnemyType.KoopaTroopa, new StrategyKoopaTroopa()},
            {EnemyType.KoopaTroopaShell, new StrategyKoopaTroopaShell()},
        };
    }

    protected override void OnDestroy()
    {
        m_AttackedEnemySpawningQueue.Dispose();
    }

    private int GetBaseScore(PlayerAttackType attackType, ScoreData scoreData)
    {
        if (!scoreData.UseComplicatedScore)
        {
            return scoreData.BaseScore;
        }

        switch (attackType)
        {
            case PlayerAttackType.Stomping:
            case PlayerAttackType.Kicking:
                return scoreData.StompedScore;
            case PlayerAttackType.FireBall:
            case PlayerAttackType.InvincibleStarman:
            case PlayerAttackType.Shell:
                return scoreData.FireBallHitScore;
        }

        return scoreData.BaseScore;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var playerEntity = Entity.Null;
        var scoringQueue = GameEntry.Instance.SharedData.EnemyAttackScoringQueue;
        var playerBounced = false;
        if (m_PlayerQuery.CalculateEntityCount() > 0)
        {
            playerEntity = m_PlayerQuery.GetSingletonEntity();
        }

        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer();

        Entities.WithoutBurst()
            .ForEach((int entityInQueryIndex, Entity enemyEntity,
                ref EnemyAttackedByPlayerData attackData,
                in ScoreData scoreData,
                in Translation translation,
                in EnemyTag enemyTag) =>
            {
                var strategy = m_Strategies[enemyTag.Type];
                var updateScoringQueue = strategy.GetMethod(attackData.PlayerAttackType)(this, enemyEntity, translation.Value, commandBuffer,
                    m_AttackedEnemySpawningQueue,
                    attackData.IsFromRight);
                playerBounced = attackData.PlayerAttackType == PlayerAttackType.Stomping;

                if (updateScoringQueue)
                {
                    scoringQueue.Enqueue(new EnemyAttackScoreData
                    {
                        AttackType = attackData.PlayerAttackType,
                        BaseScore = GetBaseScore(attackData.PlayerAttackType, scoreData),
                        WorldPosition = translation.Value,
                    });
                }

                attackData.PlayerAttackType = PlayerAttackType.None;
            }).Run();

        if (playerBounced)
        {
            EntityManager.SetComponentData(playerEntity, new PlayerBouncedByEnemyData {IsBounced = true});
        }

        while (m_AttackedEnemySpawningQueue.TryDequeue(out var attackedEnemySpawningData))
        {
            var attackedEnemyEntity = EntityManager.Instantiate(attackedEnemySpawningData.PrefabEntity);
            var originalPosition = GetComponentDataFromEntity<Translation>(true)[attackedEnemyEntity].Value;
            EntityManager.SetComponentData(attackedEnemyEntity, new Translation
            {
                Value = new float3(
                    attackedEnemySpawningData.Position.x, attackedEnemySpawningData.Position.y, originalPosition.z)
            });
            var movementDataEntities = GetComponentDataFromEntity<MovementData>();
            var simpleMovementAbilityEntities = GetComponentDataFromEntity<SimpleMovementAbilityData>(true);
            if (movementDataEntities.HasComponent(attackedEnemyEntity) && simpleMovementAbilityEntities.HasComponent(attackedEnemyEntity))
            {
                var horizontalSpeed = simpleMovementAbilityEntities[attackedEnemyEntity].HorizontalSpeed;
                var movementData = movementDataEntities[attackedEnemyEntity];
                movementData.Velocity.x = math.abs(horizontalSpeed) * (attackedEnemySpawningData.AttackedIsFromRight ? -1 : 1);
                movementDataEntities[attackedEnemyEntity] = movementData;
            }
        }

        return default;
    }
}