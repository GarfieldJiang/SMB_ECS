using Unity.Entities;

[GenerateAuthoringComponent]
public struct CoinBrickStates : IComponentData
{
    public bool TimerStarted;
    public float TimeElapsed;
    public int CoinLeft;
}