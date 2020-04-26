using Unity.Entities;

[GenerateAuthoringComponent]
public struct ItemTag : IComponentData
{
    public ItemType ItemType;
    public float InitialMaxVerticalSpeed;
    public float InitialMinVerticalSpeed;
}