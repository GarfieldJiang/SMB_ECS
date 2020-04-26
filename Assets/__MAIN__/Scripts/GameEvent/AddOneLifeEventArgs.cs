using COL.UnityGameWheels.Core;
using Unity.Mathematics;

public class AddOneLifeEventArgs : BaseEventArgs
{
    public static readonly int TheEventId = EventIdToTypeMap.Generate<AddOneLifeEventArgs>();

    public override int EventId => TheEventId;
    public float3 WorldPosition;
    public AddLifeType Type;
}