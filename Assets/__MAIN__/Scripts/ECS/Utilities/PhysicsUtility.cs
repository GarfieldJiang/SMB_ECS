using COL.UnityGameWheels.Unity;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using BoxCollider = Unity.Physics.BoxCollider;

public static class PhysicsUtility
{
    public static float3 GetScale(this Entity entity,
        ComponentDataFromEntity<CompositeScale> compositeScaleEntities,
        ComponentDataFromEntity<Scale> scaleEntities)
    {
        if (compositeScaleEntities.HasComponent(entity))
        {
            var matrix = compositeScaleEntities[entity].Value;
            return new float3(matrix[0][0], matrix[1][1], matrix[2][2]);
        }

        if (scaleEntities.HasComponent(entity))
        {
            var scaleValue = scaleEntities[entity].Value;
            return new float3(scaleValue, scaleValue, scaleValue);
        }

        return new float3(1, 1, 1);
    }

    private static AABB GetBounds(float3 position, float3 colliderSize)
    {
        var ret = new AABB();
        ret.Center = position;
        ret.Extents = colliderSize / 2f;
        //Log.Info(colliderSize);
        return ret;
    }

    public static CollisionData CalculateCollisionData(Entity entityA, Entity entityB,
        ComponentDataFromEntity<CompositeScale> compositeScaleEntities,
        ComponentDataFromEntity<Scale> scaleEntities,
        ComponentDataFromEntity<Translation> translationEntities,
        ComponentDataFromEntity<PhysicsCollider> colliderEntities)

    {
        var ret = new CollisionData();
        ret.EntityA = entityA;
        ret.EntityB = entityB;
        var translationA = translationEntities[entityA];
        var translationB = translationEntities[entityB];

        // Currently we use the translation's value as the center of the colliders.
        ret.ColliderBoundsA = GetBounds(translationA.Value, GetBoxColliderSize(colliderEntities[entityA]));
        ret.ColliderBoundsB = GetBounds(translationB.Value, GetBoxColliderSize(colliderEntities[entityB]));
        var overlapXMin = math.max(ret.ColliderBoundsA.Min.x, ret.ColliderBoundsB.Min.x);
        var overlapXMax = math.min(ret.ColliderBoundsA.Max.x, ret.ColliderBoundsB.Max.x);
        var overlapYMin = math.max(ret.ColliderBoundsA.Min.y, ret.ColliderBoundsB.Min.y);
        var overlapYMax = math.min(ret.ColliderBoundsA.Max.y, ret.ColliderBoundsB.Max.y);
        ret.OverlapX = overlapXMax - overlapXMin;
        ret.OverlapY = overlapYMax - overlapYMin;
        var signX = math.sign(translationA.Value.x - translationB.Value.x);
        ret.SignX = math.abs(signX) < float.Epsilon ? 1 : signX;
        var signY = math.sign(translationA.Value.y - translationB.Value.y);
        ret.SignY = math.abs(signY) < float.Epsilon ? 1 : signY;
        return ret;
    }

    public static CollisionData SwapCollisionData(CollisionData originalCollisionData)
        => new CollisionData
        {
            EntityA = originalCollisionData.EntityB,
            EntityB = originalCollisionData.EntityA,
            ColliderBoundsA = originalCollisionData.ColliderBoundsB,
            ColliderBoundsB = originalCollisionData.ColliderBoundsA,
            OverlapX = originalCollisionData.OverlapX,
            OverlapY = originalCollisionData.OverlapY,
            SignX = -originalCollisionData.SignX,
            SignY = -originalCollisionData.SignY,
        };

    public static unsafe void SetBoxColliderSize(PhysicsCollider physicsCollider, float3 size)
    {
        Assert.IsTrue(physicsCollider.ColliderPtr->Type == ColliderType.Box);
        var scPtr = (BoxCollider*)physicsCollider.ColliderPtr;
        var geometry = scPtr->Geometry;
        geometry.Size = size;
        scPtr->Geometry = geometry;
    }

    public static unsafe float3 GetBoxColliderSize(PhysicsCollider physicsCollider)
    {
        Assert.IsTrue(physicsCollider.ColliderPtr->Type == ColliderType.Box);
        return ((BoxCollider*)physicsCollider.ColliderPtr)->Size;
    }

    /// <summary>
    /// When a moving object collides with a static one, use this method to calculate from which direction the moving object comes.
    /// </summary>
    /// <param name="collisionData">entityA of which is the moving object</param>
    /// <returns></returns>
    public static CollisionFromDirection CalculateCollisionFromDirection(float movingEntityScaleFactor, CollisionData collisionData)
    {
        var slope = movingEntityScaleFactor;
        var movingObjectPosition = collisionData.ColliderBoundsA.Center;
        var staticObjectPosition = collisionData.ColliderBoundsB.Center;
        var staticObjectExtents = collisionData.ColliderBoundsB.Extents;
        if (collisionData.SignY >= 0 &&
            movingObjectPosition.y - staticObjectPosition.y - staticObjectExtents.y >=
            -slope * (movingObjectPosition.x - staticObjectPosition.x + staticObjectExtents.x) &&
            movingObjectPosition.y - staticObjectPosition.y - staticObjectExtents.y >=
            slope * (movingObjectPosition.x - staticObjectPosition.x - staticObjectExtents.x))
        {
            return CollisionFromDirection.Top;
        }

        if (collisionData.SignY < 0 &&
            movingObjectPosition.y - staticObjectPosition.y + staticObjectExtents.y <
            slope * (movingObjectPosition.x - staticObjectPosition.x + staticObjectExtents.x) &&
            movingObjectPosition.y - staticObjectPosition.y + staticObjectExtents.y <
            -slope * (movingObjectPosition.x - staticObjectPosition.x - staticObjectExtents.x))
        {
            return CollisionFromDirection.Bottom;
        }

        if (collisionData.SignX < 0 &&
            movingObjectPosition.y - staticObjectPosition.y - staticObjectExtents.y <
            -slope * (movingObjectPosition.x - staticObjectPosition.x + staticObjectExtents.x) &&
            movingObjectPosition.y - staticObjectPosition.y + staticObjectExtents.y >=
            slope * (movingObjectPosition.x - staticObjectPosition.x + staticObjectExtents.x))
        {
            return CollisionFromDirection.Left;
        }

        return CollisionFromDirection.Right;
    }
}