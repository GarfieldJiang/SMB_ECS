using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(TransformSystemGroup))]
public class PlayerMotionStatusSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entities.WithAll<PlayerTag>().WithoutBurst().ForEach((ref PlayerStates states, in MovementData movementData) =>
        {
            if (states.Main != PlayerMainStatus.Alive)
            {
                return;
            }

            var wasGrounded = PlayerUtility.IsGrounded(states);
            var lastMotionStatus = states.Motion;

            if (states.FallingStopped || wasGrounded && math.abs(movementData.Velocity.y) < float.Epsilon)
            {
                if (math.abs(movementData.Velocity.x) < float.Epsilon)
                {
                    states.Motion = PlayerMotionStatus.Static;
                }
                else
                {
                    states.Motion = PlayerMotionStatus.Running;
                }
            }
            else
            {
                if (movementData.Velocity.y > 0)
                {
                    states.Motion = PlayerMotionStatus.Jumping;
                }
                else
                {
                    states.Motion = PlayerMotionStatus.Falling;
                }
            }

            // Reset flags.
            states.FallingStopped = false;
            if (states.Motion != PlayerMotionStatus.Jumping)
            {
                states.JumpKeepTime = 0;
            }

            if (lastMotionStatus != states.Motion)
            {
                var eventArgs = GameEntry.Instance.RefPool.GetOrAdd<PlayerMotionStatusChangeEventArgs>().Acquire();
                eventArgs.LastMotionStatus = lastMotionStatus;
                eventArgs.PlayerStates = states;
                GameEntry.Instance.Event.SendEvent(this, eventArgs);
            }
        }).Run();
        return default;
    }
}