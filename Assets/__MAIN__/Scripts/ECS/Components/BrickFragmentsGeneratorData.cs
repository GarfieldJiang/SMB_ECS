using Unity.Entities;

[GenerateAuthoringComponent]
public struct BrickFragmentsGeneratorData : IComponentData
{
    public Entity PrefabEntity;
}