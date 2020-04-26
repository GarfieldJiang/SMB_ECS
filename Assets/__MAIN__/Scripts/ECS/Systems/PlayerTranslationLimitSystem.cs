using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[UpdateAfter(typeof(CollisionSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public class PlayerTranslationLimitSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var playerEntity = GetEntityQuery(typeof(PlayerTag), typeof(Translation), typeof(MovementData)).GetSingletonEntity();
        var playerTranslation = GetComponentDataFromEntity<Translation>()[playerEntity];
        var playerMovementData = GetComponentDataFromEntity<MovementData>()[playerEntity];
        var mainCameraPosition = GameEntry.Instance.MainCameraTransform.position;
        var mainCamera = GameEntry.Instance.MainCamera;
        var mainCameraAdapter = GameEntry.Instance.MainCameraAdapter;
        var leftMost = mainCameraPosition.x - mainCameraAdapter.TargetScreenRatio.x * mainCamera.orthographicSize / mainCameraAdapter.TargetScreenRatio.y;
        if (!(playerTranslation.Value.x < leftMost)) return default;
        playerTranslation.Value.x = leftMost;
        if (playerMovementData.Velocity.x < 0)
        {
            playerMovementData.Velocity.x = 0;
            EntityManager.SetComponentData(playerEntity, playerMovementData);
        }

        EntityManager.SetComponentData(playerEntity, playerTranslation);

        return inputDeps;
    }
}