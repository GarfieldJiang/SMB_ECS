using Unity.Entities;

[GenerateAuthoringComponent]
public struct GravityAbility : IComponentData
{
    public float GravityAcc;
}