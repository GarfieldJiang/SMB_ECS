using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using COL.UnityGameWheels.Unity.Editor;
using UnityEditor.Build.Content;

internal static class MenuEntry
{
    private const string MenuPrefix = "SMB_ECS/";

    [MenuItem(MenuPrefix + "Asset Bundle/Organizer")]
    private static void OpenAssetBundleOrganizerEditorWindow()
    {
        AssetBundleOrganizerEditorWindow.Open();
    }

    [MenuItem(MenuPrefix + "Asset Bundle/Builder")]
    private static void OpenAssetBundleBuilderEditorWindow()
    {
        AssetBundleBuilderEditorWindow.Open();
    }

    [MenuItem(MenuPrefix + "Asset Bundle/Build For Current Platform/Copy Client")]
    private static void BuildAssetBundlesForCurrentPlatformAndCopyClient()
    {
        BuildAssetBundlesForCurrentPlatform(false);
    }

    [MenuItem(MenuPrefix + "Asset Bundle/Build For Current Platform/Copy Client Full")]
    private static void BuildAssetBundlesForCurrentPlatformAndCopyClientFull()
    {
        BuildAssetBundlesForCurrentPlatform(true);
    }

    private static void BuildAssetBundlesForCurrentPlatform(bool copyClientFull)
    {
        var assetBundleBuilder = new AssetBundleBuilder();
        var resourcePlatform = AssetBundleBuilder.GetResourcePlatformFromBuildTarget(EditorUserBuildSettings.activeBuildTarget);
        var internalResourceVersion = AssetBundleBuilder.GetInternalResourceVersion(Application.version, resourcePlatform);
        assetBundleBuilder.BuildPlatform(resourcePlatform, true, false, BuildAssetBundleOptions.None);
        var outputDirectory = Path.Combine(assetBundleBuilder.GetOutputDirectory(resourcePlatform, internalResourceVersion),
            copyClientFull ? AssetBundleBuilder.ClientFullFolderName : AssetBundleBuilder.ClientFolderName);

        var di = new DirectoryInfo(Application.streamingAssetsPath);
        foreach (var file in di.GetFiles())
        {
            if (file.Name.StartsWith("."))
            {
                continue;
            }

            file.Delete();
        }

        foreach (var dir in di.GetDirectories())
        {
            dir.Delete(true);
        }

        foreach (var dirPath in Directory.GetDirectories(outputDirectory, "*",
            SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(outputDirectory, Application.streamingAssetsPath));
        }

        foreach (var filePath in Directory.GetFiles(outputDirectory, "*.*",
            SearchOption.AllDirectories))
        {
            if (Regex.IsMatch(Path.GetFileName(filePath) ?? string.Empty, @".*index\..*\.dat\.json"))
            {
                continue;
            }

            File.Copy(filePath, filePath.Replace(outputDirectory, Application.streamingAssetsPath));
        }

        AssetDatabase.Refresh();
    }

    [MenuItem(MenuPrefix + "Scriptable Object Creator")]
    private static void OpenScriptableObjectCreatorEditorWindow()
    {
        ScriptableObjectCreatorEditorWindow.Open();
    }

    [MenuItem(MenuPrefix + "Project/Save &s")]
    private static void SaveProject()
    {
        Utility.Project.SaveProject();
    }

    [MenuItem(MenuPrefix + "Project/Open Data Path")]
    private static void OpenDataPath()
    {
        Utility.Project.OpenDataPath();
    }

    [MenuItem(MenuPrefix + "Project/Open Persistent Data Path")]
    private static void OpenPersistentDataPath()
    {
        Utility.Project.OpenPersistentDataPath();
    }


    [MenuItem(MenuPrefix + "Project/Open Streaming Assets Path")]
    private static void OpenStreamingAssetsPath()
    {
        Utility.Project.OpenStreamingAssetsPath();
    }

    [MenuItem(MenuPrefix + "Project/Open Temporary Cache Path")]
    private static void OpenTemporaryCachePath()
    {
        Utility.Project.OpenTemporaryCachePath();
    }

    [MenuItem(MenuPrefix + "Build game")]
    private static void BuildGame()
    {
        BuildAssetBundlesForCurrentPlatformAndCopyClientFull();
        var dataDirParentPath = Directory.GetParent(Application.dataPath).FullName;
        var dataDirGrandpaPath = Directory.GetParent(dataDirParentPath).FullName;
        var dataDirParentName = Path.GetFileName(dataDirParentPath);
        var outputPath = Path.Combine(dataDirGrandpaPath, dataDirParentName + "_Build", EditorUserBuildSettings.activeBuildTarget.ToString());
        BuildPipeline.BuildPlayer(new[]
        {
            EditorBuildSettings.scenes.Single(scene => Path.GetFileNameWithoutExtension(scene.path) == "Launch")
        }, outputPath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
    }
}