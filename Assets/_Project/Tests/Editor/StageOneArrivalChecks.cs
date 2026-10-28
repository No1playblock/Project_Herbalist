using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Herbalist.Levels;
using Herbalist.StageOne;

namespace Herbalist.Editor
{
    public static class StageOneArrivalChecks
    {
        public static string Run()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Run in Play mode in CharacterBranch.");
            var flow = UnityEngine.Object.FindAnyObjectByType<StageOneFlow>();
            flow.RefreshActors();
            var actors = flow.Actors.ToArray();
            Require(actors.All(a => a != null && a.Available), "Two test actors required");
            var positions = actors.Select(a => a.transform.position).ToArray();
            var originalInterior = flow.interior;
            var exit = flow.GetComponent<StageExit>();
            bool exitEnabled = exit != null && exit.enabled;
            bool flowEnabled = flow.enabled;
            var progress = flow.Progress;
            var consumed = progress.Consumed.ToArray();
            uint crafted = progress.Crafted;
            bool cleared = progress.Cleared;
            var tick = typeof(StageOneFlow).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            var contact = typeof(StageOneFlow).GetMethod("IsInside", BindingFlags.Instance | BindingFlags.NonPublic);
            var scope = new GameObject("TemporaryArrivalContactCheck");
            try
            {
                if (exit != null) exit.enabled = false;
                flow.enabled = false;
                // The real scene's authority/gate prerequisites remain in use.
                Require(flow.Authority, "Run on authoritative/offline peer");
                progress.Crafted = flow.settings.RequiredMask;
                for (int i = 0; i < 2; i++) progress.Consumed[i] = flow.settings.ItemId(flow.settings.recipes[i].result);
                Require(flow.GateOpen, "Scene exit prerequisites satisfied");
                var boxes = new[] { originalInterior, scope.AddComponent<BoxCollider>() };
                boxes[1].isTrigger = true;
                boxes[1].size = originalInterior.size;
                scope.transform.SetPositionAndRotation(new Vector3(1200, 300, 1200), Quaternion.Euler(0, 37, 0));
                foreach (var box in boxes)
                {
                    flow.interior = box;
                    for (int first = 0; first < 2; first++)
                    {
                        progress.Cleared = false;
                        for (int i = 0; i < 2; i++) Place(i, false);
                        tick.Invoke(flow, null);
                        Require(flow.InsideCount == 0 && !progress.Cleared, "Neither touching");
                        Place(first, true);
                        for (int i = 0; i < 60; i++) tick.Invoke(flow, null);
                        Require(flow.InsideCount == 1 && !progress.Cleared, "First player waits");
                        Place(first, false); Place(1 - first, true);
                        tick.Invoke(flow, null);
                        Require(flow.InsideCount == 1 && !progress.Cleared, "Past contact is not retained");
                        Place(first, true);
                        tick.Invoke(flow, null);
                        Require(flow.InsideCount == 2 && progress.Cleared, "Sequential contacts clear");
                        var cc = actors[first].GetComponent<CharacterController>();
                        cc.enabled = false;
                        Require(!(bool)contact.Invoke(flow, new object[] { actors[first] }), "Disabled body ignored");
                        cc.enabled = true;
                    }
                    void Place(int slot, bool touching)
                    {
                        var actor = actors[slot];
                        var cc = actor.GetComponent<CharacterController>();
                        var scale = cc.transform.lossyScale;
                        float radius = cc.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
                        var center = box.transform.TransformPoint(box.center);
                        float halfWidth = box.size.x * Mathf.Abs(box.transform.lossyScale.x) * .5f;
                        // Body overlaps the edge, while the old interaction point is outside.
                        var bodyCenter = center + box.transform.right * (halfWidth + radius * (touching ? .5f : 3f));
                        actor.GetComponent<Herbalist.Player.PlayerController>().Motor.Teleport(bodyCenter - cc.transform.TransformVector(cc.center));
                        Require(!touching || Mathf.Abs((box.transform.InverseTransformPoint(actor.transform.position + flow.settings.interactionOffset) - box.center).x) > box.size.x * .5f,
                            "Regression covers root/probe outside with body touching");
                    }
                }
                return "PASS: actual/rotated exit, both entry orders, waiting, departure, body-edge overlap and disabled colliders.";
            }
            finally
            {
                flow.interior = originalInterior;
                progress.Crafted = crafted; progress.Cleared = cleared;
                Array.Copy(consumed, progress.Consumed, 2);
                for (int i = 0; i < 2; i++) actors[i].GetComponent<Herbalist.Player.PlayerController>().Motor.Teleport(positions[i]);
                flow.enabled = flowEnabled;
                if (exit != null) exit.enabled = exitEnabled;
                UnityEngine.Object.DestroyImmediate(scope);
            }
        }
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }
    }
}
