using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private UnityEngine.UI.RawImage _spotlightOverlay;
        [SerializeField] private Material _worldRingMaterial;
        [SerializeField] private SpotlightWorldRingSetup _worldRingSetup;
        [SerializeField] private RectTransform _guideArrow;
        [SerializeField] private RectTransform _guideConnector;
        [SerializeField] private TMP_Text _guideText;
        [SerializeField] private Vector2 _captionMinimumSize = new(260, 76);
        [SerializeField] private Vector2 _captionMaximumSize = new(560, 180);
        [SerializeField] private Vector2 _captionPadding = new(88, 32);
        [SerializeField, Min(0)] private float _captionTargetGap = 24f;
        [SerializeField] private CanvasGroup _recoveryFade;
        [SerializeField, Min(.1f)] private float _guideSeconds = 3.5f;
        [SerializeField, Min(.1f)] private float _recoverySeconds = .35f;
        [SerializeField, Min(.1f)] private float _leafGuideRange = 12f;
        [SerializeField, Min(.1f)] private float _sapGuideRange = 12f;
        [SerializeField, Min(.1f)] private float _guideRingRadius = 1.2f;
        [SerializeField, Min(0f)] private float _guideRingCameraOffset = .2f;
        [SerializeField] private Vector2 _guideMinimumRadii = new(.065f, .085f);
        [SerializeField] private Vector2 _guideMaximumRadii = new(.24f, .216f);
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
        [SerializeField] private string _sapOnlyBranchGuide = "가지에 붙인 나뭇잎은 수액 능력자만 탈 수 있어요.";

        private static readonly List<string> History = new();
        private readonly Queue<string> _dialogue = new();
        private readonly uint[] _seenRecovery = new uint[2];
        private InputAction _historyAction;
        private bool _openingSeen, _harvestSeen, _leafFallSeen;
        private bool _sapOnlyBranchGuideSeen;
        private int _abilityGuides;
        private float _dialogueRemaining, _guideRemaining, _fadeRemaining;
        private Vector3 _guidePoint;
        private Camera _guideCamera;
        private LeafInstallTarget _guideTarget;
        private bool _guideAllowOffscreen;
        private Material _spotlightMaterial;
        private SpotlightWorldRing _worldRing;
        private Transform[] _sapMarkers;
        private Vector2 _captionSize;
        private Vector2 _captionViewport;
        private string _captionCopy;
        public bool NarrativeControlled { get; set; }
        public bool GuideActive => _guideRemaining > 0f;
        public void QueueDialogue(string text) { if (!string.IsNullOrEmpty(text)) _dialogue.Enqueue(text); }
        public bool ShowGuide(Camera camera, Vector3 point, string message, LeafInstallTarget target = null)
        {
            // Narrative tutorials are acknowledged only when their target is visible.
            if (!StartGuide(camera, point, message, false, target)) return false;
            return true;
        }
        public void HideGuide() => _guideRemaining = 0f;

        private static readonly int CenterId = Shader.PropertyToID("_Center");
        private static readonly int RadiiId = Shader.PropertyToID("_Radii");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetHistory() => History.Clear();

        private void Awake()
        {
            if (StageOneFlow.Instance != null) History.Clear();
            _sapMarkers = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(t => t.name.StartsWith("CuredBlueSap")).ToArray();
        }

        internal bool TryLeafGuidePoint(Vector3 origin, float range, LeafInstallTarget fallback, out Vector3 point)
        {
            point = default;
            float nearest = range * range;
            bool found = false;
            foreach (var marker in _sapMarkers)
            {
                if (marker == null || !marker.gameObject.activeInHierarchy) continue;
                float distance = (marker.position - origin).sqrMagnitude;
                if (distance > nearest) continue;
                nearest = distance; point = marker.position; found = true;
            }
            if (found) return true;
            if (fallback != null) { point = fallback.HasSocket ? fallback.SocketPosition : fallback.transform.position; return true; }
            return StageOneLeafZone.TryNearbyGuidePoint(origin, range, out point);
        }

        private void OnEnable()
        {
            _historyPanel.SetActive(false);
            _historyAction = _dialogueAction.action.Clone();
            _historyAction.performed += _ => ToggleHistory();
            _historyAction.Enable();
        }

        private void OnDisable()
        {
            _spotlight.gameObject.SetActive(false);
            CloseHistory();
            _historyAction?.Dispose();
            _historyAction = null;
        }

        private void OnDestroy()
        {
            _worldRing?.Dispose();
            if (_spotlightMaterial != null) Destroy(_spotlightMaterial);
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
            if (local == null)
            {
                _spotlight.gameObject.SetActive(false);
                return;
            }
            float dt = Time.unscaledDeltaTime;
            var flow = StageOneFlow.Instance;
            if (flow != null) UpdateStageOne(local, flow, dt);
            bool guideVisible = _guideRemaining > 0 && PresentSpotlight();
            _spotlight.gameObject.SetActive(guideVisible);
            if (guideVisible) _guideRemaining = Mathf.Max(0f, _guideRemaining - dt);
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
            if (!NarrativeControlled && !_openingSeen) { _openingSeen = true; _dialogue.Enqueue(_opening); }
            if (!NarrativeControlled && !_harvestSeen && flow.Progress.Harvested != 0)
            { _harvestSeen = true; _dialogue.Enqueue(_firstGather); }
            if (!NarrativeControlled && !_leafFallSeen && flow.FirstLeafFallSlot >= 0)
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
            if (NarrativeControlled) return;
            if (!local.Abilities.Unlocked || flow.Minigame != StageMinigameKind.None) return;
            if (_guideRemaining > 0) return;
            var branch = !_sapOnlyBranchGuideSeen ? FindSapOnlyBranchGuideTarget(local, flow) : null;
            if (branch != null && StartGuide(local.Camera, BranchGuidePoint(branch), _sapOnlyBranchGuide, true, branch))
            {
                _sapOnlyBranchGuideSeen = true;
                return;
            }
            bool leaf = local.Abilities.Kind == PlayerAbilityKind.Leaf;
            int bit = leaf ? 1 : 2;
            if ((_abilityGuides & bit) != 0) return;
            Vector3 point;
            bool found = leaf ? TryLeafGuidePoint(local.transform.position, _leafGuideRange, null, out point) ||
                ExteriorJumpPad.TryLeafInstallHint(local, out point) :
                TrySapGuidePoint(local, flow.FirstSapGuidePort, out point);
            if (found && StartGuide(local.Camera, point, leaf ? _leafGuide : _sapGuide, !leaf))
                _abilityGuides |= bit;
        }

        private LeafInstallTarget FindSapOnlyBranchGuideTarget(StageActor local, StageOneFlow flow)
        {
            float nearest = _leafGuideRange * _leafGuideRange;
            LeafInstallTarget closest = null;
            foreach (var target in flow.SapOnlyBranchGuideTargets)
            {
                if (target == null || !target.isActiveAndEnabled) continue;
                float distance = (local.transform.position - BranchGuidePoint(target)).sqrMagnitude;
                if (distance > nearest) continue;
                nearest = distance;
                closest = target;
            }
            return closest;
        }

        private static Vector3 BranchGuidePoint(LeafInstallTarget target) =>
            target.HasLeaf ? target.SocketPosition : target.transform.position;

        private bool TrySapGuidePoint(StageActor actor, SapInjectionPort port, out Vector3 point)
        {
            if (port == null) return SapInjectionPort.TryInteractionHint(actor, out point);
            point = port.InteractionPoint;
            return port.isActiveAndEnabled && (actor.transform.position - point).sqrMagnitude <= _sapGuideRange * _sapGuideRange;
        }

        private bool StartGuide(Camera camera, Vector3 point, string message, bool allowOffscreen,
            LeafInstallTarget target = null)
        {
            if (camera == null || !camera.isActiveAndEnabled) return false;
            if (!allowOffscreen && !IsVisible(camera.WorldToViewportPoint(point))) return false;
            _guideCamera = camera;
            _guidePoint = point;
            _guideTarget = target;
            _guideAllowOffscreen = allowOffscreen;
            _guideText.text = message;
            _guideRemaining = _guideSeconds;
            return true;
        }

        private bool PresentSpotlight()
        {
            if (_guideCamera == null || !_guideCamera.isActiveAndEnabled) return false;
            Rect viewport = _guideCamera.rect;
            if (viewport.width <= 0 || viewport.height <= 0) return false;
            Vector3 target = _guideTarget != null ? BranchGuidePoint(_guideTarget) : _guidePoint;
            Vector3 targetProjection = _guideCamera.WorldToViewportPoint(target);
            bool visible = IsVisible(targetProjection);
            if (!visible && !_guideAllowOffscreen) return false;
            Fit(_spotlight, viewport.min, viewport.max);
            _spotlightOverlay.gameObject.SetActive(visible);
            if (_guideArrow != null) _guideArrow.gameObject.SetActive(visible);
            _guideText.transform.parent.gameObject.SetActive(visible);
            if (_guideConnector != null) _guideConnector.gameObject.SetActive(visible);
            if (!visible) return true;

            var cameraTransform = _guideCamera.transform;
            float radius = _worldRingSetup != null ? _worldRingSetup.radius : _guideRingRadius;
            float offset = _worldRingSetup != null ? _worldRingSetup.cameraOffset : _guideRingCameraOffset;
            // Bring the marker slightly forward so it clears the surface it is pointing at.
            Vector3 ringCenter = Vector3.MoveTowards(target, cameraTransform.position,
                Mathf.Min(offset, targetProjection.z * .12f));
            Vector2 center = _guideCamera.WorldToViewportPoint(ringCenter);
            Vector2 right = _guideCamera.WorldToViewportPoint(ringCenter + cameraTransform.right * radius);
            Vector2 up = _guideCamera.WorldToViewportPoint(ringCenter + cameraTransform.up * radius);
            Vector2 opening = new Vector2(right.x - center.x, up.y - center.y) * 1.25f;
            float ringScale = Mathf.Min(1f,
                Mathf.Min(_guideMaximumRadii.x / Mathf.Max(.001f, opening.x),
                    _guideMaximumRadii.y / Mathf.Max(.001f, opening.y)));
            Vector2 radii = Vector2.Max(_guideMinimumRadii, opening * ringScale);

            if (_spotlightMaterial == null)
            {
                _spotlightMaterial = new Material(_spotlightOverlay.material);
                _spotlightOverlay.material = _spotlightMaterial;
            }
            _spotlightMaterial.SetVector(CenterId, new Vector4(center.x, center.y, 0, 0));
            _spotlightMaterial.SetVector(RadiiId, new Vector4(radii.x, radii.y, 0, 0));
            _worldRing ??= new SpotlightWorldRing();
            _worldRing.Configure(_worldRingSetup);
            _worldRing.Draw(_guideCamera, ringCenter, radius * ringScale, _worldRingMaterial);
            var captionRect = (RectTransform)_guideText.transform.parent;
            Vector2 viewportSize = _spotlight.rect.size;
            Vector2 captionSize = MeasureCaption(viewportSize);
            captionRect.anchorMin = captionRect.anchorMax = new Vector2(.5f, .5f);
            captionRect.pivot = new Vector2(.5f, .5f);
            captionRect.sizeDelta = captionSize;
            Vector2 halfCaption = captionSize / viewportSize * .5f;
            Vector2 captionCenter = new(
                Mathf.Clamp(center.x, .03f + halfCaption.x, .97f - halfCaption.x),
                Mathf.Clamp(center.y + radii.y + halfCaption.y + .03f,
                    .03f + halfCaption.y, .97f - halfCaption.y));
            float gap = _captionTargetGap / Mathf.Max(1f, viewportSize.x);
            float left = center.x - radii.x - gap - halfCaption.x;
            float rightSide = center.x + radii.x + gap + halfCaption.x;
            // Prefer a callout beside the target when there is enough viewport room.
            if (left - halfCaption.x >= .03f) captionCenter.x = left;
            else if (rightSide + halfCaption.x <= .97f) captionCenter.x = rightSide;
            captionRect.anchoredPosition = Vector2.Scale(captionCenter - Vector2.one * .5f, viewportSize);
            if (_guideConnector != null)
            {
                var caption = (RectTransform)_guideText.transform.parent;
                Vector2 towardTarget = center - captionCenter;
                float edgeFraction = Mathf.Min(halfCaption.x / Mathf.Max(.0001f, Mathf.Abs(towardTarget.x)),
                    halfCaption.y / Mathf.Max(.0001f, Mathf.Abs(towardTarget.y)));
                Vector2 from = captionCenter + towardTarget * edgeFraction;
                Vector2 towardText = (from - center).normalized;
                Vector2 end = center + Vector2.Scale(towardText, radii);
                Vector2 size = _spotlight.rect.size;
                Vector2 startPixels = Vector2.Scale(from, size);
                Vector2 delta = Vector2.Scale(end, size) - startPixels;
                _guideConnector.anchorMin = _guideConnector.anchorMax = Vector2.zero;
                _guideConnector.pivot = new Vector2(0f, .5f);
                _guideConnector.anchoredPosition = startPixels;
                _guideConnector.sizeDelta = new Vector2(delta.magnitude, 2f);
                _guideConnector.localRotation = Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
            }

            float arrowY = Mathf.Min(.93f, center.y + radii.y + .025f);
            float arrowX = Mathf.Clamp(center.x, .03f, .97f);
            if (_guideArrow != null) Fit(_guideArrow, new(arrowX - .03f, arrowY), new(arrowX + .03f, arrowY + .05f));
            return true;
        }

        private Vector2 MeasureCaption(Vector2 viewportSize)
        {
            if (_captionCopy == _guideText.text && _captionViewport == viewportSize) return _captionSize;
            Vector2 maximum = Vector2.Min(_captionMaximumSize, viewportSize * new Vector2(.8f, .3f));
            Vector2 minimum = Vector2.Min(_captionMinimumSize, maximum);
            Vector2 preferred = _guideText.GetPreferredValues(_guideText.text,
                Mathf.Max(1f, maximum.x - _captionPadding.x), Mathf.Infinity);
            _captionSize = new Vector2(Mathf.Clamp(preferred.x + _captionPadding.x, minimum.x, maximum.x),
                Mathf.Clamp(preferred.y + _captionPadding.y, minimum.y, maximum.y));
            _captionCopy = _guideText.text;
            _captionViewport = viewportSize;
            return _captionSize;
        }

        private static bool IsVisible(Vector3 point) =>
            point.z > 0f && point.x >= 0f && point.x <= 1f && point.y >= 0f && point.y <= 1f;

        private static void Fit(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
