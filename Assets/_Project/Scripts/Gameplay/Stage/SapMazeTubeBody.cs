using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Deforming centreline: either end or the middle may be pulled. No head history.
    public sealed class SapMazeTubeBody
    {
        public const int Capacity = 128;
        private readonly SapMazeSurface _surface;
        private readonly SapMazeTrailSettings _settings;
        private readonly float _worldScale, _initialVolume;
        private readonly List<Vector2> _points = new List<Vector2>(Capacity);
        private readonly List<Vector2> _path = new List<Vector2>();
        private readonly List<Vector2> _resampled = new List<Vector2>(Capacity);
        private float _volume = -1, _spacing;
        private int _targetCount;
        public IReadOnlyList<Vector2> Points => _points;
        public int Grabbed { get; private set; }

        public SapMazeTubeBody(SapMazeSurface surface, SapMazeTrailSettings settings, float worldScale, float initialVolume)
        { _surface = surface; _settings = settings; _worldScale = worldScale; _initialVolume = initialVolume; }

        public void Reset(Vector2 start, Vector2 goal, float volume)
        {
            _points.Clear(); _volume = -1; Grabbed = 0;
            _surface.TryFindPath(start, goal, _path);
            if (_path.Count == 0) _path.Add(start);
            Resize(volume, _path);
        }

        public void SetVolume(float volume)
        {
            if (Mathf.Abs(volume - _volume) < .0001f) return;
            Resize(volume, _points);
        }

        private void Resize(float volume, IReadOnlyList<Vector2> source)
        {
            float length = Mathf.Clamp(_settings.tubeInitialLengthWorld * volume / Mathf.Max(.01f, _initialVolume),
                _settings.tubeWidthWorld, _settings.tubeMaxLengthWorld) / _worldScale;
            int count = Mathf.Clamp(Mathf.CeilToInt(length * _worldScale / _settings.tubePointSpacingWorld) + 1, 2, Capacity);
            _targetCount = count;
            _spacing = length / (count - 1);
            _resampled.Clear(); int segment = 0, previousSegment = 0; float travelled = 0;
            for (int i = 0; i < count; i++)
            {
                float distance = i * _spacing;
                while (segment + 1 < source.Count && travelled + Vector2.Distance(source[segment], source[segment + 1]) < distance)
                { travelled += Vector2.Distance(source[segment], source[segment + 1]); segment++; }
                Vector2 point;
                if (segment + 1 < source.Count)
                {
                    float span = Vector2.Distance(source[segment], source[segment + 1]);
                    point = Vector2.Lerp(source[segment], source[segment + 1], span > .000001f ? (distance - travelled) / span : 0);
                }
                else
                {
                    var end = source[source.Count - 1];
                    var direction = source.Count > 1 ? (end - source[source.Count - 2]).normalized : Vector2.up;
                    point = _surface.Move(end, direction * Mathf.Max(0, distance - travelled));
                }
                if (_resampled.Count > 0 && !_surface.IsSegmentOpen(_resampled[_resampled.Count - 1], point))
                    for (int corner = previousSegment + 1; corner <= segment && _resampled.Count < Capacity - 1; corner++)
                        _resampled.Add(source[corner]);
                if (_resampled.Count == Capacity) break;
                _resampled.Add(point); previousSegment = segment;
            }
            float grabbedFraction = _points.Count > 1 ? (float)Grabbed / (_points.Count - 1) : 0;
            _points.Clear(); _points.AddRange(_resampled);
            Grabbed = Mathf.Clamp(Mathf.RoundToInt(grabbedFraction * (_points.Count - 1)), 0, _points.Count - 1);
            _volume = volume;
        }

        public bool Grab(Vector2 point, float radiusWorld)
        {
            float best = radiusWorld / _worldScale; int found = -1;
            for (int i = 0; i < _points.Count; i++)
            {
                float d = Vector2.Distance(point, _points[i]);
                if (d <= best) { best = d; found = i; }
            }
            if (found < 0) return false;
            Grabbed = found; return true;
        }

        public void Pull(Vector2 point)
        {
            if (_points.Count < 2) return;
            RemoveRedundantBends();
            MoveNode(Grabbed, point - _points[Grabbed]);
            for (int pass = 0; pass < _settings.tubeConstraintPasses; pass++)
            {
                for (int i = Grabbed + 1; i < _points.Count; i++) i = Follow(i - 1, i);
                for (int i = Grabbed - 1; i >= 0; i--) i = Follow(i + 1, i);
                SeparateFolds();
            }
            RemoveRedundantBends();
        }

        private void RemoveRedundantBends()
        {
            for (int i = _points.Count - 2; i > 0 && _points.Count > _targetCount; i--)
            {
                if (i == Grabbed || Vector2.Distance(_points[i - 1], _points[i + 1]) > _spacing * 2) continue;
                if (!_surface.IsSegmentOpen(_points[i - 1], _points[i + 1])) continue;
                _points.RemoveAt(i);
                if (Grabbed > i) Grabbed--;
            }
        }

        private int MoveNode(int index, Vector2 delta)
        {
            var old = _points[index]; var moved = _surface.Move(old, delta);
            if ((moved - old).sqrMagnitude < .00000001f) return index;
            bool before = index > 0 && !_surface.IsSegmentOpen(_points[index - 1], moved);
            bool after = index + 1 < _points.Count && !_surface.IsSegmentOpen(moved, _points[index + 1]);
            if (_points.Count + (before ? 1 : 0) + (after ? 1 : 0) > Capacity) return index;
            // Keep the existing bend as a live body point instead of running a
            // whole-maze path search for every joint on every simulation tick.
            if (before)
            {
                _points.Insert(index, old);
                if (Grabbed >= index) Grabbed++;
                index++;
            }
            _points[index] = moved;
            if (after)
            {
                _points.Insert(index + 1, old);
                if (Grabbed > index) Grabbed++;
            }
            return index;
        }

        private int Follow(int anchor, int follower)
        {
            var a = _points[anchor]; var p = _points[follower];
            var delta = p - a; float distance = delta.magnitude;
            if (distance < .000001f || !_surface.IsSegmentOpen(a, p)) return follower;
            if (distance <= _spacing && distance >= _spacing * .7f) return follower;
            return MoveNode(follower, a + delta / distance * _spacing - p);
        }

        private void SeparateFolds()
        {
            float minimum = _settings.tubeWidthWorld / _worldScale * .8f;
            int skip = Mathf.Max(2, Mathf.CeilToInt(minimum / _spacing));
            for (int i = 0; i < _points.Count; i++)
            for (int j = i + skip + 1; j < _points.Count; j++)
            {
                var delta = _points[j] - _points[i]; float distance = delta.magnitude;
                if (distance >= minimum || distance < .000001f || !_surface.IsSegmentOpen(_points[i], _points[j])) continue;
                var correction = delta / distance * ((minimum - distance) * .5f);
                if (i != Grabbed) MoveSeparated(i, -correction);
                if (j != Grabbed) MoveSeparated(j, correction);
            }
        }

        private void MoveSeparated(int index, Vector2 delta)
        {
            var moved = _surface.Move(_points[index], delta);
            if (index > 0 && !_surface.IsSegmentOpen(_points[index - 1], moved)) return;
            if (index + 1 < _points.Count && !_surface.IsSegmentOpen(moved, _points[index + 1])) return;
            _points[index] = moved;
        }

        public void ReadPoints(IReadOnlyList<Vector2> points)
        { _points.Clear(); for (int i = 0; i < points.Count; i++) _points.Add(points[i]); }

        public float Distance(Vector2 point)
        {
            float best = float.PositiveInfinity;
            for (int i = 0; i < _points.Count; i++)
            {
                best = Mathf.Min(best, Vector2.Distance(point, _points[i]));
                if (i == 0) continue;
                var delta = _points[i] - _points[i - 1];
                float t = delta.sqrMagnitude > .00000001f ? Mathf.Clamp01(Vector2.Dot(point - _points[i - 1], delta) / delta.sqrMagnitude) : 0;
                best = Mathf.Min(best, Vector2.Distance(point, _points[i - 1] + delta * t));
            }
            return Mathf.Max(0, best - _settings.tubeWidthWorld / _worldScale * .5f);
        }
    }
}
