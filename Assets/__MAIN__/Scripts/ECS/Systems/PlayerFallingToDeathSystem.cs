using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(PlayerAttackedByEnemySystem))]
[UpdateBefore(typeof(PlayerMotionStatusSystem))]
[AlwaysSynchronizeSystem]
public class PlayerFallingToDeathSystem : JobComponentSystem
{
    private float m_PlayerYPositionToDie;

    protected override void OnCreate()
    {
        m_PlayerYPositionToDie = GameEntry.Instance.Config.Global.Player.PositionYToDie;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var playerQuery = GetEntityQuery(typeof(PlayerTag));
        if (playerQuery.CalculateEntityCount() == 0)
        {
            return default;
        }

        var playerEntity = playerQuery.GetSingletonEntity();
        var playerStates = GetComponentDataFromEntity<PlayerStates>(true)[playerEntity];

        if (playerStates.Main == PlayerMainStatus.Dying)
        {
            return default;
        }

        var translation = GetComponentDataFromEntity<Translation>(true)[playerEntity];
        if (translation.Value.y > m_PlayerYPositionToDie)
        {
            return default;
        }

        playerStates.Main = PlayerMainStatus.Dying;
        EntityManager.SetComponentData(playerEntity, playerStates);
        GameEntry.Instance.PlayerData.Die();
        return inputDeps;
    }
}