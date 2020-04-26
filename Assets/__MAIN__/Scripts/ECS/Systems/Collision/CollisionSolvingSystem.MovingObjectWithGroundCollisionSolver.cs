using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public abstract partial class CollisionSolvingSystem
{
    protected struct MovingObjectWithGroundCollisionSolver
    {
        public NativeList<CollisionData> CollisionDataList;
        public ComponentDataFromEntity<Translation> TranslationEntities;
        public ComponentDataFromEntity<MovementData> MovementDataEntities;

        public delegate void CorrectionDelegate(ref Translation movingEntityTranslation, ref MovementData movementData, ref AABB movingEntityBounds,
            float boundRefValue);

        public delegate void TackleHeadableBlockDelegate(int index, AABB movingEntityBounds, CollisionData collisionData,
            NativeArray<CollisionFromDirection> fromDirections);

        public delegate void CustomSetFromDirectionDelegate(int index, AABB movingEntityBounds, CollisionData collisionData,
            NativeArray<CollisionFromDirection> fromDirections);

        public CorrectionDelegate TopCorrectionMethod;
        public CorrectionDelegate BottomCorrectionMethod;
        public CorrectionDelegate RightCorrectionMethod;
        public CorrectionDelegate LeftCorrectionMethod;
        public TackleHeadableBlockDelegate TackleHeadableBlockMethod;
        public CustomSetFromDirectionDelegate CustomSetFromDirectionMethod;

        public void Execute(int beg, int end)
        {
            var fromDirections = new NativeArray<CollisionFromDirection>(end - beg, Allocator.TempJob);
            var entityA = CollisionDataList[beg].EntityA;
            var maxBoundYFromTop = float.NegativeInfinity;
            var minBoundYFromBottom = float.PositiveInfinity;
            var movingEntityBounds = CollisionDataList[beg].ColliderBoundsA;
            var movingEntityTranslation = TranslationEntities[entityA];
            var movementData = MovementDataEntities[entityA];
            for (int i = beg; i < end; i++)
            {
                fromDirections[i - beg] =
                    PhysicsUtility.CalculateCollisionFromDirection(movingEntityBounds.Size.y / movingEntityBounds.Size.x, CollisionDataList[i]);
                CustomSetFromDirectionMethod?.Invoke(i - beg, movingEntityBounds, CollisionDataList[i], fromDirections);
                switch (fromDirections[i - beg])
                {
                    case CollisionFromDirection.Top:
                        maxBoundYFromTop = math.max(maxBoundYFromTop, CollisionDataList[i].ColliderBoundsB.Max.y);
                        break;
                    case CollisionFromDirection.Bottom:
                        minBoundYFromBottom = math.min(minBoundYFromBottom, CollisionDataList[i].ColliderBoundsB.Min.y);
                        break;
                }
            }

            // There is at least one collision in which the moving object comes from the top of the static object.
            if (maxBoundYFromTop > float.NegativeInfinity)
            {
                for (int i = beg; i < end; i++)
                {
                    if (fromDirections[i - beg] == CollisionFromDirection.Top || fromDirections[i - beg] == CollisionFromDirection.Bottom)
                    {
                        continue;
                    }

                    var collisionData = CollisionDataList[i];
                    // Collisions from left or right should be viewed as from top sometimes.
                    if (collisionData.ColliderBoundsB.Max.y <= maxBoundYFromTop)
                    {
                        fromDirections[i - beg] = CollisionFromDirection.Top;
                    }
                }

                TopCorrectionMethod?.Invoke(ref movingEntityTranslation, ref movementData, ref movingEntityBounds, maxBoundYFromTop);
            }
            else if (minBoundYFromBottom < float.PositiveInfinity)
            {
                for (int i = beg; i < end; i++)
                {
                    if (fromDirections[i - beg] != CollisionFromDirection.Bottom)
                    {
                        continue;
                    }

                    TackleHeadableBlockMethod?.Invoke(i - beg, movingEntityBounds, CollisionDataList[i], fromDirections);
                }

                BottomCorrectionMethod?.Invoke(ref movingEntityTranslation, ref movementData, ref movingEntityBounds, minBoundYFromBottom);
            }


            var maxBoundXFromRight = float.NegativeInfinity;
            var minBoundXFromLeft = float.PositiveInfinity;
            for (int i = beg; i < end; i++)
            {
                switch (fromDirections[i - beg])
                {
                    case CollisionFromDirection.Right:
                        maxBoundXFromRight = math.max(maxBoundXFromRight, CollisionDataList[i].ColliderBoundsB.Max.x);
                        break;
                    case CollisionFromDirection.Left:
                        minBoundXFromLeft = math.min(minBoundXFromLeft, CollisionDataList[i].ColliderBoundsB.Min.x);
                        break;
                }
            }

            if (maxBoundXFromRight > float.NegativeInfinity)
            {
                RightCorrectionMethod?.Invoke(ref movingEntityTranslation, ref movementData, ref movingEntityBounds, maxBoundXFromRight);
            }
            else if (minBoundXFromLeft < float.PositiveInfinity)
            {
                LeftCorrectionMethod?.Invoke(ref movingEntityTranslation, ref movementData, ref movingEntityBounds, minBoundXFromLeft);
            }


            TranslationEntities[entityA] = movingEntityTranslation;
            MovementDataEntities[entityA] = movementData;
            fromDirections.Dispose();
        }
    }
}