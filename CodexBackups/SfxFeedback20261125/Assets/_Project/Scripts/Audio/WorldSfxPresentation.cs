using Herbalist.Abilities;
using Herbalist.GameUI;
using Herbalist.Levels;
using UnityEngine;

namespace Herbalist.Audio
{
    public enum SfxWorldKind { Leaf, Water, JumpPad, Cloth, Maze, Descent, BranchRide, AltarRing, Altar }

    [DefaultExecutionOrder(240)]
    [RequireComponent(typeof(SfxEmitter))]
    public sealed class WorldSfxPresentation : MonoBehaviour
    {
        [SerializeField] private SfxWorldKind _kind;
        [SerializeField] private Component _subject;
        [SerializeField] private Transform _motion;
        [SerializeField] private Transform[] _stones = System.Array.Empty<Transform>();
        private SfxEmitter _emitter;
        private bool _initialized, _impact, _placed, _exit, _blocked;
        private float _charge, _flight, _clock, _angle;
        private int _phase, _branches;
        private Vector3 _position;
        private Vector2 _mazePosition;
        private LeafState _leafState = LeafState.Complete;
        private bool[] _stoneActive, _stoneContact;
        private Vector3[] _stonePositions;
        private void Awake()
        {
            _emitter = GetComponent<SfxEmitter>();
            _stoneActive = new bool[_stones.Length]; _stoneContact = new bool[_stones.Length];
            _stonePositions = new Vector3[_stones.Length];
        }
        private void LateUpdate()
        {
            if (_subject == null || _emitter.Catalog == null) return;
            if (GameplayPause.IsPaused) return;
            switch (_kind)
            {
                case SfxWorldKind.Leaf: Leaf((LeafProjectile)_subject); break;
                case SfxWorldKind.Water: Water((SapDeposit)_subject); break;
                case SfxWorldKind.JumpPad: Pad((ExteriorJumpPad)_subject); break;
                case SfxWorldKind.Cloth: Cloth(); break;
                case SfxWorldKind.Maze: Maze((SapMazeBoard)_subject); break;
                case SfxWorldKind.Descent: Descent((DescendingMazeFlow)_subject); break;
                case SfxWorldKind.BranchRide: Branch((BranchRideFlow)_subject); break;
                case SfxWorldKind.AltarRing: Ring((AltarRing)_subject); break;
                case SfxWorldKind.Altar:
                    bool exit = ((RotatingAltar)_subject).CanExit;
                    if (_initialized && exit && !_exit) _emitter.Play(58);
                    _exit = exit; break;
            }
            _initialized = true;
        }
        private void Leaf(LeafProjectile leaf)
        {
            if (leaf.State != _leafState)
            {
                if (leaf.State == LeafState.Flying) _emitter.Play(33);
                else if (leaf.State == LeafState.Installed) _emitter.Play(35);
                else if (leaf.State == LeafState.Returning) _emitter.Play(36);
            }
            _emitter.SetLoop(34, leaf.State == LeafState.Flying);
            _leafState = leaf.State;
        }
        private void Water(SapDeposit water)
        {
            _emitter.SetLoop(31, water.HasStream && !water.HoseRetracting);
            if (water.HoseImpact && !_impact) _emitter.PlayAt(32, water.StreamEnd);
            if (water.IsPlaced && !_placed) _emitter.Play(32);
            _impact = water.HoseImpact; _placed = water.IsPlaced;
        }
        private void Pad(ExteriorJumpPad pad)
        {
            float charge = pad.ChargeFraction, flight = pad.PresentationFlight;
            if (_initialized && charge > _charge && flight <= 0) _emitter.Play(39);
            if (_initialized && flight > 0 && _flight <= 0)
            {
                _emitter.Play(40);
                if (pad.PresentationHasLeaf) _emitter.Play(41);
            }
            _charge = charge; _flight = flight;
        }
        private void Cloth()
        {
            if (_motion == null) return;
            if (_initialized && Time.deltaTime > 0 &&
                Vector3.Distance(_motion.position, _position) / Time.deltaTime > _emitter.Catalog.clothMotionThreshold)
                _emitter.PlayAt(38, _motion.position);
            _position = _motion.position;
        }
        private void Maze(SapMazeBoard board)
        {
            bool moving = _initialized && board.Active && board.State.Phase == MazePhase.Running &&
                Vector2.Distance(board.SapPosition, _mazePosition) > _emitter.Catalog.movementThreshold * Time.deltaTime;
            Vector3 point = _motion != null ? _motion.position : transform.position;
            _emitter.SetLoop(42, moving, 1, point);
            if (board.Active && board.AnyLeak) _emitter.PlayAt(44, point);
            _mazePosition = board.SapPosition;
        }
        private void Descent(DescendingMazeFlow flow)
        {
            int phase = flow.PresentationPhase;
            Vector3 point = _motion != null ? _motion.position : transform.position;
            if (_initialized && phase == 2 && _phase != 2) { _emitter.PlayAt(45, point); _emitter.PlayAt(46, point); }
            if (_initialized && _phase == 2 && phase != 2) _emitter.PlayAt(47, point);
            _phase = phase;
        }
        private void Branch(BranchRideFlow flow)
        {
            int mask = flow.PresentationBranchMask;
            if (_initialized && mask != _branches) { _emitter.Play(51); _emitter.Play(52); }
            if (_motion != null)
            {
                bool moving = _initialized && Time.deltaTime > 0 &&
                    Vector3.Distance(_motion.position, _position) > _emitter.Catalog.movementThreshold * Time.deltaTime;
                _emitter.SetLoop(49, moving, 1, _motion.position); _position = _motion.position;
            }
            if (_initialized && _blocked && !flow.PresentationBlockedByLeaf &&
                flow.PresentationStoneClock > _clock + Time.deltaTime) _emitter.Play(57);
            _branches = mask; _clock = flow.PresentationStoneClock; _blocked = flow.PresentationBlockedByLeaf;
            bool rolling = false;
            for (int i = 0; i < _stones.Length; i++)
            {
                var stone = _stones[i]; if (stone == null) continue;
                bool active = stone.gameObject.activeInHierarchy;
                if (active && !_stoneActive[i]) _emitter.PlayAt(53, stone.position);
                var renderer = stone.GetComponentInChildren<Renderer>();
                Vector3 bottom = renderer != null ? new Vector3(stone.position.x, renderer.bounds.min.y, stone.position.z) : stone.position;
                bool contact = active && Physics.Raycast(bottom, Vector3.down, out var hit,
                    _emitter.Catalog.stoneContactDistance, _emitter.Catalog.groundMask, QueryTriggerInteraction.Ignore) &&
                    hit.collider.transform != stone && !hit.collider.transform.IsChildOf(stone);
                if (contact && !_stoneContact[i]) _emitter.PlayAt(54, stone.position);
                rolling |= contact && _stoneActive[i] &&
                    Vector3.ProjectOnPlane(stone.position - _stonePositions[i], Vector3.up).magnitude >
                    _emitter.Catalog.movementThreshold * Time.deltaTime;
                if (_initialized && (_stoneContact[i] && !contact || _stoneActive[i] && !active)) _emitter.PlayAt(56, stone.position);
                _stoneActive[i] = active; _stoneContact[i] = contact; _stonePositions[i] = stone.position;
            }
            _emitter.SetLoop(55, rolling);
        }
        private void Ring(AltarRing ring)
        {
            if (_initialized && !ring.Frozen && Mathf.Abs(Mathf.DeltaAngle(_angle, ring.CurrentAngle)) > .01f) _emitter.Play(52, .45f);
            _angle = ring.CurrentAngle;
        }
        private void OnDisable() { _emitter?.StopLoops(); _initialized = false; }
    }
}
