using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Herbalist.Player;
using Herbalist.Abilities;
using Herbalist.Interaction;

namespace Herbalist.Editor
{
    public static class PlayerInteractionChecks
    {
        public static string Run()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Run in Play mode.");
            var results = new List<string>();
            var config = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<PlayerTuning>("Assets/_Project/Data/Characters/SO_PlayerTuning.asset"));
            var scope = new GameObject("TemporaryPlayerInteractionCheck");
            scope.transform.position = new Vector3(1000, 0, 1000);
            try
            {
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.SetParent(scope.transform, false);
                floor.transform.localPosition = Vector3.down * .5f;
                floor.transform.localScale = new Vector3(20, 1, 20);
                var leafPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_LeafProjectile.prefab");
                foreach (LeafMode mode in new[] { LeafMode.Pin, LeafMode.Platform })
                {
                    var leaf = UnityEngine.Object.Instantiate(leafPrefab, scope.transform).GetComponent<LeafProjectile>();
                    leaf.transform.localPosition = Vector3.up * .7f;
                    leaf.transform.localRotation = Quaternion.identity;
                    leaf.ApplyReplica(mode, LeafState.Installed, 0);
                    var data = new SerializedObject(leaf);
                    var surface = (Collider)data.FindProperty(mode == LeafMode.Pin ? "pinCollider" : "platformCollider").objectReferenceValue;
                    var player = new GameObject("TestMotor");
                    player.SetActive(false);
                    player.transform.SetParent(scope.transform, false);
                    var cc = player.AddComponent<CharacterController>();
                    cc.height = 1.8f; cc.center = Vector3.up * .9f; cc.radius = .25f; cc.skinWidth = .03f;
                    var motor = player.AddComponent<CharacterControllerMotor>();
                    var motorData = new SerializedObject(motor);
                    motorData.FindProperty("controller").objectReferenceValue = cc;
                    motorData.FindProperty("tuning").objectReferenceValue = config;
                    motorData.ApplyModifiedPropertiesWithoutUndo();
                    player.SetActive(true);
                    Physics.SyncTransforms();
                    var center = surface.bounds.center;
                    var origin = new Vector3(center.x, .03f, center.z);
                    float top = surface.bounds.max.y;
                    foreach (bool restoreEachTick in new[] { false, true })
                    {
                        config.jumpHeight = 1.5f;
                        motor.Teleport(origin);
                        Physics.SyncTransforms();
                        for (int i = 0; i < 10; i++) motor.Simulate(Vector3.zero, .02f);
                        Require(motor.TryJump(), "Ground jump");
                        float peak = player.transform.position.y;
                        for (int i = 0; i < 100; i++)
                        {
                            if (restoreEachTick) motor.RestoreState(motor.CaptureState());
                            motor.Simulate(Vector3.zero, .02f);
                            peak = Mathf.Max(peak, player.transform.position.y);
                        }
                        Require(peak > top + .1f, mode + " passes upward");
                        Require(motor.IsGrounded && Mathf.Abs(player.transform.position.y - top) < .15f, mode + " lands above");
                        results.Add(mode + (restoreEachTick ? " prediction restore" : " offline") + " passes and lands");
                    }
                    config.jumpHeight = .1f;
                    motor.Teleport(origin); Physics.SyncTransforms();
                    for (int i = 0; i < 10; i++) motor.Simulate(Vector3.zero, .02f);
                    Require(motor.TryJump(), "Short jump");
                    for (int i = 0; i < 100; i++) motor.Simulate(Vector3.zero, .02f);
                    Require(player.transform.position.y < .15f && motor.IsGrounded, mode + " short jump returns below");
                    var other = new GameObject("OtherPlayer").AddComponent<CharacterController>();
                    other.transform.SetParent(scope.transform, false);
                    other.height = 1.8f; other.center = Vector3.up * .9f;
                    other.transform.position = new Vector3(center.x, top + .2f, center.z);
                    OneWayPlatform.PrepareMove(cc, Vector3.up);
                    OneWayPlatform.PrepareMove(other, Vector3.down);
                    Require(Physics.GetIgnoreCollision(cc, surface) && !Physics.GetIgnoreCollision(other, surface), "Per-player collision");
                    OneWayPlatform.Release(other);
                    Require(surface.Raycast(new Ray(new Vector3(center.x, top + 2, center.z), Vector3.down), out var hit, 4), "Aim ray preserved");
                    leaf.BeginReturn();
                    Require(!surface.enabled, "Recall removes surface");
                    UnityEngine.Object.DestroyImmediate(other.gameObject);
                    UnityEngine.Object.DestroyImmediate(player);
                    UnityEngine.Object.DestroyImmediate(leaf.gameObject);
                }
                config.headTrackingFadeAngles = new Vector2(75, 90);
                Require(Quaternion.Angle(config.ResolveHeadLook(0, 180, 35), Quaternion.identity) < .01f, "Front camera neutral head");
                Require(Quaternion.Angle(config.ResolveHeadLook(350, 170, -45), Quaternion.identity) < .01f, "Wrapped front camera");
                Require(Quaternion.Angle(config.ResolveHeadLook(0, 0, 35), Quaternion.Euler(35, 0, 0)) < .01f, "Rear camera keeps pitch");
                Require(Quaternion.Angle(config.ResolveHeadLook(0, 45, 0), Quaternion.Euler(0, 45, 0)) < .01f, "Rear-side tracking");
                var ui = AssetDatabase.LoadAssetAtPath<Herbalist.GameUI.GameUiSettings>("Assets/_Project/Data/UI/SO_GameUiSettings.asset");
                bool switchFound = false, cycleFound = false;
                foreach(var hint in ui.controls)
                {
                    if(hint.action.action.name == "SwitchAbility") switchFound = hint.offlineTestOnly && hint.action.action.bindings[0].effectivePath == "<Keyboard>/q";
                    if(hint.action.action.name == "Cycle") cycleFound = !hint.offlineTestOnly && hint.label == "능력 모드 / 제어 전환";
                }
                Require(switchFound && cycleFound, "Q/R help data");
                return "PASS: " + string.Join(", ", results) + "; short jumps, per-player pairs, recall, aim ray, head tracking, Q/R help.";
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(scope);
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        private static void Require(bool condition, string message)
        {
            if(!condition) throw new Exception("FAIL: " + message);
        }
    }
}
