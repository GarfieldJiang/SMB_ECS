using System;
using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Core.Asset;
using COL.UnityGameWheels.Unity;
using COL.UnityGameWheels.Unity.Asset;
using Unity.Entities;
using UnityEngine;

public class GameEntry : MonoBehaviourEx
{
    [SerializeField]
    private AssetManager m_AssetManager = null;

    public AssetManager Asset => m_AssetManager;

    [SerializeField]
    private AudioManager m_AudioManager = null;

    public AudioManager Audio => m_AudioManager;

    [SerializeField]
    private RefPoolManager m_RefPoolManager = null;

    public RefPoolManager RefPool => m_RefPoolManager;

    [SerializeField]
    private ConfigManager m_ConfigManager = null;

    public ConfigManager Config => m_ConfigManager;

    [SerializeField]
    private SceneManager m_SceneManager = null;

    public SceneManager Scene => m_SceneManager;

    [SerializeField]
    private UIManager m_UIManager = null;

    public UIManager UI => m_UIManager;

    [SerializeField]
    private EventManager m_EventManager = null;

    public EventManager Event => m_EventManager;

    [SerializeField]
    private PlayerDataManager m_PlayerDataManager = null;

    public PlayerDataManager PlayerData => m_PlayerDataManager;

    [SerializeField]
    private SharedDataManager m_SharedDataManager = null;

    public SharedDataManager SharedData => m_SharedDataManager;

    [SerializeField]
    private PlayerDeathManager m_PlayerDeathManager = null;

    public PlayerDeathManager PlayerDeath => m_PlayerDeathManager;

    [SerializeField]
    private Transform m_MainCameraTransform = null;

    public Transform MainCameraTransform => m_MainCameraTransform;

    [SerializeField]
    private Camera m_MainCamera = null;

    public Camera MainCamera => m_MainCamera;

    [SerializeField]
    private CameraAdapter m_MainCameraAdapter = null;

    public CameraAdapter MainCameraAdapter => m_MainCameraAdapter;

    public static GameEntry Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        if (Instance != null)
        {
            throw new InvalidOperationException($"There is already a [{GetType()}] instance.");
        }

        Instance = this;
        Log.SetLogger(new LoggerImpl());
    }

    private void Start()
    {
        Asset.RefPoolModule = RefPool.Module;
        Event.EventArgsReleaser = new SimpleEventArgsReleaser(RefPool);
        RefPool.Init();
        Event.Init();
        Asset.Init();
        PlayerData.Init();
        PlayerDeath.Init();
        gameObject.AddComponent<AudioHandler>();

        Asset.Prepare(new AssetManagerPrepareCallbackSet
        {
            OnSuccess = OnAssetModulePrepareSuccess,
        }, null);
    }

    private void OnAssetModulePrepareSuccess(object context)
    {
        Asset.CheckUpdate(null, new UpdateCheckCallbackSet
        {
            OnSuccess = OnAssetUpdateCheckSuccess,
        }, null);
    }

    private void OnAssetUpdateCheckSuccess(object context)
    {
        DefaultWorldInitialization.Initialize("Default World", false);
        Audio.LoadAll(() => { UI.Open(UIPage.Cover, null); });
    }

    protected override void OnDestroy()
    {
        Instance = null;
        base.OnDestroy();
    }

    private class SimpleEventArgsReleaser : IEventArgsReleaser
    {
        private readonly IRefPoolManager m_RefPoolManager;

        public SimpleEventArgsReleaser(IRefPoolManager refPoolManager)
        {
            m_RefPoolManager = refPoolManager;
        }

        public void Release(BaseEventArgs eventArgs)
        {
            // Clean up fields in event args instance, if needed.
            m_RefPoolManager.GetOrAdd(eventArgs.GetType()).ReleaseObject(eventArgs);
        }
    }
}