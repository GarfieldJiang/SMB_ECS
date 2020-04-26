using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[AlwaysSynchronizeSystem]
public class PlayerInputSystem : JobComponentSystem
{
    private const KeyCode Left = KeyCode.A;
    private const KeyCode Right = KeyCode.D;
    private const KeyCode Down = KeyCode.S;
    private const KeyCode Fire = KeyCode.J;
    private const KeyCode Jump = KeyCode.K;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entities.WithAll<PlayerTag>().ForEach((ref PlayerInputData inputData) =>
        {
            inputData.HorizontalInput = 0;
            inputData.HorizontalInput -= Input.GetKey(Left) ? 1 : 0;
            inputData.HorizontalInput += Input.GetKey(Right) ? 1 : 0;
            inputData.Down = Input.GetKey(Down);
            inputData.LastFrameFireHit = inputData.FireHit;
            inputData.FireHit = Input.GetKeyDown(Fire);
            inputData.LastFrameFireKeep = inputData.FireKeep;
            inputData.FireKeep = Input.GetKey(Fire);
            inputData.LastFrameJumpHit = inputData.JumpHit;
            inputData.JumpHit = Input.GetKeyDown(Jump);
            inputData.LastFrameJumpKeep = inputData.JumpKeep;
            inputData.JumpKeep = Input.GetKey(Jump);
        }).Run();
        return default;
    }
}