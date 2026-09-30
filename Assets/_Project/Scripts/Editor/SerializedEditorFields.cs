using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    internal static class SerializedEditorFields
    {
        private static SerializedProperty Property(Object target, string name, out SerializedObject serialized)
        {
            serialized = new SerializedObject(target);
            return serialized.FindProperty(name) ?? throw new System.ArgumentException("Missing serialized field " + name);
        }

        public static void Set(Object target, string name, Object value)
        {
            var property = Property(target, name, out var serialized);
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetInt(Object target, string name, int value)
        {
            var property = Property(target, name, out var serialized);
            property.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetBool(Object target, string name, bool value)
        {
            var property = Property(target, name, out var serialized);
            property.boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetArray(Object target, string name, Object[] values)
        {
            var property = Property(target, name, out var serialized);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
