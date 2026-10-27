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
        private bool _clothAirborne;
        private float _clothReleaseY;
        private bool _initialized;
        private bool _hasViewSample;
        private bool _cameraMoving;
        private float _lastViewYaw, _lastViewPitch, _cameraStillElapsed;
        public SodamPose? CurrentPose => _character != null && _animator != null &&
            _character.ActiveAnimator == _animator ? _pose : null;
        public uint PoseSequence => _poseSequence;
        public float RootMotionScale => _animator != null ? _animator.transform.lossyScale.x : 1f;
        public SodamAnimationProfile Profile => _profile;

        private void Awake()
        {
            _character = GetComponent<PlayerCharacterPresentation>();
            _controller = GetComponent<PlayerController>();
            _abilities = GetComponent<PlayerAbilityController>();
            _actor = GetComponent<StageActor>();
        }

        private void Update()
        {
            if (_profile == null || _character == null || _character.ActiveAnimator == null)
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

            // Keep Duyeong's existing locomotion controller; share only the ability poses.
            if (!_character.IsSodamActive)
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
            bool moving = animator.GetFloat(Animator.StringToHash("Speed")) > _profile.movingSpeedThreshold;

            if (!_initialized)
            {
                _lastHeld = held; _lastConsumed = consumed;
                _lastCatchSequence = _abilities != null ? _abilities.Leaf.CatchSequence : 0;
                _lastThrowSequence = _abilities != null ? _abilities.Leaf.ThrowSequence : 0;
                _lastResultSequence = flow != null ? flow.MinigameResultSequence : 0;
                _lastMinigame = flow != null ? flow.Minigame : StageMinigameKind.None;
                _wasGrounded = grounded; _wasOnCloth = onCloth; _wasOnTalisman = onTalisman;
                _clothAirborne = false; _oneShot = null; _initialized = true;
                Apply(ContinuousPose(flow, held, grounded, moving, onCloth, onTalisman));
                return;
            }

            if (!Herbalist.GameUI.GameplayPause.IsPaused)
                _oneShotRemaining -= Time.deltaTime;
            if (_oneShot.HasValue && _oneShotRemaining <= 0)
            {
                if (_oneShot.Value == SodamPose.CraftSuccessRise) PlayOneShot(SodamPose.CraftSuccessShow);
                else if (_oneShot.Value == SodamPose.CraftFailRise) PlayOneShot(SodamPose.CraftFailDust);
                else _oneShot = null;
            }

            if (onCloth && !_wasOnCloth)
            {
                _clothAirborne = false;
                PlayOneShot(SodamPose.ClothGrab);
            }
            else if (!onCloth && _wasOnCloth)
            {
                _clothAirborne = true;
                _clothReleaseY = transform.position.y;
                PlayOneShot(SodamPose.ClothFall);
            }
            if (onTalisman && !_wasOnTalisman) PlayOneShot(SodamPose.TalismanBoard);

            // A previous full-body action must not keep grounded legs during takeoff.
            if (!grounded && _wasGrounded && !onCloth && !onTalisman && !_clothAirborne)
                _oneShot = null;

            if (_clothAirborne && grounded && !_wasGrounded)
            {
                _clothAirborne = false;
                PlayOneShot(transform.position.y >= _clothReleaseY - _profile.successLandingDrop
                    ? SodamPose.ClothSwingLand : SodamPose.ClothHardLand);
            }
            if (flow != null && flow.MinigameResultSequence != _lastResultSequence &&
                _lastMinigame == StageMinigameKind.Crafting && validSlot &&
                flow.RoleFor(_actor) == Herbalist.Presentation.CharacterRole.Sodam)
                PlayOneShot(flow.MinigameSucceeded ? SodamPose.CraftSuccessRise : SodamPose.CraftFailRise);

            if (flow != null && flow.Progress != null && validSlot && held != _lastHeld)
            {
                var oldItem = flow.settings.Item(_lastHeld);
                var newItem = flow.settings.Item(held);
                if (oldItem != null && oldItem.type == StageItemType.Potion && held == 0 && consumed != _lastConsumed)
                    PlayOneShot(SodamPose.Drink);
                else if (oldItem == null && newItem != null)
                    PlayOneShot(SodamPose.Receive);
                else if (oldItem != null && newItem == null)
                    PlayOneShot(SodamPose.Pass);
            }

            uint catchSequence = _abilities != null ? _abilities.Leaf.CatchSequence : 0;
            if (catchSequence != _lastCatchSequence)
                PlayOneShot(onTalisman ? SodamPose.TalismanCatch : SodamPose.LeafCatch);
            _lastCatchSequence = catchSequence;
            uint throwSequence = _abilities != null ? _abilities.Leaf.ThrowSequence : 0;
            if (throwSequence != _lastThrowSequence)
                PlayOneShot(onTalisman ? SodamPose.TalismanThrow : SodamPose.LeafThrow, _abilities.Leaf.ThrowElapsed);
            _lastThrowSequence = throwSequence;

            if (_abilities != null && _abilities.Sap != null && _abilities.Sap.Spraying &&
                (_oneShot == SodamPose.LeafThrow || _oneShot == SodamPose.LeafCatch)) _oneShot = null;

            if (!_oneShot.HasValue)
                Apply(ContinuousPose(flow, held, grounded, moving, onCloth, onTalisman));
            UpdateActionLegs(grounded);
            UpdateVisualOffset();

            _lastHeld = held; _lastConsumed = consumed;
            _lastResultSequence = flow != null ? flow.MinigameResultSequence : 0;
            _lastMinigame = flow != null ? flow.Minigame : StageMinigameKind.None;
            _wasGrounded = grounded; _wasOnCloth = onCloth; _wasOnTalisman = onTalisman;
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
            if (catchSequence != _lastCatchSequence) PlayOneShot(SodamPose.LeafCatch);
            if (throwSequence != _lastThrowSequence) PlayOneShot(SodamPose.LeafThrow, _abilities.Leaf.ThrowElapsed);
            _lastCatchSequence = catchSequence;
            _lastThrowSequence = throwSequence;
            bool spraying = _abilities != null && _abilities.Sap != null && _abilities.Sap.Spraying;
            if (spraying) _oneShot = null;
            bool grounded = _animator.GetBool(Animator.StringToHash("Grounded"));
            bool moving = _animator.GetFloat(Animator.StringToHash("Speed")) > _profile.movingSpeedThreshold;
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
                var locomotion = _profile.Find(!grounded ? SodamPose.Jump : moving ? SodamPose.Move : SodamPose.Idle);
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
            return moving ? SodamPose.Move : _cameraMoving ? SodamPose.IdleCameraMoving : SodamPose.Idle;
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
                bool rotating = degrees / Time.deltaTime > _profile.cameraMovementThresholdDegreesPerSecond;
                _cameraStillElapsed = rotating ? 0f : _cameraStillElapsed + Time.deltaTime;
                _cameraMoving = rotating || _cameraMoving &&
                    _cameraStillElapsed < _profile.cameraStillDelaySeconds;
            }
            _lastViewYaw = yaw;
            _lastViewPitch = pitch;
            _hasViewSample = true;
        }

        private void UpdateActionLegs(bool grounded)
        {
            if (_pose != SodamPose.LeafThrow && _pose != SodamPose.LeafCatch && _pose != SodamPose.Sap) return;
            int layer = _animator.GetLayerIndex("leg");
            if (layer < 0) return;
            bool moving = _animator.GetFloat(Animator.StringToHash("Speed")) > _profile.movingSpeedThreshold;
            // Catch contains authored steps even with no movement input. Keep its upper action,
            // but explicitly cover those steps with idle legs while standing.
            if (grounded && !moving && _pose != SodamPose.LeafCatch) { _animator.SetLayerWeight(layer, 0f); return; }
            var locomotion = _profile.Find(!grounded ? SodamPose.Jump : moving ? SodamPose.Move : SodamPose.Idle);
            if (locomotion == null || locomotion.motions == null) return;
            foreach (var motion in locomotion.motions)
            {
                if (motion == null || motion.layer != (grounded ? "Base Layer" : "leg")) continue;
                int hash = Animator.StringToHash("leg." + motion.state);
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
            var definition = _profile.Find(pose);
            if (definition == null || definition.durationSeconds <= elapsed) return;
            _oneShot = pose;
            _oneShotRemaining = definition.durationSeconds - elapsed;
            _pose = null;
            Apply(pose, elapsed);
        }

        private void Apply(SodamPose pose, float elapsed = 0f)
        {
            if (_animator == null) return;
            var definition = _profile.Find(pose);
            if (definition == null || definition.motions == null) return;
            if (_pose.HasValue && _pose.Value == pose) return;
            _pose = pose;
            _poseSequence++;
            _previousVisualOffsetY = _animator.transform.localPosition.y - _restVisualPosition.y;
            _poseElapsed = elapsed;
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
                if (_animator.HasState(layer, hash))
                    _animator.CrossFadeInFixedTime(hash, definition.blendSeconds, layer,
                        motion.startTimeSeconds + elapsed);
            }
        }

        private void UpdateVisualOffset()
        {
            if (_animator == null || !_pose.HasValue) return;
            var definition = _profile.Find(_pose.Value);
            if (definition == null) return;
            if (!Herbalist.GameUI.GameplayPause.IsPaused) _poseElapsed += Time.deltaTime;
            float targetOffset = definition.modelOffsetY;
            if (definition.visualOffsetY != null && definition.visualOffsetY.length > 0)
                targetOffset += definition.visualOffsetY.Evaluate(_poseElapsed * definition.rootMotionPlaybackSpeed);
            float blend = definition.blendSeconds > 0f ? Mathf.Clamp01(_poseElapsed / definition.blendSeconds) : 1f;
            _animator.transform.localPosition = _restVisualPosition + Vector3.up * Mathf.Lerp(_previousVisualOffsetY, targetOffset, blend);
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
