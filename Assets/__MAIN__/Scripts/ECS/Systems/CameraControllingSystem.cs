using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[AlwaysSynchronizeSystem]
public class CameraControllingSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var mainCameraTransform = GameEntry.Instance.MainCameraTransform;
        if (mainCameraTransform == null)
        {
            return default;
        }

        var sceneData = GameEntry.Instance.Scene.SceneData;
        var playerQuery = GetEntityQuery(typeof(PlayerTag), typeof(Translation));
        var playerPosition = playerQuery.GetSingleton<Translation>().Value;
        var mainCameraPosition = mainCameraTransform.position;
        mainCameraPosition.x = math.clamp(playerPosition.x, mainCameraPosition.x, sceneData.CameraMaxX);
        mainCameraTransform.position = mainCameraPosition;

        return inputDeps;
    }
}