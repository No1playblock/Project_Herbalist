using System.Collections.Generic;
using System.Linq;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Presentation;
using Herbalist.Player;
using Herbalist.StageOne;
using TMPro;
using UnityEngine;

namespace Herbalist.GameUI
{
    public sealed class StageNarrativePresenter : MonoBehaviour
    {
        private const string StoneTutorial = "T3-03";
        [SerializeField]
        private StageNarrativeText _text;
        [SerializeField]
        private StagePlaytestHud _hud;
        [SerializeField]
        private TMP_Text _mainQuest, _subQuest;
        [SerializeField]
        private ExteriorJumpPad _firstPad;
        [SerializeField]
        private LeafInstallTarget _firstPadLeaf;
        [SerializeField]
        private SapInjectionPort _firstPadInlet;
        [SerializeField, Min(.1f)]
        private float _noticeRange = 12f;
        [SerializeField, Min(.1f)]
        private float _rangeDialogueInterval = 5f;
        private readonly HashSet<string> _seen = new();
        private readonly HashSet<HerbSource> _discovered = new();
        private string _activeTutorial;
        private float _rangeDialogueRemaining;
        private uint _leafFeedback;
        private int _refillMask;
        private Vector2 _firstWaterPosition;
        private bool _waterPositionKnown;
        private DescendingMazeFlow _maze;
        private BranchRideFlow _ride;
        private bool _pairedPadArrival;
        private Vector3 _tutorialPoint;
        private string _detailId;
        [SerializeField, Min(0)] private float _questRightMargin = 24f;
        private RectTransform _questRoot;
        private Vector3[] _questOriginalPositions;
        private RectTransform[] _questLayoutTargets;
        private PlayScreenController _screen;
        public string MainQuestText => _mainQuest != null ? _mainQuest.text : string.Empty;
        public bool InlineQuestCount { get; private set; }
        public bool ParenthesizeQuestCount { get; private set; }
        public string DetailQuestText => _subQuest != null && _subQuest.gameObject.activeSelf ? _subQuest.text : string.Empty;
        public bool QuestVisible => _mainQuest != null && _mainQuest.transform.parent.gameObject.activeSelf;
        public bool TryNavigationTarget(out Vector3 point, out string label)
        {
            point = default;
            label = string.Empty;
            var flow = StageOneFlow.Instance;
            if (flow == null)
                return false;
            if (_detailId == "S1-09" && _firstPad != null)
            {
                point = _firstPad.transform.position;
                label = _text.Find(_detailId);
                return true;
            }

            return false;
        }

        private void Awake()
        {
            _questRoot = (RectTransform)_mainQuest.transform.parent;
            _questLayoutTargets = new[] { _questRoot };
            _questOriginalPositions = new[] { _questRoot.anchoredPosition3D };
            _screen = FindAnyObjectByType<PlayScreenController>();
            _hud.NarrativeControlled = true;
            _maze = FindAnyObjectByType<DescendingMazeFlow>();
            _ride = FindAnyObjectByType<BranchRideFlow>();
        }

        private void LateUpdate()
        {
            PlayerHud.PositionQuestPanel((RectTransform)_questRoot.parent, _questRoot, _questLayoutTargets,
                _questOriginalPositions, _screen != null && _screen.IsSplitVisible, _questRightMargin);
            if (GameplayPause.IsPaused || _text == null)
                return;
            var actor = StageActor.All.FirstOrDefault(a => a.Local);
            if (actor == null)
                return;
            if (_activeTutorial != null && !Near(actor, _tutorialPoint))
            {
                if (_activeTutorial != StoneTutorial) _seen.Remove(_activeTutorial);
                _hud.HideGuide();
                _activeTutorial = null;
            }

            _rangeDialogueRemaining -= Time.unscaledDeltaTime;
            if (StageOneFlow.Instance is
            {
            Progress: not null
            }

            one)
                StageOne(actor, one);
            else if (_maze != null)
                StageTwo(actor);
            else if (_ride != null)
                StageThree(actor);
            RangeDialogue(actor);
        }

