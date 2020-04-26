using Unity.Entities;

[GenerateAuthoringComponent]
public struct ItemStates : IComponentData
{
    public ItemStatus Status;
    public float UpToGo;
    public float UpSpeed;
}