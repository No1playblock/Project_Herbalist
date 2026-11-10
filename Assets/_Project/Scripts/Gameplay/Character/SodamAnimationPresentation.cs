using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Player
{
    // Reads replicated gameplay state, but never changes movement or ability simulation.
    [DefaultExecutionOrder(110)]
    [RequireComponent(typeof(PlayerCharacterPresentation))]
    public sealed class SodamAnimationPresentation : MonoBehaviour
    {
        [SerializeField] private SodamAnimationProfile _profile;
        [SerializeField] private SodamAnimationProfile _duyeongProfile;
        private PlayerCharacterPresentation _character;
        private PlayerController _controller;
        private PlayerAbilityController _abilities;
        private StageActor _actor;
        private Animator _animator;
        private Vector3 _restVisualPosition;
        private float _poseElapsed;
        private float _previousVisualOffsetY;
        private SodamPose? _pose;
        private uint _poseSequence;
        private SodamPose? _oneShot;
        private float _oneShotRemaining;
        private int _lastHeld, _lastConsumed;
        private uint _lastResultSequence;
        private uint _lastCatchSequence;
        private uint _lastThrowSequence;
        private StageMinigameKind _lastMinigame;
        private bool _wasGrounded, _wasOnCloth, _wasOnTalisman;
        private bool _wasGrabbingCloth, _wasBoardingTalisman;
        private bool _clothAirborne;
        private float _clothReleaseY;
        private bool _initialized;
        private bool _hasViewSample;
        private bool _cameraMoving;
        private float _lastViewYaw, _lastViewPitch, _cameraStillElapsed;
        private float _idleElapsed;
        public SodamPose? CurrentPose => _character != null && _animator != null &&
            _character.ActiveAnimator == _animator ? _pose : null;
        public uint PoseSequence => _poseSequence;
        public float RootMotionScale => _animator != null ? _animator.transform.lossyScale.x : 1f;
        public SodamAnimationProfile Profile => _character != null && !_character.IsSodamActive && _duyeongProfile != null
            ? _duyeongProfile : _profile;

        private void Awake()
        {
            _character = GetComponent<PlayerCharacterPresentation>();
            _controller = GetComponent<PlayerController>();
            _abilities = GetComponent<PlayerAbilityController>();
            _actor = GetComponent<StageActor>();
        }

        private void Update()
        {
            if (Profile == null || _character == null || _character.ActiveAnimator == null)
            {
                if (_animator != null) _animator.transform.localPosition = _restVisualPosition;
                _initialized = false;
                _hasViewSample = false;
                _cameraMoving = false;
                _animator = null;
                _pose = null;
                return;
            }
            var animator = _character.ActiveAnimator;
            if (animator == null || !animator.isActiveAndEnabled) return;
            if (_animator != animator)
            {
                if (_animator != null) _animator.transform.localPosition = _restVisualPosition;
                _animator = animator;
                _restVisualPosition = animator.transform.localPosition;
                _initialized = false;
                _hasViewSample = false;
                _cameraMoving = false;
                _pose = null;
            }

            // Older test prefabs may still use the shared ability-only profile.
            if (!_character.IsSodamActive && _duyeongProfile == null)
            {
                UpdateDuyeongAbilities();
                return;
            }

            UpdateCameraMotion();

            var flow = StageOneFlow.Instance;
            bool validSlot = _actor != null && _actor.Available && _actor.Slot >= 0 && _actor.Slot < 2;
            int held = flow != null && flow.Progress != null && validSlot ? flow.Progress.Held[_actor.Slot] : 0;
            int consumed = flow != null && flow.Progress != null && validSlot ? flow.Progress.Consumed[_actor.Slot] : 0;
            bool grounded = animator.GetBool(Animator.StringToHash("Grounded"));
            bool onCloth = _actor != null && HangingCloth.IsRiding(_actor);
            bool onTalisman = _actor != null && BranchRideFlow.IsRiding(_actor);
            bool grabbingCloth = HangingCloth.TryGetHandGripPoint(_actor, out _, out float grabRemaining) && grabRemaining > 0f;
            bool boardingTalisman = BranchRideFlow.TryGetBoardingSeat(_actor, out _, out float boardRemaining) && boardRemaining > 0f;
            bool moving = animator.GetFloat(Animator.StringToHash("Speed")) > Profile.movingSpeedThreshold;

            if (!_initialized)
            {
                _lastHeld = held; _lastConsumed = consumed;
                _lastCatchSequence = _abilities != null ? _abilities.Leaf.CatchSequence : 0;
                _lastThrowSequence = _abilities != null ? _abilities.Leaf.ThrowSequence : 0;
                _lastResultSequence = flow != null ? flow.MinigameResultSequence : 0;
                _lastMinigame = flow != null ? flow.Minigame : StageMinigameKind.None;
                _wasGrounded = grounded; _wasOnCloth = onCloth; _wasOnTalisman = onTalisman;
                _wasGrabbingCloth = _wasBoardingTalisman = false;
                _clothAirborne = false; _oneShot = null; _initialized = true;
                _idleElapsed = 0f;
                if (!grabbingCloth && !boardingTalisman)
                {
                    Apply(IdlePresentation(ContinuousPose(flow, held, grounded, moving, onCloth, onTalisman), 0f));
                    return;
                }
            }

            if (!Herbalist.GameUI.GameplayPause.IsPaused)
                _oneShotRemaining -= Time.deltaTime;
            if (_oneShot.HasValue && _oneShotRemaining <= 0)
            {
                if (_oneShot.Value == SodamPose.CraftSuccessRise) PlayOneShot(SodamPose.CraftSuccessShow);
                else if (_oneShot.Value == SodamPose.CraftFailRise) PlayOneShot(SodamPose.CraftFailDust);
                else if (_oneShot.Value == SodamPose.GatherFailRise) PlayOneShot(SodamPose.GatherFailWipe);
                else if (onTalisman && !_character.IsSodamActive && _oneShot.Value == SodamPose.TalismanBoard)
                    PlayOneShot(SodamPose.TalismanKneel);
                else _oneShot = null;
            }

            if ((grabbingCloth && !_wasGrabbingCloth) || (onCloth && !_wasOnCloth))
                _clothAirborne = false;
            if (grabbingCloth && !_wasGrabbingCloth)
                PlayOneShot(SodamPose.ClothGrab, ContactElapsed(SodamPose.ClothGrab, grabRemaining));
            else if (onCloth && !_wasOnCloth && !_wasGrabbingCloth &&
                !HangingCloth.TryGetHandGripPoint(_actor, out _, out _))
            {
                PlayOneShot(SodamPose.ClothGrab);
            }
            else if (!onCloth && _wasOnCloth)
            {
                _clothAirborne = true;
                _clothReleaseY = transform.position.y;
                PlayOneShot(SodamPose.ClothFall);
            }
            else if (!grabbingCloth && !onCloth && _wasGrabbingCloth && _oneShot == SodamPose.ClothGrab)
                _oneShot = null;
            if (boardingTalisman && !_wasBoardingTalisman)
                PlayOneShot(SodamPose.TalismanBoard, ContactElapsed(SodamPose.TalismanBoard, boardRemaining));
            else if (onTalisman && !_wasOnTalisman && !_wasBoardingTalisman &&
                (Profile.Find(SodamPose.TalismanBoard)?.ContactTime ?? 0f) <= 0f)
                PlayOneShot(SodamPose.TalismanBoard);
            else if (!boardingTalisman && !onTalisman && (_wasBoardingTalisman || _wasOnTalisman) &&
                (_oneShot == SodamPose.TalismanBoard || _oneShot == SodamPose.TalismanKneel))
                _oneShot = null;

            // A previous full-body action must not keep grounded legs during takeoff.
            bool traversalAction = grabbingCloth || boardingTalisman || _oneShot == SodamPose.ClothGrab ||
                _oneShot == SodamPose.TalismanBoard || _oneShot == SodamPose.TalismanKneel;
            if (!grounded && _wasGrounded && !onCloth && !onTalisman && !_clothAirborne && !traversalAction &&
                _oneShot != SodamPose.LeafThrow && _oneShot != SodamPose.LeafCatch)
                _oneShot = null;

            if (_clothAirborne && grounded && !_wasGrounded)
            {
                _clothAirborne = false;
                PlayOneShot(transform.position.y >= _clothReleaseY - Profile.successLandingDrop
                    ? SodamPose.ClothSwingLand : SodamPose.ClothHardLand);
            }
            else if (grounded && !_wasGrounded && !moving && !onCloth && !onTalisman && !_character.IsSodamActive &&
                !_oneShot.HasValue)
                PlayOneShot(SodamPose.Land);
            // Ordinary landing is a stationary fallback. Movement can interrupt it immediately,
            // while ability, item and traversal actions retain their own priority.
            if (!_character.IsSodamActive && grounded && moving && _oneShot == SodamPose.Land)
                _oneShot = null;
            if (!traversalAction && flow != null && flow.MinigameResultSequence != _lastResultSequence &&
                _lastMinigame == StageMinigameKind.Crafting && validSlot &&
                flow.RoleFor(_actor) == Herbalist.Presentation.CharacterRole.Sodam)
                PlayOneShot(flow.MinigameSucceeded ? SodamPose.CraftSuccessRise : SodamPose.CraftFailRise);

            bool gatherResult = !traversalAction && flow != null && flow.MinigameResultSequence != _lastResultSequence &&
                _lastMinigame == StageMinigameKind.Gathering && validSlot &&
                flow.RoleFor(_actor) == Herbalist.Presentation.CharacterRole.Duyeong;
            if (gatherResult)
                PlayOneShot(flow.MinigameSucceeded ? SodamPose.GatherSuccess : SodamPose.GatherFailRise);

            if (!traversalAction && flow != null && flow.Progress != null && validSlot && held != _lastHeld)
            {
                var oldItem = flow.settings.Item(_lastHeld);
                var newItem = flow.settings.Item(held);
                if (oldItem != null && oldItem.type == StageItemType.Potion && held == 0 && consumed != _lastConsumed)
                    PlayOneShot(SodamPose.Drink);
                else if (oldItem == null && newItem != null && !gatherResult)
                    PlayOneShot(SodamPose.Receive);
                else if (oldItem != null && newItem == null)
                    PlayOneShot(SodamPose.Pass);
            }

            uint catchSequence = _abilities != null ? _abilities.Leaf.CatchSequence : 0;
            if (!traversalAction && catchSequence != _lastCatchSequence)
                PlayOneShot(onTalisman ? SodamPose.TalismanCatch : SodamPose.LeafCatch, _abilities.Leaf.CatchElapsed);
            _lastCatchSequence = catchSequence;
            uint throwSequence = _abilities != null ? _abilities.Leaf.ThrowSequence : 0;
            if (!traversalAction && throwSequence != _lastThrowSequence)
                PlayOneShot(onTalisman ? SodamPose.TalismanThrow : SodamPose.LeafThrow, _abilities.Leaf.ThrowElapsed);
            _lastThrowSequence = throwSequence;

            if (_abilities != null && _abilities.Sap != null && _abilities.Sap.Spraying &&
                (_oneShot == SodamPose.LeafThrow || _oneShot == SodamPose.LeafCatch)) _oneShot = null;

            if (_oneShot.HasValue) _idleElapsed = 0f;
            else Apply(IdlePresentation(ContinuousPose(flow, held, grounded, moving, onCloth, onTalisman),
                Herbalist.GameUI.GameplayPause.IsPaused ? 0f : Time.deltaTime));
            UpdateActionLegs(grounded);
            UpdateVisualOffset();

            _lastHeld = held; _lastConsumed = consumed;
            _lastResultSequence = flow != null ? flow.MinigameResultSequence : 0;
            _lastMinigame = flow != null ? flow.Minigame : StageMinigameKind.None;
            _wasGrounded = grounded; _wasOnCloth = onCloth; _wasOnTalisman = onTalisman;
            _wasGrabbingCloth = grabbingCloth; _wasBoardingTalisman = boardingTalisman;
        }

        private void UpdateDuyeongAbilities()
        {
            uint catchSequence = _abilities != null ? _abilities.Leaf.CatchSequence : 0;
            uint throwSequence = _abilities != null ? _abilities.Leaf.ThrowSequence : 0;
            if (!_initialized)
            {
                _lastCatchSequence = catchSequence;
                _lastThrowSequence = throwSequence;
                _oneShot = null;
                _initialized = true;
            }
            if (!Herbalist.GameUI.GameplayPause.IsPaused) _oneShotRemaining -= Time.deltaTime;
            if (_oneShot.HasValue && _oneShotRemaining <= 0f) _oneShot = null;
            if (catchSequence != _lastCatchSequence) PlayOneShot(SodamPose.LeafCatch, _abilities.Leaf.CatchElapsed);
            if (throwSequence != _lastThrowSequence) PlayOneShot(SodamPose.LeafThrow, _abilities.Leaf.ThrowElapsed);
            _lastCatchSequence = catchSequence;
            _lastThrowSequence = throwSequence;
            bool spraying = _abilities != null && _abilities.Sap != null && _abilities.Sap.Spraying;
            if (spraying) _oneShot = null;
            bool grounded = _animator.GetBool(Animator.StringToHash("Grounded"));
            bool moving = _animator.GetFloat(Animator.StringToHash("Speed")) > Profile.movingSpeedThreshold;
            SodamPose? next = _oneShot ?? (spraying ? (SodamPose?)SodamPose.Sap : null);
            if (next.HasValue)
            {
                Apply(next.Value);
                UpdateActionLegs(grounded);
                UpdateVisualOffset();
            }
            else if (_pose.HasValue)
            {
                for (int layer = 1; layer < _animator.layerCount; layer++) _animator.SetLayerWeight(layer, 0f);
                var locomotion = Profile.Find(!grounded ? SodamPose.Jump : moving ? SodamPose.Move : SodamPose.Idle);
                if (locomotion != null && locomotion.motions != null)
                    foreach (var motion in locomotion.motions)
                    {
                        if (motion == null || motion.layer != "Base Layer") continue;
                        int hash = Animator.StringToHash(motion.layer + "." + motion.state);
                        if (_animator.HasState(0, hash)) _animator.CrossFadeInFixedTime(hash, locomotion.blendSeconds, 0);
                        break;
                    }
                _animator.transform.localPosition = _restVisualPosition;
                _pose = null;
                _poseSequence++;
            }
        }

        private SodamPose ContinuousPose(StageOneFlow flow, int held, bool grounded, bool moving, bool onCloth, bool onTalisman)
        {
            if (onCloth) return SodamPose.ClothHang;
            if (onTalisman)
                return _abilities != null && _abilities.Sap != null && _abilities.Sap.Controlling
                    && _abilities.Sap.Spraying ? SodamPose.TalismanSap : SodamPose.TalismanSit;
            if (_clothAirborne) return SodamPose.ClothFall;
            if (!grounded) return SodamPose.Jump;
            if (_abilities != null && _abilities.Sap != null && _abilities.Sap.Spraying) return SodamPose.Sap;
            var item = flow != null && flow.settings != null ? flow.settings.Item(held) : null;
            if (item != null && item.type == StageItemType.Herb) return moving ? SodamPose.HerbMove : SodamPose.HerbIdle;
            if (item != null && item.type == StageItemType.Potion) return moving ? SodamPose.PotionMove : SodamPose.PotionIdle;
            return moving ? SodamPose.Move : _character.IsSodamActive && _cameraMoving ? SodamPose.IdleCameraMoving : SodamPose.Idle;
        }

        private void UpdateCameraMotion()
        {
            var view = _controller != null ? _controller.View : null;
            if (view == null) return;
            float yaw = view.Yaw;
            float pitch = view.Pitch;
            if (_hasViewSample && Time.deltaTime > 0f)
            {
                float degrees = Mathf.Max(Mathf.Abs(Mathf.DeltaAngle(_lastViewYaw, yaw)),
                    Mathf.Abs(pitch - _lastViewPitch));
                bool rotating = degrees / Time.deltaTime > Profile.cameraMovementThresholdDegreesPerSecond;
                _cameraStillElapsed = rotating ? 0f : _cameraStillElapsed + Time.deltaTime;
                _cameraMoving = rotating || _cameraMoving &&
                    _cameraStillElapsed < Profile.cameraStillDelaySeconds;
            }
            _lastViewYaw = yaw;
            _lastViewPitch = pitch;
            _hasViewSample = true;
        }

        private SodamPose IdlePresentation(SodamPose pose, float dt)
        {
            if (!_character.IsSodamActive || pose != SodamPose.Idle)
            {
                _idleElapsed = 0f;
                return pose;
            }
            _idleElapsed += dt;
            return _idleElapsed >= Profile.idleLookDelaySeconds ? pose : SodamPose.IdleCameraMoving;
        }

        private void UpdateActionLegs(bool grounded)
        {
            if (_pose != SodamPose.LeafThrow && _pose != SodamPose.LeafCatch && _pose != SodamPose.Sap) return;
            bool moving = _animator.GetFloat(Animator.StringToHash("Speed")) > Profile.movingSpeedThreshold;
            int defaultLayer = _animator.GetLayerIndex("leg");
            int throwLayer = _animator.GetLayerIndex(Profile.movingThrowLegLayer);
            bool movingThrow = _pose == SodamPose.LeafThrow && grounded && moving && throwLayer >= 0;
            int layer = movingThrow ? throwLayer : defaultLayer;
            if (layer < 0) return;
            if (defaultLayer >= 0 && defaultLayer != layer) _animator.SetLayerWeight(defaultLayer, 0f);
            if (throwLayer >= 0 && throwLayer != layer) _animator.SetLayerWeight(throwLayer, 0f);
            // Catch contains authored steps even with no movement input. Keep its upper action,
            // but explicitly cover those steps with idle legs while standing.
            if (grounded && !moving && _pose != SodamPose.LeafCatch) { _animator.SetLayerWeight(layer, 0f); return; }
            var locomotion = Profile.Find(!grounded ? SodamPose.Jump : moving ? SodamPose.Move : SodamPose.Idle);
            if (locomotion == null || locomotion.motions == null) return;
            foreach (var motion in locomotion.motions)
            {
                if (motion == null || motion.layer != (grounded ? "Base Layer" : "leg")) continue;
                int hash = Animator.StringToHash(_animator.GetLayerName(layer) + "." + motion.state);
                if (!_animator.HasState(layer, hash)) return;
                _animator.SetLayerWeight(layer, 1f);
                bool alreadyPlaying = _animator.GetCurrentAnimatorStateInfo(layer).fullPathHash == hash ||
                    _animator.IsInTransition(layer) && _animator.GetNextAnimatorStateInfo(layer).fullPathHash == hash;
                if (!alreadyPlaying)
                    _animator.CrossFadeInFixedTime(hash, locomotion.blendSeconds, layer, motion.startTimeSeconds);
                return;
            }
        }

        private void PlayOneShot(SodamPose pose, float elapsed = 0f)
        {
            var definition = Profile.Find(pose);
            if (definition == null || definition.durationSeconds <= elapsed) return;
            _oneShot = pose;
            _oneShotRemaining = definition.durationSeconds - elapsed;
            _pose = null;
            Apply(pose, elapsed);
        }

        private void Apply(SodamPose pose, float elapsed = 0f)
        {
            if (_animator == null) return;
            var definition = Profile.Find(pose);
            if (definition == null || definition.motions == null) return;
            if (_pose.HasValue && _pose.Value == pose) return;
            var previous = _pose.HasValue ? Profile.Find(_pose.Value) : null;
            bool keepContinuousLayers = definition.durationSeconds == 0f &&
                previous?.durationSeconds == 0f && previous.motions != null;
            _pose = pose;
            _poseSequence++;
            _previousVisualOffsetY = _animator.transform.localPosition.y - _restVisualPosition.y;
            _poseElapsed = elapsed;
            float blendSeconds = definition.blendSeconds;
            var settings = _abilities != null && _abilities.Leaf != null ? _abilities.Leaf.Settings : null;
            if (settings != null)
            {
                bool throwing = pose == SodamPose.LeafThrow || pose == SodamPose.TalismanThrow;
                bool catching = pose == SodamPose.LeafCatch || pose == SodamPose.TalismanCatch;
                if (throwing || catching)
                    blendSeconds = Mathf.Min(blendSeconds, Mathf.Max(0f,
                        (throwing ? settings.throwPreparationSeconds : settings.catchAnticipationSeconds) - elapsed));
            }
            for (int layer = 1; layer < _animator.layerCount; layer++)
            {
                string layerName = _animator.GetLayerName(layer);
                bool used = false;
                foreach (var motion in definition.motions)
                    if (motion != null && motion.layer == layerName) { used = true; break; }
                _animator.SetLayerWeight(layer, used ? 1f : 0f);
            }
            foreach (var motion in definition.motions)
            {
                if (motion == null) continue;
                int layer = _animator.GetLayerIndex(motion.layer);
                if (layer < 0) continue;
                int hash = Animator.StringToHash(motion.layer + "." + motion.state);
                // Locomotion can change while the same carry pose remains on an upper layer.
                if (keepContinuousLayers && layer > 0 &&
                    System.Array.Exists(previous.motions, prior => prior != null && prior.layer == motion.layer &&
                        prior.state == motion.state && prior.startTimeSeconds == motion.startTimeSeconds))
                {
                    var active = _animator.IsInTransition(layer) ? _animator.GetNextAnimatorStateInfo(layer)
                        : _animator.GetCurrentAnimatorStateInfo(layer);
                    if (active.fullPathHash == hash) continue;
                }
                if (_animator.HasState(layer, hash))
                    _animator.CrossFadeInFixedTime(hash, blendSeconds, layer,
                        motion.startTimeSeconds + elapsed);
            }
        }

        private void UpdateVisualOffset()
        {
            if (_animator == null || !_pose.HasValue) return;
            var definition = Profile.Find(_pose.Value);
            if (definition == null) return;
            if (!Herbalist.GameUI.GameplayPause.IsPaused) _poseElapsed += Time.deltaTime;
            float targetOffset = definition.modelOffsetY;
            if (definition.visualOffsetY != null && definition.visualOffsetY.length > 0)
                targetOffset += definition.visualOffsetY.Evaluate(_poseElapsed * definition.rootMotionPlaybackSpeed);
            float blend = definition.blendSeconds > 0f ? Mathf.Clamp01(_poseElapsed / definition.blendSeconds) : 1f;
            _animator.transform.localPosition = _restVisualPosition + Vector3.up * Mathf.Lerp(_previousVisualOffsetY, targetOffset, blend);
        }

        private void LateUpdate()
        {
            if (_animator == null || !_animator.isActiveAndEnabled) return;
            if ((_pose == SodamPose.TalismanBoard || _pose == SodamPose.TalismanKneel) &&
                BranchRideFlow.TryGetBoardingSeat(_actor, out var seat, out float boardRemaining))
            {
                _animator.transform.position += (seat - transform.position) * ApproachWeight(SodamPose.TalismanBoard, boardRemaining);
                return;
            }
            if (!_animator.isHuman || (_pose != SodamPose.ClothGrab && _pose != SodamPose.ClothHang) ||
                !HangingCloth.TryGetHandGripPoint(_actor, out var knot, out float grabRemaining)) return;

            var left = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            var right = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (left == null || right == null) return;
            var leftFinger = _animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
            var rightFinger = _animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            Vector3 leftPalm = leftFinger != null ? (left.position + leftFinger.position) * .5f : left.position;
            Vector3 rightPalm = rightFinger != null ? (right.position + rightFinger.position) * .5f : right.position;
            // Approach the moving knot during the jump, then keep the palms fixed at contact.
            _animator.transform.position += (knot - (leftPalm + rightPalm) * .5f) *
                ApproachWeight(SodamPose.ClothGrab, grabRemaining);
        }

        private float ContactElapsed(SodamPose pose, float remaining)
            => Mathf.Max(0f, (Profile.Find(pose)?.ContactTime ?? 0f) - remaining);

        private float ApproachWeight(SodamPose pose, float remaining)
        {
            if (remaining <= 0f) return 1f;
            var definition = Profile.Find(pose);
            if (definition == null) return 0f;
            float elapsed = Mathf.Min(_poseElapsed, Mathf.Max(0f, definition.ContactTime - remaining) + Time.deltaTime);
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(definition.approachStartSeconds, definition.ContactTime, elapsed));
        }

        private void OnDisable()
        {
            if (_animator != null) _animator.transform.localPosition = _restVisualPosition;
            _initialized = false;
            _hasViewSample = false;
            _cameraMoving = false;
            _pose = null;
        }
    }
}
