using System;
using UnityEngine;
namespace Herbalist.Abilities
{
    // Host/offline gameplay. Presentation reads the held blob's replicated stream.
    public sealed class SapHoseSprayer
    {
        private float _reach;
        private Vector3 _lastDirection = Vector3.forward;

        public void Reset()
        {
            _reach = 0f;
            _lastDirection = Vector3.forward;
        }
public void Tick(SapDeposit held, Ray aim, bool firing, float dt, SapAbilitySettings settings, Func<SapDeposit> createMark)
        {
            Vector3 origin = held.transform.position;
            float deltaTime = Mathf.Max(0f, dt);

            if (firing)
            {
                Vector3 target = aim.GetPoint(settings.controlRange + Vector3.Distance(aim.origin, origin));
                if (Physics.Raycast(aim, out var aimHit, settings.controlRange + Vector3.Distance(aim.origin, origin),
                    settings.collisionMask, QueryTriggerInteraction.Ignore))
                    target = aimHit.point;

                Vector3 toTarget = target - origin;
                if (toTarget.sqrMagnitude > 0.000001f)
                    _lastDirection = toTarget.normalized;
                else if (aim.direction.sqrMagnitude > 0.000001f)
                    _lastDirection = aim.direction.normalized;

                _reach = Mathf.Min(settings.controlRange, _reach + settings.hoseExtendSpeed * deltaTime);
            }
            else
            {
                _reach = Mathf.Max(0f, _reach - settings.hoseRetractSpeed * deltaTime);
            }

            if (_reach <= 0.01f)
            {
                _reach = 0f;
                held.SetStream(origin, false);
                return;
            }

            Vector3 end = origin + _lastDirection * _reach;
            if (Physics.Raycast(origin, _lastDirection, out var hit, _reach,
                settings.collisionMask, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;
                _reach = Mathf.Min(_reach, hit.distance);
                if (firing)
                    SapDeposit.ApplyHoseHit(hit, settings, deltaTime, createMark);
            }

            held.SetHoseStream(end, !firing);
        }
    }
}
