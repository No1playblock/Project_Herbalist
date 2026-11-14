using System;
using System.Reflection;
using Herbalist.Player;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class DuyeongMovingThrowChecks
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static void Field(object target, string name, object value) => target.GetType().GetField(name, Private).SetValue(target, value);
        private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
        public static string Run()
        {
            var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/PF_Player_2.prefab"));
            try
            {
                root.transform.position = Vector3.one * 18000;
                var character = root.GetComponent<PlayerCharacterPresentation>();
                var visual = (GameObject)new SerializedObject(character).FindProperty("_duyeongRoot").objectReferenceValue;
                visual.SetActive(true);
                var animator = visual.GetComponent<Animator>();
                var presentation = root.GetComponent<SodamAnimationPresentation>();
                Field(character, "_activeRoot", visual); Field(character, "_activeAnimator", animator);
                Field(presentation, "_character", character); Field(presentation, "_animator", animator);
                Field(presentation, "_restVisualPosition", visual.transform.localPosition);
                var apply = typeof(SodamAnimationPresentation).GetMethod("Apply", Private);
                var legs = typeof(SodamAnimationPresentation).GetMethod("UpdateActionLegs", Private);
                animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.Rebind(); animator.Update(0);
                animator.SetFloat("Speed", 3); animator.SetBool("Grounded", true); animator.SetFloat("PlaybackDirection", 1);
                apply.Invoke(presentation, new object[] { SodamPose.LeafThrow, 0f }); animator.Update(.2f);
                int layer = animator.GetLayerIndex(presentation.Profile.movingThrowLegLayer);
                Check(layer >= 0 && layer != animator.GetLayerIndex("leg"), "dedicated moving throw layer");
                var upper = new[] { HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm };
                var before = new Quaternion[upper.Length];
                for (int i = 0; i < upper.Length; i++) before[i] = animator.GetBoneTransform(upper[i]).localRotation;
                var legBefore = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).localRotation;
                legs.Invoke(presentation, new object[] { true });
                animator.Play("throw_legs.Run", layer, .25f); animator.Update(0);
                Check(animator.GetLayerWeight(layer) == 1 && animator.GetLayerWeight(animator.GetLayerIndex("leg")) == 0, "only leg-only mask active while moving");
                for (int i = 0; i < upper.Length; i++)
                    Check(Quaternion.Angle(before[i], animator.GetBoneTransform(upper[i]).localRotation) < .1f, "walking must preserve throwing upper body: " + upper[i]);
                Check(Quaternion.Angle(legBefore, animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).localRotation) > 5, "walking affects actual leg bones");
                var state = animator.GetCurrentAnimatorStateInfo(0);
                legs.Invoke(presentation, new object[] { true }); animator.Update(.05f);
                Check(animator.GetCurrentAnimatorStateInfo(0).fullPathHash == state.fullPathHash && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > state.normalizedTime, "upper throw continues without restarting");
                animator.SetFloat("Speed", 0); legs.Invoke(presentation, new object[] { true });
                Check(animator.GetLayerWeight(layer) == 0, "stopping releases walking override");
                animator.SetFloat("Speed", 3); legs.Invoke(presentation, new object[] { false }); animator.Update(.2f);
                Check(animator.GetLayerWeight(layer) == 0 && animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("leg")).IsName("leg.sodam_lower_jump"), "airborne throw retains existing jump legs");
                CheckLandingPriority(presentation, animator);
                return "PASS: Duyeong moving throw preserves upper body and walking legs; stationary landing only, immediate movement interruption, and higher-priority actions preserved.";
            }
            finally { Object.DestroyImmediate(root); }
        }
        private static void CheckLandingPriority(SodamAnimationPresentation presentation, Animator animator)
        {
            var update = typeof(SodamAnimationPresentation).GetMethod("Update", Private);
            Field(presentation, "_initialized", true);
            Field(presentation, "_oneShot", null);
            Field(presentation, "_wasGrounded", false);
            animator.SetBool("Grounded", true); animator.SetFloat("Speed", 3);
            update.Invoke(presentation, null); animator.Update(.2f);
            Check(presentation.CurrentPose == SodamPose.Move && animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Run"), "moving landing goes directly to locomotion");
            Field(presentation, "_wasGrounded", false);
            animator.SetFloat("Speed", 0);
            update.Invoke(presentation, null); animator.Update(.1f);
            Check(presentation.CurrentPose == SodamPose.Land, "stationary landing retains landing animation");
            animator.SetFloat("Speed", 3);
            update.Invoke(presentation, null); animator.Update(.2f);
            Check(presentation.CurrentPose == SodamPose.Move, "walking interrupts an already started landing");
            animator.SetFloat("Speed", 0); Field(presentation, "_wasGrounded", false);
            typeof(SodamAnimationPresentation).GetMethod("PlayOneShot", Private).Invoke(presentation, new object[] { SodamPose.LeafThrow, 0f });
            update.Invoke(presentation, null);
            Check(presentation.CurrentPose == SodamPose.LeafThrow, "landing cannot replace an active ability action");
        }
    }
}
