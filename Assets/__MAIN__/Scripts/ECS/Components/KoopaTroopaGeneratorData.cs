using Unity.Entities;

[GenerateAuthoringComponent]
public struct KoopaTroopaGeneratorData : IComponentData
{
    public Entity FlyingPrefabEntity;
    public Entity NormalPrefabEntity;
    public Entity ShellPrefabEntity;
    public Entity DeadPrefabEntity;
}