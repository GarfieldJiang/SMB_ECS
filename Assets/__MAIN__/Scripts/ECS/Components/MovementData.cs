using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct MovementData : IComponentData
{
    public float3 Velocity;
    public float3 LastPosition;
}