        private void Quest(string main, string detail = null, int count = -1)
        {
            InlineQuestCount = main is "M1-01" or "M3-01";
            ParenthesizeQuestCount = main == "M3-01";
            _detailId = detail;
            _mainQuest.text = _text.Find(main);
            if (count >= 0)
                _mainQuest.text = _mainQuest.text.Replace("(0/", "(" + count + "/");
            _subQuest.text = detail != null ? _text.Find(detail) : string.Empty;
            _subQuest.gameObject.SetActive(!string.IsNullOrEmpty(_subQuest.text));
        }

        private void Dialogue(string group, params string[] lines)
        {
            if (!_seen.Add(group))
                return;
            foreach (string line in lines)
                _hud.QueueDialogue(_text.Find(line));
        }

        private bool Near(StageActor actor, Vector3 point) => (actor.transform.position - point).sqrMagnitude <= _noticeRange * _noticeRange;
        private void Tutorial(string id, StageActor actor, Vector3 point, bool eligible, bool complete, LeafInstallTarget target = null)
        {
            if (_activeTutorial == id)
            {
                if (!eligible || complete || !_hud.GuideActive)
                {
                    if (!eligible && !complete && id != StoneTutorial) _seen.Remove(id);
                    _hud.HideGuide();
                    _activeTutorial = null;
                }

                return;
            }

            if (_activeTutorial != null || _seen.Contains(id) || !eligible || complete || !Near(actor, point))
                return;
            if (_hud.ShowGuide(actor.Camera, point, _text.Find(id), target))
            {
                _seen.Add(id);
                _activeTutorial = id;
                _tutorialPoint = point;
            }
        }

        private string PotionStep(StageOneFlow flow, PlayerAbilityKind kind, int firstStep)
        {
            int recipeIndex = System.Array.FindIndex(flow.settings.recipes, r => r.result.ability == kind);
            if (recipeIndex < 0 || (flow.Progress.ConsumedMask & (1u << recipeIndex)) != 0)
                return null;
            var recipe = flow.settings.recipes[recipeIndex];
            int step = firstStep;
            if ((flow.Progress.Crafted & (1u << recipeIndex)) != 0)
                step += 3;
            else
            {
                bool harvested = false;
                for (int i = 0; i < flow.sources.Length; i++)
                    if (flow.sources[i].item == recipe.ingredient && (flow.Progress.Harvested & (1UL << i)) != 0)
                        harvested = true;
                if (harvested)
                    step += 2;
                else if (flow.sources.Any(s => s.item == recipe.ingredient && _discovered.Contains(s)))
                    step++;
            }

            return "S1-" + step.ToString("00");
        }

#if UNITY_EDITOR
        internal ExteriorJumpPad QaFirstJumpPad => _firstPad;
#endif

