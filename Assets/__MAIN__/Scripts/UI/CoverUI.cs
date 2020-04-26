using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CoverUI : UILogic
{
    private bool m_StartedGame = false;

    public override void OnOpen(object context)
    {
        base.OnOpen(context);
        if (GameEntry.Instance.UI.IsUIOpened(UIPage.GameOver))
        {
            GameEntry.Instance.UI.Close(UIPage.GameOver);
        }

        GameEntry.Instance.PlayerData.Reset();
    }

    public override void OnClose()
    {
        m_StartedGame = false;
        base.OnClose();
    }

    private void Update()
    {
        if (m_StartedGame)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            m_StartedGame = true;
            GameEntry.Instance.UI.Open(UIPage.EnteringStage, null);
        }
    }
}