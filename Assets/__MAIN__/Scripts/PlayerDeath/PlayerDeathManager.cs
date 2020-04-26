using System.Collections;
using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Unity;
using UnityEngine;

public class PlayerDeathManager : MonoBehaviourEx, IManager
{
    [SerializeField]
    private float m_TimeToWaitAfterPlayerDeath = 3;

    public void Init()
    {
        GameEntry.Instance.Event.AddEventListener(PlayerDieEventArgs.TheEventId, OnPlayerDeath);
    }

    public void ShutDown()
    {
    }

    private void OnPlayerDeath(object sender, BaseEventArgs e)
    {
        StartCoroutine(OnPlayerDeathCo());
    }

    private IEnumerator OnPlayerDeathCo()
    {
        yield return new WaitForSeconds(m_TimeToWaitAfterPlayerDeath);
        if (GameEntry.Instance.PlayerData.Lives <= 0)
        {
            GameEntry.Instance.UI.Open(UIPage.GameOver, null);
        }
        else
        {
            GameEntry.Instance.UI.Open(UIPage.EnteringStage, null);
        }
    }
}