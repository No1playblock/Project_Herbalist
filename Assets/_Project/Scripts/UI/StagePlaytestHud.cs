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
        [SerializeField] private UnityEngine.UI.RawImage _spotlightOverlay;
        [SerializeField] private Material _worldRingMaterial;
        [SerializeField] private RectTransform _guideArrow;
        [SerializeField] private TMP_Text _guideText;
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

        private static readonly int CenterId = Shader.PropertyToID("_Center");
        private static readonly int RadiiId = Shader.PropertyToID("_Radii");

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
            bool found = leaf ? StageOneLeafZone.TryNearbyGuidePoint(local.transform.position, _leafGuideRange, out point) ||
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
            _guideArrow.gameObject.SetActive(visible);
            if (!visible) return true;

            var cameraTransform = _guideCamera.transform;
            // Bring the marker slightly forward so it clears the surface it is pointing at.
            Vector3 ringCenter = Vector3.MoveTowards(target, cameraTransform.position,
                Mathf.Min(_guideRingCameraOffset, targetProjection.z * .12f));
            Vector2 center = _guideCamera.WorldToViewportPoint(ringCenter);
            Vector2 right = _guideCamera.WorldToViewportPoint(ringCenter + cameraTransform.right * _guideRingRadius);
            Vector2 up = _guideCamera.WorldToViewportPoint(ringCenter + cameraTransform.up * _guideRingRadius);
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
            _worldRing.Draw(_guideCamera, ringCenter, _guideRingRadius * ringScale, _worldRingMaterial);

            float arrowY = Mathf.Min(.93f, center.y + radii.y + .025f);
            float arrowX = Mathf.Clamp(center.x, .03f, .97f);
            Fit(_guideArrow, new(arrowX - .03f, arrowY), new(arrowX + .03f, arrowY + .05f));
            return true;
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
