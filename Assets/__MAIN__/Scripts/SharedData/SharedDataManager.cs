using System.Collections.Generic;
using COL.UnityGameWheels.Unity;
using Unity.Collections;
using UnityEngine.Assertions;

/// <summary>
/// Shared data used by different systems. Could be split if there is a lot of them.
/// </summary>
public class SharedDataManager : MonoBehaviourEx
{
    public NativeQueue<EnemyAttackScoreData> EnemyAttackScoringQueue { get; private set; }
    public NativeQueue<FireBallOnHitData> FireBallsOnHit { get; private set; }

    private readonly Dictionary<string, NativeList<CollisionData>> m_CollisionDatasDict = new Dictionary<string, NativeList<CollisionData>>();

    protected override void Awake()
    {
        base.Awake();
        EnemyAttackScoringQueue = new NativeQueue<EnemyAttackScoreData>(Allocator.Persistent);
        FireBallsOnHit = new NativeQueue<FireBallOnHitData>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        foreach (var kv in m_CollisionDatasDict)
        {
            kv.Value.Dispose();
        }

        m_CollisionDatasDict.Clear();

        FireBallsOnHit.Dispose();
        EnemyAttackScoringQueue.Dispose();
        base.OnDestroy();
    }

    public void CreateCollisionDataList(string key, int initCapacity = 16)
    {
        Assert.IsTrue(initCapacity >= 0);
        m_CollisionDatasDict.Add(key, new NativeList<CollisionData>(initCapacity, Allocator.Persistent));
    }

    public NativeList<CollisionData> GetCollisionDataList(string key)
    {
        return m_CollisionDatasDict[key];
    }

    public void ClearAllCollisionDataLists()
    {
        foreach (var kv in m_CollisionDatasDict)
        {
            kv.Value.Clear();
        }
    }
}