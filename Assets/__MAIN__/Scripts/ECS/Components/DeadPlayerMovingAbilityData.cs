using Unity.Entities;

[GenerateAuthoringComponent]
public struct DeadPlayerMovingAbilityData : IComponentData
{
    public float Speed;
    public float UpDistance;
    public float DownDistance;
    public float InitDuration;
    public float StayAtSummitDuration;
}