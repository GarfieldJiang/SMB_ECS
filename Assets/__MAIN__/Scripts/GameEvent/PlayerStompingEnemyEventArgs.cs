using COL.UnityGameWheels.Core;

public class PlayerStompingEnemyEventArgs : BaseEventArgs
{
    public static readonly int TheEventId = EventIdToTypeMap.Generate<PlayerStompingEnemyEventArgs>();

    public override int EventId => TheEventId;
}