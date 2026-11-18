using Herbalist.GameUI;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(SpotlightWorldRingSetup))]
public sealed class SpotlightWorldRingSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        var setup = (SpotlightWorldRingSetup)target;
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Ring Preview", EditorStyles.boldLabel);
        DrawInspectorPreview(setup);
        EditorGUILayout.HelpBox(
            "Change Ring Radius and the preview updates immediately. In Scene view, drag the outer radius handle.",
            MessageType.Info);
    }

    private void OnSceneGUI()
    {
        if (Application.isPlaying) return;

        var setup = (SpotlightWorldRingSetup)target;
        var sceneView = SceneView.currentDrawingSceneView ?? SceneView.lastActiveSceneView;
        if (sceneView == null || sceneView.camera == null) return;

        Vector3 center = setup.transform.position;
        Transform view = sceneView.camera.transform;
        Vector3 axisX = view.right;
        Vector3 axisY = view.up;
        Color previousColor = Handles.color;
        DrawWorldDashes(setup, center, axisX, axisY, setup.GlowWidth * 70f, setup.GlowColor);
        DrawWorldDashes(setup, center, axisX, axisY, setup.CoreWidth * 70f, setup.CoreColor);
        Handles.color = previousColor;

        EditorGUI.BeginChangeCheck();
        float radius = Handles.RadiusHandle(view.rotation, center, setup.RingRadius);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(setup, "Adjust spotlight ring radius");
            var radiusProperty = serializedObject.FindProperty("_ringRadius");
            radiusProperty.floatValue = Mathf.Max(.01f, radius);
            serializedObject.ApplyModifiedProperties();
        }
    }

    private static void DrawInspectorPreview(SpotlightWorldRingSetup setup)
    {
        Rect rect = GUILayoutUtility.GetRect(120f, 150f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, new Color(.07f, .09f, .07f, 1f));
        Vector2 center = rect.center + Vector2.down * 4f;
        float radius = Mathf.Min(rect.width * .32f, rect.height * .34f);

        Handles.BeginGUI();
        DrawGuiDashes(center, radius, setup, setup.GlowWidth * 70f, setup.GlowColor);
        DrawGuiDashes(center, radius, setup, setup.CoreWidth * 70f, setup.CoreColor);
        Handles.EndGUI();

        GUI.Label(new Rect(rect.x + 8f, rect.y + 5f, rect.width - 16f, 20f),
            $"Radius {setup.RingRadius:F2}", EditorStyles.miniLabel);
    }

    private static void DrawGuiDashes(Vector2 center, float radius, SpotlightWorldRingSetup setup,
        float width, Color color)
    {
        int count = setup.DashCount;
        float cellAngle = Mathf.PI * 2f / count;
        float dashAngle = cellAngle * setup.DashCoverage;
        Handles.color = color;

        for (int dash = 0; dash < count; dash++)
        {
            float middle = (dash + .5f) * cellAngle;
            float start = middle - dashAngle * .5f;
            Vector3[] points = new Vector3[12];
            for (int point = 0; point < points.Length; point++)
            {
                float angle = start + dashAngle * point / (points.Length - 1);
                points[point] = center + radius * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
            Handles.DrawAAPolyLine(Mathf.Clamp(width, 1f, 24f), points);
        }
    }

    private static void DrawWorldDashes(SpotlightWorldRingSetup setup, Vector3 center,
        Vector3 axisX, Vector3 axisY, float width, Color color)
    {
        int count = setup.DashCount;
        float cellAngle = Mathf.PI * 2f / count;
        float dashAngle = cellAngle * setup.DashCoverage;
        Handles.color = color;

        for (int dash = 0; dash < count; dash++)
        {
            float middle = (dash + .5f) * cellAngle;
            float start = middle - dashAngle * .5f;
            Vector3[] points = new Vector3[12];
            for (int point = 0; point < points.Length; point++)
            {
                float angle = start + dashAngle * point / (points.Length - 1);
                points[point] = center + setup.RingRadius *
                    (axisX * Mathf.Cos(angle) + axisY * Mathf.Sin(angle));
            }
            Handles.DrawAAPolyLine(Mathf.Clamp(width, 1f, 32f), points);
        }
    }
}
