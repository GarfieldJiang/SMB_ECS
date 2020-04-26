using Unity.Entities;

[UpdateInGroup(typeof(CollisionSystemGroup))]
[AlwaysUpdateSystem]
public class CollisionDataClearingSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        GameEntry.Instance.SharedData.ClearAllCollisionDataLists();
    }
}