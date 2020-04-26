using COL.UnityGameWheels.Core;

public class PlayerCharacterCreatedEventArgs : BaseEventArgs
{
    public static int TheEventId = EventIdToTypeMap.Generate<PlayerCharacterCreatedEventArgs>();

    public override int EventId => TheEventId;
}