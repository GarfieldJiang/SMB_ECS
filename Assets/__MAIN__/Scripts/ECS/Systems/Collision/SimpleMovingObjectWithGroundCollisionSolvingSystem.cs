using COL.UnityGameWheels.Unity;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Assertions;

[UpdateInGroup(typeof(CollisionSystemGroup))]
[UpdateAfter(typeof(CollisionDataCollectingSystem))]
[UpdateBefore(typeof(CollisionDataClearingSystem))]
public class SimpleMovingObjectWithGroundCollisionSolvingSystem : MultipleEntityACollisionSolvingSystem
{
    protected override void ProcessOneEntityA(Entity entityA, int beg, int end)
    {
        var movingAbilityDataEntities = GetComponentDataFromEntity<SimpleMovementAbilityData>();
        if (movingAbilityDataEntities.HasComponent(entityA) && !movingAbilityDataEntities[entityA].InteractsWithGround)
        {
            return;
        }

        var movingAbilityData = movingAbilityDataEntities.HasComponent(entityA) ? movingAbilityDataEntities[entityA] : default;
        new MovingObjectWithGroundCollisionSolver
        {
            CollisionDataList = m_CollisionDataList,
            TranslationEntities = GetComponentDataFromEntity<Translation>(),
            MovementDataEntities = GetComponentDataFromEntity<MovementData>(),
            TopCorrectionMethod = (ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
                float _boundRefValue) =>
            {
                _movingEntityTranslation.Value.y += _boundRefValue - _movingEntityBounds.Min.y + Allowance;
                _movementData.Velocity.y = movingAbilityData.BouncedVerticalSpeed > 0
                    ? movingAbilityData.BouncedVerticalSpeed
                    : math.max(0, _movementData.Velocity.y);
            },
            BottomCorrectionMethod = BottomCorrectionMethod,
            RightCorrectionMethod = RightCorrectionMethod,
            LeftCorrectionMethod = LeftCorrectionMethod,
        }.Execute(beg, end);
    }

    private void LeftCorrectionMethod(ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
        float _boundRefValue)
    {
        _movingEntityTranslation.Value.x -= _movingEntityBounds.Max.x - _boundRefValue + Allowance;
        _movementData.Velocity.x = -math.abs(_movementData.Velocity.x);
    }

    private void RightCorrectionMethod(ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
        float _boundRefValue)
    {
        _movingEntityTranslation.Value.x += _boundRefValue - _movingEntityBounds.Min.x + Allowance;
        _movementData.Velocity.x = math.abs(_movementData.Velocity.x);
    }

    private void BottomCorrectionMethod(ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
        float _boundRefValue)
    {
        _movingEntityTranslation.Value.y -= _movingEntityBounds.Max.y - _boundRefValue + Allowance;
        _movementData.Velocity.y = math.min(0, _movementData.Velocity.y);
    }

    public override string CollisionDataListKey => "SimpleMovingObjectWithGround";
}