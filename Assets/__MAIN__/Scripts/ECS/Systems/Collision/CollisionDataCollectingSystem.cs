using System.Collections.Generic;
using COL.UnityGameWheels.Unity;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(CollisionSystemGroup))]
[AlwaysUpdateSystem]
public class CollisionDataCollectingSystem : JobComponentSystem
{
    private CollisionDataComparer m_CollisionDataComparer;
    private BuildPhysicsWorld m_BuildPhysicsWorld;
    private StepPhysicsWorld m_StepPhysicsWorld;
    private int m_FrameCounter;

    struct TriggerEventsJob : ITriggerEventsJob
    {
        public NativeList<CollisionData> PlayerWithGroundCollisionDatas;
        public NativeList<CollisionData> PlayerWithEnemyCollisionDatas;
        public NativeList<CollisionData> PlayerWithItemCollisionDatas;
        public NativeList<CollisionData> SimpleMovingObjectWithGroundCollisionDatas;
        public NativeList<CollisionData> EnemyAndDeadEnemyCollisionDatas;
        public NativeList<CollisionData> PlayerBulletWithGroundCollisionDatas;
        public NativeList<CollisionData> PlayerBulletWithEnemyCollisionDatas;


        [ReadOnly]
        public ComponentDataFromEntity<PlayerTag> PlayerTagEntities;

        [ReadOnly]
        public ComponentDataFromEntity<GroundTag> GroundTagEntities;

        [ReadOnly]
        public ComponentDataFromEntity<CompositeScale> CompositeScaleEntities;

        [ReadOnly]
        public ComponentDataFromEntity<Scale> ScaleEntities;

        [ReadOnly]
        public ComponentDataFromEntity<Translation> TranslationEntities;

        [ReadOnly]
        public ComponentDataFromEntity<PhysicsCollider> ColliderEntities;

        [ReadOnly]
        public ComponentDataFromEntity<SimpleMovementAbilityData> SimpleMovementAbilityEntities;

        [ReadOnly]
        public ComponentDataFromEntity<EnemyTag> EnemyTagEntities;

        [ReadOnly]
        public ComponentDataFromEntity<DeadEnemyTag> DeadEnemyTagEntities;

        [ReadOnly]
        public ComponentDataFromEntity<PlayerBulletTag> PlayerBulletTagEntities;

        [ReadOnly]
        public ComponentDataFromEntity<ItemTag> ItemTagEntities;

