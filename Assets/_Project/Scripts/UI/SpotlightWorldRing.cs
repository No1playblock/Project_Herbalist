using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Herbalist.GameUI
{
    // The package's rounded dashes and glow, submitted only to the guide owner's camera.
    internal sealed class SpotlightWorldRing
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private readonly Mesh _core = BuildCapsuleRing(0.04f);
        private readonly Mesh _glow = BuildCapsuleRing(0.18f);
        private readonly MaterialPropertyBlock _coreColor = MakeColor(new Color(1f, 0.79f, 0.42f, 1f));
        private readonly MaterialPropertyBlock _glowColor = MakeColor(new Color(1f, 0.56f, 0.18f, 0.38f));

        public void Draw(Camera camera, Vector3 center, float radius, Material material)
        {
            var rotation = camera.transform.rotation * Quaternion.Euler(-90f, 0f, 0f);
            var pose = Matrix4x4.TRS(center, rotation, Vector3.one * radius);
            Graphics.DrawMesh(_glow, pose, material, 0, camera, 0, _glowColor, ShadowCastingMode.Off, false,
                null, LightProbeUsage.Off);
            Graphics.DrawMesh(_core, pose, material, 0, camera, 0, _coreColor, ShadowCastingMode.Off, false,
                null, LightProbeUsage.Off);
        }

        public void Dispose()
        {
            Object.Destroy(_core);
            Object.Destroy(_glow);
        }

        private static MaterialPropertyBlock MakeColor(Color color)
        {
            var properties = new MaterialPropertyBlock();
            properties.SetColor(ColorId, color);
            return properties;
        }

        private static Mesh BuildCapsuleRing(float width)
        {
            Mesh mesh = new Mesh { name = "Spotlight Rounded Dash Ring" };
            mesh.hideFlags = HideFlags.HideAndDontSave;

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            const int count = 24;
            float cellAngle = Mathf.PI * 2f / count;
            float dashAngle = cellAngle * 0.62f;
            const int capSteps = 8;

            for (int dash = 0; dash < count; dash++)
            {
                float middle = (dash + 0.5f) * cellAngle;
                float start = middle - dashAngle * 0.5f;
                float end = middle + dashAngle * 0.5f;
                Vector2 p1 = new Vector2(Mathf.Cos(start), Mathf.Sin(start));
                Vector2 p2 = new Vector2(Mathf.Cos(end), Mathf.Sin(end));
                Vector2 direction = (p2 - p1).normalized;
                Vector2 perpendicular = new Vector2(-direction.y, direction.x);
                float capRadius = width * 0.5f;

                List<Vector2> perimeter = new List<Vector2>(capSteps * 2 + 2);
                for (int step = 0; step <= capSteps; step++)
                {
                    float capAngle = -Mathf.PI * 0.5f + Mathf.PI * step / capSteps;
                    Vector2 offset = (Mathf.Cos(capAngle) * direction + Mathf.Sin(capAngle) * perpendicular) * capRadius;
                    perimeter.Add(p2 + offset);
                }
                for (int step = 0; step <= capSteps; step++)
                {
                    float capAngle = Mathf.PI * 0.5f + Mathf.PI * step / capSteps;
                    Vector2 offset = (Mathf.Cos(capAngle) * direction + Mathf.Sin(capAngle) * perpendicular) * capRadius;
                    perimeter.Add(p1 + offset);
                }

                int centerIndex = vertices.Count;
                Vector2 center = (p1 + p2) * 0.5f;
                vertices.Add(new Vector3(center.x, 0f, center.y));
                for (int i = 0; i < perimeter.Count; i++)
                    vertices.Add(new Vector3(perimeter[i].x, 0f, perimeter[i].y));

                for (int i = 0; i < perimeter.Count; i++)
                {
                    int current = centerIndex + 1 + i;
                    int next = centerIndex + 1 + ((i + 1) % perimeter.Count);
                    triangles.Add(centerIndex);
                    triangles.Add(current);
                    triangles.Add(next);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
