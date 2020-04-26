using COL.UnityGameWheels.Core;

public class FireBallSpawnedEventArgs : BaseEventArgs
{
    public static readonly int TheEventId = EventIdToTypeMap.Generate<FireBallSpawnedEventArgs>();
    public override int EventId => TheEventId;
}