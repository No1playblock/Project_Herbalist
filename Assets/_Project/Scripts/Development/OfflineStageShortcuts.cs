using System;
using Herbalist.GameUI;
using Herbalist.Networking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Herbalist.Development
{
    // Removable, scene-authored solo test adapter. Bindings and destinations live in the scene.
    public sealed class OfflineStageShortcuts : MonoBehaviour
    {
        [Serializable]
        public sealed class Shortcut
        {
            public InputAction input;
            public string scenePath;
        }

        [SerializeField] private Shortcut[] _shortcuts = Array.Empty<Shortcut>();
        private bool _loading;

        private void OnEnable()
        {
            foreach (var shortcut in _shortcuts) shortcut.input?.Enable();
        }

        private void Update()
        {
            if (_loading || FusionLobbySession.Instance is { HasNetworkSession: true }) return;
            for (int i = 0; i < _shortcuts.Length; i++)
                if (_shortcuts[i].input != null && _shortcuts[i].input.WasPressedThisFrame() && TryStartStage(i)) return;
        }

        public bool TryStartStage(int index)
        {
            if (!isActiveAndEnabled || _loading || index < 0 || index >= _shortcuts.Length ||
                FusionLobbySession.Instance is { HasNetworkSession: true }) return false;
            string path = _shortcuts[index].scenePath;
            if (string.IsNullOrEmpty(path) || !Application.CanStreamedLevelBeLoaded(path)) return false;
            _loading = true;
            GameplayPause.SetOffline(false);
            // A direct load gives the selected stage its normal authored starting state.
            SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
            return true;
        }

        private void OnDisable()
        {
            foreach (var shortcut in _shortcuts) shortcut.input?.Disable();
        }

        private void OnDestroy()
        {
            foreach (var shortcut in _shortcuts) shortcut.input?.Dispose();
        }
    }
}
