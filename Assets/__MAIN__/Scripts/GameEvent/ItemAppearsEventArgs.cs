using COL.UnityGameWheels.Core;

public class ItemAppearsEventArgs : BaseEventArgs
{
    public static readonly int TheEventId = EventIdToTypeMap.Generate<ItemAppearsEventArgs>();
    public override int EventId => TheEventId;
}