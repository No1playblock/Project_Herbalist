using Herbalist.GameUI;
using Herbalist.Player;
using Herbalist.Presentation;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Levels
{
    public sealed class BranchRidePresentation : MonoBehaviour
    {
        [SerializeField] private BranchRideFlow _flow;
        [SerializeField] private GameOverlayHud _hud;
        [SerializeField] private Transform _firstTarget, _firstStone;
        [SerializeField] private PlayScreenController _screen;
        [SerializeField] private Transform _endingShot, _endingShotEnd;
        [SerializeField] private UnityEngine.UI.RawImage _branchPreview;
        [SerializeField] private Camera[] _branchPreviewCameras = System.Array.Empty<Camera>();
        [SerializeField, Min(.1f)] private float _branchPreviewSeconds = 1f;
        [SerializeField] private bool _narrativeControlled;
        private int _previewMask;
        private float _previewRemaining;
        private int _previewIndex = -1;
        [SerializeField, Min(1)] private float _noticeRange = 16;
        [SerializeField, Min(.1f)] private float _subtitleDuration = 4;
        [SerializeField] private bool _frameRideCamera;
        [SerializeField, Range(0, 45)] private float _rideCameraPitchOffset = 12f;
        [SerializeField, Range(0, 85)] private float _rideCameraMaxPitch = 38f;
        [SerializeField, Min(.01f)] private float _rideCameraResponseSeconds = .22f;
        [SerializeField] private string _start = "부적을 타고 가지를 따라 이동하세요.";
        [SerializeField] private string _target = "소담: 과녁이 있어요. 왜 있는 걸까요?";
        [SerializeField] private string _turned = "두영: 가지가 움직였어. 과녁으로 길을 바꿀 수 있나 봐.";
        [SerializeField] private string _stone = "두영: 잠깐 멈출 수 없을까?  소담: 돌이 지나간 뒤에 움직이는 게 좋겠어요.";
        private int _seen, _failures;
        private float _remaining;
        private PlayerController _rideCameraPlayer;
        private bool _manualRideLook;
        private bool _showingEnding;
        private Vector3 _endingStartPosition;
        private Quaternion _endingStartRotation;

        private void Awake()
        {
            if (_endingShot == null) return;
            _endingStartPosition = _endingShot.position;
            _endingStartRotation = _endingShot.rotation;
        }

        private void Update()
        {
            if (_flow == null || GameplayPause.IsPaused) return;
            UpdateBranchPreview();
            if (_flow.HasTerrainEnding &&
                (_flow.CurrentPhase is BranchRidePhase.Landing or BranchRidePhase.Ending or BranchRidePhase.Complete))
            {
                PresentEnding();
                return;
            }
            ClearEnding();
            if (_frameRideCamera) UpdateRideCamera(Time.deltaTime);
            if (_narrativeControlled) return;
            if (_hud == null) return;
            _remaining = Mathf.Max(0, _remaining - Time.deltaTime);
            if (_failures != _flow.FailureCount) { _failures = _flow.FailureCount; _seen = 0; }
            if (_flow.CurrentPhase != BranchRidePhase.Riding) return;
            if (_remaining > 0) return;
            if ((_seen & 1) == 0) Show(1, _start);
            else if ((_seen & 2) == 0 && _firstTarget != null && Vector3.Distance(_flow.Position, _firstTarget.position) < _noticeRange) Show(2, _target);
            else if ((_seen & 4) == 0 && (_flow.PresentationBranchMask & 1) != 0) Show(4, _turned);
            else if ((_seen & 8) == 0 && _firstStone != null && Vector3.Distance(_flow.Position, _firstStone.position) < _noticeRange) Show(8, _stone);
        }

        private void UpdateBranchPreview()
        {
            int changed = _previewMask ^ _flow.PresentationBranchMask;
            _previewMask = _flow.PresentationBranchMask;
            if (_flow.CurrentPhase == BranchRidePhase.Riding && changed != 0)
                for (int i = 0; i < _branchPreviewCameras.Length; i++)
                    if ((changed & (1 << i)) != 0)
                    {
                        _previewIndex = i;
                        _previewRemaining = _branchPreviewSeconds;
                        break;
                    }
            bool show = _previewRemaining > 0 && _previewIndex >= 0 &&
                _flow.CurrentPhase == BranchRidePhase.Riding &&
                _screen != null && _screen.Mode == PlayScreenMode.Personal;
            _previewRemaining = Mathf.Max(0f, _previewRemaining - Time.deltaTime);
            if (_branchPreview != null) _branchPreview.gameObject.SetActive(show);
            for (int i = 0; i < _branchPreviewCameras.Length; i++)
                if (_branchPreviewCameras[i] != null) _branchPreviewCameras[i].enabled = show && i == _previewIndex;
        }

        private void UpdateRideCamera(float dt)
        {
            PlayerController player = null;
            if (_flow.CurrentPhase is BranchRidePhase.Boarding or BranchRidePhase.Riding)
            {
                foreach (var actor in StageActor.All)
                {
                    if (!BranchRideFlow.IsRiding(actor) || !actor.TryGetComponent(out PlayerController controller) ||
                        !controller.LocallyControlled) continue;
                    player = controller;
                    break;
                }
            }
            if (player != _rideCameraPlayer)
            {
                _rideCameraPlayer = player;
                _manualRideLook = false;
            }
            if (player == null) return;
            if (player.Input.Look.sqrMagnitude > .001f) _manualRideLook = true;
            if (_manualRideLook) return;

            // Write the normal view angles so the camera and the host's aim ray use the same direction.
            Vector3 forward = _flow.Forward;
            float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            float planarLength = new Vector2(forward.x, forward.z).magnitude;
            float slope = Mathf.Atan2(-forward.y, planarLength) * Mathf.Rad2Deg;
            float pitch = Mathf.Min(Mathf.Max(0f, slope) + _rideCameraPitchOffset, _rideCameraMaxPitch);
            float blend = 1f - Mathf.Exp(-dt / _rideCameraResponseSeconds);
            var view = player.View;
            view.SetLookAngles(Mathf.LerpAngle(view.Yaw, yaw, blend), Mathf.Lerp(view.Pitch, pitch, blend));
        }

        private void PresentEnding()
        {
            if (_screen == null || _endingShot == null || _endingShotEnd == null) return;
            if (!_showingEnding)
            {
                _showingEnding = true;
                _screen.SetOverride(this, PlayScreenMode.Personal);
            }
            float t = Mathf.SmoothStep(0f, 1f, _flow.EndingProgress);
            _endingShot.SetPositionAndRotation(Vector3.Lerp(_endingStartPosition, _endingShotEnd.position, t),
                Quaternion.Slerp(_endingStartRotation, _endingShotEnd.rotation, t));
            for (int slot = 0; slot < 2; slot++)
                _screen.ViewForSlot(slot)?.SetCameraOverride(this, _endingShot);
        }

        private void OnDisable()
        {
            if (_branchPreview != null) _branchPreview.gameObject.SetActive(false);
            foreach (var camera in _branchPreviewCameras) if (camera != null) camera.enabled = false;
            _rideCameraPlayer = null;
            _manualRideLook = false;
            ClearEnding();
        }

        private void ClearEnding()
        {
            if (!_showingEnding) return;
            _showingEnding = false;
            if (_screen != null)
            {
                for (int slot = 0; slot < 2; slot++)
                    _screen.ViewForSlot(slot)?.ClearCameraOverride(this);
                _screen.SetOverride(this, null);
            }
            if (_endingShot != null) _endingShot.SetPositionAndRotation(_endingStartPosition, _endingStartRotation);
        }
        private void Show(int bit, string text) { _seen |= bit; _remaining = _subtitleDuration; _hud.ShowSubtitle(text, _subtitleDuration); }
    }
}
