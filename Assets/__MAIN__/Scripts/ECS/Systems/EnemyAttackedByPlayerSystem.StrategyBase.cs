using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial class EnemyAttackedByPlayerSystem
{
    private abstract class StrategyBase
    {
        private readonly Dictionary<PlayerAttackType, OnAttackedMethod> m_Methods = new Dictionary<PlayerAttackType, OnAttackedMethod>();

        public StrategyBase()
        {
            m_Methods.Add(PlayerAttackType.None, DummyMethod);
            m_Methods.Add(PlayerAttackType.Kicking, OnKicked);
            m_Methods.Add(PlayerAttackType.Stomping, OnStomped);
            m_Methods.Add(PlayerAttackType.Shell, OnShellHit);
            m_Methods.Add(PlayerAttackType.FireBall, OnFireBallHit);
            m_Methods.Add(PlayerAttackType.InvincibleStarman, OnInvinciblePlayerHit);
        }

        public OnAttackedMethod GetMethod(PlayerAttackType playerAttackType)
        {
            return m_Methods[playerAttackType];
        }

        protected void PlayKillSound()
        {
            GameEntry.Instance.Audio.PlaySoundEffect("smb_kick");
        }

        private bool DummyMethod(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight) => false;

        protected abstract bool OnStomped(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight);

        protected abstract bool OnFireBallHit(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight);

        protected abstract bool OnInvinciblePlayerHit(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight);

        protected abstract bool OnShellHit(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight);

        protected abstract bool OnKicked(EnemyAttackedByPlayerSystem system, Entity enemyEntity, float3 position, EntityCommandBuffer commandBuffer,
            NativeQueue<AttackedEnemySpawningData> attackedEnemySpawningQueue, bool attackIsFromRight);
    }
}