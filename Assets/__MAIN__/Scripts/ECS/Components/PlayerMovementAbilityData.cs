using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerMovementAbilityData : IComponentData
{
    public float MaxHorizontalSpeed;
    public float MaxHorizontalSpeedDashing;
    public float HorizontalAcc;
    public float HorizontalDashingAcc;
    public float HorizontalReverseAcc;
    public float JumpHitSpeed;
    public float JumpKeepMinTime;
    public float JumpKeepMaxTime;
    public float BouncedByEnemySpeed;
}