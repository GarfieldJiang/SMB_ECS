using Unity.Entities;

[GenerateAuthoringComponent]
public struct CoinBrickConfigData : IComponentData
{
    public int TotalCoins;
    public float CoinDecrementInterval;
}