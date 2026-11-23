using Herbalist.GameUI;
using UnityEditor;
using UnityEngine;

namespace Herbalist.Editor
{
    [CustomEditor(typeof(SpotlightWorldRingSetup))]
    public sealed class SpotlightWorldRingSetupEditor : UnityEditor.Editor
    {
        public override bool HasPreviewGUI() => true;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("Radius: world metres. Dash Fraction: occupied fraction per dash (lower = wider gaps). Preview Target affects Scene preview only.", MessageType.Info);
        }
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            var setup = (SpotlightWorldRingSetup)target;
            EditorGUI.DrawRect(rect, new Color(.08f, .08f, .08f));
            Handles.BeginGUI();
            float radius = Mathf.Min(rect.width, rect.height) * .36f;
            DrawDashes(rect.center, radius, setup.glowWidth * radius, setup.glowColor, setup);
            DrawDashes(rect.center, radius, setup.coreWidth * radius, setup.coreColor, setup);
            Handles.EndGUI();
        }
        private static void DrawDashes(Vector2 center, float radius, float width, Color color, SpotlightWorldRingSetup setup)
        {
            Handles.color = color;
            int count = Mathf.Clamp(setup.dashCount, 3, 128);
            for (int i = 0; i < count; i++)
            {
                var points = new Vector3[9];
                for (int j = 0; j < points.Length; j++)
                {
                    float angle = (i + .5f + (j / 8f - .5f) * setup.dashFraction) * Mathf.PI * 2f / count;
                    points[j] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                }
                Handles.DrawAAPolyLine(Mathf.Max(1, width), points);
            }
        }
        private void OnSceneGUI()
        {
            var setup = (SpotlightWorldRingSetup)target;
            Vector3 center = setup.previewTarget != null ? setup.previewTarget.position : setup.previewCenter;
            Quaternion rotation = SceneView.currentDrawingSceneView.camera.transform.rotation;
            Handles.color = setup.coreColor;
            EditorGUI.BeginChangeCheck();
            float radius = Handles.RadiusHandle(rotation, center, setup.radius);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(setup, "Change guide ring radius");
                setup.radius = Mathf.Max(.01f, radius);
                EditorUtility.SetDirty(setup);
            }
            int count = Mathf.Clamp(setup.dashCount, 3, 128);
            for (int i = 0; i < count; i++)
            {
                float angle = (i + .5f - setup.dashFraction * .5f) * 360f / count;
                Vector3 start = rotation * (Quaternion.Euler(0, 0, angle) * Vector3.right);
                Handles.DrawWireArc(center, rotation * Vector3.forward, start, setup.dashFraction * 360f / count, setup.radius);
            }
        }
    }
}
