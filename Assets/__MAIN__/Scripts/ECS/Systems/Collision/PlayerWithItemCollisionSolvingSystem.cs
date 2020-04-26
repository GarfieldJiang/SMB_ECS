using Unity.Entities;

[UpdateInGroup(typeof(CollisionSystemGroup))]
[UpdateAfter(typeof(CollisionDataCollectingSystem))]
[UpdateBefore(typeof(CollisionDataClearingSystem))]
public class PlayerWithItemCollisionSolvingSystem : CollisionSolvingSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;


    public override string CollisionDataListKey => "PlayerWithItem";

    protected override void OnCreate()
    {
        base.OnCreate();
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void ProcessCollisionDataListNonEmpty()
    {
        var playerEntity = GetSingletonEntity<PlayerTag>();
        var playerStates = GetSingleton<PlayerStates>();
        var itemStatesEntities = GetComponentDataFromEntity<ItemStates>();
        var itemTagEntities = GetComponentDataFromEntity<ItemTag>();
        var scoreDataEntities = GetComponentDataFromEntity<ScoreData>();
        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer();

        foreach (var collisionData in m_CollisionDataList)
        {
            var itemEntity = collisionData.EntityB;
            commandBuffer.DestroyEntity(itemEntity);
            if (itemStatesEntities[itemEntity].Status != ItemStatus.Normal)
            {
                return;
            }

            var itemType = itemTagEntities[itemEntity].ItemType;
            var hasScoreData = scoreDataEntities.HasComponent(itemEntity);
            var itemPosition = collisionData.ColliderBoundsB.Center;
            var scoresToAdd = 0;
            if (hasScoreData)
            {
                scoresToAdd = scoreDataEntities[itemEntity].BaseScore;
            }

            bool playerLevelChanged = false;
            switch (itemType)
            {
                case ItemType.MegaMushroom:
                    if (playerStates.Level == PlayerLevel.Default)
                    {
                        playerStates.NextLevel = PlayerLevel.Super;
                        playerLevelChanged = true;
                    }

                    break;
                case ItemType.FireFlower:
                    if (playerStates.Level == PlayerLevel.Default)
                    {
                        playerStates.NextLevel = PlayerLevel.Super;
                    }
                    else if (playerStates.Level == PlayerLevel.Super)
                    {
                        playerStates.NextLevel = PlayerLevel.Fire;
                    }

                    playerLevelChanged = true;

                    break;
                case ItemType.OneUpMushroom:
                    GameEntry.Instance.PlayerData.AddOneLifeByMushroom(itemPosition);
                    break;
                case ItemType.StarMan:
                    playerStates.InvincibleRemainingTime = GameEntry.Instance.Config.Global.Item.StarInvincibleMainTime;
                    GameEntry.Instance.Audio.PlaySoundEffect("smb_powerup");
                    GameEntry.Instance.Audio.PlayMusic("05-starman", true);
                    break;
            }

            if (scoresToAdd > 0)
            {
                GameEntry.Instance.PlayerData.AddScores(scoresToAdd, itemPosition, AddScoreType.Item);
            }

            if (playerLevelChanged)
            {
                var eventArgs = GameEntry.Instance.RefPool.GetOrAdd<PlayerLevelChangeEventArgs>().Acquire();
                eventArgs.Level = playerStates.NextLevel;
                GameEntry.Instance.Event.SendEvent(this, eventArgs);
            }

            EntityManager.SetComponentData(playerEntity, playerStates);
        }
    }
}