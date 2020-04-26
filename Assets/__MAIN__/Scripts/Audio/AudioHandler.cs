using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Unity;

public class AudioHandler : MonoBehaviourEx
{
    private IEventManager EventManager => GameEntry.Instance.Event;
    private AudioManager AudioManager => GameEntry.Instance.Audio;

    private void OnEnable()
    {
        EventManager.AddEventListener(PlayerDieEventArgs.TheEventId, OnPlayerDied);
        EventManager.AddEventListener(AddOneLifeEventArgs.TheEventId, OnAddOneLife);
        EventManager.AddEventListener(AddScoreEventArgs.TheEventId, OnAddScores);
        EventManager.AddEventListener(PlayerMotionStatusChangeEventArgs.TheEventId, OnPlayerMotionStatusChange);
        EventManager.AddEventListener(PlayerStompingEnemyEventArgs.TheEventId, OnPlayerStompingEnemy);
        EventManager.AddEventListener(PlayerLevelChangeEventArgs.TheEventId, OnPlayerLevelChanged);
        EventManager.AddEventListener(ItemAppearsEventArgs.TheEventId, OnItemAppear);
        EventManager.AddEventListener(FireBallSpawnedEventArgs.TheEventId, OnFireBallSpawned);
    }

    private void OnDisable()
    {
        if (GameEntry.Instance == null || EventManager == null) return;
        EventManager.RemoveEventListener(PlayerDieEventArgs.TheEventId, OnPlayerDied);
        EventManager.RemoveEventListener(AddOneLifeEventArgs.TheEventId, OnAddOneLife);
        EventManager.RemoveEventListener(AddScoreEventArgs.TheEventId, OnAddScores);
        EventManager.RemoveEventListener(PlayerStompingEnemyEventArgs.TheEventId, OnPlayerStompingEnemy);
        EventManager.RemoveEventListener(PlayerLevelChangeEventArgs.TheEventId, OnPlayerLevelChanged);
        EventManager.RemoveEventListener(FireBallSpawnedEventArgs.TheEventId, OnFireBallSpawned);
    }

    private void OnFireBallSpawned(object sender, BaseEventArgs e)
    {
        AudioManager.PlaySoundEffect("smb_fireball");
    }

    private void OnItemAppear(object sender, BaseEventArgs e)
    {
        AudioManager.PlaySoundEffect("smb_powerup_appears");
    }

    private void OnPlayerLevelChanged(object sender, BaseEventArgs e)
    {
        var newLevel = ((PlayerLevelChangeEventArgs)e).Level;
        if (newLevel == PlayerLevel.Default)
        {
            AudioManager.PlaySoundEffect("smb_pipe_powerdown");
        }
        else
        {
            AudioManager.PlaySoundEffect("smb_powerup");
        }
    }

    private void OnPlayerMotionStatusChange(object sender, BaseEventArgs e)
    {
        var eventArgs = (PlayerMotionStatusChangeEventArgs)e;

        if (PlayerUtility.IsGrounded(eventArgs.PlayerStates) && PlayerUtility.IsInAir(eventArgs.LastMotionStatus))
        {
            AudioManager.StopSoundEffect("smb_jump-small");
            AudioManager.StopSoundEffect("smb_jump-super");
        }

        if (eventArgs.PlayerStates.Motion == PlayerMotionStatus.Jumping && !AudioManager.IsPlayingSoundEffect("smb_stomp"))
        {
            AudioManager.PlaySoundEffect(eventArgs.PlayerStates.Level == PlayerLevel.Default ? "smb_jump-small" : "smb_jump-super");
        }
    }


    private void OnPlayerStompingEnemy(object sender, BaseEventArgs e)
    {
        AudioManager.PlaySoundEffect("smb_stomp");
    }

    private void OnAddScores(object sender, BaseEventArgs e)
    {
        switch (((AddScoreEventArgs)e).Type)
        {
            case AddScoreType.OrdinaryBrick:
                AudioManager.PlaySoundEffect("smb_breakblock");
                break;
            case AddScoreType.Item:
                AudioManager.PlaySoundEffect("smb_powerup");
                break;
            case AddScoreType.NormalCoin:
            case AddScoreType.CoinFromBlock:
                AudioManager.PlaySoundEffect("smb_coin");
                break;
        }
    }

    private void OnPlayerDied(object sender, BaseEventArgs e)
    {
        GameEntry.Instance.Audio.PlayMusic("08-you-re-dead", false);
    }

    private void OnAddOneLife(object sender, BaseEventArgs e)
    {
        GameEntry.Instance.Audio.PlaySoundEffect("smb_1-up");
    }
}