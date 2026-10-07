using System;
using UnityEngine;
namespace Herbalist.Abilities
{
    // Host/offline gameplay. Presentation reads the held blob's replicated stream.
    public sealed class SapHoseSprayer
    {
        private float _reach;
        private float _retractedLength;
        private Vector3 _lastDirection = Vector3.forward;

        public void Reset()
        {
            _reach = 0f;
            _retractedLength = 0f;
            _lastDirection = Vector3.forward;
        }
public void Tick(SapDeposit held, Ray aim, bool firing, float dt, SapAbilitySettings settings, Func<SapDeposit> createMark)
        {
            Vector3 origin = held.transform.position;
            float deltaTime = Mathf.Max(0f, dt);

            if (firing)
            {
                _retractedLength = 0f;
                Vector3 target = aim.GetPoint(settings.controlRange + Vector3.Distance(aim.origin, origin));
                bool hasTarget = Physics.Raycast(aim, out var aimHit, settings.controlRange + Vector3.Distance(aim.origin, origin),
                    settings.collisionMask, QueryTriggerInteraction.Ignore);
                if (hasTarget)
                    target = aimHit.point;

                Vector3 toTarget = target - origin;
                if (toTarget.sqrMagnitude > 0.000001f)
                    _lastDirection = hasTarget ? ResolveDirection(toTarget, settings.hoseLaunchSpeed, settings.hoseGravity) : toTarget.normalized;
                else if (aim.direction.sqrMagnitude > 0.000001f)
                    _lastDirection = aim.direction.normalized;

                _reach = Mathf.Min(settings.hoseLaunchSpeed * settings.hoseMaxFlightTime, _reach + settings.hoseExtendSpeed * deltaTime);
            }
            else
            {
                // Stop emitting at the orb. The existing stream drains from its origin
                // toward the last tip instead of pulling the tip back toward the orb.
                _retractedLength = Mathf.Min(_reach, _retractedLength + settings.hoseRetractSpeed * deltaTime);
            }

            if (_reach <= 0.01f || _retractedLength >= _reach)
            {
                _reach = 0f;
                _retractedLength = 0f;
                held.SetStream(origin, false);
                return;
            }

            Vector3 launchVelocity = _lastDirection * settings.hoseLaunchSpeed;
            float travelTime = _reach / settings.hoseLaunchSpeed;
            Vector3 end = Position(origin, launchVelocity, settings.hoseGravity, travelTime);
            bool impact = false;
            Vector3 impactNormal = Vector3.up;
            int segments = Mathf.CeilToInt(travelTime / settings.hoseCollisionStep);
            Vector3 previous = origin;
            for (int i = 1; i <= segments; i++)
            {
                float nextTime = travelTime * i / segments;
                Vector3 next = Position(origin, launchVelocity, settings.hoseGravity, nextTime);
                Vector3 step = next - previous;
                if (step.sqrMagnitude > 0.000001f && Physics.Raycast(previous, step.normalized, out var hit,
                    step.magnitude, settings.collisionMask, QueryTriggerInteraction.Ignore))
                {
                    float previousTime = travelTime * (i - 1) / segments;
                    travelTime = Mathf.Lerp(previousTime, nextTime, hit.distance / step.magnitude);
                    end = hit.point;
                    impact = hit.collider.GetComponentInParent<Herbalist.Levels.ExteriorJumpPad>() == null;
                    impactNormal = hit.normal;
                    _reach = Mathf.Min(_reach, travelTime * settings.hoseLaunchSpeed);
                    if (firing) SapDeposit.ApplyHoseHit(hit, settings, deltaTime, createMark);
                    break;
                }
                previous = next;
            }

            held.SetHoseStream(end, !firing, launchVelocity, travelTime,
                _retractedLength / settings.hoseLaunchSpeed, impact, impactNormal);
        }

        private static Vector3 Position(Vector3 origin, Vector3 velocity, float gravity, float time) =>
            origin + velocity * time + Vector3.down * (0.5f * gravity * time * time);

        private static Vector3 ResolveDirection(Vector3 toTarget, float speed, float gravity)
        {
            Vector3 planar = Vector3.ProjectOnPlane(toTarget, Vector3.up);
            float distance = planar.magnitude;
            if (distance < 0.001f) return toTarget.normalized;
            float speedSquared = speed * speed;
            float discriminant = speedSquared * speedSquared - gravity * (gravity * distance * distance + 2f * toTarget.y * speedSquared);
            if (discriminant < 0f) return toTarget.normalized;
            float tangent = (speedSquared - Mathf.Sqrt(discriminant)) / (gravity * distance);
            return (planar.normalized + Vector3.up * tangent).normalized;
        }
    }
}
