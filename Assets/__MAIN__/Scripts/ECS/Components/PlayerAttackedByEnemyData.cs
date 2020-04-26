using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerAttackedByEnemyData : IComponentData
{
    public EnemyAttackType EnemyAttackType;
}