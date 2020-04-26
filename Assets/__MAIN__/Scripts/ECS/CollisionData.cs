using Unity.Entities;
using Unity.Mathematics;

public struct CollisionData
{
    public Entity EntityA;
    public Entity EntityB;
    public AABB ColliderBoundsA;
    public AABB ColliderBoundsB;
    public float OverlapX;
    public float OverlapY;
    public float SignX;
    public float SignY;
}