using Unity.Entities;

[GenerateAuthoringComponent]
public struct FlashRenderingData : IComponentData
{
    public float TimeLeftToSwitchRenderer;
}