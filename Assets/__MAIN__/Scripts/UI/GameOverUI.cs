using Unity.Entities;
using UnityEngine;

public class GameOverUI : UILogic
{
    [SerializeField]
    private UIHead m_UIHead = null;

    [SerializeField]
    private float m_MinimumShowingTime = 3;

    private float m_TimeLeft;

    public override void OnOpen(object context)
    {
        base.OnOpen(context);
        m_UIHead.ClearTimeCount();
        m_TimeLeft = m_MinimumShowingTime;
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<DestroyAllEntitiesSystem>().Enabled = true;
        GameEntry.Instance.Audio.PlayMusic("09-game-over", false);
    }

    private void Update()
    {
        if (m_TimeLeft <= 0)
        {
            return;
        }

        m_TimeLeft -= Time.deltaTime;
        if (m_TimeLeft <= 0)
        {
            GameEntry.Instance.UI.Open(UIPage.Cover, null);
        }
    }
}