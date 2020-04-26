using Unity.Entities;

[GenerateAuthoringComponent]
public struct LifeTime : IComponentData
{
    public float RemainingLifeTime;
}