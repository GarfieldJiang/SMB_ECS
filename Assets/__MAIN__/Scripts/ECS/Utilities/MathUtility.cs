using Unity.Mathematics;

public static class MathUtility
{
    public static float4x4 ScaleToMatrix(float3 scale)
        => new float4x4
        {
            c0 = new float4(scale.x, 0, 0, 0),
            c1 = new float4(0, scale.y, 0, 0),
            c2 = new float4(0, 0, scale.z, 0),
            c3 = new float4(0, 0, 0, 1),
        };

    public static float3 MatrixToScale(float4x4 scaleMatrix)
        => new float3(scaleMatrix[0][0], scaleMatrix[1][1], scaleMatrix[2][2]);
}