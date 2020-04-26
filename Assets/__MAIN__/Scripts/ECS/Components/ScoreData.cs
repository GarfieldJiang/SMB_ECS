using Unity.Entities;

[GenerateAuthoringComponent]
public struct ScoreData : IComponentData
{
    public int BaseScore;
    public bool UseComplicatedScore;
    public int StompedScore;
    public int FireBallHitScore;
}