using System.Collections.Generic;
using Herbalist.Abilities;
using UnityEngine;

namespace Herbalist.Levels
{
    // Movement backend only. Flow owns authority, span transitions and rider lifecycle.
    public sealed class BranchRideCarrier : MonoBehaviour
    {
        public struct StepResult
        {
            public Vector3 position;
            public Quaternion rotation;
            public float distance;
            public bool atEnd, blocked;
        }
        public StepResult Step(BranchRideTrack track, float distance, Quaternion rotation, float dt,
            float speed, float gravity, float height, float leafRadius, IReadOnlyList<LeafProjectile> leaves)
        {
            var result = new StepResult { distance = distance, rotation = rotation };
            if (track.IsDrop)
            {
                result.distance += dt;
                float fall = Mathf.Max(.01f, track.Start.y - track.End.y);
                float duration = Mathf.Sqrt(2 * fall / gravity);
                float t = Mathf.Clamp01(result.distance / duration);
                result.position = Vector3.Lerp(track.Start, track.End, t) + Vector3.up * height;
                result.position.y = track.Start.y - fall * t * t + height;
                result.atEnd = t >= 1;
                return result;
            }
            Vector3 start = track.Sample(distance, out _);
            Vector3 candidate = track.Sample(distance + speed * dt, out var direction);
            foreach (var leaf in leaves)
                if (leaf != null && leaf.Installed && Vector3.Dot(leaf.ContactPoint - start, direction) >= -.05f &&
                    BranchRideTrack.DistanceToSegment(leaf.ContactPoint, start, candidate) <= leafRadius)
                { result.blocked = true; break; }
            if (!result.blocked) result.distance = Mathf.Min(track.Length, distance + speed * dt);
            result.position = track.Sample(result.distance, out direction) + Vector3.up * height;
            Vector3 planar = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planar.sqrMagnitude > .001f) result.rotation = Quaternion.LookRotation(planar);
            result.atEnd = !result.blocked && result.distance >= track.Length - .0001f;
            return result;
        }
    }
}
