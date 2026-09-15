using System.Collections.Generic;
using UnityEngine;
namespace Herbalist.Abilities
{
    // Supply and receiving surfaces are independent: one source may feed several devices.
    public sealed class SapSource : MonoBehaviour
    {
        [SerializeField] private Transform extractionPoint;
        [SerializeField] private Collider interactionVolume;
        [SerializeField] private bool extractFromNearestSurface;
        private Mesh cachedMesh;
        private Vector3[] vertices;
        private int[] triangles;
        [Tooltip("Zero means unlimited prototype supply.")]
        [SerializeField, Min(0)] private int supplyUnits;
        private int used;
        private static readonly HashSet<SapSource> sources = new();
        public Vector3 ExtractionPoint => extractionPoint != null ? extractionPoint.position : transform.position;
        public bool Available => isActiveAndEnabled && (supplyUnits == 0 || used < supplyUnits);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics() => sources.Clear();
        private void OnEnable() => sources.Add(this);
        private void OnDisable() => sources.Remove(this);
        public bool TryExtract() { if (!Available) return false; used++; return true; }
        public void Refund() => used = Mathf.Max(0, used - 1);
        public static SapSource FindNearest(Vector3 position, float range)
        { Vector3 point; return FindNearest(position, range, 0, out point); }
        public static SapSource FindNearest(Vector3 position, float range, float clearance, out Vector3 extraction)
        {
            extraction = default;
            SapSource best = null; float distance = range * range;
            foreach (var source in sources)
            {
                if (!source.Available) continue;
                if (source.interactionVolume != null && source.interactionVolume.bounds.SqrDistance(position) > distance) continue;
                Vector3 point, normal;
                if (!source.TrySurface(position, out point, out normal)) continue;
                float candidate = (point - position).sqrMagnitude;
                if (candidate <= distance)
                {
                    distance = candidate; best = source;
                    extraction = source.extractFromNearestSurface ? point + normal * clearance : source.ExtractionPoint;
                }
            }
            return best;
        }
        private bool TrySurface(Vector3 position, out Vector3 point, out Vector3 normal)
        {
            point = ExtractionPoint; normal = Vector3.up;
            var meshCollider = interactionVolume as MeshCollider;
            if (meshCollider == null || meshCollider.convex)
            {
                if (interactionVolume != null) point = interactionVolume.ClosestPoint(position);
                normal = (position - point).normalized;
                return true;
            }
            // PhysX ClosestPoint does not support concave MeshColliders. Query triangles
            // only on extraction input, never each simulation tick. World-space distances
            // also handle non-uniformly scaled imported tree models.
            var mesh = meshCollider.sharedMesh;
            if (mesh == null || !mesh.isReadable) return false;
            if (cachedMesh != mesh) { cachedMesh = mesh; vertices = mesh.vertices; triangles = mesh.triangles; }
            float best = float.PositiveInfinity;
            var matrix = meshCollider.transform.localToWorldMatrix;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var a = matrix.MultiplyPoint3x4(vertices[triangles[i]]);
                var b = matrix.MultiplyPoint3x4(vertices[triangles[i + 1]]);
                var c = matrix.MultiplyPoint3x4(vertices[triangles[i + 2]]);
                var faceNormal = Vector3.Cross(b - a, c - a);
                if (faceNormal.sqrMagnitude < Mathf.Epsilon) continue;
                var closest = ClosestTriangle(position, a, b, c);
                float distance = (position - closest).sqrMagnitude;
                if (distance >= best) continue;
                best = distance; point = closest; normal = faceNormal.normalized;
                if (Vector3.Dot(normal, position - point) < 0) normal = -normal;
            }
            return !float.IsPositiveInfinity(best);
        }
        private static Vector3 ClosestTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = b-a; var ac = c-a; var ap = p-a;
            float d1=Vector3.Dot(ab,ap), d2=Vector3.Dot(ac,ap);
            if (d1<=0 && d2<=0) return a;
            var bp=p-b; float d3=Vector3.Dot(ab,bp), d4=Vector3.Dot(ac,bp);
            if (d3>=0 && d4<=d3) return b;
            float vc=d1*d4-d3*d2;
            if (vc<=0 && d1>=0 && d3<=0) return a+ab*(d1/(d1-d3));
            var cp=p-c; float d5=Vector3.Dot(ab,cp), d6=Vector3.Dot(ac,cp);
            if (d6>=0 && d5<=d6) return c;
            float vb=d5*d2-d1*d6;
            if (vb<=0 && d2>=0 && d6<=0) return a+ac*(d2/(d2-d6));
            float va=d3*d6-d5*d4;
            if (va<=0 && d4-d3>=0 && d5-d6>=0) return b+(c-b)*((d4-d3)/((d4-d3)+(d5-d6)));
            float inverse=1/(va+vb+vc);
            return a+ab*(vb*inverse)+ac*(vc*inverse);
        }
    }
}
