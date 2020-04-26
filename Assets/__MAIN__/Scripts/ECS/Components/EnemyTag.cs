using Unity.Entities;

[GenerateAuthoringComponent]
public struct EnemyTag : IComponentData
{
    public bool Inited;
    public EnemyType Type;
}