using System;
using System.Linq;
using System.Reflection;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEditor;
using UnityEngine;

namespace Herbalist.Editor
{
    public static class StageOneFollowupChecks
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static T Read<T>(object obj, string name) => (T)obj.GetType().GetField(name, Private).GetValue(obj);
        private static void Check(bool value, string message) { if (!value) throw new Exception(message); }

        public static string WaterSignature()
        {
            var text = new System.Text.StringBuilder();
            var streams = UnityEngine.Object.FindObjectsByType<StageOneFixedWaterStream>(FindObjectsSortMode.None)
                .Where(w => Read<CapsuleCollider>(w, "_pushVolume") != null).OrderBy(w => w.transform.parent.name).ToArray();
            Check(streams.Length == 3, "Three solid blocking streams.");
            foreach (var stream in streams)
            {
                stream.RefreshPresentation();
                var child = stream.transform.Find("Conical Water Mesh");
                var mesh = child.GetComponent<MeshFilter>().sharedMesh;
                var material = child.GetComponent<MeshRenderer>().sharedMaterial;
                Check(mesh != null && mesh.bounds.size.sqrMagnitude > 1, "Edit and Play both build nonempty volume.");
                Check(material.GetFloat("_SolidWater") == 1 && material.GetFloat("_ZWrite") == 1, "Opaque depth-writing filled water.");
                Check(!stream.GetComponent<LineRenderer>().enabled, "Flat preview disabled.");
                var points = mesh.vertices; var indices = mesh.triangles;
                Check(indices.Count(i => i == points.Length - 1) == 12 && indices.Count(i => i == points.Length - 2) == 12,
                    "Both water openings have complete twelve-triangle caps.");
                foreach (var p in points) text.Append(p.x.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append(',')
                    .Append(p.y.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append(',')
                    .Append(p.z.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append(';');
            }
            using (var sha = System.Security.Cryptography.SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text.ToString()))).Replace("-", "");
        }

        public static string BranchHolePlay()
        {
            Check(Application.isPlaying, "Offline Stage One Play required.");
            var pad = GameObject.Find("Stage1_DocumentGimmicks/BranchHole_LeftLeaf_RightWater_20261109").GetComponent<ExteriorJumpPad>();
            var target = Read<LeafInstallTarget[]>(pad, "_sockets")[0];
            var inlet = Read<SapInjectionPort[]>(pad, "_ports")[0];
            Check(Vector3.Distance(target.SocketPosition, new Vector3(278.4957f, 87.2164f, 330.1857f)) < .01f, "Photo-left leaf socket.");
            Check(Vector3.Distance(inlet.InteractionPoint, new Vector3(287.4343f, 83.6032f, 322.0099f)) < .01f, "Photo-right water inlet.");
            Check(GameObject.Find("Stage1_DocumentGimmicks/OutletCorrections_20261104/Photo1_Branch_UpperLeaf_LowerWater") != null, "branch_hole (1) pair retained.");
            var actor = StageActor.All.First(a => a.Available); var player = actor.GetComponent<PlayerController>();
            var saved = player.Motor.CaptureState(); var settings = StageLevel.Instance.abilityRules;
            LeafProjectile leaf = null; SapDeposit water = null;
            try
            {
                leaf = UnityEngine.Object.Instantiate(settings.leaf.offlinePrefab);
                leaf.Initialize(settings.leaf, actor.transform, target.SocketPosition, target.SocketPosition, null, true);
                Check(leaf.TryInstallAtSocket(target), "Actual leaf installation.");
                var cc = actor.GetComponent<CharacterController>(); var surface = Read<Collider>(leaf, "_surface");
                Physics.SyncTransforms(); var feet = surface.bounds.center; feet.y = surface.bounds.max.y + .02f;
                player.Motor.Teleport(feet - actor.transform.TransformVector(cc.center) + Vector3.up * cc.height * Mathf.Abs(cc.transform.lossyScale.y) * .5f);
                Physics.SyncTransforms(); Check(leaf.Supports(cc, .64f), "Actual rider stands on left leaf.");
                water = UnityEngine.Object.Instantiate(settings.sap.offlinePrefab); water.transform.position = inlet.InteractionPoint + Vector3.up * 5;
                var hose = new SapHoseSprayer(); var ray = new Ray(water.transform.position, Vector3.down);
                for (int i = 0; i < 100; i++) { hose.Tick(water, ray, true, .02f, settings.sap, () => null); pad.Tick(.02f); }
                Check(pad.ChargeFraction >= 1, "Actual water charges right inlet.");
                pad.Tick(3.1f);
                Check(player.Motor.Velocity.y > 0 && leaf.State == LeafState.Returning, "Left rider launches and used leaf recalls.");
                return "PASS: branch_hole photo-left leaf, photo-right ballistic water charge, real rider launch and leaf recall; branch_hole (1) retained.";
            }
            finally
            {
                if (leaf != null) UnityEngine.Object.DestroyImmediate(leaf.gameObject);
                if (water != null) UnityEngine.Object.DestroyImmediate(water.gameObject);
                player.Motor.RestoreState(saved);
            }
        }
    }
}
