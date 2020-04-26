using COL.UnityGameWheels.Core;

public class PlayerMotionStatusChangeEventArgs : BaseEventArgs
{
    public static readonly int TheEventId = EventIdToTypeMap.Generate<PlayerMotionStatusChangeEventArgs>();

    public override int EventId => TheEventId;
    public PlayerMotionStatus LastMotionStatus;
    public PlayerStates PlayerStates;
}