using COL.UnityGameWheels.Unity;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
[ExecuteInEditMode]
public class CanvasScalerUpdater : MonoBehaviourEx
{
    private CanvasScaler m_CanvasScaler;

    protected override void Awake()
    {
        base.Awake();
        m_CanvasScaler = GetComponent<CanvasScaler>();
        UpdateCanvasScaler();
    }

    void Start()
    {
        UpdateCanvasScaler();
    }

    void Update()
    {
        UpdateCanvasScaler();
    }

    private void UpdateCanvasScaler()
    {
        var referenceResolution = m_CanvasScaler.referenceResolution;
        m_CanvasScaler.matchWidthOrHeight = referenceResolution.y * Screen.width < referenceResolution.x * Screen.height ? 0 : 1;
    }
}