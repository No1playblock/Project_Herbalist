using System.Collections.Generic;
using UnityEngine;

namespace LatticeTool
{
    [System.Serializable]
    public class SelectedHandles
    {
        [SerializeField]
        private List<int> selectedIndices = new List<int>();

        public IReadOnlyList<int> Selected => selectedIndices;

        public int Count => selectedIndices.Count;

        public bool Contains(int index)
        {
            return selectedIndices.Contains(index);
        }

        public void Add(int index)
        {
            if (!selectedIndices.Contains(index))
                selectedIndices.Add(index);
        }

        public void Remove(int index)
        {
            selectedIndices.Remove(index);
        }

        public void Clear()
        {
            selectedIndices.Clear();
        }

        public void Invert(int totalHandles)
        {
            List<int> newSelection = new List<int>();
            for (int i = 0; i < totalHandles; i++)
            {
                if (!selectedIndices.Contains(i))
                    newSelection.Add(i);
            }
            selectedIndices = newSelection;
        }
    }
}