        public int FrameCounter;

        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.Entities.EntityA;
            var entityB = triggerEvent.Entities.EntityB;
            var collisionData =
                PhysicsUtility.CalculateCollisionData(entityA, entityB, CompositeScaleEntities, ScaleEntities, TranslationEntities, ColliderEntities);
            var collisionDataSwapped = PhysicsUtility.SwapCollisionData(collisionData);
            var aIsGround = GroundTagEntities.HasComponent(entityA);
            var bIsGround = GroundTagEntities.HasComponent(entityB);
            if (PlayerTagEntities.HasComponent(entityA) && bIsGround)
            {
                PlayerWithGroundCollisionDatas.Add(collisionData);
            }
            else if (PlayerTagEntities.HasComponent(entityB) && aIsGround)
            {
                PlayerWithGroundCollisionDatas.Add(collisionDataSwapped);
            }
            else if ((EnemyTagEntities.HasComponent(entityA) || DeadEnemyTagEntities.HasComponent(entityA)) &&
                     (EnemyTagEntities.HasComponent(entityB) || DeadEnemyTagEntities.HasComponent(entityB)))
            {
                EnemyAndDeadEnemyCollisionDatas.Add(collisionData);
            }
            else if (PlayerTagEntities.HasComponent(entityA) && EnemyTagEntities.HasComponent(entityB))
            {
                PlayerWithEnemyCollisionDatas.Add(collisionData);
            }
            else if (PlayerTagEntities.HasComponent(entityB) && EnemyTagEntities.HasComponent(entityA))
            {
                PlayerWithEnemyCollisionDatas.Add(collisionDataSwapped);
            }
            else if (PlayerTagEntities.HasComponent(entityA) && ItemTagEntities.HasComponent(entityB))
            {
                PlayerWithItemCollisionDatas.Add(collisionData);
            }
            else if (PlayerTagEntities.HasComponent(entityB) && ItemTagEntities.HasComponent(entityA))
            {
                PlayerWithItemCollisionDatas.Add(collisionDataSwapped);
            }
            else if (PlayerBulletTagEntities.HasComponent(entityA) && GroundTagEntities.HasComponent(entityB))
            {
                PlayerBulletWithGroundCollisionDatas.Add(collisionData);
            }
            else if (PlayerBulletTagEntities.HasComponent(entityB) && GroundTagEntities.HasComponent(entityA))
            {
                PlayerBulletWithGroundCollisionDatas.Add(collisionDataSwapped);
            }
            else if (PlayerBulletTagEntities.HasComponent(entityA) && EnemyTagEntities.HasComponent(entityB))
            {
                PlayerBulletWithEnemyCollisionDatas.Add(collisionData);
            }
            else if (PlayerBulletTagEntities.HasComponent(entityB) && EnemyTagEntities.HasComponent(entityA))
            {
                PlayerBulletWithEnemyCollisionDatas.Add(collisionDataSwapped);
            }
            else if (SimpleMovementAbilityEntities.HasComponent(entityA) && bIsGround)
            {
                SimpleMovingObjectWithGroundCollisionDatas.Add(collisionData);
            }
            else if (SimpleMovementAbilityEntities.HasComponent(entityB) && aIsGround)
            {
                SimpleMovingObjectWithGroundCollisionDatas.Add(collisionDataSwapped);
            }
        }
    }

    private string GetCollisionDataListKeyForSystem<TSystem>() where TSystem : ComponentSystemBase, ICollisionSolvingSystem
    {
        return World.GetOrCreateSystem<TSystem>().CollisionDataListKey;
    }

    private NativeList<CollisionData> GetCollisionDataListForSystem<TSystem>(SharedDataManager sharedDataManager)
        where TSystem : ComponentSystemBase, ICollisionSolvingSystem
    {
        return sharedDataManager.GetCollisionDataList(GetCollisionDataListKeyForSystem<TSystem>());
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        m_FrameCounter++;
        var sharedDataManager = GameEntry.Instance.SharedData;
        var jobHandle = new TriggerEventsJob
        {
            PlayerWithGroundCollisionDatas = GetCollisionDataListForSystem<PlayerWithGroundCollisionSolvingSystem>(sharedDataManager),
            PlayerWithEnemyCollisionDatas = GetCollisionDataListForSystem<PlayerWithEnemyCollisionSolvingSystem>(sharedDataManager),
            PlayerWithItemCollisionDatas = GetCollisionDataListForSystem<PlayerWithItemCollisionSolvingSystem>(sharedDataManager),
            SimpleMovingObjectWithGroundCollisionDatas = GetCollisionDataListForSystem<SimpleMovingObjectWithGroundCollisionSolvingSystem>(sharedDataManager),
            EnemyAndDeadEnemyCollisionDatas = GetCollisionDataListForSystem<EnemyAndDeadEnemyCollisionSolvingSystem>(sharedDataManager),
            PlayerBulletWithGroundCollisionDatas = GetCollisionDataListForSystem<PlayerBulletWithGroundCollisionSolvingSystem>(sharedDataManager),
            PlayerBulletWithEnemyCollisionDatas = GetCollisionDataListForSystem<PlayerBulletWithEnemyCollisionSolvingSystem>(sharedDataManager),
            PlayerTagEntities = GetComponentDataFromEntity<PlayerTag>(),
            GroundTagEntities = GetComponentDataFromEntity<GroundTag>(),
            CompositeScaleEntities = GetComponentDataFromEntity<CompositeScale>(),
            ScaleEntities = GetComponentDataFromEntity<Scale>(),
            TranslationEntities = GetComponentDataFromEntity<Translation>(),
            ColliderEntities = GetComponentDataFromEntity<PhysicsCollider>(),
            SimpleMovementAbilityEntities = GetComponentDataFromEntity<SimpleMovementAbilityData>(),
            EnemyTagEntities = GetComponentDataFromEntity<EnemyTag>(),
            DeadEnemyTagEntities = GetComponentDataFromEntity<DeadEnemyTag>(),
            ItemTagEntities = GetComponentDataFromEntity<ItemTag>(),
            PlayerBulletTagEntities = GetComponentDataFromEntity<PlayerBulletTag>(),
            FrameCounter = m_FrameCounter,
        }.Schedule(m_StepPhysicsWorld.Simulation, ref m_BuildPhysicsWorld.PhysicsWorld, inputDeps);
        jobHandle.Complete();
        GetCollisionDataListForSystem<SimpleMovingObjectWithGroundCollisionSolvingSystem>(sharedDataManager).Sort(m_CollisionDataComparer);
        GetCollisionDataListForSystem<PlayerBulletWithGroundCollisionSolvingSystem>(sharedDataManager).Sort(m_CollisionDataComparer);
        GetCollisionDataListForSystem<PlayerBulletWithEnemyCollisionSolvingSystem>(sharedDataManager).Sort(m_CollisionDataComparer);
        return jobHandle;
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        m_FrameCounter = 0;
        m_CollisionDataComparer = new CollisionDataComparer();
    }

    private class CollisionDataComparer : IComparer<CollisionData>
    {
        public int Compare(CollisionData x, CollisionData y)
        {
            if (x.EntityA == y.EntityA)
            {
                return 0;
            }

            var aIndexCompareResult = x.EntityA.Index.CompareTo(y.EntityA.Index);
            return aIndexCompareResult != 0 ? aIndexCompareResult : x.EntityA.Version.CompareTo(y.EntityA.Version);
        }
    }
}