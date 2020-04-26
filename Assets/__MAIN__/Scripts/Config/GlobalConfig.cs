using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class GlobalConfig : ScriptableObject
{
    public PlayerConfig Player;

    public PlayerLevelDownProtectionRenderingConfig PlayerLevelDownProtectionRendering;

    public FireBallConfig FireBall;

    [Tooltip("Moving parameters of question marks and bricks when headed by the player.")]
    public MovableBlockConfig MovableBlock;

    public CollisionConfig Collision;

    public CameraConfig Camera;

    public ItemConfig Item;

    public CoinConfig Coin;

    public MovingEntitiesConfig MovingEntities;

    public GravityConfig Gravity;

    public BrickFragmentsConfig BrickFragments;

    [Serializable]
    public class MovableBlockConfig
    {
        public float VerticalSpeed;
        public float VerticalRange;
    }

    [Serializable]
    public class BrickFragmentsConfig
    {
        public float4 HorizontalVelocities;
        public float4 VerticalVelocities;
        public float4 ScaleXs;

        [FormerlySerializedAs("OffsetX")]
        public float4 OffsetXs;

        [FormerlySerializedAs("OffsetY")]
        public float4 OffsetYs;
    }

    [Serializable]
    public class CollisionConfig
    {
        [Tooltip("How much on the x axis must the player and the headable block overlap (ratio of the block size) to trigger the block.")]
        public float HeadableBlockOverlapXThreshold = .25f;
    }

    [Serializable]
    public class CameraConfig
    {
        public float PositionY = 4;
    }

    [Serializable]
    public class ItemConfig
    {
        public float ItemGravityAcc = 9.8f;
        public float SpawnOffsetY = 0.3f;
        public float StarInvincibleTotalTime = 13f;
        public float StarInvincibleMainTime = 11.5f;
    }

    [Serializable]
    public class CoinConfig
    {
        public int BaseScore = 200;
        public int CountToAddLife = 100;
    }

    [Serializable]
    public class MovingEntitiesConfig
    {
        public float DistanceXThresholdFromCameraToCreate = 9f;

        public float DistanceXThresholdFromCameraToDestroy = 30f;

        public float MinPositionY = -4;

        public float MaxPositionY = 14;
    }

    [Serializable]
    public class PlayerConfig
    {
        public int InitialLives = 3;

        [Tooltip("Consider the player character as dead when it falls under this height value.")]
        public float PositionYToDie = -3;

        public float LevelChangeTime = 1;

        public float3 DefaultScale = new float3(.7f, .9f, 1f);
        public float3 SuperScale = new float3(.95f, 1.9f, 1f);
        public float LevelDownProtectionTime = 2;
    }

    [Serializable]
    public class PlayerLevelDownProtectionRenderingConfig
    {
        [Tooltip("Now we simply enable/disable the renderer.")]
        public float FlashingPeriod = .2f;
    }

    [Serializable]
    public class GravityConfig
    {
        public float VerticalSpeedLimit = 10;
    }

    [Serializable]
    public class FireBallConfig
    {
        [Tooltip("Both components should be positive.")]
        public float2 Speed = new float2(13, 13);

        public float BounceHeight = 1;

        public float2 SpawnOffset = new float2(.4f, 0f);

        public int MaxCount = 2;

        public float DistanceXThresholdFromCameraToDestroy = 9;
    }
}