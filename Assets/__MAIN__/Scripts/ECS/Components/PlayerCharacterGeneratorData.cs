using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerCharacterGeneratorData : IComponentData
{
    public Entity PrefabEntity;
}