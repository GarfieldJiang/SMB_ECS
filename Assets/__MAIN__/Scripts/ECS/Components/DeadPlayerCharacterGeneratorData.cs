using Unity.Entities;

[GenerateAuthoringComponent]
public struct DeadPlayerCharacterGeneratorData : IComponentData
{
    public Entity PrefabEntity;
}