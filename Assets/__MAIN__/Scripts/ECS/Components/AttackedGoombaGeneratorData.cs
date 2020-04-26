using Unity.Entities;

[GenerateAuthoringComponent]
public struct AttackedGoombaGeneratorData : IComponentData
{
    public Entity StompedGoombaPrefabEntity;

    public Entity DeadGoombaPrefabEntity;
}