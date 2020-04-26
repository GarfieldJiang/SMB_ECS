using System;
using Unity.Mathematics;

[Serializable]
public struct SceneData
{
    public string SceneName;
    public float CameraStartX;
    public float CameraMaxX;
    public float3 PlayerSpawningPosition;
    public string MusicName;
}