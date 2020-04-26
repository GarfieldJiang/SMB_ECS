using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Unity;
using UnityEngine;
using UnityEngine.UI;

public class UIHead : MonoBehaviourEx
{
#pragma warning disable 414
    [SerializeField]
    private Text m_PlayerCharacterName = null;
#pragma warning restore 414

    [SerializeField]
    private Text m_Scores = null;

    [SerializeField]
    private Text m_CoinCount = null;


#pragma warning disable 414
    [SerializeField]
    private Text m_StageName = null;
#pragma warning restore 414

    [SerializeField]
    private Text m_TimeCount = null;

    private void SetScores()
    {
        int scores = GameEntry.Instance.PlayerData.Scores;
        m_Scores.text = scores.ToString().PadLeft(6, '0');
    }

    private void SetCoins()
    {
        int coins = GameEntry.Instance.PlayerData.Coins;
        m_CoinCount.text = "x" + coins.ToString().PadLeft(2, '0');
    }

    private void OnEnable()
    {
        GameEntry.Instance.Event.AddEventListener(AddScoreEventArgs.TheEventId, OnAddScores);
        GameEntry.Instance.Event.AddEventListener(AddCoinEventArgs.TheEventId, OnAddCoin);
        SetScores();
        SetCoins();
    }

    private void OnDisable()
    {
        GameEntry.Instance.Event.RemoveEventListener(AddScoreEventArgs.TheEventId, OnAddScores);
        GameEntry.Instance.Event.RemoveEventListener(AddCoinEventArgs.TheEventId, OnAddCoin);
    }

    private void OnAddCoin(object sender, BaseEventArgs e)
    {
        SetCoins();
    }

    private void OnAddScores(object sender, BaseEventArgs e)
    {
        SetScores();
    }

    public void SetTimeCount(int timeCount)
    {
        m_TimeCount.text = timeCount.ToString().PadLeft(3, '0');
    }

    public void ClearTimeCount()
    {
        m_TimeCount.text = string.Empty;
    }
}