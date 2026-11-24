using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    // Authored UGUI silhouettes for the sap HUD and world-space maze markers.
    public sealed class SapMazeShapeGraphic : MaskableGraphic
    {
        public enum Shape { Leaf, Droplet, RoundedOutline, RoundedPanel, HerbalFlowFrame }

        [SerializeField] private Shape _shape;
        [SerializeField, Tooltip("Optional droplet outline in normalized rect coordinates (-0.5 to 0.5).")]
        private Vector2[] _dropletContour;
        [SerializeField, Range(0, 1)] private float _fillAmount = .55f;
        [SerializeField, Min(.1f)] private float _lineThickness = 2f;
        [SerializeField, Min(0)] private float _cornerRadius = 18f;
        [SerializeField] private Color _outlineColor = new Color(.72f, .91f, 1f, 1f);
        [SerializeField] private Color _innerOutlineColor = new Color(.82f, .88f, .75f, .28f);
        [SerializeField] private Color _iconFillColor = new Color(.17f, .29f, .21f, .62f);
        [SerializeField] private Color _iconOutlineColor = new Color(.60f, .76f, .62f, .60f);
        [SerializeField] private Color _dividerColor = new Color(.78f, .85f, .72f, .25f);

        private readonly List<Vector2> _rightSide = new List<Vector2>(28);
        private readonly List<Vector2> _outlinePoints = new List<Vector2>(56);
        private readonly List<Vector2> _fluidPoints = new List<Vector2>(64);
        private readonly List<Vector2> _outerFrame = new List<Vector2>(40);
        private readonly List<Vector2> _innerFrame = new List<Vector2>(40);

        public void SetFillAmount(float amount)
        {
            var clamped = Mathf.Clamp01(amount);
            if (Mathf.Approximately(_fillAmount, clamped)) return;
            _fillAmount = clamped;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var rect = GetPixelAdjustedRect();

            if (_shape == Shape.Droplet)
            {
                PopulateDroplet(mesh, rect);
                return;
            }
            if (_shape == Shape.RoundedOutline)
            {
                PopulateRoundedOutline(mesh, rect);
                return;
            }
            if (_shape == Shape.RoundedPanel)
            {
                PopulateRoundedPanel(mesh, rect);
                return;
            }
            if (_shape == Shape.HerbalFlowFrame)
            {
                PopulateHerbalFlowFrame(mesh, rect);
                return;
            }

            PopulateLeaf(mesh, rect);
        }

        private void PopulateLeaf(VertexHelper mesh, Rect rect)
        {
            var center = rect.center;
            var vertex = UIVertex.simpleVert;
            vertex.color = color;
            vertex.position = center;
            mesh.AddVert(vertex);

            const int segments = 32;
            for (var i = 0; i <= segments; i++)
            {
                var half = i <= segments / 2 ? i * 2f / segments : (i - segments / 2) * 2f / segments;
                var x = i <= segments / 2 ? 1f - half : half;
                var y = Mathf.Sin(Mathf.PI * half) * (i <= segments / 2 ? 1f : -1f);
                vertex.position = new Vector3(rect.xMin + x * rect.width,
                    center.y + y * rect.height * .5f);
                mesh.AddVert(vertex);
            }
            for (var i = 1; i <= segments; i++) mesh.AddTriangle(0, i, i + 1);
        }

        private void PopulateDroplet(VertexHelper mesh, Rect rect)
        {
            BuildDropletPath(rect);
            if (_outlinePoints.Count < 3) return;

            var level = Mathf.Lerp(rect.yMin, rect.yMax, _fillAmount);
            ClipBelowLevel(_outlinePoints, _fluidPoints, level);
            var fluidBottom = Color.Lerp(color, new Color(.015f, .08f, .16f, color.a), .2f);
            var fluidTop = Color.Lerp(color, new Color(.55f, .9f, 1f, color.a), .34f);
            AddPolygonGradient(mesh, _fluidPoints, fluidBottom, fluidTop, rect.yMin, rect.yMax);

            if (_fluidPoints.Count >= 3)
            {
                var minX = float.PositiveInfinity;
                var maxX = float.NegativeInfinity;
                for (var i = 0; i < _fluidPoints.Count; i++)
                {
                    var point = _fluidPoints[i];
                    if (Mathf.Abs(point.y - level) > .01f) continue;
                    minX = Mathf.Min(minX, point.x);
                    maxX = Mathf.Max(maxX, point.x);
                }
                if (maxX - minX > .5f)
                    AddLine(mesh, new Vector2(minX, level), new Vector2(maxX, level),
                        new Color(.78f, .94f, 1f, .86f), _lineThickness * .65f);
            }

            for (var i = 0; i < _outlinePoints.Count; i++)
                AddLine(mesh, _outlinePoints[i], _outlinePoints[(i + 1) % _outlinePoints.Count],
                    _outlineColor, _lineThickness);
        }

        private void BuildDropletPath(Rect rect)
        {
            _outlinePoints.Clear();
            if (_dropletContour != null && _dropletContour.Length >= 3)
            {
                foreach (var point in _dropletContour)
                    _outlinePoints.Add(ToRectPoint(point, rect));
                return;
            }

            _rightSide.Clear();
            AppendBezier(_rightSide, new Vector2(0f, .5f), new Vector2(.16f, .30f),
                new Vector2(.49f, .16f), new Vector2(.49f, -.08f), 14, true);
            AppendBezier(_rightSide, new Vector2(.49f, -.08f), new Vector2(.49f, -.34f),
                new Vector2(.22f, -.5f), new Vector2(0f, -.5f), 14, false);

            for (var i = 0; i < _rightSide.Count; i++)
                _outlinePoints.Add(ToRectPoint(_rightSide[i], rect));
            for (var i = _rightSide.Count - 2; i >= 1; i--)
                _outlinePoints.Add(ToRectPoint(new Vector2(-_rightSide[i].x, _rightSide[i].y), rect));
        }

        private static void AppendBezier(List<Vector2> points, Vector2 p0, Vector2 p1,
            Vector2 p2, Vector2 p3, int steps, bool includeStart)
        {
            var first = includeStart ? 0 : 1;
            for (var i = first; i <= steps; i++)
            {
                var t = i / (float)steps;
                var oneMinusT = 1f - t;
                points.Add(oneMinusT * oneMinusT * oneMinusT * p0 +
                    3f * oneMinusT * oneMinusT * t * p1 +
                    3f * oneMinusT * t * t * p2 + t * t * t * p3);
            }
        }

        private static Vector2 ToRectPoint(Vector2 normalized, Rect rect)
        {
            return new Vector2(rect.center.x + normalized.x * rect.width,
                rect.center.y + normalized.y * rect.height);
        }

        private static void ClipBelowLevel(List<Vector2> source, List<Vector2> result, float level)
        {
            result.Clear();
            if (source.Count < 3) return;

            var previous = source[source.Count - 1];
            var previousInside = previous.y <= level;
            for (var i = 0; i < source.Count; i++)
            {
                var current = source[i];
                var currentInside = current.y <= level;
                if (currentInside != previousInside)
                {
                    var denominator = current.y - previous.y;
                    if (Mathf.Abs(denominator) > Mathf.Epsilon)
                    {
                        var t = (level - previous.y) / denominator;
                        result.Add(Vector2.LerpUnclamped(previous, current, t));
                    }
                }
                if (currentInside) result.Add(current);
                previous = current;
                previousInside = currentInside;
            }
        }

        private void PopulateRoundedOutline(VertexHelper mesh, Rect rect)
        {
            var thickness = Mathf.Clamp(_lineThickness, .1f, Mathf.Min(rect.width, rect.height) * .45f);
            var radius = Mathf.Clamp(_cornerRadius, 0f, Mathf.Min(rect.width, rect.height) * .5f);
            PopulateRoundedRing(mesh, rect, radius, thickness, color);
        }

        private void PopulateRoundedPanel(VertexHelper mesh, Rect rect)
        {
            var thickness = Mathf.Clamp(_lineThickness, .1f, Mathf.Min(rect.width, rect.height) * .45f);
            var radius = Mathf.Clamp(_cornerRadius, 0f, Mathf.Min(rect.width, rect.height) * .5f);
            BuildRoundedRect(rect, radius, _outerFrame);
            var topColor = Color.Lerp(color, new Color(.22f, .32f, .23f, color.a), .2f);
            var bottomColor = Color.Lerp(color, new Color(.035f, .075f, .06f, color.a), .12f);
            AddPolygonGradient(mesh, _outerFrame, bottomColor, topColor, rect.yMin, rect.yMax);

            var medallionCenter = new Vector2(rect.xMin + rect.width * .195f, rect.center.y);
            var medallionRadius = Mathf.Min(rect.height * .29f, 29f);
            AddCircle(mesh, medallionCenter, medallionRadius - 1.5f, _iconFillColor);
            AddCircleRing(mesh, medallionCenter, medallionRadius, 36, 1.2f, _iconOutlineColor);

            var dividerX = rect.xMin + rect.width * .36f;
            AddLine(mesh, new Vector2(dividerX, rect.yMin + 14f),
                new Vector2(dividerX, rect.yMax - 14f), _dividerColor, .85f);

            PopulateRoundedRing(mesh, rect, radius, thickness, _outlineColor);
            var inset = 4f;
            var innerRect = Rect.MinMaxRect(rect.xMin + inset, rect.yMin + inset,
                rect.xMax - inset, rect.yMax - inset);
            PopulateRoundedRing(mesh, innerRect, Mathf.Max(0f, radius - inset), .65f, _innerOutlineColor);
        }

        private void PopulateHerbalFlowFrame(VertexHelper mesh, Rect rect)
        {
            var dropCenter = new Vector2(rect.xMin + rect.width * .19f, rect.center.y);
            var radius = Mathf.Min(rect.height * .32f, 32f);
            var gold = WithAlpha(_outlineColor, .92f);
            var jade = WithAlpha(_innerOutlineColor, .82f);

            // One continuous leaf-shaped contour links the droplet badge and the readout.
            AddArc(mesh, dropCenter, radius + 3f, 46f, 314f, 32, gold, 1.3f);
            AddArc(mesh, dropCenter, radius + 6f, 72f, 108f, 8, jade, .85f);
            AddLeaf(mesh, PointOnCircle(dropCenter, radius + 5f, 142f),
                new Vector2(-.72f, .69f), 8f, 2.5f, jade);

            var upperStart = PointOnCircle(dropCenter, radius + 3f, 48f);
            var lowerStart = PointOnCircle(dropCenter, radius + 3f, 312f);
            var upperEnd = new Vector2(rect.xMax - 23f, rect.yMax - 18f);
            var lowerEnd = new Vector2(rect.xMax - 23f, rect.yMin + 18f);
            AddBezierLine(mesh, upperStart,
                new Vector2(dropCenter.x + 44f, rect.yMax - 6f),
                new Vector2(rect.xMax - 60f, rect.yMax - 7f), upperEnd, gold, 1.2f);
            AddBezierLine(mesh, lowerEnd,
                new Vector2(rect.xMax - 60f, rect.yMin + 7f),
                new Vector2(dropCenter.x + 44f, rect.yMin + 6f), lowerStart, gold, 1.2f);

            var pointedEnd = new Vector2(rect.xMax - 5f, rect.center.y);
            AddBezierLine(mesh, upperEnd,
                new Vector2(rect.xMax - 8f, rect.yMax - 8f),
                new Vector2(rect.xMax - 4f, rect.center.y + 8f), pointedEnd, jade, .9f);
            AddBezierLine(mesh, pointedEnd,
                new Vector2(rect.xMax - 4f, rect.center.y - 8f),
                new Vector2(rect.xMax - 8f, rect.yMin + 8f), lowerEnd, jade, .9f);
            AddLeaf(mesh, pointedEnd + new Vector2(-2f, 0f), Vector2.right, 8f, 2.4f, gold);
        }

        private static Color WithAlpha(Color source, float multiplier)
        {
            source.a *= multiplier;
            return source;
        }

        private static Vector2 PointOnCircle(Vector2 center, float radius, float degrees)
        {
            var angle = degrees * Mathf.Deg2Rad;
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private static void AddArc(VertexHelper mesh, Vector2 center, float radius,
            float startDegrees, float endDegrees, int segments, Color arcColor, float thickness)
        {
            var previous = PointOnCircle(center, radius, startDegrees);
            for (var i = 1; i <= segments; i++)
            {
                var angle = Mathf.Lerp(startDegrees, endDegrees, i / (float)segments);
                var current = PointOnCircle(center, radius, angle);
                AddLine(mesh, previous, current, arcColor, thickness);
                previous = current;
            }
        }

        private void AddBezierLine(VertexHelper mesh, Vector2 p0, Vector2 p1,
            Vector2 p2, Vector2 p3, Color lineColor, float thickness)
        {
            _rightSide.Clear();
            AppendBezier(_rightSide, p0, p1, p2, p3, 20, true);
            for (var i = 1; i < _rightSide.Count; i++)
                AddLine(mesh, _rightSide[i - 1], _rightSide[i], lineColor, thickness);
        }

        private static void AddLeaf(VertexHelper mesh, Vector2 center, Vector2 direction,
            float length, float width, Color leafColor)
        {
            var axis = direction.normalized;
            var side = new Vector2(-axis.y, axis.x) * width;
            var halfAxis = axis * (length * .5f);
            var points = new List<Vector2>(4)
            {
                center - halfAxis,
                center + side,
                center + halfAxis,
                center - side
            };
            AddPolygon(mesh, points, leafColor);
        }

        private void PopulateRoundedRing(VertexHelper mesh, Rect rect, float radius, float thickness, Color ringColor)
        {
            BuildRoundedRect(rect, radius, _outerFrame);
            var inner = Rect.MinMaxRect(rect.xMin + thickness, rect.yMin + thickness,
                rect.xMax - thickness, rect.yMax - thickness);
            BuildRoundedRect(inner, Mathf.Max(0f, radius - thickness), _innerFrame);

            var count = Mathf.Min(_outerFrame.Count, _innerFrame.Count);
            if (count < 3) return;
            var baseIndex = mesh.currentVertCount;
            for (var i = 0; i < count; i++) AddVertex(mesh, _outerFrame[i], ringColor);
            for (var i = 0; i < count; i++) AddVertex(mesh, _innerFrame[i], ringColor);
            for (var i = 0; i < count; i++)
            {
                var next = (i + 1) % count;
                mesh.AddTriangle(baseIndex + i, baseIndex + next, baseIndex + count + next);
                mesh.AddTriangle(baseIndex + i, baseIndex + count + next, baseIndex + count + i);
            }
        }
        private static void BuildRoundedRect(Rect rect, float radius, List<Vector2> points)
        {
            points.Clear();
            radius = Mathf.Min(radius, Mathf.Min(rect.width, rect.height) * .5f);
            const int steps = 8;
            AddCorner(points, new Vector2(rect.xMax - radius, rect.yMax - radius), radius, 0f, 90f, steps);
            AddCorner(points, new Vector2(rect.xMin + radius, rect.yMax - radius), radius, 90f, 180f, steps);
            AddCorner(points, new Vector2(rect.xMin + radius, rect.yMin + radius), radius, 180f, 270f, steps);
            AddCorner(points, new Vector2(rect.xMax - radius, rect.yMin + radius), radius, 270f, 360f, steps);
        }

        private static void AddCorner(List<Vector2> points, Vector2 center, float radius,
            float startDegrees, float endDegrees, int steps)
        {
            for (var i = 0; i < steps; i++)
            {
                var angle = Mathf.Lerp(startDegrees, endDegrees, i / (float)steps) * Mathf.Deg2Rad;
                points.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }

        private static void AddPolygonGradient(VertexHelper mesh, List<Vector2> points,
            Color bottomColor, Color topColor, float bottomY, float topY)
        {
            if (points.Count < 3) return;
            var center = Vector2.zero;
            for (var i = 0; i < points.Count; i++) center += points[i];
            center /= points.Count;
            var baseIndex = mesh.currentVertCount;
            AddVertex(mesh, center, GradientAt(center.y, bottomColor, topColor, bottomY, topY));
            for (var i = 0; i < points.Count; i++)
                AddVertex(mesh, points[i], GradientAt(points[i].y, bottomColor, topColor, bottomY, topY));
            for (var i = 0; i < points.Count; i++)
                mesh.AddTriangle(baseIndex, baseIndex + 1 + i, baseIndex + 1 + (i + 1) % points.Count);
        }

        private static Color GradientAt(float y, Color bottomColor, Color topColor, float bottomY, float topY)
        {
            var amount = topY > bottomY ? Mathf.Clamp01((y - bottomY) / (topY - bottomY)) : 0f;
            return Color.Lerp(bottomColor, topColor, amount);
        }
        private static void AddPolygon(VertexHelper mesh, List<Vector2> points, Color vertexColor)
        {
            if (points.Count < 3) return;
            var center = Vector2.zero;
            for (var i = 0; i < points.Count; i++) center += points[i];
            center /= points.Count;
            var baseIndex = mesh.currentVertCount;
            AddVertex(mesh, center, vertexColor);
            for (var i = 0; i < points.Count; i++) AddVertex(mesh, points[i], vertexColor);
            for (var i = 0; i < points.Count; i++)
                mesh.AddTriangle(baseIndex, baseIndex + 1 + i, baseIndex + 1 + (i + 1) % points.Count);
        }

        private static void AddCircle(VertexHelper mesh, Vector2 center, float radius, Color fillColor)
        {
            const int segments = 36;
            var baseIndex = mesh.currentVertCount;
            AddVertex(mesh, center, fillColor);
            for (var i = 0; i < segments; i++)
            {
                var angle = Mathf.PI * 2f * i / segments;
                AddVertex(mesh, center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, fillColor);
            }
            for (var i = 0; i < segments; i++)
                mesh.AddTriangle(baseIndex, baseIndex + 1 + i, baseIndex + 1 + (i + 1) % segments);
        }

        private static void AddCircleRing(VertexHelper mesh, Vector2 center, float radius,
            int segments, float thickness, Color ringColor)
        {
            for (var i = 0; i < segments; i++)
            {
                var a0 = Mathf.PI * 2f * i / segments;
                var a1 = Mathf.PI * 2f * (i + 1) / segments;
                var start = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
                var end = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
                AddLine(mesh, start, end, ringColor, thickness);
            }
        }
        private static void AddLine(VertexHelper mesh, Vector2 start, Vector2 end, Color lineColor, float thickness)
        {
            var delta = end - start;
            if (delta.sqrMagnitude < .0001f) return;
            var half = new Vector2(-delta.y, delta.x).normalized * (thickness * .5f);
            var baseIndex = mesh.currentVertCount;
            AddVertex(mesh, start - half, lineColor);
            AddVertex(mesh, start + half, lineColor);
            AddVertex(mesh, end + half, lineColor);
            AddVertex(mesh, end - half, lineColor);
            mesh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            mesh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
        }

        private static void AddVertex(VertexHelper mesh, Vector2 position, Color vertexColor)
        {
            var vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = vertexColor;
            mesh.AddVert(vertex);
        }
    }
}