        private void StageOne(StageActor actor, StageOneFlow flow)
        {
            Dialogue("G1-01", "D-001", "D-002");
            foreach (var source in flow.sources)
                if (source != null && flow.Actors.Any(a => a != null && a.Available && Near(a, source.Point)))
                    if (_discovered.Add(source) && source.item.ability == PlayerAbilityKind.Sap)
                        Dialogue("G1-02", "D-003", "D-004");
            if (flow.Progress.Crafted != 0)
                Dialogue("G1-03", "D-005", "D-006");
            bool potionsComplete = flow.Progress.GateOpen;
            if (!potionsComplete)
            {
                string detail = PotionStep(flow, PlayerAbilityKind.Sap, 1) ?? PotionStep(flow, PlayerAbilityKind.Leaf, 5);
                Quest("M1-01", detail, flow.Progress.Consumed.Count(x => x != 0));
                // The offline T shortcut grants abilities without completing either potion.
                // Let it exercise tutorials without advancing narrative or gate progress.
                if (!actor.Abilities.OfflineBothUnlocked) return;
            }
            else Dialogue("G1-04", "D-007", "D-008");
            if (_firstPad != null)
            {
                if (potionsComplete)
                {
                    _pairedPadArrival |= flow.Actors.Count(a => a != null && a.Available && a.Abilities.Unlocked && Near(a, _firstPad.transform.position)) == 2;
                    if (_pairedPadArrival)
                        Dialogue("G1-05", "D-009");
                    if (_firstPad.LaunchCount > 0)
                    {
                        Quest("M1-03");
                        Dialogue("G1-06", "D-010");
                    }
                    else
                        Quest("M1-02", !_pairedPadArrival ? "S1-09" : _firstPadLeaf != null && _firstPadLeaf.HasLeaf ? "S1-11" : "S1-10");
                }
                bool atPad = actor.Abilities.Unlocked && Near(actor, _firstPad.transform.position);
                if (actor.Abilities.Kind == PlayerAbilityKind.Sap && _firstPadInlet != null)
                    Tutorial("T1-01", actor, _firstPadInlet.InteractionPoint, atPad, _firstPad.ChargeFraction > 0 || _firstPad.LaunchCount > 0);
                if (actor.Abilities.Kind == PlayerAbilityKind.Leaf && _firstPadLeaf != null)
                {
                    if (_hud.TryLeafGuidePoint(actor.transform.position, _noticeRange, _firstPadLeaf, out var leafPoint))
                        Tutorial("T1-02", actor, leafPoint, atPad, _firstPadLeaf.HasLeaf || _firstPad.LaunchCount > 0);
                }
            }

            foreach (var target in flow.SapOnlyBranchGuideTargets)
                if (target != null)
                    Tutorial("T1-03", actor, target.SocketPosition, target.isActiveAndEnabled && target.HasLeaf && Near(actor, target.SocketPosition), !target.HasLeaf, target);
            if (actor.Abilities.Kind == PlayerAbilityKind.Leaf)
            {
                bool hint = HangingCloth.TryInteractionHint(actor, out var knot, out bool release);
                Tutorial("T1-04", actor, knot, hint, release || HangingCloth.IsRiding(actor));
                if (hint)
                    Dialogue("G1-07", flow.RoleFor(actor) == CharacterRole.Duyeong ? "D-011" : "D-012");
            }

            if (flow.InsideCount == 2)
                Dialogue("G1-08", "D-013");
        }

