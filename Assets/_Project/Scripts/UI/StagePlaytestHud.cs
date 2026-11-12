using System.Collections.Generic;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Presentation;
using Herbalist.StageOne;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Herbalist.GameUI
{
    public sealed class StagePlaytestHud : MonoBehaviour
    {
        [SerializeField] private GameOverlayHud _overlay;
        [SerializeField] private InputActionReference _dialogueAction;
        [SerializeField] private GameObject _historyPanel;
        [SerializeField] private TMP_Text _historyText;
        [SerializeField] private UnityEngine.UI.ScrollRect _historyScroll;
        [SerializeField] private RectTransform _spotlight;
        [SerializeField] private RectTransform[] _shade;
        [SerializeField] private RectTransform _guideFrame;
        [SerializeField] private RectTransform _guideArrow;
        [SerializeField] private TMP_Text _guideText;
        [SerializeField] private CanvasGroup _recoveryFade;
        [SerializeField, Min(.1f)] private float _guideSeconds = 3.5f;
        [SerializeField, Min(.1f)] private float _recoverySeconds = .35f;
        [SerializeField, Min(.1f)] private float _leafGuideRange = 12f;
        [SerializeField] private Vector2 _guideHalfSize = new(.075f, .10f);
        [SerializeField] private string _emptyHistory = "아직 기록된 대사가 없습니다.";
        [SerializeField] private string _opening = "두영: 먼저 주변에서 약초를 찾아보자.";
        [SerializeField] private string _firstGather = "소담: 첫 약초를 찾았군요. 제게 건네주시면 약제를 만들 수 있어요.";
        [SerializeField] private string[] _sodamLeafFall =
        {
            "소담: 아… 제가 밟으니까 나뭇잎이 회수되는군요. 두영이 건너야겠어요.",
            "두영: 그럼 이번엔 내가 먼저 가볼게!"
        };
        [SerializeField] private string[] _duyeongLeafFall =
        {
            "두영: 어? 내가 밟으니까 나뭇잎이 사라졌어!",
            "소담: 나뭇잎을 사용하는 사람이 밟으면 회수되는 모양이에요. 이번에는 제가 건너볼게요."
        };
        [SerializeField] private string _leafGuide = "수액 흔적에는 나뭇잎을 던질 수 있습니다.";
        [SerializeField] private string _sapGuide = "나무 홈에는 수액을 주입할 수 있습니다.";

        private static readonly List<string> History = new();
        private readonly Queue<string> _dialogue = new();
        private readonly uint[] _seenRecovery = new uint[2];
        private InputAction _historyAction;
        private bool _openingSeen, _harvestSeen, _leafFallSeen;
        private int _abilityGuides;
        private float _dialogueRemaining, _guideRemaining, _fadeRemaining;
        private Vector3 _guidePoint;
        private Camera _guideCamera;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetHistory() => History.Clear();

        private void Awake()
        {
            if (StageOneFlow.Instance != null) History.Clear();
        }

        private void OnEnable()
        {
            _historyAction = _dialogueAction.action.Clone();
            _historyAction.performed += _ => ToggleHistory();
            _historyAction.Enable();
        }

        private void OnDisable()
        {
            CloseHistory();
            _historyAction?.Dispose();
            _historyAction = null;
        }

        public void RecordDialogue(string text)
        {
            History.Add(text);
            if (_historyPanel.activeSelf) UpdateHistory();
        }

        public bool CloseHistory()
        {
            if (!_historyPanel.activeSelf) return false;
            _historyPanel.SetActive(false);
            if (GameplayCursor.Instance != null)
            {
                GameplayCursor.Instance.SetUiMode(this, false);
                if (!GameplayCursor.WorldPointerActive) GameplayCursor.Instance.Capture();
            }
            return true;
        }

        private void ToggleHistory()
        {
            if (CloseHistory() || GameplayPause.IsPaused || !GameplayCursor.HasGameplayFocus) return;
            _historyPanel.SetActive(true);
            GameplayCursor.Instance?.SetUiMode(this, true);
            UpdateHistory();
        }

        private void UpdateHistory()
        {
            _historyText.text = History.Count == 0 ? _emptyHistory : string.Join("\n\n", History);
            Canvas.ForceUpdateCanvases();
            _historyScroll.verticalNormalizedPosition = 0f;
        }

        private void LateUpdate()
        {
            if (GameplayPause.IsPaused || _historyPanel.activeSelf)
            {
                _spotlight.gameObject.SetActive(false);
                return;
            }
            StageActor local = null;
            foreach (var actor in StageActor.All)
                if (actor.Local && actor.Slot is >= 0 and < 2) { local = actor; break; }
            if (local == null) return;
            float dt = Time.unscaledDeltaTime;
            var flow = StageOneFlow.Instance;
            if (flow != null) UpdateStageOne(local, flow, dt);
            _guideRemaining -= dt;
            _spotlight.gameObject.SetActive(_guideRemaining > 0 && PresentSpotlight());
            _dialogueRemaining -= dt;
            if (_dialogue.Count > 0 && _dialogueRemaining <= 0 && _fadeRemaining <= 0 &&
                (flow == null || flow.Minigame == StageMinigameKind.None))
            {
                _dialogueRemaining = _overlay.settings.feedbackDuration;
                _overlay.ShowSubtitle(_dialogue.Dequeue(), _dialogueRemaining);
            }
        }

        private void UpdateStageOne(StageActor local, StageOneFlow flow, float dt)
        {
            if (!_openingSeen) { _openingSeen = true; _dialogue.Enqueue(_opening); }
            if (!_harvestSeen && flow.Progress.Harvested != 0)
            { _harvestSeen = true; _dialogue.Enqueue(_firstGather); }
            if (!_leafFallSeen && flow.FirstLeafFallSlot >= 0)
            {
                _leafFallSeen = true;
                var role = flow.settings.slotRoles[flow.FirstLeafFallSlot];
                foreach (string line in role == CharacterRole.Sodam ? _sodamLeafFall : _duyeongLeafFall)
                    _dialogue.Enqueue(line);
            }
            uint sequence = flow.RecoverySequence(local.Slot);
            if (_seenRecovery[local.Slot] != sequence)
            {
                _seenRecovery[local.Slot] = sequence;
                _fadeRemaining = _recoverySeconds;
            }
            _fadeRemaining = Mathf.Max(0f, _fadeRemaining - dt);
            _recoveryFade.alpha = _fadeRemaining / _recoverySeconds;
            _recoveryFade.gameObject.SetActive(_fadeRemaining > 0f);
            if (!local.Abilities.Unlocked || flow.Minigame != StageMinigameKind.None || _guideRemaining > 0) return;
            bool leaf = local.Abilities.Kind == PlayerAbilityKind.Leaf;
            int bit = leaf ? 1 : 2;
            if ((_abilityGuides & bit) != 0) return;
            Vector3 point;
            bool found = leaf ? StageOneLeafZone.TryNearbyGuidePoint(local.transform.position, _leafGuideRange, out point) ||
                ExteriorJumpPad.TryLeafInstallHint(local, out point) : SapInjectionPort.TryInteractionHint(local, out point);
            if (found && StartGuide(local.Camera, point, leaf ? _leafGuide : _sapGuide))
                _abilityGuides |= bit;
        }

        private bool StartGuide(Camera camera, Vector3 point, string message)
        {
            _guideCamera = camera;
            _guidePoint = point;
            if (!PresentSpotlight()) return false;
            _guideText.text = message;
            _guideRemaining = _guideSeconds;
            return true;
        }

        private bool PresentSpotlight()
        {
            if (_guideCamera == null) return false;
            Rect viewport = _guideCamera.rect;
            if (viewport.width <= 0 || viewport.height <= 0) return false;
            var point = _guideCamera.WorldToViewportPoint(_guidePoint);
            if (point.z <= 0) return false;
            Vector2 center = point;
            Vector2 half = _guideHalfSize;
            if (center.x < 0 || center.x > 1 || center.y < 0 || center.y > 1) return false;
            Vector2 low = Vector2.Max(Vector2.zero, center - half);
            Vector2 high = Vector2.Min(Vector2.one, center + half);
            Fit(_spotlight, viewport.min, viewport.max);
            Fit(_shade[0], Vector2.zero, new(1, low.y));
            Fit(_shade[1], new(0, high.y), Vector2.one);
            Fit(_shade[2], new(0, low.y), new(low.x, high.y));
            Fit(_shade[3], new(high.x, low.y), new(1, high.y));
            Fit(_guideFrame, low, high);
            float arrowY = Mathf.Min(.93f, high.y + .025f);
            Fit(_guideArrow, new(center.x - .03f, arrowY), new(center.x + .03f, arrowY + .05f));
            return true;
        }

        private static void Fit(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
