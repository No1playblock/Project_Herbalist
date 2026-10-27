using System;
using System.Collections.Generic;
using System.Reflection;
using Herbalist.Abilities;
using Herbalist.Player;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    // Isolated checks: can run without changing the open scene or controlling a live player.
    public static class SodamAnimationTimingChecks
    {
        private static void Check(bool value, string label)
        { if (!value) throw new Exception(label); }
        private static void Field(object target, string name, object value)
        { target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value); }

        public static string Run()
        {
            var objects = new List<Object>();
            try
            {
                var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<LeafAbilitySettings>(
                    "Assets/_Project/Data/Abilities/Leaf/SO_LeafAbilitySettings.asset"));
                objects.Add(settings);
                settings.hitMask = 0;
                settings.launchOffset = Vector3.zero;
                settings.returnSpeed = 10;
                var owner = new GameObject("AnimationTimingCheck");
                owner.SetActive(false); objects.Add(owner);
                owner.transform.position = Vector3.one * 10000;
                var ability = owner.AddComponent<LeafThrowAbility>();
                Field(ability, "settings", settings);
                Field(ability, "displaySlots", new LeafProjectile[settings.capacity]);
                LeafProjectile shot = null;
                int spawned = 0;
                ability.Configure(() => {
                    var go = new GameObject("TimingLeaf"); go.SetActive(false); objects.Add(go);
                    shot = go.AddComponent<LeafProjectile>(); spawned++; return shot;
                }, _ => {}, true);
                var aim = new Ray(owner.transform.position, Vector3.forward);
                Check(ability.TryThrow(aim), "throw accepted");
                Check(ability.ThrowSequence == 1 && ability.PreparingThrow && spawned == 0,
                    "motion begins before projectile spawn");
                Check(!ability.TryThrow(aim), "repeat input cannot duplicate a prepared throw");
                ability.Tick(settings.throwPreparationSeconds - .01f, aim);
                Check(spawned == 0, "no early projectile");
                ability.Tick(.011f, aim);
                Check(spawned == 1 && !ability.PreparingThrow, "one projectile at release time");
                shot.transform.position = owner.transform.position + Vector3.forward * 4;
                shot.BeginReturn();
                shot.Tick(.01f);
                Check(ability.CatchSequence == 0, "distant returning leaf does not trigger catch yet");
                shot.transform.position = owner.transform.position + Vector3.forward * 1.5f;
                shot.Tick(.01f);
                Check(ability.CatchSequence == 1 && shot.State == LeafState.Returning,
                    "catch starts while projectile is still approaching");
                shot.Tick(.01f);
                Check(ability.CatchSequence == 1, "one catch cue per return");
                ability.Clear();
                ability.Tick(settings.cooldown + .01f, aim);
                Check(ability.TryThrow(aim), "second throw accepted");
                ability.Clear(); ability.Tick(1, aim);
                Check(spawned == 1, "clearing cancels a prepared projectile");

                var profile = AssetDatabase.LoadAssetAtPath<SodamAnimationProfile>(
                    "Assets/_Project/Data/Characters/SO_SodamAnimationProfile.asset");
                Check(!profile.Find(SodamPose.LeafCatch).useRootMotion,
                    "catch does not move a stationary player and trigger walking feedback");
                var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/_Project/Art/Characters/sodam/BranchV12/Prefabs/sodamv12.prefab"));
                objects.Add(model); model.transform.position = Vector3.one * 11000;
                var animator = model.GetComponentInChildren<Animator>();
                animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    "Assets/_Project/Animations/Characters/Sodam/AC_SodamLocomotion.controller");
                animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.Rebind(); animator.Update(0);
                var presentation = owner.AddComponent<SodamAnimationPresentation>();
                Field(presentation, "_profile", profile); Field(presentation, "_animator", animator);
                var apply = typeof(SodamAnimationPresentation).GetMethod("Apply", BindingFlags.Instance | BindingFlags.NonPublic);
                apply.Invoke(presentation, new object[] { SodamPose.Jump, 0f }); animator.Update(.05f);
                int leg = animator.GetLayerIndex("leg");
                Check(animator.GetLayerWeight(leg) == 1 &&
                    animator.GetCurrentAnimatorStateInfo(leg).IsName("leg.sodam_lower_jump"), "jump lower layer active");
                Check(animator.GetCurrentAnimatorStateInfo(leg).normalizedTime > .38f,
                    "lower jump starts at takeoff instead of crouch preparation");
                apply.Invoke(presentation, new object[] { SodamPose.Idle, 0f }); animator.Update(.2f);
                Check(animator.GetLayerWeight(leg) == 0, "jump legs released after landing");
                apply.Invoke(presentation, new object[] { SodamPose.LeafThrow, 0f });
                animator.Update(.05f); animator.Update(.2f);
                var throwState = animator.GetCurrentAnimatorStateInfo(0);
                Check(throwState.IsName("Base Layer.sodam_throw") &&
                    throwState.normalizedTime * 3.3333335f > .5f &&
                    throwState.normalizedTime * 3.3333335f < .75f, "release occurs at the hand extension segment");
                var legs = typeof(SodamAnimationPresentation).GetMethod("UpdateActionLegs", BindingFlags.Instance | BindingFlags.NonPublic);
                legs.Invoke(presentation, new object[] { false }); animator.Update(.05f);
                Check(animator.GetLayerWeight(leg) == 1 &&
                    animator.GetCurrentAnimatorStateInfo(leg).IsName("leg.sodam_lower_jump"), "airborne throw keeps jump legs");
                legs.Invoke(presentation, new object[] { true });
                Check(animator.GetLayerWeight(leg) == 0, "landing during throw releases jump legs");
                foreach (var action in new[] { SodamPose.LeafThrow, SodamPose.LeafCatch, SodamPose.Sap })
                {
                    apply.Invoke(presentation, new object[] { action, 0f });
                    animator.SetFloat("Speed", 3f);
                    legs.Invoke(presentation, new object[] { true }); animator.Update(.2f);
                    Check(animator.GetLayerWeight(leg) == 1 &&
                        animator.GetCurrentAnimatorStateInfo(leg).IsName("leg.Run"), action + " uses moving legs");
                    var upper = animator.GetCurrentAnimatorStateInfo(0);
                    float stride = animator.GetCurrentAnimatorStateInfo(leg).normalizedTime;
                    legs.Invoke(presentation, new object[] { true }); animator.Update(.15f);
                    Check(animator.GetCurrentAnimatorStateInfo(leg).normalizedTime > stride,
                        action + " stride is not restarted every frame");
                    Check(animator.GetCurrentAnimatorStateInfo(0).fullPathHash == upper.fullPathHash &&
                        animator.GetCurrentAnimatorStateInfo(0).normalizedTime > upper.normalizedTime,
                        action + " upper action keeps playing during locomotion");
                    legs.Invoke(presentation, new object[] { false }); animator.Update(.05f);
                    Check(animator.GetCurrentAnimatorStateInfo(leg).IsName("leg.sodam_lower_jump"),
                        action + " switches moving legs to jump legs");
                    animator.SetFloat("Speed", 0f); legs.Invoke(presentation, new object[] { true });
                    animator.Update(.2f);
                    if (action == SodamPose.LeafCatch)
                    {
                        Check(animator.GetLayerWeight(leg) == 1 &&
                            animator.GetCurrentAnimatorStateInfo(leg).IsName("leg.Idle"), "stopped catch uses idle legs");
                        var left = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                        var right = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                        var leftPosition = model.transform.InverseTransformPoint(left.position);
                        var rightPosition = model.transform.InverseTransformPoint(right.position);
                        var upperBefore = animator.GetCurrentAnimatorStateInfo(0);
                        float maxFootMovement = 0f;
                        for (int sample = 0; sample < 20; sample++)
                        {
                            legs.Invoke(presentation, new object[] { true }); animator.Update(.02f);
                            maxFootMovement = Mathf.Max(maxFootMovement,
                                Vector3.Distance(leftPosition, model.transform.InverseTransformPoint(left.position)),
                                Vector3.Distance(rightPosition, model.transform.InverseTransformPoint(right.position)));
                        }
                        Check(maxFootMovement < .035f, "stationary catch feet do not stride");
                        Check(animator.GetCurrentAnimatorStateInfo(0).fullPathHash == upperBefore.fullPathHash &&
                            animator.GetCurrentAnimatorStateInfo(0).normalizedTime > upperBefore.normalizedTime,
                            "catch upper body continues while feet remain idle");
                        animator.SetFloat("Speed", 3f); legs.Invoke(presentation, new object[] { true }); animator.Update(.2f);
                        Check(animator.GetCurrentAnimatorStateInfo(leg).IsName("leg.Run"), "catch resumes moving legs when movement resumes");
                    }
                    else Check(animator.GetLayerWeight(leg) == 0, action + " restores authored standing legs on stop");
                }
                apply.Invoke(presentation, new object[] { SodamPose.Sap, 0f }); animator.Update(.1f);
                var readyHand = model.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.RightHand).position);
                Check(readyHand.y > 1.1f && readyHand.z > .2f, "water begins with arms ready");
                animator.Update(4f);
                var heldHand = model.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.RightHand).position);
                Check(Vector3.Distance(readyHand, heldHand) < .03f,
                    "water holds its ready pose instead of repeating preparation");
                apply.Invoke(presentation, new object[] { SodamPose.Idle, 0f }); animator.Update(.2f);
                Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle"), "water blends back to idle");
                return "PASS: delayed release, repeat guard, cancellation, catch before arrival, one catch cue, jump leg/takeoff, moving/airborne/stopped legs for throw/catch/water without restarting upper actions, water ready hold and idle return.";
            }
            finally
            {
                for (int i = objects.Count - 1; i >= 0; i--) if (objects[i] != null) Object.DestroyImmediate(objects[i]);
            }
        }
    }
}
