using System;
using System.Linq;
using System.Reflection;
using Herbalist.Audio;
using Herbalist.StageOne;
using UnityEditor;
using UnityEngine;

namespace Herbalist.Editor
{
    [InitializeOnLoad]
    public static class SfxRevisionChecks
    {
        private const string PendingCheck = "Herbalist.SfxRevisionChecks.Pending";
        static SfxRevisionChecks()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(PendingCheck, false))
                    EditorApplication.update += RunPending;
            };
        }
        public static void StartPlayChecks()
        {
            SessionState.SetBool(PendingCheck, true);
            if (EditorApplication.isPlaying) EditorApplication.update += RunPending;
            else EditorApplication.isPlaying = true;
        }
        private static void RunPending()
        {
            if (!EditorApplication.isPlaying || Time.frameCount < 5) return;
            EditorApplication.update -= RunPending;
            SessionState.SetBool(PendingCheck, false);
            string report;
            try
            {
                report = SfxIntegrationChecks.ValidatePlayback() + "\n" + ValidateMinigamePlayback() + "\n" + ValidateFootContacts();
                report += "\nAudioListener volume=" + AudioListener.volume;
            }
            catch (Exception error) { report = "FAIL: " + error; }
            System.IO.File.WriteAllText("Temp/SfxRevisionPlayChecks.txt", report);
            Debug.Log(report);
        }
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static void Check(bool condition, string message)
        { if (!condition) throw new Exception("SFX revision: " + message); }
        private static void Call(object target, string method, params object[] args) =>
            target.GetType().GetMethod(method, Private).Invoke(target, args);
        private static bool Playing(SfxEmitter pool, int id)
        {
            var clip = pool.Catalog.Find(id).clips[0];
            return pool.GetComponentsInChildren<AudioSource>().Any(s => s.isPlaying && s.clip == clip);
        }

        public static string ValidateFootContacts()
        {
            Check(EditorApplication.isPlaying, "requires Play mode");
            var actor = StageActor.All.First(a => a.Local && a.GetComponent<Herbalist.Player.PlayerCharacterPresentation>() != null);
            var character = actor.GetComponent<CharacterController>();
            var presentation = actor.GetComponent<Herbalist.Player.PlayerCharacterPresentation>();
            var sfx = actor.GetComponent<PlayerSfxPresentation>();
            var pool = SfxEmitter.ScenePool;
            var tuning = pool.Catalog;
            var original = actor.transform.position;
            bool wasSodam = presentation.IsSodamActive, controllerEnabled = character.enabled;
            var ground = new GameObject("TemporaryFootContactProbe");
            ground.AddComponent<BoxCollider>().size = new Vector3(30, .2f, 30);
            float cooldown = tuning.Find(10).cooldown;
            try
            {
                character.enabled = false;
                actor.transform.position = new Vector3(10000, 100, 10000);
                float height = character.height * Mathf.Abs(actor.transform.lossyScale.y);
                float bottom = actor.transform.TransformPoint(character.center).y - height * .5f;
                ground.transform.position = new Vector3(10000, bottom - .1f, 10000);
                Physics.SyncTransforms();
                // Sample two entire cycles synchronously; bypass real-time cooldown for this accelerated check.
                tuning.Find(10).cooldown = 0;
                foreach (var role in new[] { Herbalist.Presentation.CharacterRole.Sodam, Herbalist.Presentation.CharacterRole.Duyeong })
                {
                    Check(presentation.SetOfflineRole(role), "offline role switch");
                    var animator = presentation.ActiveAnimator;
                    var culling = animator.cullingMode;
                    try
                    {
                        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                        animator.SetFloat("Speed", 10); animator.SetBool("Grounded", true); animator.SetFloat("PlaybackDirection", 1);
                        Call(sfx, "ResetFeet");
                        int played = pool.PlayedCount;
                        for (int i = 0; i < 180; i++)
                        {
                            animator.Play("Base Layer.Run", 0, i / 90f); animator.Update(0);
                            Call(sfx, "Footsteps", tuning, height);
                        }
                        Check(pool.PlayedCount - played == 4, role + " has four footfalls over two run cycles");
                        Call(sfx, "ResetFeet");
                        played = pool.PlayedCount;
                        for (int i = 0; i < 10; i++) Call(sfx, "Footsteps", tuning, height);
                        Check(pool.PlayedCount == played, "planted feet do not repeat on consecutive frames");
                    }
                    finally { animator.cullingMode = culling; }
                }
                return "PASS: Sodam and Duyeong each play four foot contacts over two actual run cycles; planted feet do not repeat.";
            }
            finally
            {
                tuning.Find(10).cooldown = cooldown;
                actor.transform.position = original; character.enabled = controllerEnabled;
                presentation.SetOfflineRole(wasSodam ? Herbalist.Presentation.CharacterRole.Sodam : Herbalist.Presentation.CharacterRole.Duyeong);
                UnityEngine.Object.DestroyImmediate(ground); Physics.SyncTransforms();
            }
        }

        // Runs against the authored main scene and its actual result/loop presentation.
        // Uses normal start/end paths without consuming herbs or altering progression.
        public static string ValidateMinigamePlayback()
        {
            Check(EditorApplication.isPlaying, "requires Play mode");
            var flow = StageOneFlow.Instance;
            var scene = UnityEngine.Object.FindFirstObjectByType<SceneSfxPresentation>();
            var pool = SfxEmitter.ScenePool;
            Check(flow != null && scene != null && pool != null, "authored scene audio and flow exist");
            Check(flow.Minigame == StageMinigameKind.None, "finish current minigame before checking");
            var actor = StageActor.All.FirstOrDefault(a => a.GetComponent<PlayerSfxPresentation>() != null);
            Check(actor != null && flow.sources.Length > 0 && flow.settings.recipes.Length > 0, "test actor/content");
            bool enabled = flow.enabled;
            try
            {
                flow.enabled = false;
                Call(scene, "LateUpdate");
                Call(flow, "StartGathering", actor, 0);
                Call(scene, "LateUpdate");
                Call(actor.GetComponent<PlayerSfxPresentation>(), "OnLocalCommand", StageCommand.Up);
                Check(Playing(pool, 20), "gather input actually playing");
                Call(flow, "EndMinigame", true, true);
                Call(scene, "LateUpdate");
                Check(Playing(pool, 21) && Playing(pool, 22), "successful gathering plays pull and success");
                Call(flow, "StartGathering", actor, 0);
                Call(scene, "LateUpdate");
                Call(flow, "EndMinigame", false, true);
                Call(scene, "LateUpdate");
                Check(Playing(pool, 23), "failed gathering plays failure");
                Call(flow, "StartCrafting", 0);
                typeof(StageOneFlow).GetProperty("Temperature").SetValue(flow, pool.Catalog.boilingTemperature + 1);
                Call(scene, "LateUpdate");
                Check(Playing(pool, 24) && Playing(pool, 25), "hearth and boiling loops actually playing");
                var crafter = StageActor.All.First(a => flow.RoleFor(a) == Herbalist.Presentation.CharacterRole.Sodam);
                int beforeFan = pool.PlayedCount;
                Call(crafter.GetComponent<PlayerSfxPresentation>(), "OnLocalCommand", StageCommand.Craft);
                Call(crafter.GetComponent<PlayerSfxPresentation>(), "OnLocalCommand", StageCommand.Craft);
                Check(pool.PlayedCount == beforeFan + 2 && Playing(pool, 26), "every brewing press plays a fan cue");
                Check(pool.GetComponentsInChildren<AudioSource>().Any(s => s.isPlaying && s.loop && s.clip == pool.Catalog.Find(25).clips[0]), "boiling loops");
                Call(flow, "EndMinigame", true, true);
                Call(scene, "LateUpdate");
                Check(!Playing(pool, 24) && !Playing(pool, 25), "craft completion stops hearth and boiling");
                Check(Playing(pool, 28), "craft result actually playing");
                return "PASS: authored main scene; gather input 20, successful pull/result 21/22, failure 23; hearth/boiling 24/25 loop during crafting and stop on completion; each brewing press plays 26; craft result 28.";
            }
            finally
            {
                if (flow.Minigame != StageMinigameKind.None) Call(flow, "EndMinigame", false, false);
                flow.enabled = enabled;
            }
        }
    }
}
