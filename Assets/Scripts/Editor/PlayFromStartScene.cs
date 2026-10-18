using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Play mode always begins on the start screen.
/// </summary>
[InitializeOnLoad]
static class PlayFromStartScene
{
    const string StartScenePath = "Assets/Scenes/StartScene.unity";

    static PlayFromStartScene()
    {
        SceneAsset startScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePath);
        if (startScene != null)
            EditorSceneManager.playModeStartScene = startScene;
    }
}
