#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Herbalist.Abilities;
using UnityEngine;
namespace Herbalist.Networking
{
    // Injects sequence counters into the existing input reader; Fusion OnInput and Host validation remain in the path.
    // This tests transport/gameplay, not physical keyboard focus or cursor capture.
    public static class SapNetworkSmokeTest
    {
        public static string Result { get; private set; }
        private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
        private static void Press(AbilityInputReader input, string property)
        {
            var field = typeof(AbilityInputReader).GetField("<" + property + ">k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(input, unchecked((uint)field.GetValue(input) + 1));
        }
        private static async Task Until(Func<bool> predicate, string label, float timeout = 15)
        {
            float end = Time.realtimeSinceStartup + timeout;
            while (!predicate() && Time.realtimeSinceStartup < end) await Task.Delay(50);
            Check(predicate(), label);
        }
        private static SapDeposit Placed() => UnityEngine.Object.FindObjectsByType<SapDeposit>(FindObjectsSortMode.None).FirstOrDefault(s => s.IsPlaced);
        private static void AimAt(NetworkPlayer player, Vector3 point)
        {
            Vector3 direction = point - (player.transform.position + Vector3.up * 1.3f);
            var rotation = Quaternion.LookRotation(direction).eulerAngles;
            player.Player.View.SetLookAngles(rotation.y, Mathf.DeltaAngle(0, rotation.x));
        }
        public static async void RunHost()
        {
            Result = "Running";
            try
            {
                await Until(() => NetworkPlayer.Local != null, "local host player", 60);
                var local = NetworkPlayer.Local; Check(local.HasStateAuthority, "must run on Host");
                var ability = local.GetComponent<PlayerAbilityController>(); Check(ability.Kind == PlayerAbilityKind.Sap && ability.Unlocked, "Host sap grant");
                var input = ability.Input;
                var target = LeafInstallTarget.Find(100); Check(target != null, "platform test target");
                Vector3 point = new Vector3(target.transform.position.x, 1.3f, target.transform.position.z);
                AimAt(local, point);
                Press(input, "CycleSequence"); await Until(() => ability.Sap.Controlling, "host extraction");
                await Task.Delay(750); Check(local.MovementBlocked, "network movement lock");
                Press(input, "CycleSequence"); await Until(() => !ability.Sap.Controlling, "host cancel");
                await Task.Delay(400); Check(!local.MovementBlocked, "network movement unlock");
                for (int attempt=0; attempt<2; attempt++)
                {
                    Press(input, "CycleSequence"); await Until(() => ability.Sap.CanPlace, "host can place");
                    Press(input, "UseSequence"); await Until(() => Placed() != null, "host placed");
                    var placed = Placed();
                    if (attempt == 0)
                    {
                        Debug.Log("[SapNetworkTest] HOST_ATTACHED_EXPIRY");
                        await Until(() => placed == null || placed.State == SapState.Complete, "host expiry", 10);
                        await Task.Delay(1000);
                    }
                    else
                    {
                        await Until(() => placed.State == SapState.Bound, "client leaf binds host sap", 5);
                        await Task.Delay(6500); Check(placed != null && placed.State == SapState.Bound, "bound beyond lifetime");
                        var leaf = UnityEngine.Object.FindObjectsByType<LeafProjectile>(FindObjectsSortMode.None).First(l => l.State == LeafState.Bound);
                        leaf.BeginReturn(); await Until(() => ability.Sap.ActiveCount == 0, "sap removed on recall");
                    }
                }
                Result = "PASS: Host sap extraction/cancel, movement lock, attachment, expiry, client leaf binding, persistence and recall.";
                Debug.Log("[SapNetworkTest] " + Result);
            }
            catch (Exception e) { Result = "FAIL " + e; Debug.LogError("[SapNetworkTest] " + Result); }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void ObserveClient()
        {
            if (!Environment.GetCommandLineArgs().Contains("-herbalist-test-sap-observer")) return;
            try
            {
                await Until(() => NetworkPlayer.Local != null, "client spawn", 90);
                var local = NetworkPlayer.Local; Check(!local.HasStateAuthority, "client authority");
                await Until(() => Placed() != null, "first sap received", 90);
                var first = Placed(); Debug.Log("[SapNetworkTest] CLIENT_ATTACHED position=" + first.transform.position);
                await Until(() => first == null || first.State == SapState.Complete, "expiry replicated", 10);
                await Until(() => Placed() != null, "second sap received", 20);
                var second = Placed(); var ability = local.GetComponent<PlayerAbilityController>();
                Check(ability.Unlocked && ability.Kind == PlayerAbilityKind.Leaf, "client leaf grant");
                AimAt(local, second.transform.position);
                Press(ability.Input, "CycleSequence"); await Task.Delay(250); Press(ability.Input, "CycleSequence");
                await Until(() => ability.Mode == LeafMode.Platform, "client platform mode");
                Press(ability.Input, "UseSequence"); await Until(() => second.State == SapState.Bound, "binding replicated", 4);
                Check(UnityEngine.Object.FindObjectsByType<LeafProjectile>(FindObjectsSortMode.None).Any(l => l.State == LeafState.Bound), "bound leaf received");
                Debug.Log("[SapNetworkTest] CLIENT_BOUND");
                await Until(() => second == null || second.State == SapState.Complete, "recall cleanup replicated", 15);
                Debug.Log("[SapNetworkTest] CLIENT_PASS: received sap, expiry, own leaf input through Host, binding and recall cleanup.");
            }
            catch (Exception e) { Debug.LogError("[SapNetworkTest] CLIENT_FAIL " + e); }
        }
    }
}
#endif
