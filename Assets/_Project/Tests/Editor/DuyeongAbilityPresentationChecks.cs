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
    public static class DuyeongAbilityPresentationChecks
    {
        private static void Check(bool condition, string label)
        { if (!condition) throw new Exception("Duyeong: " + label); }
        private static void Field(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        public static string Run()
        {
            var objects = new List<Object>();
            try
            {
                var owner = new GameObject("DuyeongAbilityCheck"); owner.SetActive(false); objects.Add(owner);
                var abilities = owner.AddComponent<PlayerAbilityController>();
                var leaf = owner.GetComponent<LeafThrowAbility>();
                Field(leaf, "settings", AssetDatabase.LoadAssetAtPath<LeafAbilitySettings>("Assets/_Project/Data/Abilities/Leaf/SO_LeafAbilitySettings.asset"));
                var sap = owner.AddComponent<SapControlAbility>();
                Field(abilities, "<Leaf>k__BackingField", leaf); Field(abilities, "<Sap>k__BackingField", sap);
                var character = owner.AddComponent<PlayerCharacterPresentation>();
                var presentation = owner.AddComponent<SodamAnimationPresentation>();
                var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Characters/Duyeong/prefab/dy_v5.prefab"));
                objects.Add(model); model.transform.position = Vector3.one * 12000;
                var animator = model.GetComponentInChildren<Animator>();
                Check(animator != null && animator.isHuman, "humanoid avatar available");
                animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/Characters/Duyeong/AC_DuyeongLocomotion.controller");
                animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.Rebind(); animator.Update(0f);
                Field(character, "_activeAnimator", animator); Field(character, "_activeRoot", model);
                Field(presentation, "_character", character); Field(presentation, "_animator", animator);
                Field(presentation, "_abilities", abilities); Field(presentation, "_restVisualPosition", model.transform.localPosition);
                Field(presentation, "_profile", AssetDatabase.LoadAssetAtPath<SodamAnimationProfile>("Assets/_Project/Data/Characters/SO_SodamAnimationProfile.asset"));
                var update = typeof(SodamAnimationPresentation).GetMethod("UpdateDuyeongAbilities", BindingFlags.Instance | BindingFlags.NonPublic);
                Action tick = () => update.Invoke(presentation, null);
                animator.SetBool("Grounded", true); animator.SetFloat("Speed", 0f); animator.SetFloat("PlaybackDirection", 1f);
                tick();
                Field(leaf, "<ThrowSequence>k__BackingField", 1u); tick(); animator.Update(.2f);
                Check(presentation.CurrentPose == SodamPose.LeafThrow && animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.sodam_throw"), "throw sequence drives Duyeong animation");
                Field(leaf, "<CatchSequence>k__BackingField", 1u); tick(); animator.Update(.2f);
                int leg = animator.GetLayerIndex("leg");
                Check(presentation.CurrentPose == SodamPose.LeafCatch && animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.sodam_catch"), "catch sequence drives Duyeong animation");
                Check(animator.GetLayerWeight(leg) == 1f && animator.GetCurrentAnimatorStateInfo(leg).IsName("leg.Idle"), "standing catch keeps idle legs");
                var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                var footPosition = model.transform.InverseTransformPoint(foot.position);
                var upper = animator.GetCurrentAnimatorStateInfo(0);
                for (int i = 0; i < 15; i++) { tick(); animator.Update(.02f); }
                Check(Vector3.Distance(footPosition, model.transform.InverseTransformPoint(foot.position)) < .035f, "standing foot does not stride");
                Check(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > upper.normalizedTime, "catch continues without restart");
                animator.SetFloat("Speed", 3f); tick(); animator.Update(.2f);
                Check(animator.GetCurrentAnimatorStateInfo(leg).IsName("leg.Run"), "moving catch runs");
                animator.SetBool("Grounded", false); tick(); animator.Update(.1f);
                Check(animator.GetCurrentAnimatorStateInfo(leg).IsName("leg.sodam_lower_jump"), "airborne catch uses jump legs");
                sap.ApplyReplica(true, true, true, true); tick(); animator.Update(.2f);
                Check(presentation.CurrentPose == SodamPose.Sap && animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.water_cannon"), "spray replaces catch with water pose");
                sap.ApplyReplica(false, false); animator.SetBool("Grounded", true); animator.SetFloat("Speed", 0f); tick(); animator.Update(.3f);
                Check(presentation.CurrentPose == null && animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle") && animator.GetLayerWeight(leg) == 0f, "spray stop restores Duyeong locomotion and releases overlay");
                animator.SetFloat("Speed", 3f); animator.Update(.2f); animator.Update(.2f);
                Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Run"), "original automatic locomotion still runs");
                return "PASS: Duyeong Humanoid throw/catch/spray, stationary feet, moving and airborne legs, continuous upper action, spray stop and original locomotion.";
            }
            finally { for (int i = objects.Count - 1; i >= 0; i--) if (objects[i] != null) Object.DestroyImmediate(objects[i]); }
        }
    }
}
