using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(CollisionSystemGroup))]
[UpdateAfter(typeof(CollisionDataCollectingSystem))]
[UpdateBefore(typeof(CollisionDataClearingSystem))]
public class PlayerBulletWithEnemyCollisionSolvingSystem : MultipleEntityACollisionSolvingSystem
{
    public override string CollisionDataListKey => "PlayerBulletWithEnemy";

    private ComponentDataFromEntity<EnemyAttackedByPlayerData> m_EnemyAttackedByPlayerDataEntities;

    protected override void ProcessOneEntityA(Entity entityA, int beg, int end)
    {
        for (int i = beg; i < end; i++)
        {
            var entityB = m_CollisionDataList[i].EntityB;
            m_EnemyAttackedByPlayerDataEntities[entityB] = new EnemyAttackedByPlayerData
            {
                PlayerAttackType = PlayerAttackType.FireBall,
                IsFromRight = m_CollisionDataList[i].SignX > 0,
            };
            GameEntry.Instance.SharedData.FireBallsOnHit.Enqueue(new FireBallOnHitData {FireBallEntity = entityA, IsHittingGround = false});
        }
    }

    protected override void SetUpFrame()
    {
        base.SetUpFrame();
        m_EnemyAttackedByPlayerDataEntities = GetComponentDataFromEntity<EnemyAttackedByPlayerData>();
    }

    protected override void TearDownFrame()
    {
        m_EnemyAttackedByPlayerDataEntities = default;
        base.TearDownFrame();
    }
}