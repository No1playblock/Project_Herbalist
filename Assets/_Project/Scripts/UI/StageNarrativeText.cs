using System;
using UnityEngine;

namespace Herbalist.GameUI
{
    [CreateAssetMenu(menuName = "Herbalist/Game UI/Stage Narrative Text")]
    public sealed class StageNarrativeText : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string id;
            public string speaker;
            [TextArea]
            public string text;
        }

        public Entry[] entries = Array.Empty<Entry>();
        public string Find(string id)
        {
            foreach (var entry in entries)
                if (entry.id == id)
                    return string.IsNullOrEmpty(entry.speaker) ? entry.text : entry.speaker + ": " + entry.text;
            return string.Empty;
        }
    }
}
