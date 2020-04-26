using COL.UnityGameWheels.Core;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class EnteringStageUI : UILogic
{
    [SerializeField]
    private UIHead m_UIHead = null;

    [SerializeField]
    private Text m_LifeCount = null;

    [SerializeField]
    private float m_MinimumShowingTime = 3;

    private float m_OpenTime;
    private bool m_MinimumShowingTimeSatisfied = false;
    private bool m_PlayerCharacterCreatedEventHeard = false;

    public override void OnOpen(object context)
    {
        base.OnOpen(context);
        m_MinimumShowingTimeSatisfied = false;
        m_PlayerCharacterCreatedEventHeard = false;
        m_OpenTime = Time.realtimeSinceStartup;
        m_UIHead.ClearTimeCount();
        m_LifeCount.text = "x " + GameEntry.Instance.PlayerData.Lives;

        if (GameEntry.Instance.UI.IsUIOpened(UIPage.Cover))
        {
            GameEntry.Instance.UI.Close(UIPage.Cover);
        }

        World.DefaultGameObjectInjectionWorld.GetExistingSystem<DestroyAllEntitiesSystem>().Enabled = true;

        GameEntry.Instance.Scene.LoadScene(new SceneData
        {
            SceneName = "SampleScene",
            CameraMaxX = 1000f,
            CameraStartX = 7f,
            PlayerSpawningPosition = new float3(1, 1.5f, -.5f),
            MusicName = "01-main-theme-overworld",
        }, () =>
        {
            var cameraPosition = GameEntry.Instance.MainCameraTransform.position;
            cameraPosition.x = GameEntry.Instance.Scene.SceneData.CameraStartX;
            cameraPosition.y = GameEntry.Instance.Config.Global.Camera.PositionY;
            GameEntry.Instance.MainCameraTransform.position = cameraPosition;
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<PlayerCharacterSpawningSystem>().Enabled = true;
        });

        GameEntry.Instance.Event.AddEventListener(PlayerCharacterCreatedEventArgs.TheEventId, OnPlayerCharacterCreated);
    }

    void Update()
    {
        if (!m_MinimumShowingTimeSatisfied && Time.realtimeSinceStartup - m_OpenTime < m_MinimumShowingTime)
        {
            return;
        }

        m_MinimumShowingTimeSatisfied = true;
        if (m_PlayerCharacterCreatedEventHeard)
        {
            CloseSelf();
        }
    }

    public override void OnClose()
    {
        GameEntry.Instance.Event.RemoveEventListener(PlayerCharacterCreatedEventArgs.TheEventId, OnPlayerCharacterCreated);
        base.OnClose();
    }

    private void OnPlayerCharacterCreated(object sender, BaseEventArgs e)
    {
        m_PlayerCharacterCreatedEventHeard = true;
        if (m_MinimumShowingTimeSatisfied)
        {
            CloseSelf();
        }
    }

    private void CloseSelf()
    {
        GameEntry.Instance.UI.Close(UIPage.EnteringStage);
        GameEntry.Instance.Audio.PlayMusic(GameEntry.Instance.Scene.SceneData.MusicName, true);
    }
}