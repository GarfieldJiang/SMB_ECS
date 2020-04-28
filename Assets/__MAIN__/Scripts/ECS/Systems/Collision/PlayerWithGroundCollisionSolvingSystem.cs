using COL.UnityGameWheels.Unity;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(CollisionSystemGroup))]
[UpdateAfter(typeof(CollisionDataCollectingSystem))]
[UpdateBefore(typeof(PlayerWithEnemyCollisionSolvingSystem))]
public class PlayerWithGroundCollisionSolvingSystem : CollisionSolvingSystem
{
    public override string CollisionDataListKey => "PlayerWithGround";

    protected override void ProcessCollisionDataListNonEmpty()
    {
        var movingEntity = m_CollisionDataList[0].EntityA;
        var playerStatesEntities = GetComponentDataFromEntity<PlayerStates>();
        var playerStates = playerStatesEntities[movingEntity];
        int triggeredHeadables = 0;
        new MovingObjectWithGroundCollisionSolver
        {
            CollisionDataList = m_CollisionDataList,
            TranslationEntities = GetComponentDataFromEntity<Translation>(),
            MovementDataEntities = GetComponentDataFromEntity<MovementData>(),
            TackleHeadableBlockMethod = (_index, _movingEntityBounds, _collisionData, _fromDirections) =>
            {
                if (_collisionData.OverlapX > GameEntry.Instance.Config.Global.Collision.HeadableBlockOverlapXThreshold * _movingEntityBounds.Size.x)
                {
                    triggeredHeadables += 1;
                    var headableBlockTagEntities = GetComponentDataFromEntity<HeadableBlockTag>();
                    if (headableBlockTagEntities.HasComponent(_collisionData.EntityB))
                    {
                        var headableBlockTag = headableBlockTagEntities[_collisionData.EntityB];
                        headableBlockTag.IsHeaded = true;
                        headableBlockTagEntities[_collisionData.EntityB] = headableBlockTag;
                    }
                    else
                    {
                        GameEntry.Instance.Audio.PlaySoundEffect("smb_bump");
                    }
                }
                else
                {
                    _fromDirections[_index] = _collisionData.SignX > 0 ? CollisionFromDirection.Right : CollisionFromDirection.Left;
                }
            },
            TopCorrectionMethod = (ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
                float _boundRefValue) =>
            {
                _movingEntityTranslation.Value.y += _boundRefValue - _movingEntityBounds.Min.y + Allowance;
                if (_movementData.Velocity.y < 0)
                {
                    // Player specific
                    playerStates.FallingStopped = true;
                }

                _movementData.Velocity.y = math.max(0, _movementData.Velocity.y);
            },
            BottomCorrectionMethod = (ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
                float _boundRefValue) =>
            {
                if (triggeredHeadables > 0)
                {
                    _movingEntityTranslation.Value.y -= _movingEntityBounds.Max.y - _boundRefValue + Allowance;
                    _movementData.Velocity.y = math.min(0, _movementData.Velocity.y);
                }
            },
            RightCorrectionMethod = RightCorrectionMethod,
            LeftCorrectionMethod = LeftCorrectionMethod,
        }.Execute(0, m_CollisionDataList.Length);

        playerStatesEntities[movingEntity] = playerStates;
    }

    private void LeftCorrectionMethod(ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
        float _boundRefValue)
    {
        _movingEntityTranslation.Value.x -= _movingEntityBounds.Max.x - _boundRefValue + Allowance;
        _movementData.Velocity.x = math.min(0, _movementData.Velocity.x);
    }

    private void RightCorrectionMethod(ref Translation _movingEntityTranslation, ref MovementData _movementData, ref AABB _movingEntityBounds,
        float _boundRefValue)
    {
        _movingEntityTranslation.Value.x += _boundRefValue - _movingEntityBounds.Min.x + Allowance;
        _movementData.Velocity.x = math.max(0, _movementData.Velocity.x);
    }
}