        private void StageTwo(StageActor actor)
        {
            int completed = Mathf.Clamp(_maze.CurrentMaze, 0, 3);
            Quest("M2-01", completed < 3 ? "S2-" + (completed + 1).ToString("00") : null, completed);
            var board = _maze.CurrentBoard;
            if (board == null)
            {
                _hud.HideGuide();
                _activeTutorial = null;
                return;
            }

            Dialogue("G2-01", "D-014", "D-015");
            if (completed == 0)
            {
                if (!_waterPositionKnown)
                {
                    _firstWaterPosition = board.SapPosition;
                    _waterPositionKnown = true;
                }

                bool moved = (board.SapPosition - _firstWaterPosition).sqrMagnitude > .01f;
                if (actor.Abilities.Kind == PlayerAbilityKind.Sap)
                {
                    Tutorial("T2-01", actor, board.StartMarker.position, true, moved);
                    var marker = board.RefillMarkers.Where((candidate, index) => candidate != null &&
                        (board.State.CollectedRefillMask & (1 << index)) == 0).FirstOrDefault() ??
                        board.RefillMarkers.FirstOrDefault(t => t != null);
                    if (marker != null)
                        Tutorial("T2-03", actor, marker.position, moved, board.State.CollectedRefillMask != 0);
                }
                else
                {
                    var leak = board.Leaks.FirstOrDefault(x => x != null && !x.Blocked) ??
                        board.Leaks.FirstOrDefault(x => x != null);
                    if (leak != null)
                        Tutorial("T2-02", actor, leak.transform.position, true, board.Leaks.Any(x => x.Blocked));
                    Tutorial("T2-04", actor, board.GoalMarker.position, board.Leaks.Any(x => x.Blocked), board.State.CollectedRefillMask != _refillMask);
                }
            }

            if (board.AnyLeak)
            {
                var leafActor = StageActor.All.FirstOrDefault(a => a.Available && a.Abilities.Kind == PlayerAbilityKind.Leaf);
                bool sodam = leafActor != null && leafActor.GetComponent<PlayerCharacterPresentation>().IsSodamActive;
                Dialogue("G2-02", sodam ? "D-016" : "D-018");
            }

            if (board.State.Phase == MazePhase.Retrying && !board.CanReachRequiredVolume())
            {
                var sap = StageActor.All.FirstOrDefault(a => a.Available && a.Abilities.Kind == PlayerAbilityKind.Sap);
                Dialogue("G2-03C", sap != null && sap.GetComponent<PlayerCharacterPresentation>().IsSodamActive ? "D-042" : "D-041");
            }
            else if (board.State.Phase == MazePhase.Running && board.RemainingVolume < board.RequiredVolume && Vector2.Distance(board.SapPosition, board.SurfaceGoal) < board.Definition.refillRadius)
            {
                var sap = StageActor.All.FirstOrDefault(a => a.Available && a.Abilities.Kind == PlayerAbilityKind.Sap);
                bool sodam = sap != null && sap.GetComponent<PlayerCharacterPresentation>().IsSodamActive;
                Dialogue("G2-03", sodam ? "D-022" : "D-020", sodam ? "D-023" : "D-021");
            }

            if (completed == 2 && board.State.Phase == MazePhase.Solved)
                Dialogue("G2-04", "D-024");
            _refillMask = board.State.CollectedRefillMask;
        }

        private void StageThree(StageActor actor)
        {
            bool atBottom = _ride.CurrentPhase is BranchRidePhase.Landed or BranchRidePhase.Ending or BranchRidePhase.Complete;
            _mainQuest.transform.parent.gameObject.SetActive(!atBottom);
            if (atBottom)
            {
                _hud.HideGuide();
                _activeTutorial = null;
                return;
            }
            Quest("M3-01", _ride.CurrentPhase == BranchRidePhase.Boarding ? "S3-01" : null, _ride.BoardedCount);
            Dialogue("G3-01", "D-025", "D-026");
            Tutorial("T3-01", actor, _ride.Position, Near(actor, _ride.Position), _ride.CurrentPhase != BranchRidePhase.Boarding);
            Tutorial(StoneTutorial, actor, _ride.Position, actor.Abilities.Kind == PlayerAbilityKind.Leaf, false);
            if (_ride.CurrentPhase != BranchRidePhase.Riding)
                return;
            var first = _ride.Switches[0];
            if (Near(actor, first.Port.InteractionPoint))
            {
                Dialogue("G3-02", "D-027", "D-028");
                if (actor.Abilities.Kind == PlayerAbilityKind.Sap)
                    Tutorial("T3-02", actor, first.Port.InteractionPoint, true, _ride.PresentationBranchMask != 0);
            }

            foreach (var stone in _ride.Hazards)
                if (stone != null && stone.Sample(_ride.PresentationStoneClock, out var point) && Near(actor, point))
                {
                    Dialogue("G3-03", "D-029", "D-030");
                    break;
                }
        }

        private void RangeDialogue(StageActor actor)
        {
            var ability = actor.Abilities;
            if (ability == null || ability.Leaf.FeedbackSequence == _leafFeedback)
                return;
            _leafFeedback = ability.Leaf.FeedbackSequence;
            if (!ability.Leaf.RangeRejected || _rangeDialogueRemaining > 0)
                return;
            bool sodam = actor.GetComponent<PlayerCharacterPresentation>().IsSodamActive;
            int first = sodam ? 39 : 37;
            _hud.QueueDialogue(_text.Find("D-" + (first + Random.Range(0, 2)).ToString("000")));
            _rangeDialogueRemaining = _rangeDialogueInterval;
        }
    }
}
