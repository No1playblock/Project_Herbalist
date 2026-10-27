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
        private readonly List<Vector2> _path = new List<Vector2>(128);
        private readonly List<float> _widths = new List<float>(128);

        public SapMazeTrailSettings Settings => _settings;
        public Vector2 HeadCenter { get; private set; }
        public float HeadWidthWorld { get; private set; }

        public float Present(IReadOnlyList<Vector2> history, Vector2 head, float fraction, SapMazeSurface surface = null)
        {
            if (_line == null || _settings == null) return Mathf.Pow(Mathf.Clamp01(fraction), 1f / 3f);
            var remainingMl = _settings.capacityMl * Mathf.Clamp01(fraction);
            var budget = _settings.VisibleLength(fraction);
            _visible.Clear();
            _widths.Clear();
            var cursor = head;
            _visible.Add(new Vector3(head.x, head.y, surface != null && surface.Ready ? surface.SurfaceZ(head, _settings.localZ) : _settings.localZ));
            var usedLength = 0f;
            for (var i = history.Count - 1; i >= 0 && usedLength < budget; i--)
            {
                var previous = history[i];
                var length = Vector2.Distance(cursor, previous);
                if (length < .0001f) continue;
                if (usedLength + length >= budget)
                {
                    var tail = Vector2.Lerp(cursor, previous, (budget - usedLength) / length);
                    _visible.Add(new Vector3(tail.x, tail.y, surface != null && surface.Ready ? surface.SurfaceZ(tail, _settings.localZ) : _settings.localZ));
                    usedLength = budget;
                    break;
                }
                _visible.Add(new Vector3(previous.x, previous.y, surface != null && surface.Ready ? surface.SurfaceZ(previous, _settings.localZ) : _settings.localZ));
                usedLength += length;
                cursor = previous;
            }
            _visible.Reverse();
            HeadCenter = head;
            HeadWidthWorld = 0;
            if (surface != null && surface.Ready)
            {
                _line.alignment = LineAlignment.TransformZ;
                _path.Clear();
                for (var i = 0; i < _visible.Count; i++)
                    _path.Add(new Vector2(_visible[i].x, _visible[i].y));
                for (var i = 0; i < _path.Count; i++)
                {
                    var previous = _path[Mathf.Max(0, i - 1)];
                    var next = _path[Mathf.Min(_path.Count - 1, i + 1)];
                    var travel = next - previous;
                    var width = surface.ChannelSection(_path[i], travel,
                        _settings.maximumChannelWidth, _settings.channelEdgeInset, out var center);
                    if (width <= 0) { width = _settings.trailWidth; center = _path[i]; }
                    _widths.Add(width);
                    _visible[i] = new Vector3(center.x, center.y, surface.SurfaceZ(center, _settings.localZ));
                }
                HeadCenter = new Vector2(_visible[_visible.Count - 1].x, _visible[_visible.Count - 1].y);
                HeadWidthWorld = _widths[_widths.Count - 1];
            }
            _line.positionCount = _visible.Count >= 2 && usedLength > .001f ? _visible.Count : 0;
            for (var i = 0; i < _line.positionCount; i++) _line.SetPosition(i, _visible[i]);
            if (_line.positionCount > 1 && _widths.Count == _visible.Count)
            {
                var length = 0f;
                for (var i = 1; i < _visible.Count; i++) length += Vector3.Distance(_visible[i - 1], _visible[i]);
                var keys = new Keyframe[_visible.Count];
                var distance = 0f;
                for (var i = 0; i < _visible.Count; i++)
                {
                    if (i > 0) distance += Vector3.Distance(_visible[i - 1], _visible[i]);
                    keys[i] = new Keyframe(length > .0001f ? distance / length : (float)i / (_visible.Count - 1), _widths[i]);
                }
                _line.widthCurve = new AnimationCurve(keys);
                _line.widthMultiplier = 1f;
            }
            else _line.widthMultiplier = _settings.trailWidth * Mathf.Sqrt(Mathf.Clamp01(fraction));
            var headMl = Mathf.Max(0, remainingMl - usedLength * _settings.millilitersPerUnit);
            return Mathf.Pow(Mathf.Clamp01(headMl / _settings.capacityMl), 1f / 3f);
        }

        public void Clear()
        {
            if (_line != null) _line.positionCount = 0;
        }
    }
}
