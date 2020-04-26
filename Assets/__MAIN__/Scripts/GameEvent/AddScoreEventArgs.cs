using COL.UnityGameWheels.Core;
using Unity.Mathematics;

public class AddScoreEventArgs : BaseEventArgs
{
    public static readonly int TheEventId = EventIdToTypeMap.Generate<AddScoreEventArgs>();

    public override int EventId => TheEventId;

    public float3 WorldPosition;

    public int ScoresToAdd;
    public AddScoreType Type;
}