using Unity.Entities;

[GenerateAuthoringComponent]
public struct DeadBlockGeneratorData : IComponentData
{
    public Entity PrefabEntity;
}