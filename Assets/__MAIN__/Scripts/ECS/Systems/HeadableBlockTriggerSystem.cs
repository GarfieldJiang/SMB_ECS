using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;

[UpdateAfter(typeof(TransformSystemGroup))]
[AlwaysSynchronizeSystem]
public class HeadableBlockTriggerSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_CommandBufferSystem;
    private const int BrickFragmentCount = 4;


    protected override void OnCreate()
    {
        m_CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var playerQuery = GetEntityQuery(typeof(PlayerTag));
        if (playerQuery.CalculateEntityCount() == 0)
        {
            return default;
        }

        var translationEntities = GetComponentDataFromEntity<Translation>();
        var playerEntity = playerQuery.GetSingletonEntity();
        var playerTranslation = translationEntities[playerEntity];
        var playerStates = GetComponentDataFromEntity<PlayerStates>()[playerEntity];
        var deadBlockGeneratorData = GetSingleton<DeadBlockGeneratorData>();
        var headableBlockQuery = GetEntityQuery(typeof(HeadableBlockTag));
        var headableBlocks = headableBlockQuery.ToEntityArray(Allocator.TempJob);
        var headableTagEntities = GetComponentDataFromEntity<HeadableBlockTag>();
        var coinQuestionMarkEntities = GetComponentDataFromEntity<CoinQuestionMarkData>();
        var ordinaryBrickEntities = GetComponentDataFromEntity<OrdinaryBrickData>();
        var itemBlockMarkEntities = GetComponentDataFromEntity<ItemBlockData>();
        var coinBrickConfigDataEntities = GetComponentDataFromEntity<CoinBrickConfigData>();
        var coinBrickStatesEntities = GetComponentDataFromEntity<CoinBrickStates>();
        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer();
        Entity headedBlock = Entity.Null;
        float shortestHorizontalDistance = float.MaxValue;
        int headedCount = 0;
        foreach (var headableBlock in headableBlocks)
        {
            var tag = headableTagEntities[headableBlock];
            if (!tag.IsHeaded)
            {
                continue;
            }

            ++headedCount;
            tag.IsHeaded = false;
            headableTagEntities[headableBlock] = tag;

            var position = translationEntities[headableBlock].Value;
            var horizontalDistance = math.abs(position.x - playerTranslation.Value.x);
            if (horizontalDistance < shortestHorizontalDistance)
            {
                shortestHorizontalDistance = horizontalDistance;
                headedBlock = headableBlock;
            }
        }

        if (headedBlock != Entity.Null)
        {
            var position = translationEntities[headedBlock].Value;
            var movableBlockStates = new MovableBlockStates
            {
                Status = MovableBlockStatus.MovingUp,
                OriginalY = position.y,
            };

            if (coinQuestionMarkEntities.HasComponent(headedBlock))
            {
                DealWithCoinQuestionMark(deadBlockGeneratorData, position, movableBlockStates, commandBuffer, headedBlock);
            }
            else if (itemBlockMarkEntities.HasComponent(headedBlock))
            {
                DealWithItemBlock(deadBlockGeneratorData, position, movableBlockStates, commandBuffer, headedBlock, playerStates);
            }
            else if (ordinaryBrickEntities.HasComponent(headedBlock))
            {
                DealWithOrdinaryBrick(playerStates, headedBlock, movableBlockStates, commandBuffer, position);
            }
            else if (coinBrickConfigDataEntities.HasComponent(headedBlock))
            {
                DealWithCoinBrick(deadBlockGeneratorData, position, movableBlockStates, commandBuffer, headedBlock, playerStates,
                    coinBrickConfigDataEntities, coinBrickStatesEntities);
            }
        }

        headableBlocks.Dispose();
        return inputDeps;
    }

    private void DealWithCoinBrick(DeadBlockGeneratorData deadBlockGeneratorData, float3 position, MovableBlockStates movableBlockStates,
        EntityCommandBuffer commandBuffer, Entity headedBlock, PlayerStates playerStates,
        ComponentDataFromEntity<CoinBrickConfigData> coinBrickConfigDataEntities, ComponentDataFromEntity<CoinBrickStates> coinBrickStatesEntities)
    {
        var coinBrickStates = coinBrickStatesEntities[headedBlock];
        var coinBrickConfigData = coinBrickConfigDataEntities[headedBlock];
        if (!coinBrickStates.TimerStarted)
        {
            coinBrickStates.TimerStarted = true;
            coinBrickStates.CoinLeft = coinBrickConfigData.TotalCoins;
            coinBrickStates.TimeElapsed = 0;
        }

        Assert.IsTrue(coinBrickStates.CoinLeft > 0);
        coinBrickStates.CoinLeft -= 1;
        GameEntry.Instance.PlayerData.AddCoin(position, CoinType.Block);

        EntityManager.SetComponentData(headedBlock, coinBrickStates);
        EntityManager.SetComponentData(headedBlock, movableBlockStates);
        if (coinBrickStates.CoinLeft <= 0)
        {
            var deadBlock = EntityManager.Instantiate(deadBlockGeneratorData.PrefabEntity);
            EntityManager.SetComponentData(deadBlock, new Translation {Value = position});
            EntityManager.SetComponentData(deadBlock, movableBlockStates);
            commandBuffer.DestroyEntity(headedBlock);
        }
    }

    private void DealWithOrdinaryBrick(PlayerStates playerStates, Entity headedBlock, MovableBlockStates movableBlockStates, EntityCommandBuffer commandBuffer,
        float3 position)
    {
        if (playerStates.Level == PlayerLevel.Default)
        {
            EntityManager.SetComponentData(headedBlock, movableBlockStates);
            return;
        }

        var scoreData = GetComponentDataFromEntity<ScoreData>()[headedBlock];
        commandBuffer.DestroyEntity(headedBlock);
        GameEntry.Instance.PlayerData.AddScores(scoreData.BaseScore, position, AddScoreType.OrdinaryBrick);
        var brickFragmentsGeneratorQuery = GetEntityQuery(typeof(BrickFragmentsGeneratorData));
        var brickFragmentsGeneratorData = brickFragmentsGeneratorQuery.GetSingleton<BrickFragmentsGeneratorData>();
        var config = GameEntry.Instance.Config.Global.BrickFragments;
        for (int i = 0; i < BrickFragmentCount; i++)
        {
            var brickFragmentEntity = EntityManager.Instantiate(brickFragmentsGeneratorData.PrefabEntity);
            var prefabPosition = GetComponentDataFromEntity<Translation>()[brickFragmentsGeneratorData.PrefabEntity].Value;
            var prefabScale =
                brickFragmentsGeneratorData.PrefabEntity.GetScale(GetComponentDataFromEntity<CompositeScale>(), GetComponentDataFromEntity<Scale>());

            EntityManager.SetComponentData(brickFragmentEntity, new Translation
            {
                Value = new float3(position.x + config.OffsetXs[i], position.y + config.OffsetYs[i], prefabPosition.z),
            });
            EntityManager.SetComponentData(brickFragmentEntity, new MovementData
            {
                Velocity = new float3(config.HorizontalVelocities[i], config.VerticalVelocities[i], 0),
            });

            var compositeScale = new CompositeScale
            {
                Value = MathUtility.ScaleToMatrix(new float3(config.ScaleXs[i] * prefabScale.x, prefabScale.y, prefabScale.z)),
            };

            if (GetComponentDataFromEntity<CompositeScale>().HasComponent(brickFragmentEntity))
            {
                EntityManager.SetComponentData(brickFragmentEntity, compositeScale);
            }
            else
            {
                EntityManager.AddComponentData(brickFragmentEntity, compositeScale);
            }
        }
    }

    private void DealWithItemBlock(DeadBlockGeneratorData deadBlockGeneratorData, float3 position, MovableBlockStates movableBlockStates,
        EntityCommandBuffer commandBuffer, Entity headedBlock, PlayerStates playerStates)
    {
        var deadBlock = EntityManager.Instantiate(deadBlockGeneratorData.PrefabEntity);
        EntityManager.SetComponentData(deadBlock, new Translation {Value = position});
        EntityManager.SetComponentData(deadBlock, movableBlockStates);

        commandBuffer.DestroyEntity(headedBlock);

        var itemBlockData = GetComponentDataFromEntity<ItemBlockData>()[headedBlock];
        var generatorData = GetEntityQuery(typeof(ItemGeneratorData)).GetSingleton<ItemGeneratorData>();
        Entity prefabEntity = Entity.Null;
        switch (itemBlockData.BlockItemType)
        {
            case BlockItemType.StarMan:
                prefabEntity = generatorData.Starman;
                break;
            case BlockItemType.OneUpMushroom:
                prefabEntity = generatorData.OneUpMushroom;
                break;
            case BlockItemType.MegaMushroomOrFireFlower:
            default:
                prefabEntity = playerStates.Level == PlayerLevel.Default ? generatorData.MegaMushroom : generatorData.FireFlower;
                break;
        }

        var itemEntity = EntityManager.Instantiate(prefabEntity);

        EntityManager.SetComponentData(itemEntity, new Translation
        {
            Value = position + new float3(0, GameEntry.Instance.Config.Global.Item.SpawnOffsetY, 0)
        });

        GameEntry.Instance.Event.SendEvent(this, GameEntry.Instance.RefPool.GetOrAdd<ItemAppearsEventArgs>().Acquire());
    }

    private void DealWithCoinQuestionMark(DeadBlockGeneratorData deadBlockGeneratorData, float3 position, MovableBlockStates movableBlockStates,
        EntityCommandBuffer commandBuffer, Entity headedBlock)
    {
        GameEntry.Instance.PlayerData.AddCoin(position, CoinType.Block);
        var deadBlock = EntityManager.Instantiate(deadBlockGeneratorData.PrefabEntity);
        EntityManager.SetComponentData(deadBlock, new Translation {Value = position});
        EntityManager.SetComponentData(deadBlock, movableBlockStates);
        commandBuffer.DestroyEntity(headedBlock);
    }
}