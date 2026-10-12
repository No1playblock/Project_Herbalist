using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Presentation-only water volume: the moving head and visible trail share one budget.
    [RequireComponent(typeof(LineRenderer))]
    public sealed class SapMazeTrailView : MonoBehaviour
    {
        [SerializeField] private SapMazeTrailSettings _settings;
        [SerializeField] private LineRenderer _line;
        private readonly List<Vector3> _visible = new List<Vector3>(128);

        public SapMazeTrailSettings Settings => _settings;

        public float Present(IReadOnlyList<Vector2> history, Vector2 head, float fraction)
        {
            if (_line == null || _settings == null) return Mathf.Pow(Mathf.Clamp01(fraction), 1f / 3f);
            var remainingMl = _settings.capacityMl * Mathf.Clamp01(fraction);
            var budget = Mathf.Max(0, remainingMl - _settings.minimumHeadMl) / _settings.millilitersPerUnit;
            _visible.Clear();
            var cursor = head;
            _visible.Add(new Vector3(head.x, head.y, _settings.localZ));
            var usedLength = 0f;
            for (var i = history.Count - 1; i >= 0 && usedLength < budget; i--)
            {
                var previous = history[i];
                var length = Vector2.Distance(cursor, previous);
                if (length < .0001f) continue;
                if (usedLength + length >= budget)
                {
                    var tail = Vector2.Lerp(cursor, previous, (budget - usedLength) / length);
                    _visible.Add(new Vector3(tail.x, tail.y, _settings.localZ));
                    usedLength = budget;
                    break;
                }
                _visible.Add(new Vector3(previous.x, previous.y, _settings.localZ));
                usedLength += length;
                cursor = previous;
            }
            _visible.Reverse();
            _line.positionCount = _visible.Count >= 2 && usedLength > .001f ? _visible.Count : 0;
            for (var i = 0; i < _line.positionCount; i++) _line.SetPosition(i, _visible[i]);
            _line.widthMultiplier = _settings.trailWidth * Mathf.Sqrt(Mathf.Clamp01(fraction));
            var headMl = Mathf.Max(0, remainingMl - usedLength * _settings.millilitersPerUnit);
            return Mathf.Pow(Mathf.Clamp01(headMl / _settings.capacityMl), 1f / 3f);
        }

        public void Clear()
        {
            if (_line != null) _line.positionCount = 0;
        }
    }
}
