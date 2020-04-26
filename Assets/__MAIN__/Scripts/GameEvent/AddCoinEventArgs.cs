using COL.UnityGameWheels.Core;
using Unity.Mathematics;

public class AddCoinEventArgs : BaseEventArgs
{
    public static readonly int TheEventId = EventIdToTypeMap.Generate<AddCoinEventArgs>();
    public override int EventId => TheEventId;
    public float3 WorldPosition;
    public CoinType Type;
}