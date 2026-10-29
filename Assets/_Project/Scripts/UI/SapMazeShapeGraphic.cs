using UnityEngine;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    // Vector silhouettes keep the authored world-space display crisp at any camera distance.
    public sealed class SapMazeShapeGraphic : MaskableGraphic
    {
        public enum Shape { Leaf, Droplet }
        [SerializeField] private Shape _shape;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var rect = GetPixelAdjustedRect();
            var center = rect.center;
            var vertex = UIVertex.simpleVert;
            vertex.color = color;
            vertex.position = center;
            mesh.AddVert(vertex);

            const int segments = 32;
            for (var i = 0; i <= segments; i++)
            {
                if (_shape == Shape.Leaf)
                {
                    var half = i <= segments / 2 ? i * 2f / segments : (i - segments / 2) * 2f / segments;
                    var x = i <= segments / 2 ? 1f - half : half;
                    var y = Mathf.Sin(Mathf.PI * half) * (i <= segments / 2 ? 1f : -1f);
                    vertex.position = new Vector3(rect.xMin + x * rect.width,
                        center.y + y * rect.height * .5f);
                }
                else
                {
                    var angle = Mathf.PI * 2f * i / segments;
                    var x = Mathf.Cos(angle);
                    var y = Mathf.Sin(angle);
                    var width = .25f + .75f * Mathf.Clamp01((1f - y) * .5f);
                    vertex.position = new Vector3(center.x + x * width * rect.width * .5f,
                        center.y + y * rect.height * .5f);
                }
                mesh.AddVert(vertex);
            }
            for (var i = 1; i <= segments; i++)mesh.AddTriangle(0, i, i + 1);
        }
    }
}
