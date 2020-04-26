using Unity.Entities;

[GenerateAuthoringComponent]
public struct DeadPlayerMovingStates : IComponentData
{
    public DeadPlayerMovingStatus Status;
    public float UpDistanceCovered;
    public float DownDistanceCovered;
    public float InitTime;
    public float StayAtSummitTime;
}