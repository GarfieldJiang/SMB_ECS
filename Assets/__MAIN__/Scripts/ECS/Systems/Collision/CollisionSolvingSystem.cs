using Unity.Collections;
using Unity.Entities;

public abstract partial class CollisionSolvingSystem : ComponentSystem, ICollisionSolvingSystem
{
    protected const float Allowance = .001f;

    public abstract string CollisionDataListKey { get; }

    protected NativeList<CollisionData> m_CollisionDataList;

    protected override void OnCreate()
    {
        base.OnCreate();
        GameEntry.Instance.SharedData.CreateCollisionDataList(CollisionDataListKey);
        m_CollisionDataList = GameEntry.Instance.SharedData.GetCollisionDataList(CollisionDataListKey);
    }

    protected sealed override void OnUpdate()
    {
        SetUpFrame();
        if (m_CollisionDataList.Length > 0)
        {
            ProcessCollisionDataListNonEmpty();
        }

        TearDownFrame();
    }

    protected abstract void ProcessCollisionDataListNonEmpty();

    protected virtual void SetUpFrame()
    {
    }

    protected virtual void TearDownFrame()
    {
    }
}