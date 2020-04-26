using Unity.Entities;

[GenerateAuthoringComponent]
public struct HeadableBlockTag : IComponentData
{
    public bool IsHeaded;
}