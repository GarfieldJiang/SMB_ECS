using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerStates : IComponentData
{
    public PlayerMainStatus Main;
    public PlayerMotionStatus Motion;
    public PlayerLevel Level;
    public PlayerLevel NextLevel;

    // Flag to check whether falling is stopped.
    public bool FallingStopped;
    public float JumpKeepTime;
    public float LevelChangeTimeUsed;
    public float LevelDownProtectionTime;
    public bool IsFacingLeft;
    public float InvincibleRemainingTime;
}

public enum PlayerMainStatus
{
    Alive,
    Dying,
}

public enum PlayerMotionStatus
{
    Static,
    Running,
    Jumping,
    Falling,
}

public enum PlayerLevel
{
    Default,
    Super,
    Fire,
}