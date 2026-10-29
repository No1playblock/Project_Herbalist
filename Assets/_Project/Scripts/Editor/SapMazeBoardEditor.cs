using Herbalist.Levels;
using UnityEditor;
using UnityEngine;

namespace Herbalist.Editor
{
    [CustomEditor(typeof(SapMazeBoard))]
    public sealed class SapMazeBoardEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("미로 시작·도착 지점", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_startMarker"), new GUIContent("시작 마커"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_goalMarker"), new GUIContent("도착 마커"));
            serializedObject.ApplyModifiedProperties();
            var board = (SapMazeBoard)target;
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(board.StartMarker == null))
                    if (GUILayout.Button("시작점 선택")) Select(board.StartMarker);
                using (new EditorGUI.DisabledScope(board.GoalMarker == null))
                    if (GUILayout.Button("도착점 선택")) Select(board.GoalMarker);
            }
            EditorGUILayout.HelpBox("Play를 끈 상태에서 마커를 통로 위로 옮기세요. 보드를 선택하면 시작/도착 이동 핸들이 표시됩니다. 지정한 Transform은 실행 시 변경하지 않습니다.", MessageType.Info);
            DrawPropertiesExcluding(serializedObject, "_startMarker", "_goalMarker");
            serializedObject.ApplyModifiedProperties();
        }
        private static void Select(Transform marker)
        {
            Selection.activeGameObject = marker.gameObject;
            EditorGUIUtility.PingObject(marker);
            Tools.current = Tool.Move;
        }
        private void OnSceneGUI()
        {
            var board = (SapMazeBoard)target;
            MoveMarker(board.StartMarker, "시작", Color.green);
            MoveMarker(board.GoalMarker, "도착", Color.yellow);
        }
        private static void MoveMarker(Transform marker, string label, Color color)
        {
            if (marker == null || Application.isPlaying) return;
            Handles.color = color;
            Handles.Label(marker.position, label);
            EditorGUI.BeginChangeCheck();
            var position = Handles.PositionHandle(marker.position, Quaternion.identity);
            if (!EditorGUI.EndChangeCheck()) return;
            Undo.RecordObject(marker, "Move maze " + label);
            marker.position = position;
            PrefabUtility.RecordPrefabInstancePropertyModifications(marker);
        }
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawMarkers(SapMazeBoard board, GizmoType type)
        {
            DrawMarker(board.StartMarker, "시작", Color.green);
            DrawMarker(board.GoalMarker, "도착", Color.yellow);
        }
        private static void DrawMarker(Transform marker, string label, Color color)
        {
            if (marker == null) return;
            float size = HandleUtility.GetHandleSize(marker.position) * .06f;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(marker.position, size);
            Handles.color = color;
            Handles.Label(marker.position + Vector3.up * size, label);
        }
    }
}
