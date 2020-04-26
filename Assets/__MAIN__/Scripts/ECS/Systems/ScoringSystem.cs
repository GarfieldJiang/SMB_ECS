using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public class ScoringSystem : ComponentSystem
{
    private NativeQueue<EnemyAttackScoreData> m_EnemyAttackScoreDataQueue;

    protected override void OnCreate()
    {
        m_EnemyAttackScoreDataQueue = GameEntry.Instance.SharedData.EnemyAttackScoringQueue;
    }

    protected override void OnUpdate()
    {
        var playerDataManager = GameEntry.Instance.PlayerData;

        // TODO: More complicated scoring logic
        while (m_EnemyAttackScoreDataQueue.TryDequeue(out var enemyAttackScoreData))
        {
            playerDataManager.AddScores(enemyAttackScoreData.BaseScore, enemyAttackScoreData.WorldPosition, AddScoreType.Enemy);
        }
    }
}