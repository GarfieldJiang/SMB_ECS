using Unity.Entities;

[GenerateAuthoringComponent]
public struct SpawnPointData : IComponentData
{
    public Entity PrefabEntity;
}