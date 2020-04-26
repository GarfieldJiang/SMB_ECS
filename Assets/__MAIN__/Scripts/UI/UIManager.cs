using System;
using System.Collections.Generic;
using COL.UnityGameWheels.Core.Asset;
using COL.UnityGameWheels.Unity;
using UnityEngine;
using UnityEngine.Assertions;

public class UIManager : MonoBehaviourEx
{
    private struct OpenedUIData
    {
        public GameObject RootGO;
        public IAssetAccessor AssetAccessor;
    }

    private struct OpenUIContext
    {
        public object Context;
        public UIPage UIPage;
    }

    [SerializeField]
    private Transform RootTransform = null;

    private HashSet<UIPage> m_BeingOpenedUIs;
    private Dictionary<UIPage, OpenedUIData> m_OpenedUIs;

    protected override void Awake()
    {
        base.Awake();
        m_BeingOpenedUIs = new HashSet<UIPage>();
        m_OpenedUIs = new Dictionary<UIPage, OpenedUIData>();
    }

    public bool IsUIBeingOpened(UIPage uiPage) => m_BeingOpenedUIs.Contains(uiPage);

    public bool IsUIOpened(UIPage uiPage) => m_OpenedUIs.ContainsKey(uiPage);


    public void Open(UIPage uiPage, object context)
    {
        Assert.IsFalse(IsUIOpened(uiPage) || IsUIBeingOpened(uiPage));
        var config = GameEntry.Instance.Config.UI;
        foreach (var configItem in config.Items)
        {
            if (configItem.UIPage == uiPage)
            {
                Assert.IsTrue(m_BeingOpenedUIs.Add(uiPage));
                GameEntry.Instance.Asset.LoadAsset(configItem.AssetPath, new LoadAssetCallbackSet
                {
                    OnSuccess = OnLoadUIAssetSuccess,
                }, new OpenUIContext
                {
                    Context = context,
                    UIPage = uiPage,
                });
                return;
            }
        }

        throw new Exception($"UI [{uiPage}] doesn't exist in config.");
    }

    public void Close(UIPage uiPage)
    {
        if (!m_OpenedUIs.TryGetValue(uiPage, out var data))
        {
            throw new InvalidOperationException($"Trying to close [{uiPage}] that isn't opened.");
        }

        var uiLogic = data.RootGO.GetComponent<UILogic>();
        uiLogic.OnClose();
        Destroy(data.RootGO);
        GameEntry.Instance.Asset.UnloadAsset(data.AssetAccessor);
        m_OpenedUIs.Remove(uiPage);
    }

    private void OnLoadUIAssetSuccess(IAssetAccessor assetAccessor, object context)
    {
        var originalContext = (OpenUIContext)context;
        m_BeingOpenedUIs.Remove(originalContext.UIPage);
        var go = Instantiate((GameObject)assetAccessor.AssetObject, RootTransform);
        m_OpenedUIs.Add(originalContext.UIPage, new OpenedUIData {AssetAccessor = assetAccessor, RootGO = go});
        var uiLogic = go.GetComponent<UILogic>();
        uiLogic.OnOpen(originalContext.Context);
    }
}