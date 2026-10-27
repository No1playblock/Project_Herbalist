using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Interaction
{
    // Collision is per character; other players and aim rays remain unaffected.
    [RequireComponent(typeof(Collider))]
    public sealed class OneWayPlatform : MonoBehaviour
    {
        [SerializeField] private Collider _surface;
        [SerializeField, Min(0)] private float _surfaceTolerance = 0.02f;
        private static readonly HashSet<OneWayPlatform> Active = new HashSet<OneWayPlatform>();
        private readonly HashSet<CharacterController> _ignored = new HashSet<CharacterController>();

        private void OnEnable()
        {
            if (_surface == null) _surface = GetComponent<Collider>();
            Active.Add(this);
        }

        public static void PrepareMove(CharacterController character, Vector3 velocity)
        {
            Vector3 feet = character.transform.TransformPoint(character.center);
            feet.y -= character.height * Mathf.Abs(character.transform.lossyScale.y) * 0.5f;
            foreach (var platform in Active)
            {
                var surface = platform._surface;
                if (surface == null || !surface.enabled) continue;
                var bounds = surface.bounds;
                // A tilted leaf's AABB top can be far above the actual landing surface.
                // Use the surface under the feet, with the nearest edge as a fallback.
                Vector3 probe = new Vector3(feet.x, bounds.max.y + 1f, feet.z);
                float distance = bounds.size.y + 2f;
                bool found = surface.Raycast(new Ray(probe, Vector3.down), out var hit, distance);
                if (!found)
                {
                    var edge = surface.ClosestPoint(probe);
                    probe.x = Mathf.Lerp(edge.x, bounds.center.x, .001f);
                    probe.z = Mathf.Lerp(edge.z, bounds.center.z, .001f);
                    found = surface.Raycast(new Ray(probe, Vector3.down), out hit, distance);
                }
                float top = found ? hit.point.y : bounds.max.y;
                float skin = character.skinWidth * Mathf.Abs(character.transform.lossyScale.y);
                bool ignore = velocity.y > 0 || feet.y < top - skin - platform._surfaceTolerance;
                // Fusion RestoreState toggles the controller; reapply even when the desired pair state is unchanged.
                Physics.IgnoreCollision(character, surface, ignore);
                if (ignore) platform._ignored.Add(character);
                else platform._ignored.Remove(character);
            }
        }

        public static void Release(CharacterController character)
        {
            if (character == null) return;
            foreach (var platform in Active)
                if (platform._ignored.Remove(character) && platform._surface != null)
                    Physics.IgnoreCollision(character, platform._surface, false);
        }

        private void OnDisable()
        {
            Active.Remove(this);
            foreach (var character in _ignored)
                if (character != null && _surface != null) Physics.IgnoreCollision(character, _surface, false);
            _ignored.Clear();
        }
    }
}
