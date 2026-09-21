using UnityEditor;

public static class ForceSceneLighting
{
    [MenuItem("Tools/Force Scene Lighting ON")]
    public static void EnableSceneLighting()
    {
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.sceneLighting = true;
            SceneView.lastActiveSceneView.Repaint();
        }
    }
}