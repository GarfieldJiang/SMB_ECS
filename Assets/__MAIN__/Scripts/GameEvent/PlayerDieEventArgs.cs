using COL.UnityGameWheels.Core;

public class PlayerDieEventArgs : BaseEventArgs
{
    public static readonly int TheEventId = EventIdToTypeMap.Generate<PlayerDieEventArgs>();
    public override int EventId => TheEventId;
}