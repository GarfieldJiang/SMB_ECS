using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerInputData : IComponentData
{
    public int HorizontalInput;
    public bool Down;
    public bool FireHit;
    public bool LastFrameFireHit;
    public bool FireKeep;
    public bool LastFrameFireKeep;
    public bool JumpHit;
    public bool LastFrameJumpHit;
    public bool JumpKeep;
    public bool LastFrameJumpKeep;
}