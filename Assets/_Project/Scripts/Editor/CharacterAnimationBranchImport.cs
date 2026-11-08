using System;
using System.Collections.Generic;
using System.Linq;
using Herbalist.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    // Reapply the character artist's visual changes without replacing player gameplay roots.
    public static class CharacterAnimationBranchImport
    {
        private const string Source = "Assets/_Project/Prefabs/Characters/PF_Player_2.prefab";
        private const string ProfilePath = "Assets/_Project/Data/Characters/SO_DuyeongAnimationProfile.asset";

        public static void ConfigureMovingLeafThrow()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/_Project/Animations/Characters/Duyeong/AC_DuyeongLocomotion.controller");
            const string maskPath = "Assets/_Project/Animations/Characters/Duyeong/AM_DuyeongMovingThrowLegs.mask";
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
            if (mask == null)
            {
                mask = new AvatarMask { name = "AM_DuyeongMovingThrowLegs" };
                AssetDatabase.CreateAsset(mask, maskPath);
            }
            for (int part = 0; part < (int)AvatarMaskBodyPart.LastBodyPart; part++)
            {
                var bodyPart = (AvatarMaskBodyPart)part;
                mask.SetHumanoidBodyPartActive(bodyPart, bodyPart == AvatarMaskBodyPart.LeftLeg ||
                    bodyPart == AvatarMaskBodyPart.RightLeg || bodyPart == AvatarMaskBodyPart.LeftFootIK ||
                    bodyPart == AvatarMaskBodyPart.RightFootIK);
            }
            const string layerName = "throw_legs";
            if (!controller.layers.Any(l => l.name == layerName)) controller.AddLayer(layerName);
            var layers = controller.layers;
            var layer = layers.First(l => l.name == layerName);
            layer.avatarMask = mask; layer.defaultWeight = 0;
            var run = layer.stateMachine.states.FirstOrDefault(s => s.state.name == "Run").state;
            if (run == null) run = layer.stateMachine.AddState("Run");
            var original = layers.First(l => l.name == "leg").stateMachine.states.First(s => s.state.name == "Run").state;
            run.motion = original.motion; run.speed = original.speed;
            layer.stateMachine.defaultState = run;
            controller.layers = layers;
            var profile = AssetDatabase.LoadAssetAtPath<SodamAnimationProfile>(ProfilePath);
            profile.movingThrowLegLayer = layerName;
            EditorUtility.SetDirty(mask); EditorUtility.SetDirty(controller); EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        public static string ConfigureAnimations()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/_Project/Animations/Characters/Duyeong/AC_DuyeongLocomotion.controller");
            AnimatorState State(string layer, string state) => controller.layers.First(l => l.name == layer)
                .stateMachine.states.First(s => s.state.name == state).state;
            SodamLayerMotion Motion(string state, string layer = "Base Layer") =>
                new SodamLayerMotion { layer = layer, state = state };
            var poses = new List<SodamPoseDefinition>();
            void Pose(SodamPose pose, bool oneShot, params SodamLayerMotion[] motions)
            {
                var state = State(motions[0].layer, motions[0].state);
                var clip = state.motion as AnimationClip;
                if (clip == null) throw new Exception("Missing clip: " + state.name);
                poses.Add(new SodamPoseDefinition {
                    pose = pose, durationSeconds = oneShot ? clip.length / Mathf.Max(.01f, state.speed) : 0f,
                    motions = motions, blendSeconds = .12f
                });
            }
            State("Base Layer", "hand_pass").speed = .5f;
            State("Base Layer", "falling").speed = 1.2f;
            State("Base Layer", "falling").iKOnFeet = false;
            var legRun = State("leg", "Run");
            legRun.motion = State("Base Layer", "Run").motion; legRun.speed = 1.2f;
            var legJump = State("leg", "sodam_lower_jump");
            legJump.speed = .8f;
            // The arm layer leaves walking, jumping and stationary legs under locomotion control.
            Pose(SodamPose.Idle, false, Motion("Idle"));
            Pose(SodamPose.Move, false, Motion("Run"));
            Pose(SodamPose.Jump, false, Motion("Jump"));
            Pose(SodamPose.Land, true, Motion("Base_Jump_End_2_InPlace"));
            Pose(SodamPose.HerbIdle, false, Motion("Idle"), Motion("grip_hand", "right_arm"));
            Pose(SodamPose.HerbMove, false, Motion("Run"), Motion("grip_hand", "right_arm"));
            Pose(SodamPose.PotionIdle, false, Motion("Idle"), Motion("carry_pickup(hand)", "both_arm"));
            Pose(SodamPose.PotionMove, false, Motion("Run"), Motion("carry_pickup(hand)", "both_arm"));
            Pose(SodamPose.Drink, true, Motion("dy_drink"));
            Pose(SodamPose.Pass, true, Motion("hand_pass"));
            Pose(SodamPose.Receive, true, Motion("pickup_clear"));
            Pose(SodamPose.GatherSuccess, true, Motion("pickup_clear"));
            Pose(SodamPose.GatherFailRise, true, Motion("pickup_clear"));
            Pose(SodamPose.GatherFailWipe, true, Motion("wiping_sweet"));
            Pose(SodamPose.Sap, false, Motion("Idle"), Motion("water_cannon", "right_arm"));
            Pose(SodamPose.LeafThrow, true, Motion("dy_throw"));
            Pose(SodamPose.LeafCatch, true, Motion("dy_catch"));
            Pose(SodamPose.ClothGrab, true, Motion("Jump and Grab"));
            Pose(SodamPose.ClothHang, false, Motion("dy_hanging"));
            Pose(SodamPose.ClothFall, true, Motion("falling"));
            Pose(SodamPose.ClothHardLand, true, Motion("dy_landing"));
            Pose(SodamPose.ClothSwingLand, true, Motion("swing to landing"));
            Pose(SodamPose.TalismanBoard, true, Motion("dy_blance_walk"));
            Pose(SodamPose.TalismanKneel, true, Motion("kneel"));
            Pose(SodamPose.TalismanSit, false, Motion("dy_sit"));
            Pose(SodamPose.TalismanSap, false, Motion("dy_sit"), Motion("dy_sit_water_arm", "right_arm"));
            Pose(SodamPose.TalismanThrow, true, Motion("dy_sit_throw"));
            Pose(SodamPose.TalismanCatch, true, Motion("dy_sit_catch"));
            var swingLanding = poses.First(p => p.pose == SodamPose.ClothSwingLand);
            swingLanding.motions[0].startTimeSeconds = 23f / 30f;
            swingLanding.durationSeconds -= swingLanding.motions[0].startTimeSeconds;
            var profile = AssetDatabase.LoadAssetAtPath<SodamAnimationProfile>(ProfilePath);
            if (profile == null) { profile = ScriptableObject.CreateInstance<SodamAnimationProfile>(); AssetDatabase.CreateAsset(profile, ProfilePath); }
            profile.poses = poses.ToArray();
            var sodam = AssetDatabase.LoadAssetAtPath<SodamAnimationProfile>(
                "Assets/_Project/Data/Characters/SO_SodamAnimationProfile.asset");
            foreach (var pose in sodam.poses)
                foreach (var motion in pose.motions)
                    if (motion.layer == "right_arm" && motion.state == "girb_hand") motion.state = "girb_hand(sodam)";
            var sodamController = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/_Project/Animations/Characters/Sodam/AC_SodamLocomotion.controller");
            var lowerJump = sodamController.layers.First(l => l.name == "leg").stateMachine.states
                .First(s => s.state.name == "sodam_lower_jump").state;
            legJump.motion = lowerJump.motion;
            const string lowerMaskPath = "Assets/_Project/Animations/Characters/Duyeong/AM_DuyeongLowerBody.mask";
            var lowerMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(lowerMaskPath);
            if (lowerMask == null)
            {
                lowerMask = Object.Instantiate(controller.layers.First(l => l.name == "leg").avatarMask);
                lowerMask.name = "AM_DuyeongLowerBody";
                AssetDatabase.CreateAsset(lowerMask, lowerMaskPath);
            }
            // The new catch clip moves the hips; include them in the locomotion override to keep planted feet.
            lowerMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
            EditorUtility.SetDirty(lowerMask);
            var layers = controller.layers;
            layers.First(l => l.name == "leg").avatarMask = lowerMask;
            controller.layers = layers;
            var jump = profile.Find(SodamPose.Jump);
            jump.motions = new[] { Motion("Jump"), new SodamLayerMotion {
                layer = "leg", state = "sodam_lower_jump", startTimeSeconds = .7125f
            } };
            AlignLeafMotion(profile, controller, SodamPose.LeafThrow, .75f);
            AlignLeafMotion(profile, controller, SodamPose.LeafCatch, .75f);
            AlignLeafMotion(profile, controller, SodamPose.TalismanThrow, 1.1f);
            AlignLeafMotion(profile, controller, SodamPose.TalismanCatch, .5f);
            AlignLeafMotion(sodam, sodamController, SodamPose.LeafThrow, .5f);
            AlignLeafMotion(sodam, sodamController, SodamPose.LeafCatch, .5f);
            AlignLeafMotion(sodam, sodamController, SodamPose.TalismanThrow, .45f);
            AlignLeafMotion(sodam, sodamController, SodamPose.TalismanCatch, .5f);
            var carry = sodamController.layers[0].stateMachine.states.First(s => s.state.name == "carry_pickup").state;
            var success = sodam.Find(SodamPose.CraftSuccessRise);
            success.motions = new[] { Motion("carry_pickup") };
            success.durationSeconds = ((AnimationClip)carry.motion).length / Mathf.Max(.01f, carry.speed);
            success.useRootMotion = false;
            var show = sodam.Find(SodamPose.CraftSuccessShow);
            show.motions = sodam.Find(SodamPose.PotionIdle).motions;
            EditorUtility.SetDirty(sodam);
            EditorUtility.SetDirty(profile); EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets();
            ConfigureMovingLeafThrow();
            return "Configured " + poses.Count + " Duyeong poses";
        }

        private static void AlignLeafMotion(SodamAnimationProfile profile, AnimatorController controller,
            SodamPose pose, float contactClipSeconds)
        {
            var definition = profile.Find(pose);
            // On a seated pose the last motion is the arm overlay; the base stays seated.
            var motion = definition.motions.Last();
            var state = controller.layers.First(l => l.name == motion.layer).stateMachine.states
                .First(s => s.state.name == motion.state).state;
            var settings = AssetDatabase.LoadAssetAtPath<Herbalist.Abilities.LeafAbilitySettings>(
                "Assets/_Project/Data/Abilities/Leaf/SO_LeafAbilitySettings.asset");
            bool throwing = pose == SodamPose.LeafThrow || pose == SodamPose.TalismanThrow;
            float lead = throwing ? settings.throwPreparationSeconds : settings.catchAnticipationSeconds;
            float endTime = definition.durationSeconds + motion.startTimeSeconds;
            motion.startTimeSeconds = Mathf.Max(0f, contactClipSeconds / state.speed - lead);
            definition.durationSeconds = Mathf.Min(endTime, ((AnimationClip)state.motion).length / state.speed)
                - motion.startTimeSeconds;
            definition.blendSeconds = Mathf.Min(definition.blendSeconds, lead * .3f);
        }

        public static void CopyVisual(GameObject source, GameObject destination)
        {
            var map = new Dictionary<Object, Object>();
            var pairs = new List<KeyValuePair<Component, Component>>();
            foreach (var original in source.GetComponentsInChildren<Transform>(true))
            {
                string path = AnimationUtility.CalculateTransformPath(original, source.transform);
                var target = path.Length == 0 ? destination.transform : destination.transform.Find(path);
                if (target == null) throw new Exception("Missing visual bone: " + path);
                map[original.gameObject] = target.gameObject; map[original] = target;
                if (path.Length != 0)
                {
                    target.localPosition = original.localPosition; target.localRotation = original.localRotation;
                    target.localScale = original.localScale;
                }
                foreach (var group in original.GetComponents<Component>().Where(c => c != null && !(c is Transform)).GroupBy(c => c.GetType()))
                {
                    var from = group.ToArray(); var to = target.GetComponents(group.Key).ToList();
                    while (to.Count < from.Length) to.Add(target.gameObject.AddComponent(group.Key));
                    for (int i = 0; i < from.Length; i++) { map[from[i]] = to[i]; pairs.Add(new KeyValuePair<Component, Component>(from[i], to[i])); }
                    for (int i = to.Count - 1; i >= from.Length; i--) Object.DestroyImmediate(to[i]);
                }
            }
            foreach (var pair in pairs)
            {
                EditorUtility.CopySerialized(pair.Key, pair.Value);
                var serialized = new SerializedObject(pair.Value); var property = serialized.GetIterator();
                while (property.Next(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null &&
                        map.TryGetValue(property.objectReferenceValue, out var replacement)) property.objectReferenceValue = replacement;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                if (pair.Value is Animator animator) animator.applyRootMotion = false;
                EditorUtility.SetDirty(pair.Value);
            }
        }

        public static void Apply(PlayerCharacterPresentation target)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Source).GetComponent<PlayerCharacterPresentation>();
            var from = new SerializedObject(source); var to = new SerializedObject(target);
            foreach (string field in new[] { "characterRoot", "_duyeongRoot" })
            {
                var original = (GameObject)from.FindProperty(field).objectReferenceValue;
                var destination = (GameObject)to.FindProperty(field).objectReferenceValue;
                if (destination == null)
                {
                    var otherRoot = (GameObject)to.FindProperty("characterRoot").objectReferenceValue;
                    destination = Object.Instantiate(original, otherRoot.transform.parent, false);
                    destination.name = original.name;
                    destination.SetActive(false);
                    to.FindProperty(field).objectReferenceValue = destination;
                    to.FindProperty("_duyeongAnimator").objectReferenceValue = destination.GetComponent<Animator>();
                    to.ApplyModifiedPropertiesWithoutUndo();
                }
                else CopyVisual(original, destination);
            }
            var presentation = target.GetComponent<SodamAnimationPresentation>();
            if (presentation == null) presentation = target.gameObject.AddComponent<SodamAnimationPresentation>();
            var profile = new SerializedObject(presentation);
            if (profile.FindProperty("_profile").objectReferenceValue == null)
                profile.FindProperty("_profile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SodamAnimationProfile>(
                    "Assets/_Project/Data/Characters/SO_SodamAnimationProfile.asset");
            profile.FindProperty("_duyeongProfile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SodamAnimationProfile>(ProfilePath);
            profile.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(presentation);
        }

        public static string ApplyPrefabs()
        {
            int count = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs/Characters" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path == Source) continue;
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset.GetComponent<PlayerCharacterPresentation>() == null) continue;
                var root = PrefabUtility.LoadPrefabContents(path);
                try { Apply(root.GetComponent<PlayerCharacterPresentation>()); PrefabUtility.SaveAsPrefabAsset(root, path); count++; }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            return "Updated " + count + " player prefabs";
        }
    }
}
