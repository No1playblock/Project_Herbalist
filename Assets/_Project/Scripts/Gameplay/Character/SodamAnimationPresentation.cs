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
        private int _lastHeld, _lastConsumed, _lastLeafSlots;
        private uint _lastResultSequence;
        private StageMinigameKind _lastMinigame;
        private bool _wasGrounded, _wasOnCloth, _wasOnTalisman;
        private bool _clothAirborne;
        private float _clothReleaseY;
        private bool _initialized;
        public SodamPose? CurrentPose => _character != null && _character.IsSodamActive ? _pose : null;
        public uint PoseSequence => _poseSequence;
        public float RootMotionScale => _animator != null ? _animator.transform.lossyScale.x : 1f;
        public SodamAnimationProfile Profile => _profile;

        private void Awake()
        {
            _character = GetComponent<PlayerCharacterPresentation>();
            _abilities = GetComponent<PlayerAbilityController>();
            _actor = GetComponent<StageActor>();
        }

        private void LateUpdate()
        {
            if (_profile == null || _character == null || !_character.IsSodamActive)
            {
                if (_animator != null) _animator.transform.localPosition = _restVisualPosition;
                _initialized = false;
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
                _pose = null;
            }

            var flow = StageOneFlow.Instance;
            bool validSlot = _actor != null && _actor.Available && _actor.Slot >= 0 && _actor.Slot < 2;
            int held = flow != null && flow.Progress != null && validSlot ? flow.Progress.Held[_actor.Slot] : 0;
            int consumed = flow != null && flow.Progress != null && validSlot ? flow.Progress.Consumed[_actor.Slot] : 0;
            int leafSlots = _abilities != null ? _abilities.DisplayLeafSlotStates : 0;
            bool grounded = animator.GetBool(Animator.StringToHash("Grounded"));
            bool onCloth = _actor != null && HangingCloth.IsRiding(_actor);
            bool onTalisman = _actor != null && BranchRideFlow.IsRiding(_actor);
            bool moving = animator.GetFloat(Animator.StringToHash("Speed")) > _profile.movingSpeedThreshold;

            if (!_initialized)
            {
                _lastHeld = held; _lastConsumed = consumed; _lastLeafSlots = leafSlots;
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

            if (_clothAirborne && grounded && !_wasGrounded)
            {
                _clothAirborne = false;
                PlayOneShot(transform.position.y >= _clothReleaseY - _profile.successLandingDrop
                    ? SodamPose.ClothSwingLand : SodamPose.ClothHardLand);
            }
            else if (grounded && !_wasGrounded && !onCloth && !onTalisman && !_clothAirborne)
                PlayOneShot(SodamPose.Land);

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

            if (leafSlots != _lastLeafSlots)
            {
                bool thrown = false, caught = false;
                for (int i = 0; i < 3; i++)
                {
                    int before = (_lastLeafSlots >> (i * 2)) & 3;
                    int after = (leafSlots >> (i * 2)) & 3;
                    thrown |= before == 0 && after == 1;
                    caught |= before == 2 && after == 0;
                }
                if (thrown) PlayOneShot(onTalisman ? SodamPose.TalismanThrow : SodamPose.LeafThrow);
                else if (caught) PlayOneShot(onTalisman ? SodamPose.TalismanCatch : SodamPose.LeafCatch);
            }

            if (!_oneShot.HasValue)
                Apply(ContinuousPose(flow, held, grounded, moving, onCloth, onTalisman));
            UpdateVisualOffset();

            _lastHeld = held; _lastConsumed = consumed; _lastLeafSlots = leafSlots;
            _lastResultSequence = flow != null ? flow.MinigameResultSequence : 0;
            _lastMinigame = flow != null ? flow.Minigame : StageMinigameKind.None;
            _wasGrounded = grounded; _wasOnCloth = onCloth; _wasOnTalisman = onTalisman;
        }

        private SodamPose ContinuousPose(StageOneFlow flow, int held, bool grounded, bool moving, bool onCloth, bool onTalisman)
        {
            if (onCloth) return SodamPose.ClothHang;
            if (onTalisman)
                return _abilities != null && _abilities.Sap != null && _abilities.Sap.Controlling
                    ? SodamPose.TalismanSap : SodamPose.TalismanSit;
            if (_clothAirborne) return SodamPose.ClothFall;
            if (!grounded) return SodamPose.Jump;
            if (_abilities != null && _abilities.Sap != null && _abilities.Sap.Controlling) return SodamPose.Sap;
            var item = flow != null && flow.settings != null ? flow.settings.Item(held) : null;
            if (item != null && item.type == StageItemType.Herb) return moving ? SodamPose.HerbMove : SodamPose.HerbIdle;
            if (item != null && item.type == StageItemType.Potion) return moving ? SodamPose.PotionMove : SodamPose.PotionIdle;
            return moving ? SodamPose.Move : SodamPose.Idle;
        }

        private void PlayOneShot(SodamPose pose)
        {
            var definition = _profile.Find(pose);
            if (definition == null || definition.durationSeconds <= 0) return;
            _oneShot = pose;
            _oneShotRemaining = definition.durationSeconds;
            _pose = null;
            Apply(pose);
        }

        private void Apply(SodamPose pose)
        {
            if (_animator == null) return;
            var definition = _profile.Find(pose);
            if (definition == null || definition.motions == null) return;
            if (_pose.HasValue && _pose.Value == pose) return;
            _pose = pose;
            _poseSequence++;
            _previousVisualOffsetY = _animator.transform.localPosition.y - _restVisualPosition.y;
            _poseElapsed = 0f;
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
                if (_animator.HasState(layer, hash)) _animator.CrossFadeInFixedTime(hash, definition.blendSeconds, layer);
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
            _pose = null;
        }
    }
}
