using COL.UnityGameWheels.Core;

public class PlayerLevelChangeEventArgs : BaseEventArgs
{
    public static readonly int TheEventId = EventIdToTypeMap.Generate<PlayerLevelChangeEventArgs>();

    public override int EventId => TheEventId;

    public PlayerLevel Level;
}