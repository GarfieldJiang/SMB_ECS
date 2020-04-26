using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(PlayerInputSystem))]
[UpdateBefore(typeof(GravitySystem))]
public class PlayerMovementSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = TimeUtility.GetLimitedDeltaTime();
        return Entities.WithAll<PlayerTag>().ForEach((
            ref Translation translation,
            ref MovementData movementData,
            ref PlayerStates states,
            ref PlayerBouncedByEnemyData bouncedByEnemyData,
            ref PlayerInputData inputData,
            in PlayerMovementAbilityData movementAbilityData) =>
        {
            if (states.Main == PlayerMainStatus.Dying)
            {
                return;
            }

            UpdateHorizontal(ref movementData, inputData, movementAbilityData, deltaTime, states);


            if (PlayerUtility.IsGrounded(states))
            {
                UpdateJumpStart(ref states, ref movementData, inputData, movementAbilityData);
            }
            else if (states.Motion == PlayerMotionStatus.Jumping)
            {
                UpdateJumpKeep(ref states, ref movementData, movementAbilityData, inputData, deltaTime);
            }
            else if (states.Motion == PlayerMotionStatus.Falling)
            {
                UpdateBouncedByEnemy(ref bouncedByEnemyData, ref states, ref inputData, ref movementData, movementAbilityData);
            }

            bouncedByEnemyData.IsBounced = false;
        }).Schedule(inputDeps);
    }

    private static void UpdateBouncedByEnemy(ref PlayerBouncedByEnemyData bouncedByEnemyData,
        ref PlayerStates playerStates,
        ref PlayerInputData inputData, ref MovementData movementData,
        PlayerMovementAbilityData movementAbilityData)
    {
        if (!bouncedByEnemyData.IsBounced)
        {
            return;
        }

        var vel = movementData.Velocity;
        vel.y = movementAbilityData.BouncedByEnemySpeed;
        movementData.Velocity = vel;
        var ratio = math.abs(vel.x) / movementAbilityData.MaxHorizontalSpeedDashing;
        playerStates.JumpKeepTime = movementAbilityData.JumpKeepMinTime +
                              ratio * (movementAbilityData.JumpKeepMaxTime - movementAbilityData.JumpKeepMinTime);

        // TODO: Is it possible not to use InputData to fake a jump when bounced up by an enemy?
        inputData.LastFrameJumpKeep = true;
    }

    private static void UpdateJumpStart(ref PlayerStates states, ref MovementData movementData,
        PlayerInputData inputData,
        PlayerMovementAbilityData movementAbilityData)
    {
        if (!inputData.JumpHit)
        {
            return;
        }

        var vel = movementData.Velocity;
        vel.y = movementAbilityData.JumpHitSpeed;
        movementData.Velocity = vel;
        var ratio = math.abs(vel.x) / movementAbilityData.MaxHorizontalSpeedDashing;
        states.JumpKeepTime = movementAbilityData.JumpKeepMinTime +
                              ratio * (movementAbilityData.JumpKeepMaxTime - movementAbilityData.JumpKeepMinTime);
    }

    private static MovementData UpdateHorizontal(ref MovementData movementData, PlayerInputData inputData, PlayerMovementAbilityData movementAbilityData,
        float deltaTime, PlayerStates states)
    {
        var vel = movementData.Velocity;

        var maxHorizontalSpeed = inputData.FireKeep ? movementAbilityData.MaxHorizontalSpeedDashing : movementAbilityData.MaxHorizontalSpeed;
        var horizontalAcc = inputData.FireKeep ? movementAbilityData.HorizontalDashingAcc : movementAbilityData.HorizontalAcc;
        if (inputData.HorizontalInput != 0)
        {
            vel.x = math.clamp(vel.x + inputData.HorizontalInput * horizontalAcc * deltaTime,
                -maxHorizontalSpeed, maxHorizontalSpeed);
        }
        else if (PlayerUtility.IsGrounded(states))
        {
            var sign = math.sign(vel.x);
            var value = math.abs(vel.x);
            var amountToReduce = movementAbilityData.HorizontalReverseAcc * deltaTime;
            value = value < amountToReduce ? 0 : value - amountToReduce;
            vel.x = sign * value;
        }

        movementData.Velocity = vel;
        return movementData;
    }

    private static void UpdateJumpKeep(ref PlayerStates states, ref MovementData movementData,
        PlayerMovementAbilityData movementAbilityData,
        PlayerInputData inputData, float deltaTime)
    {
        if (states.JumpKeepTime <= 0)
        {
            return;
        }

        if (!inputData.LastFrameJumpHit && !inputData.LastFrameJumpKeep ||
            !inputData.JumpKeep)
        {
            states.JumpKeepTime = 0;
            return;
        }

        states.JumpKeepTime -= deltaTime;
        var vel = movementData.Velocity;
        vel.y = movementAbilityData.JumpHitSpeed;
        movementData.Velocity = vel;

        if (states.JumpKeepTime < 0)
        {
            states.JumpKeepTime = 0;
        }
    }
}