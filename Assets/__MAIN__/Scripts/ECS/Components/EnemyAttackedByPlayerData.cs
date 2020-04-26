using Unity.Entities;

[GenerateAuthoringComponent]
public struct EnemyAttackedByPlayerData : IComponentData
{
    public PlayerAttackType PlayerAttackType;
    public bool IsFromRight;
}