using Unity.Entities;

public abstract class MultipleEntityACollisionSolvingSystem : CollisionSolvingSystem
{
    protected sealed override void ProcessCollisionDataListNonEmpty()
    {
        // Assume that m_CollisionDataList is sorted in terms of their 'entityA' fields.
        var entityA = Entity.Null;
        var begIndex = 0;
        for (int i = 0; i < m_CollisionDataList.Length; i++)
        {
            var collisionData = m_CollisionDataList[i];
            if (entityA == collisionData.EntityA)
            {
                continue;
            }

            ProcessOneEntityA(entityA, begIndex, i);
            begIndex = i;
            entityA = collisionData.EntityA;
        }

        ProcessOneEntityA(entityA, begIndex, m_CollisionDataList.Length);
    }

    protected abstract void ProcessOneEntityA(Entity entityA, int beg, int end);
}