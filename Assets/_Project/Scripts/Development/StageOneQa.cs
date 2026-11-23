#if UNITY_EDITOR
using System;
using System.Linq;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.GameUI;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.Presentation;
using Herbalist.StageOne;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.StageOne
{
    public sealed partial class StageOneFlow
    {
        internal void ApplyQaPoint(string point, int sapSlot)
        {
            if (!Authority) throw new InvalidOperationException("Host가 1스테이지 상태를 준비해야 합니다.");
            RefreshActors();
            if (point == "start") return;

            var present = Actors.Where(a => a != null).ToArray();
            var duyeong = present.FirstOrDefault(a => RoleFor(a) == CharacterRole.Duyeong) ?? present[0];
            var sodam = present.FirstOrDefault(a => RoleFor(a) == CharacterRole.Sodam) ?? present[0];
            if (point.Contains("herb") || point.Contains("gather") || point.Contains("craft") || point == "transfer")
            {
                bool sap = point.StartsWith("sap-");
                var recipe = Array.FindIndex(settings.recipes, r => r.result.ability == (sap ? PlayerAbilityKind.Sap : PlayerAbilityKind.Leaf));
                if (recipe < 0) throw new InvalidOperationException("필요한 포션 레시피를 찾을 수 없습니다.");
                int source = Array.FindIndex(sources, h => h != null && h.item == settings.recipes[recipe].ingredient);
                if (source < 0) throw new InvalidOperationException("채집용 약초 위치를 찾을 수 없습니다.");
                var herb = sources[source];
                if (point.EndsWith("craft"))
                {
                    if (IsSoloRoleTest(sodam) && RoleFor(sodam) != CharacterRole.Sodam)
                        sodam.GetComponent<OfflineAbilitySwitcher>().SetOfflineRole(CharacterRole.Sodam);
                    Progress.Harvested |= 1UL << source;
                    Progress.Held[settings.SlotForRole(CharacterRole.Sodam)] = settings.ItemId(herb.item);
                    StartCrafting(recipe);
                }
                else if (point == "transfer")
                {
                    if (duyeong == sodam) throw new InvalidOperationException("약초 전달에는 두 플레이어가 필요합니다.");
                    QaStagePlacement.Near(duyeong, herb.Point);
                    QaStagePlacement.Near(sodam, duyeong.transform.position + duyeong.transform.right * 1.5f, 1f);
                    Progress.Gather(duyeong.Slot, source, settings.ItemId(herb.item), CharacterRole.Duyeong);
                }
                else
                {
                    if (IsSoloRoleTest(duyeong) && RoleFor(duyeong) != CharacterRole.Duyeong)
                        duyeong.GetComponent<OfflineAbilitySwitcher>().SetOfflineRole(CharacterRole.Duyeong);
                    QaStagePlacement.Near(duyeong, herb.Point, settings.interactionRange * .7f);
                    if (point.EndsWith("gather")) StartGathering(duyeong, source);
                }
                return;
            }

            foreach (var actor in present)
            {
                var kind = actor.Slot == sapSlot ? PlayerAbilityKind.Sap : PlayerAbilityKind.Leaf;
                var recipe = settings.recipes.First(r => r.result.ability == kind);
                Progress.Held[actor.Slot] = settings.ItemId(recipe.result);
                if (point != "drink" && !Drink(actor))
                    throw new InvalidOperationException("테스트용 포션을 복용할 수 없습니다.");
            }
            Progress.Harvested = sources.Length == 64 ? ulong.MaxValue : (1UL << sources.Length) - 1;
            Progress.Crafted = settings.RequiredMask;
            if (point is "drink" or "potions") return;

            var traversal = Object.FindAnyObjectByType<StageOneTraversal>();
            if (point.StartsWith("save-") || point.StartsWith("forest-") || point == "first-landed")
            {
                traversal.ApplyQaCheckpoint(point, present);
                return;
            }

            var sapActor = Actors[sapSlot] ?? present[0];
            var leafActor = Actors[1 - sapSlot] ?? present[0];
            if (point is "arrival-one" or "arrival-two")
            {
                Vector3 center = interior.transform.TransformPoint(interior.center);
                var local = present.FirstOrDefault(a => a.Local) ?? present[0];
                var waiter = point == "arrival-one" && present.Length == 2 ? present.First(a => a != local) : local;
                Vector3 toward = Vector3.ProjectOnPlane(waiter.transform.position - center, Vector3.up).normalized;
                if (toward.sqrMagnitude < .01f) toward = -interior.transform.forward;
                Vector3 edge = interior.ClosestPoint(center + toward * 100f);
                foreach (var actor in present)
                {
                    bool shouldBeInside = actor != waiter;
                    var capsule = actor.GetComponent<CharacterController>();
                    Vector3 target = shouldBeInside ? center : edge + toward * 2f;
                    Vector3 position = QaStagePlacement.Ground(actor, target, 8f, accepts: candidate =>
                        Physics.ComputePenetration(interior, interior.transform.position, interior.transform.rotation,
                            capsule, candidate, capsule.transform.rotation, out _, out _) == shouldBeInside);
                    QaStagePlacement.Teleport(actor, position);
                }
                return;
            }

            var mechanisms = Object.FindObjectsByType<StageOneDocumentMechanism>();
            var cloths = Object.FindObjectsByType<HangingCloth>();
            switch (point)
            {
                case "first-platform":
                    var firstPad = Object.FindAnyObjectByType<StageNarrativePresenter>()?.QaFirstJumpPad;
                    if (firstPad == null || firstPad.QaTarget == null || firstPad.QaPort == null)
                        throw new InvalidOperationException("첫 점프대의 위치를 찾을 수 없습니다.");
                    QaStagePlacement.Near(leafActor, firstPad.QaTarget.SocketPosition, 8f);
                    if (sapActor != leafActor) QaStagePlacement.Near(sapActor, firstPad.QaPort.InteractionPoint, 10f);
                    break;
                case "cloth":
                    var cloth = cloths.First(c => c.name == "Coop_1_SingleUseCloth");
                    cloth.PrepareQaLowered();
                    QaStagePlacement.Near(leafActor, cloth.QaGripPoint);
                    break;
                case "inlet":
                    var firstInlet = cloths.First(c => c.name == "Coop_1_SingleUseCloth");
                    QaStagePlacement.Near(sapActor, firstInlet.QaWaterPoint, 12f);
                    break;
                case "leaf-cross":
                    var target = Object.FindObjectsByType<LeafInstallTarget>().First(t => t.name == "LeafBranch_31");
                    QaStagePlacement.Near(leafActor, target.SocketPosition, 9f);
                    break;
                case "water-block":
                    var blocker = mechanisms.First(m => m.Kind == StageOneDocumentMechanismKind.BlockJet && m.QaTarget != null);
                    QaStagePlacement.Near(leafActor, blocker.QaTarget.SocketPosition, 12f);
                    break;
                case "cloth-lower":
                case "last-cloth":
                    var waterCloth = cloths.First(c => c.name == (point == "cloth-lower" ? "Coop_2_WaterCloth" : "Coop_5_WaterCloth"));
                    if (point == "last-cloth")
                    {
                        waterCloth.PrepareQaLowered();
                        QaStagePlacement.Near(leafActor, waterCloth.QaGripPoint);
                    }
                    else QaStagePlacement.Near(sapActor, waterCloth.QaWaterPoint, 12f);
                    break;
                case "grow":
                    mechanisms.First(m => m.Kind == StageOneDocumentMechanismKind.GrowingMushrooms && m.QaPort != null)
                        .PrepareQaInteraction(sapActor);
                    break;
                case "launch":
                    var pad = Object.FindObjectsByType<ExteriorJumpPad>()
                        .Where(p => p.isActiveAndEnabled && p.QaTarget != null && p.QaPort != null)
                        .OrderBy(p => p.QaTarget.SocketPosition.y).First();
                    QaStagePlacement.Near(sapActor, pad.QaPort.InteractionPoint, 14f);
                    QaStagePlacement.Near(leafActor, pad.QaTarget.SocketPosition, 12f);
                    break;
                case "cloth-swap":
                    mechanisms.First(m => m.name == "Coop_4_LeafButtonLowersBranch").PrepareQaInteraction(leafActor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(point), point, "등록되지 않은 1스테이지 지점");
            }
        }
    }
}

