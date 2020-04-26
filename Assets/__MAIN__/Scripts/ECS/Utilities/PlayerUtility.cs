public static class PlayerUtility
{
    public static bool IsGrounded(PlayerStates states)
        => IsGrounded(states.Motion);

    public static bool IsGrounded(PlayerMotionStatus motionStatus)
        => motionStatus == PlayerMotionStatus.Static || motionStatus == PlayerMotionStatus.Running;

    public static bool IsInAir(PlayerStates states)
        => !IsGrounded(states);

    public static bool IsInAir(PlayerMotionStatus motionStatus)
        => !IsGrounded(motionStatus);

    public static bool IsChangingLevel(PlayerStates states)
        => states.Level != states.NextLevel;

    public static bool IsLevelDown(PlayerStates states)
        => IsChangingLevel(states) && states.NextLevel == PlayerLevel.Default;

    public static bool IsLevelDownProtection(PlayerStates states)
        => states.LevelDownProtectionTime > 0;
}