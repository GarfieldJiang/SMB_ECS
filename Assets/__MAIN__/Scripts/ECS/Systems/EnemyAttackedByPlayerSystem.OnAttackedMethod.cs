using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial class EnemyAttackedByPlayerSystem
{
    private delegate bool OnAttackedMethod(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
        NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight);
}