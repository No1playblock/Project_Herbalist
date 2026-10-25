using System;
using UnityEngine;

namespace Herbalist.Player
{
    public enum SodamPose
    {
        Idle, Move, Jump, Land, HerbIdle, HerbMove, PotionIdle, PotionMove,
        CraftSuccessRise, CraftSuccessShow, CraftFailRise, CraftFailDust,
        Drink, Pass, Receive, Sap, LeafThrow, LeafCatch,
        ClothGrab, ClothHang, ClothFall, ClothHardLand, ClothSwingLand,
        TalismanBoard, TalismanSit, TalismanSap, TalismanThrow, TalismanCatch,
        IdleCameraMoving
    }

    [Serializable]
    public sealed class SodamLayerMotion
    {
        public string layer;
        public string state;
    }

    [Serializable]
    public sealed class SodamPoseDefinition
    {
        public SodamPose pose;
        [Min(0)] public float blendSeconds = 0.12f;
        [Min(0)] public float durationSeconds;
        public float modelOffsetY;
        public AnimationCurve visualOffsetY;
        public bool useRootMotion;
        public bool overrideMovementInput;
        public bool ignoreRootMotionY;
        public AnimationClip rootMotionClip;
        [Min(0)] public float rootMotionClipLength;
        [Min(0)] public float rootMotionPlaybackSpeed = 1f;
        public AnimationCurve rootMotionX;
        public AnimationCurve rootMotionY;
        public AnimationCurve rootMotionZ;
        public SodamLayerMotion[] motions;
    }

    [CreateAssetMenu(menuName = "Herbalist/Characters/Sodam Animation Profile")]
    public sealed class SodamAnimationProfile : ScriptableObject
    {
        [Min(0)] public float movingSpeedThreshold = 0.08f;
        [Min(0)] public float successLandingDrop = 1f;
        [Min(0)] public float cameraMovementThresholdDegreesPerSecond = 2f;
        [Min(0)] public float cameraStillDelaySeconds = 0.15f;
        public SodamPoseDefinition[] poses;

        public SodamPoseDefinition Find(SodamPose pose)
        {
            if (poses == null) return null;
            foreach (var definition in poses)
                if (definition != null && definition.pose == pose) return definition;
            return null;
        }

        public Vector3 RootMotionDelta(SodamPose pose, float elapsedSeconds, float deltaTime)
        {
            var definition = Find(pose);
            if (definition == null || !definition.useRootMotion || definition.rootMotionClipLength <= 0f)
                return Vector3.zero;
            float speed = Mathf.Max(0f, definition.rootMotionPlaybackSpeed);
            float from = Mathf.Clamp(elapsedSeconds * speed, 0f, definition.rootMotionClipLength);
            float to = Mathf.Clamp((elapsedSeconds + deltaTime) * speed, 0f, definition.rootMotionClipLength);
            if (to <= from) return Vector3.zero;
            return new Vector3(
                Difference(definition.rootMotionX, from, to),
                definition.ignoreRootMotionY ? 0f : Difference(definition.rootMotionY, from, to),
                Difference(definition.rootMotionZ, from, to));
        }

        private static float Difference(AnimationCurve curve, float from, float to) =>
            curve == null || curve.length == 0 ? 0f : curve.Evaluate(to) - curve.Evaluate(from);
    }
}
