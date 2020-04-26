using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class PlayerInvincibleStarmanSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = TimeUtility.GetLimitedDeltaTime();
        var totalInvincibleTime = GameEntry.Instance.Config.Global.Item.StarInvincibleTotalTime;
        var transitionTime = GameEntry.Instance.Config.Global.Item.StarInvincibleTotalTime - GameEntry.Instance.Config.Global.Item.StarInvincibleMainTime;
        Entities.WithoutBurst().ForEach((ref PlayerStates playerStates) =>
        {
            if (playerStates.Main != PlayerMainStatus.Alive)
            {
                return;
            }

            if (playerStates.InvincibleRemainingTime <= 0)
            {
                return;
            }

            var lastInvincibleRemainingTime = playerStates.InvincibleRemainingTime;
            playerStates.InvincibleRemainingTime = math.max(0, lastInvincibleRemainingTime - deltaTime);
            if (playerStates.InvincibleRemainingTime < transitionTime && lastInvincibleRemainingTime >= transitionTime)
            {
                GameEntry.Instance.Audio.PlayMusic(GameEntry.Instance.Scene.SceneData.MusicName, true);
            }
        }).Run();
        return default;
    }
}