using System;
using System.IO;
using COL.UnityGameWheels.Core.Asset;
using COL.UnityGameWheels.Unity;

public class SceneManager : MonoBehaviourEx
{
    private const string SceneRootPath = "Assets/__MAIN__/Scenes";
    private const string SceneFileExt = ".unity";
    private SceneData m_SceneData;
    private bool m_IsLoadingScene;
    private IAssetAccessor m_SceneAssetAccessor;
    private Action m_OnSceneLoaded;

    public SceneData SceneData => m_SceneData;

    public void LoadScene(SceneData sceneData, Action onSceneLoaded)
    {
        if (m_IsLoadingScene)
        {
            throw new InvalidOperationException($"Scene [{m_SceneData.SceneName}] is being loaded while you're trying to load scene [{sceneData.SceneName}]");
        }

        if (string.IsNullOrEmpty(sceneData.SceneName))
        {
            throw new InvalidOperationException("Invalid scene name.");
        }

        if (m_SceneAssetAccessor != null)
        {
            GameEntry.Instance.Asset.UnloadAsset(m_SceneAssetAccessor);
            m_SceneAssetAccessor = null;
        }

        m_SceneData = sceneData;
        m_OnSceneLoaded = onSceneLoaded;
        m_IsLoadingScene = true;
        GameEntry.Instance.Asset.LoadSceneAsset(Path.Combine(SceneRootPath, m_SceneData.SceneName + SceneFileExt), new LoadAssetCallbackSet
        {
            OnSuccess = OnLoadSceneAssetSuccess,
        }, null);
    }

    private void OnLoadSceneAssetSuccess(IAssetAccessor assetAccessor, object context)
    {
        m_SceneAssetAccessor = assetAccessor;
        StartCoroutine(LoadUnitySceneCo());
    }

    private System.Collections.IEnumerator LoadUnitySceneCo()
    {
        // TODO: Destroy entities.
        var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(m_SceneAssetAccessor.AssetPath);
        yield return asyncOperation;
        m_IsLoadingScene = false;
        m_OnSceneLoaded?.Invoke();
    }
}