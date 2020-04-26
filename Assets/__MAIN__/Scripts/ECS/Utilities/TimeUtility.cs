using UnityEngine;

public static class TimeUtility
{
    public const float DeltaTimeLimit = .05f;

    public static float GetLimitedDeltaTime() => Mathf.Min(DeltaTimeLimit, Time.deltaTime);
}