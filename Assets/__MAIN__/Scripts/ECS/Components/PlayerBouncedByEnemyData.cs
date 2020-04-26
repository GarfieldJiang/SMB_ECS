using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerBouncedByEnemyData : IComponentData
{
    public bool IsBounced;
}