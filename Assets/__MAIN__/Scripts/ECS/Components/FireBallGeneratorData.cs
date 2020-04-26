using Unity.Entities;

[GenerateAuthoringComponent]
public struct FireBallGeneratorData : IComponentData
{
    public Entity FireBallPrefabEntity;
    public Entity FireBallHitPrefabEntity;
}