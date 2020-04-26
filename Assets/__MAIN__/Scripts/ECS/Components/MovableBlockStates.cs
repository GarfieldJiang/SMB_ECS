using Unity.Entities;

[GenerateAuthoringComponent]
public struct MovableBlockStates : IComponentData
{
    public MovableBlockStatus Status;
    public float OriginalY;
}