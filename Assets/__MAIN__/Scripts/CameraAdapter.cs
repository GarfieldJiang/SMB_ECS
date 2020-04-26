using COL.UnityGameWheels.Unity;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Camera))]
//[ExecuteInEditMode]
public class CameraAdapter : MonoBehaviourEx
{
    private Camera m_Camera;

    [SerializeField]
    private Vector2 m_TargetScreenRatio = new Vector2(16, 15);

    public float2 TargetScreenRatio => new float2(m_TargetScreenRatio);

    protected override void Awake()
    {
        base.Awake();
        m_Camera = GetComponent<Camera>();
        UpdateCamera();
    }

#if UNITY_EDITOR
    private void Update()
    {
        UpdateCamera();
    }
#endif

    private void UpdateCamera()
    {
        var ratio = (float)Screen.width / Screen.height;
        var targetRatio = m_TargetScreenRatio.x / m_TargetScreenRatio.y;
        if (ratio > targetRatio)
        {
            m_Camera.rect = new Rect((1f - targetRatio / ratio) / 2f, 0, targetRatio / ratio, 1);
        }
        else
        {
            m_Camera.rect = new Rect(0, (1f - ratio / targetRatio) / 2f, 1, ratio / targetRatio);
        }
    }
}