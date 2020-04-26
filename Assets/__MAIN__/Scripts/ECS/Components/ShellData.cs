using Unity.Entities;

[GenerateAuthoringComponent]
public struct ShellData : IComponentData
{
    public float HorizontalSpeedConfig;
    public float ReviveTimeConfig;
    public float ReviveTimeUsed;
    public bool IsMoving;
}