namespace Herbalist.Levels
{
    internal static class QaStagePlacement
    {
        internal static void AssignOfflineAbility(StageActor actor, int sapSlot)
        {
            var ability = actor.Abilities;
            var desired = actor.Slot == sapSlot ? PlayerAbilityKind.Sap : PlayerAbilityKind.Leaf;
            if (ability.Unlocked && ability.Kind == desired) return;
            if (ability.Unlocked) ability.RevokeLeaf();
            if (desired == PlayerAbilityKind.Sap) ability.UnlockSap();
            else ability.UnlockLeaf();
        }

        internal static void Teleport(StageActor actor, Vector3 position)
        {
            if (actor.GetComponent<NetworkPlayer>() is { } net && net.Object != null && net.Object.IsValid)
                net.TeleportAuthoritatively(position);
            else
            {
                var motor = actor.GetComponent<PlayerController>().Motor;
                motor.Teleport(position);
                motor.ResetMotion();
            }
        }

        internal static Vector3 Ground(StageActor actor, Vector3 target, float radius, Collider preferred = null, Func<Vector3, bool> accepts = null)
        {
            var character = actor.GetComponent<CharacterController>();
            float footOffset = character.height * Mathf.Abs(character.transform.lossyScale.y) * .5f -
                character.transform.TransformVector(character.center).y + .08f;
            const int directions = 12;
            const int rings = 5;
            Vector3 best = target;
            float bestScore = float.PositiveInfinity;
            for (int ring = 0; ring <= rings; ring++)
            {
                int count = ring == 0 ? 1 : directions;
                for (int j = 0; j < count; j++)
                {
                    float theta = j * Mathf.PI * 2f / directions;
                    Vector3 offset = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)) * (ring * radius / rings);
                    var ray = new Ray(target + offset + Vector3.up * 4f, Vector3.down);
                    foreach (var hit in Physics.RaycastAll(ray, preferred != null ? 18f : Mathf.Max(12f, radius * 2f), ~0, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider is CharacterController || hit.normal.y < .55f || preferred != null && hit.collider != preferred) continue;
                        Vector3 candidate = hit.point + Vector3.up * footOffset;
                        if (accepts != null && !accepts(candidate)) continue;
                        float score = offset.magnitude + Mathf.Abs(hit.point.y - target.y) * 1.2f;
                        if (score >= bestScore) continue;
                        best = candidate;
                        bestScore = score;
                    }
                }
            }
            if (!float.IsFinite(bestScore))
                throw new InvalidOperationException("QA 지점 근처의 발 디딜 곳을 찾을 수 없습니다. " + target);
            return best;
        }

        internal static void Near(StageActor actor, Vector3 target, float radius = 10f)
        {
            Teleport(actor, Ground(actor, target, radius));
            var view = actor.GetComponent<PlayerController>().View;
            Vector3 direction = target - actor.transform.position;
            if (direction.sqrMagnitude < .1f || !actor.Local) return;
            float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            view.SetLookAngles(yaw, view.Pitch);
        }
    }

    public sealed partial class StageOneTraversal
    {
        internal void ApplyQaCheckpoint(string point, StageActor[] players)
        {
            if (point == "forest-before")
            {
                Vector3 anchor = _forestExitCheckpoint.bounds.center;
                Vector3 toward = Vector3.ProjectOnPlane(_starts[0].position - anchor, Vector3.up).normalized;
                for (int i = 0; i < players.Length; i++)
                    QaStagePlacement.Near(players[i], anchor + toward * (_forestExitCheckpoint.bounds.extents.magnitude + 1f) +
                        _forestExitCheckpoint.transform.right * (i * 1.5f), 5f);
                return;
            }

            int index = point == "forest-saved" ? 0 : point == "first-landed" ?
                Array.FindIndex(_sharedCheckpoints, 1, g => Array.Exists(g.surfaces, s => s != null && !s.isTrigger)) :
                int.Parse(point.Substring("save-".Length));
            if (index < 0 || index >= _sharedCheckpoints.Length) throw new InvalidOperationException("체크포인트가 없습니다.");
            var group = _sharedCheckpoints[index];
            var surface = group.surfaces.FirstOrDefault(s => s != null && s.enabled && s.gameObject.activeInHierarchy);
            if (surface == null) throw new InvalidOperationException("현재 씬에서 사용할 수 없는 체크포인트입니다.");
            _activeCheckpoint = index;
            _activeCheckpointHeight = surface.bounds.max.y;
            foreach (var actor in players)
            {
                int slot = actor.Slot;
                Vector3 target = group.recoveryPoint != null ? group.recoveryPoint.position : surface.bounds.center;
                Vector3 offset = (_starts[slot].position - _starts[0].position) * .45f;
                if (group.recoveryPoint != null) offset = _starts[slot].position - _starts[0].position;
                var position = QaStagePlacement.Ground(actor, target + offset,
                    Mathf.Max(4f, Mathf.Min(15f, surface.bounds.extents.magnitude)), surface.isTrigger ? null : surface);
                group.positions[slot] = position;
                group.arrived |= 1 << slot;
                _forestExitReached.Add(actor);
                _recovery[actor] = position;
                QaStagePlacement.Teleport(actor, position);
            }
        }
    }

    public sealed partial class StageOneDocumentMechanism
    {
        internal SapInjectionPort QaPort => _port;
        internal LeafInstallTarget QaTarget => _leafTarget;

        internal void PrepareQaInteraction(StageActor actor)
        {
            _requiredCloth?.PrepareQaUsed();
            Vector3 point = _port != null ? _port.InteractionPoint : _leafTarget != null ? _leafTarget.SocketPosition : transform.position;
            QaStagePlacement.Near(actor, _activationArea != null ? _activationArea.bounds.center : point, 12f);
        }
    }

    public sealed partial class HangingCloth
    {
        internal Vector3 QaGripPoint => _knot.position;
        internal Vector3 QaWaterPoint => _loweringPort != null ? _loweringPort.InteractionPoint : _knot.position;

        internal void PrepareQaUsed()
        {
            if (!Authority) return;
            _used = true;
            _retraction = 1f;
            _completedUse = true;
            if (Online) CompletedUse = true;
            Publish();
        }

        internal void PrepareQaLowered()
        {
            if (!Authority) return;
            _deployment = 1f;
            if (Online) Deployment = _deployment;
            Publish();
            Present(_angles, false, default, 0, default);
        }
    }

    public sealed partial class ExteriorJumpPad
    {
        internal LeafInstallTarget QaTarget => _sockets?.FirstOrDefault(t => t != null);
        internal SapInjectionPort QaPort => _ports?.FirstOrDefault(p => p != null);
    }
}
#endif
