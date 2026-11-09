using System.Collections.Generic;
using Herbalist.Abilities;
using UnityEngine;

namespace Herbalist.Levels
{
    // Movement backend only. Flow owns authority, span transitions and rider lifecycle.
    public sealed class BranchRideCarrier : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _cornerBlendDistance = .9f;
        [SerializeField, Min(.01f)] private float _turnResponseSeconds = .12f;
        [SerializeField, Tooltip("Paper footprint in world units: half width (X) and front reach (Y).")]
        private Vector2 _leafStopFootprint = new(2.8f, 2.05f);

        public struct StepResult
        {
            public Vector3 position;
            public Quaternion rotation;
            public float distance;
            public bool atEnd, blocked;
        }
        public StepResult Step(BranchRideTrack track, float distance, Quaternion rotation, float dt,
            float speed, float gravity, float height, float leafVerticalTolerance, IReadOnlyList<LeafProjectile> leaves,
            Vector3 incoming, Vector3 outgoing)
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
            float length = track.Length;
            float nextDistance = Mathf.Min(length, distance + speed * dt);
            Vector3 start = track.Sample(distance, out _, _cornerBlendDistance, incoming, outgoing);
            Vector3 candidate = track.Sample(nextDistance, out var direction, _cornerBlendDistance, incoming, outgoing);
            result.blocked = IsBlockedByLeaf(track, distance, start, candidate, direction, leafVerticalTolerance, leaves);
            if (!result.blocked) result.distance = nextDistance;
            else candidate = track.Sample(distance, out direction, _cornerBlendDistance, incoming, outgoing);
            result.position = candidate + Vector3.up * height;
            Vector3 planar = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planar.sqrMagnitude > .001f)
                result.rotation = Quaternion.Slerp(rotation, Quaternion.LookRotation(planar),
                    1f - Mathf.Exp(-dt / _turnResponseSeconds));
            result.atEnd = !result.blocked && result.distance >= length - .0001f;
            return result;
        }

        private bool IsBlockedByLeaf(BranchRideTrack track, float distance, Vector3 start, Vector3 candidate,
            Vector3 direction, float verticalTolerance, IReadOnlyList<LeafProjectile> leaves)
        {
            Vector3 forward = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            float frontReach = _leafStopFootprint.y + Mathf.Max(0f, Vector3.Dot(candidate - start, forward));
            float horizontalFraction = Vector3.Dot(direction, forward);
            foreach (var leaf in leaves)
            {
                if (leaf == null || !leaf.Installed) continue;
                Vector3 contact = leaf.ContactPoint;
                Vector3 delta = contact - start;
                float ahead = Vector3.Dot(delta, forward);
                if (ahead < -.05f || ahead > frontReach || Mathf.Abs(Vector3.Dot(delta, right)) > _leafStopFootprint.x)
                    continue;

                // Check height against the branch under the paper, including on sloped spans.
                float alongTrack = Mathf.Max(0f, ahead) / Mathf.Max(.01f, horizontalFraction);
                float trackHeight = track.Sample(Mathf.Min(track.Length, distance + alongTrack), out _,
                    _cornerBlendDistance).y;
                if (Mathf.Abs(contact.y - trackHeight) <= verticalTolerance) return true;
            }
            return false;
        }
    }
}
