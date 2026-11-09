using Herbalist.GameUI;
using Herbalist.Levels;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Audio
{
    [DefaultExecutionOrder(220)]
    [RequireComponent(typeof(PlayerController), typeof(SfxEmitter))]
    public sealed class PlayerSfxPresentation : MonoBehaviour
    {
        private PlayerController _player;
        private NetworkPlayer _network;
        private StageActor _actor;
        private CharacterController _character;
        private SfxEmitter _emitter;
        private SapMazeBoard[] _boards;
        private Vector3 _position, _velocity;
        private bool _initialized, _grounded, _onTalisman, _fallingWind;
        private PlayerCharacterPresentation _presentation;
        private Animator _footAnimator;
        private Transform _leftFoot, _rightFoot;
        private bool _leftLifted, _rightLifted;
        private int _consumed;
        private void Awake()
        {
            _player = GetComponent<PlayerController>(); _network = GetComponent<NetworkPlayer>();
            _actor = GetComponent<StageActor>(); _character = GetComponent<CharacterController>();
            _emitter = GetComponent<SfxEmitter>();
            _presentation = GetComponent<PlayerCharacterPresentation>();
            _boards = FindObjectsByType<SapMazeBoard>(FindObjectsSortMode.None);
            if (_actor != null) _actor.LocalCommandRequested += OnLocalCommand;
        }
        private void OnLocalCommand(StageCommand command)
        {
            var flow = StageOneFlow.Instance;
            if (flow == null) return;
            if (flow.Minigame == StageMinigameKind.Gathering && command >= StageCommand.Up && command <= StageCommand.Right)
                _emitter.Play(20);
            if (flow.Minigame == StageMinigameKind.Crafting && command == StageCommand.Craft &&
                flow.RoleFor(_actor) == Herbalist.Presentation.CharacterRole.Sodam) _emitter.Play(26);
        }
        private void LateUpdate()
        {
            var tuning = _emitter.Catalog;
            if (tuning == null || _character == null || _player.Motor == null) return;
            bool online = _network != null && _network.Object != null && _network.Object.IsValid;
            if (_network != null && !online) { _emitter.StopLoops(); _initialized = false; return; }
            bool grounded = online ? (bool)_network.Grounded : _player.Motor.IsGrounded;
            Vector3 velocity = online ? _network.SimulationVelocity : _player.Motor.Velocity;
            var flow = StageOneFlow.Instance;
            int slot = _actor != null ? _actor.Slot : -1;
            int consumed = flow != null && flow.Progress != null && slot >= 0 && slot < 2 ? flow.Progress.Consumed[slot] : 0;
            bool onTalisman = _actor != null && BranchRideFlow.IsRiding(_actor);
            if (!_initialized)
            {
                _position = transform.position; _velocity = velocity; _grounded = grounded; _consumed = consumed;
                _onTalisman = onTalisman; _initialized = true; _emitter.Play(9); return;
            }
            if (GameplayPause.IsPaused) return;
            float height = _character.height * Mathf.Abs(transform.lossyScale.y);
            var delta = transform.position - _position;
            bool teleported = delta.magnitude > height * tuning.teleportDistanceInHeights;
            if (teleported) _emitter.Play(9);
            var surface = GroundSurface(tuning);
            if (!teleported && _grounded && !grounded && velocity.y > 0 &&
                !(online ? (bool)_network.ExternalFlight : _player.Motor.CaptureState().ExternalFlight))
            {
                bool sodam = _actor != null && flow != null ? flow.RoleFor(_actor) == Herbalist.Presentation.CharacterRole.Sodam :
                    GetComponent<PlayerCharacterPresentation>() != null && GetComponent<PlayerCharacterPresentation>().IsSodamActive;
                _emitter.Play(sodam ? 13 : 14);
            }
            if (!teleported && !_grounded && grounded && _velocity.y < 0)
                _emitter.Play(surface == SfxSurfaceKind.Wood || surface == SfxSurfaceKind.Bark ? 16 : 15,
                    1, -_velocity.y < tuning.strongLandingSpeed);
            if (grounded && !teleported && (flow == null || flow.Minigame == StageMinigameKind.None))
            {
                float distance = Vector3.ProjectOnPlane(delta, Vector3.up).magnitude;
                if (distance > tuning.movementThreshold * Time.deltaTime)
                {
                    Footsteps(tuning, height);
                }
                else ResetFeet();
            }
            else ResetFeet();
            bool fallingWind = !grounded && velocity.y < -tuning.fallingWindSpeed;
            if (tuning.fallingWindEnabled && fallingWind && !_fallingWind) _emitter.Play(17);
            _fallingWind = fallingWind;
            if (consumed != _consumed && consumed != 0) _emitter.Play(29);
            if (onTalisman != _onTalisman) _emitter.Play(onTalisman ? 48 : 50);
            if (_actor != null && _actor.Local && _actor.Abilities != null && _actor.Abilities.Input.UseHeld)
            {
                foreach (var board in _boards)
                    if (board.Controlling && !SapMazeBoard.TryReadLocalPointer(_player.View, out _)) { _emitter.Play(43); break; }
            }
            _position = transform.position; _velocity = velocity; _grounded = grounded; _consumed = consumed;
            _onTalisman = onTalisman;
        }
        private void ResetFeet() { _leftLifted = _rightLifted = false; }
        private void Footsteps(SfxCatalog tuning, float height)
        {
            var animator = _presentation != null ? _presentation.ActiveAnimator : null;
            if (animator != _footAnimator)
            {
                _footAnimator = animator;
                _leftFoot = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.LeftFoot) : null;
                _rightFoot = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.RightFoot) : null;
                ResetFeet();
            }
            if (animator == null || !animator.isActiveAndEnabled || !animator.isHuman) return;
            Footfall(_leftFoot, animator.leftFeetBottomHeight, ref _leftLifted, tuning, height);
            Footfall(_rightFoot, animator.rightFeetBottomHeight, ref _rightLifted, tuning, height);
        }
        private void Footfall(Transform foot, float bottomHeight, ref bool lifted, SfxCatalog tuning, float height)
        {
            if (foot == null) return;
            float sole = bottomHeight * Mathf.Abs(_footAnimator.transform.lossyScale.y);
            Vector3 origin = foot.position + Vector3.up * tuning.groundProbeHeight;
            if (!Physics.Raycast(origin, Vector3.down, out var hit,
                tuning.groundProbeHeight + tuning.groundProbeDistance, tuning.groundMask, QueryTriggerInteraction.Ignore))
            { lifted = false; return; }
            float clearance = foot.position.y - sole - hit.point.y;
            if (clearance > height * tuning.footLiftInHeights) lifted = true;
            else if (lifted && clearance <= height * tuning.footContactInHeights)
            {
                lifted = false;
                _emitter.Play(tuning.footstepSoundId);
            }
        }
        private SfxSurfaceKind GroundSurface(SfxCatalog tuning)
        {
            Vector3 feet = transform.TransformPoint(_character.center) - Vector3.up * (_character.height * Mathf.Abs(transform.lossyScale.y) * .5f);
            if (Physics.Raycast(feet + Vector3.up * tuning.groundProbeHeight, Vector3.down, out var hit,
                tuning.groundProbeHeight + tuning.groundProbeDistance, tuning.groundMask, QueryTriggerInteraction.Ignore))
            {
                var surface = hit.collider.GetComponentInParent<SfxSurface>();
                if (surface != null) return surface.Kind;
                if (hit.collider is TerrainCollider) return SfxSurfaceKind.Grass;
            }
            return SfxSurfaceKind.Dirt;
        }
        private void OnDisable() { _initialized = false; ResetFeet(); _emitter?.StopLoops(); }
        private void OnDestroy() { if (_actor != null) _actor.LocalCommandRequested -= OnLocalCommand; }
    }
}
