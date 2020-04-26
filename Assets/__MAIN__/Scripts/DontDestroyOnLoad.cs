using COL.UnityGameWheels.Unity;

public class DontDestroyOnLoad : MonoBehaviourEx
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}