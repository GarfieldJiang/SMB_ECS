using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(CollisionSystemGroup))]
[UpdateAfter(typeof(CollisionDataCollectingSystem))]
[UpdateBefore(typeof(CollisionDataClearingSystem))]
public class PlayerBulletWithGroundCollisionSolvingSystem : MultipleEntityACollisionSolvingSystem
{
    protected override void ProcessOneEntityA(Entity entityA, int beg, int end)
    {
        new MovingObjectWithGroundCollisionSolver
        {
            CollisionDataList = m_CollisionDataList,
            TranslationEntities = GetComponentDataFromEntity<Translation>(),
            MovementDataEntities = GetComponentDataFromEntity<MovementData>(),
            TopCorrectionMethod = TopCorrectionMethod,
            BottomCorrectionMethod = BottomCorrectionMethod,
            RightCorrectionMethod = (ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
                float _boundRefValue) =>
            {
                GameEntry.Instance.SharedData.FireBallsOnHit.Enqueue(new FireBallOnHitData {FireBallEntity = entityA, IsHittingGround = true});
            },
            LeftCorrectionMethod = (ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
                float _boundRefValue) =>
            {
                GameEntry.Instance.SharedData.FireBallsOnHit.Enqueue(new FireBallOnHitData {FireBallEntity = entityA, IsHittingGround = true});
            },
            CustomSetFromDirectionMethod = CustomSetFromDirectionMethod,
        }.Execute(beg, end);
    }

    private void CustomSetFromDirectionMethod(int _index, AABB _movingEntityBounds, CollisionData _collisionData,
        NativeArray<CollisionFromDirection> _fromDirections)
    {
        if (_collisionData.OverlapY < _movingEntityBounds.Size.y - Allowance)
        {
            _fromDirections[_index] = _collisionData.SignY >= 0 ? CollisionFromDirection.Top : CollisionFromDirection.Bottom;
        }
    }

    private void BottomCorrectionMethod(ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
        float _boundRefValue)
    {
        _movingEntityTranslation.Value.y -= _movingEntityBounds.Max.y - _boundRefValue + Allowance;
        _movementData.Velocity.y = math.min(0, _movementData.Velocity.y);
    }

    private void TopCorrectionMethod(ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
        float _boundRefValue)
    {
        _movingEntityTranslation.Value.y += _boundRefValue - _movingEntityBounds.Min.y + Allowance;
        _movementData.Velocity.y = math.abs(_movementData.Velocity.y);
    }

    public override string CollisionDataListKey => "PlayerBulletWithGround";
}