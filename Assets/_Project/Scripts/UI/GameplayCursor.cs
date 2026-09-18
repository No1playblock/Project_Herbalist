using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Herbalist.Presentation
{
    public sealed class GameplayCursor : MonoBehaviour
    {
        [SerializeField] private InputActionReference releaseAction;
        [SerializeField] private InputActionReference captureAction;
        [SerializeField] private bool captureOnStart = true;
        private InputAction release, capture;
        private bool captured;
        private bool suppressCapturePress;
        private int captureFrame;
        private readonly HashSet<object> uiOwners = new();
        public static GameplayCursor Instance { get; private set; }
        public static bool AllowsPointerInput => Instance == null || Instance.CanReadPointer;
        private bool CanReadPointer => captured && Cursor.lockState == CursorLockMode.Locked &&
            HasGameFocus && uiOwners.Count == 0 && Time.frameCount > captureFrame && !suppressCapturePress;
        private bool HasGameFocus
        {
            get
            {
                if (!Application.isFocused) return false;
#if UNITY_EDITOR
                var window = UnityEditor.EditorWindow.focusedWindow;
                if (window == null || window.GetType().Name != "GameView") return false;
#endif
                return true;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;
        private void OnEnable()
        {
            Instance = this;
            release = releaseAction.action.Clone(); capture = captureAction.action.Clone();
            release.performed += OnRelease; capture.performed += OnCapture;
            capture.canceled += _ => suppressCapturePress = false;
            release.Enable(); capture.Enable();
        }
        private void Start() { if (captureOnStart) Capture(); }
        private void OnRelease(InputAction.CallbackContext context) => Release();
        private void OnCapture(InputAction.CallbackContext context)
        {
            if (captured && Cursor.lockState == CursorLockMode.Locked) return;
            // A click in another window must never capture the cursor.
            if (context.control.device is Mouse mouse)
            {
                var point = mouse.position.ReadValue();
                if (point.x < 0 || point.y < 0 || point.x >= Screen.width || point.y >= Screen.height) return;
            }
            Capture();
            suppressCapturePress = captured;
        }
        public void Capture()
        {
            if (!HasGameFocus || uiOwners.Count > 0) return;
            captured = true; captureFrame = Time.frameCount;
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        }
        public void Release()
        { captured = false; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        // Menus release only their own block; closing one menu must not close another.
        public void SetUiMode(object owner, bool open)
        {
            if (owner == null) return;
            if (open) { uiOwners.Add(owner); Release(); }
            else uiOwners.Remove(owner);
        }
        private void Update()
        {
            if (captured && (!HasGameFocus || Cursor.lockState != CursorLockMode.Locked)) Release();
        }
        private void OnApplicationFocus(bool focus) { if (!focus) Release(); }
        private void OnDisable()
        {
            release?.Dispose(); capture?.Dispose(); release = capture = null;
            if (Instance == this) { Release(); Instance = null; }
        }
    }
}
