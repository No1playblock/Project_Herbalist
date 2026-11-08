using System;
using System.Reflection;
using System.Linq;
using Herbalist.Player;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class ImportedCharacterAnimationChecks
    {
        private static void Field(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        public static string Run()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/PF_Player_2.prefab");
            var root = Object.Instantiate(asset);
            try
            {
                root.transform.position = Vector3.one * 16000f;
                var character = root.GetComponent<PlayerCharacterPresentation>();
                var presentation = root.GetComponent<SodamAnimationPresentation>();
                Field(presentation, "_character", character);
                var abilities = root.GetComponent<Herbalist.Abilities.PlayerAbilityController>();
                Field(abilities, "<Leaf>k__BackingField", root.GetComponent<Herbalist.Abilities.LeafThrowAbility>());
                Field(presentation, "_abilities", abilities);
                var serialized = new SerializedObject(character);
                int count = 0;
                foreach (string field in new[] { "characterRoot", "_duyeongRoot" })
                {
                    var visual = (GameObject)serialized.FindProperty(field).objectReferenceValue;
                    visual.SetActive(true);
                    var animator = visual.GetComponent<Animator>();
                    if (animator == null || !animator.isHuman || !animator.avatar.isValid)
                        throw new Exception(field + ": invalid humanoid avatar");
                    Field(character, "_activeAnimator", animator); Field(character, "_activeRoot", visual);
                    Field(presentation, "_animator", animator);
                    Field(presentation, "_restVisualPosition", visual.transform.localPosition);
                    animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animator.Rebind(); animator.Update(0f);
                    animator.SetBool("Grounded", true); animator.SetFloat("Speed", 0f);
                    animator.SetFloat("PlaybackDirection", 1f);
                    var profile = presentation.Profile;
                    bool duyeong = field == "_duyeongRoot";
                    if (profile == null || profile.name != (duyeong ? "SO_DuyeongAnimationProfile" : "SO_SodamAnimationProfile"))
                        throw new Exception("Role-specific profile was not selected");
                    foreach (var pose in profile.poses)
                    {
                        Field(presentation, "_pose", null);
                        typeof(SodamAnimationPresentation).GetMethod("Apply", BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(presentation, new object[] { pose.pose, 0f });
                        animator.Update(.2f);
                        foreach (var motion in pose.motions)
                        {
                            int layer = animator.GetLayerIndex(motion.layer);
                            if (layer < 0 || !animator.HasState(layer, Animator.StringToHash(motion.layer + "." + motion.state)))
                                throw new Exception(profile.name + ": missing state " + motion.state);
                            if (!animator.GetCurrentAnimatorStateInfo(layer).IsName(motion.layer + "." + motion.state))
                                throw new Exception(profile.name + ": did not play " + motion.state);
                            if (layer > 0 && animator.GetLayerWeight(layer) != 1f)
                                throw new Exception("Missing pose layer: " + motion.layer);
                        }
                        if (animator.applyRootMotion) throw new Exception("Animator moved the gameplay root");
                        count++;
                    }
                    var controller = (UnityEditor.Animations.AnimatorController)animator.runtimeAnimatorController;
                    foreach (var pose in new[] { SodamPose.LeafThrow, SodamPose.LeafCatch,
                        SodamPose.TalismanThrow, SodamPose.TalismanCatch })
                    {
                        bool throwing = pose == SodamPose.LeafThrow || pose == SodamPose.TalismanThrow;
                        float lead = throwing ? .25f : .3f;
                        float expected = duyeong
                            ? pose == SodamPose.TalismanThrow ? 1.1f : pose == SodamPose.TalismanCatch ? .5f : .75f
                            : pose == SodamPose.TalismanThrow ? .45f : .5f;
                        foreach (float late in new[] { 0f, lead * .8f })
                        {
                            Field(presentation, "_pose", null);
                            typeof(SodamAnimationPresentation).GetMethod("Apply", BindingFlags.Instance | BindingFlags.NonPublic)
                                .Invoke(presentation, new object[] { pose, late });
                            animator.Update(0f);
                            animator.Update(lead - late);
                            var motion = profile.Find(pose).motions.Last();
                            int layer = animator.GetLayerIndex(motion.layer);
                            var clip = (AnimationClip)controller.layers[layer].stateMachine.states
                                .First(s => s.state.name == motion.state).state.motion;
                            var contactState = animator.IsInTransition(layer)
                                ? animator.GetNextAnimatorStateInfo(layer) : animator.GetCurrentAnimatorStateInfo(layer);
                            float clipTime = contactState.normalizedTime * clip.length;
                            if (Mathf.Abs(clipTime - expected) > .025f)
                                throw new Exception(profile.name + ": " + pose + " late=" + late + " contact timing " + clipTime + " expected " + expected);
                        }
                    }
                    var jump = profile.Find(SodamPose.Jump);
                    if (!jump.motions.Any(m => m.layer == "leg" && m.state == "sodam_lower_jump"))
                        throw new Exception(profile.name + ": walking jump has no lower-body jump override");
                    var lower = controller.layers.First(l => l.name == "leg").stateMachine.states
                        .First(s => s.state.name == "sodam_lower_jump").state.motion;
                    if (AssetDatabase.GetAssetPath(lower) != "Assets/_Project/Art/Character_anime/sodam_animeclip/Jump.fbx")
                        throw new Exception("Jump legs still use the wrong clip");
                    visual.SetActive(false);
                }
                return "PASS: " + count + " role poses; standing/seated throw and catch contact timing, late notification compensation, and walking jump lower-body clips.";
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
