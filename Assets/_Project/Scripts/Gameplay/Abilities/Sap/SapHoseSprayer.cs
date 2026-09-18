using System;
using UnityEngine;
namespace Herbalist.Abilities
{
    // Host/offline gameplay. Presentation reads the held blob's replicated stream.
    public sealed class SapHoseSprayer
    {
        private float reach;
        public void Reset() => reach = 0;
        public void Tick(SapDeposit held, Ray aim, bool firing, float dt, SapAbilitySettings settings, Func<SapDeposit> createMark)
        {
            if (!firing) { reach = 0; held.SetStream(held.transform.position, false); return; }
            Vector3 origin = held.transform.position;
            Vector3 target = aim.GetPoint(settings.controlRange + Vector3.Distance(aim.origin, origin));
            if (Physics.Raycast(aim, out var aimHit, settings.controlRange + Vector3.Distance(aim.origin, origin),
                settings.collisionMask, QueryTriggerInteraction.Ignore)) target = aimHit.point;
            Vector3 direction = (target - origin).normalized;
            reach = Mathf.Min(settings.controlRange, reach + settings.flightSpeed * dt);
            Vector3 end = origin + direction * reach;
            if (Physics.Raycast(origin, direction, out var hit, reach, settings.collisionMask, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;
                SapDeposit.ApplyHoseHit(hit, settings, dt, createMark);
            }
            held.SetHoseStream(end);
        }
    }
}
