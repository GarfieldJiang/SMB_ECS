using Unity.Entities;

[GenerateAuthoringComponent]
public struct SimpleMovementAbilityData : IComponentData
{
    public bool InteractsWithGround;
    public float HorizontalSpeed;
    public float BouncedVerticalSpeed;
}