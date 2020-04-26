using System;
using COL.UnityGameWheels.Unity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerDataManager : MonoBehaviourEx, IManager
{
    [field: SerializeField]
    public int Scores { get; private set; }

    [field: SerializeField]
    public int Lives { get; private set; }

    [field: SerializeField]
    public PlayerLevel Level { get; private set; }

    [field: SerializeField]
    public int Coins { get; private set; }

    public void Init()
    {
    }

    public void Reset()
    {
        Scores = 0;
        Lives = GameEntry.Instance.Config.Global.Player.InitialLives;
        Coins = 0;
        Level = PlayerLevel.Default;
    }

    public void ShutDown()
    {
    }

    public void Die()
    {
        Assert.IsTrue(Lives > 0);
        Lives--;
        Level = PlayerLevel.Default;
        GameEntry.Instance.Event.SendEvent(this, GameEntry.Instance.RefPool.GetOrAdd<PlayerDieEventArgs>().Acquire());
    }

    public void AddScores(int scoresToAdd, float3 worldPosition, AddScoreType type)
    {
        if (type == AddScoreType.NormalCoin || type == AddScoreType.CoinFromBlock)
        {
            throw new ArgumentException("Use AddCoin instead.");
        }

        AddScores_Internal(scoresToAdd, worldPosition, type);
    }

    private void AddScores_Internal(int scoresToAdd, float3 worldPosition, AddScoreType type)
    {
        Assert.AreNotEqual(scoresToAdd, 0);
        Log.Info($"[PlayerDataManager.AddScores_Internal] scoresToAdd = {scoresToAdd}, worldPosition = {worldPosition}, type = {type}");
        Scores += scoresToAdd;
        var eventArgs = GameEntry.Instance.RefPool.GetOrAdd<AddScoreEventArgs>().Acquire();
        eventArgs.ScoresToAdd = scoresToAdd;
        eventArgs.WorldPosition = worldPosition;
        eventArgs.Type = type;
        GameEntry.Instance.Event.SendEvent(this, eventArgs);
    }

    public void AddOneLifeByMushroom(float3 worldPosition)
    {
        AddOneLife_Internal(worldPosition, AddLifeType.OneUpMushroom);
    }

    public void ChangePlayerLevel(PlayerLevel newPlayerLevel)
    {
        if (Level == newPlayerLevel)
        {
            return;
        }

        Level = newPlayerLevel;
    }

    public void AddCoin(float3 worldPosition, CoinType type)
    {
        Log.Info($"[PlayerDataManager.AddCoin] worldPosition = {worldPosition}, type = {type}");
        Coins++;
        bool shouldAddLife = false;
        if (Coins >= GameEntry.Instance.Config.Global.Coin.CountToAddLife)
        {
            shouldAddLife = true;
            Coins = 0;
            var eventArgs = GameEntry.Instance.RefPool.GetOrAdd<AddCoinEventArgs>().Acquire();
            eventArgs.WorldPosition = worldPosition;
            eventArgs.Type = type;
        }

        var coinScore = GameEntry.Instance.Config.Global.Coin.BaseScore;
        AddScores_Internal(coinScore, worldPosition, CoinType.Block == type ? AddScoreType.CoinFromBlock : AddScoreType.NormalCoin);

        if (shouldAddLife)
        {
            AddOneLife_Internal(worldPosition, AddLifeType.Coin);
        }
    }

    private void AddOneLife_Internal(float3 worldPosition, AddLifeType type)
    {
        Log.Info($"[PlayerDataManager.AddOneLife_Internal] worldPosition = {worldPosition}, type = {type}");
        Lives++;
        var eventArgs = GameEntry.Instance.RefPool.GetOrAdd<AddOneLifeEventArgs>().Acquire();
        eventArgs.Type = type;
        GameEntry.Instance.Event.SendEvent(this, eventArgs);
    }
}