using Unity.Entities;

[GenerateAuthoringComponent]
public struct ItemBlockData : IComponentData
{
    public BlockItemType BlockItemType;
}