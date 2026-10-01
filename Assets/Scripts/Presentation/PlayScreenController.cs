using System;
using System.Collections.Generic;
using Herbalist.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Herbalist.Presentation
{
    public enum PlayScreenMode { Split, Personal }
    public enum CharacterRole { Duyeong, Sodam }

    [Serializable]
    public sealed class CharacterScreenRegion
    {
        public CharacterRole role;
        public int spawnSlot;
        public Rect viewport;
        public RectTransform hudRoot;
        [NonSerialized] public PlayerView view;
        [NonSerialized] public bool local;
    }

    public sealed class PlayScreenController : MonoBehaviour
    {
        [SerializeField] private InputActionReference toggleAction;
        [SerializeField] private PlayScreenMode initialMode = PlayScreenMode.Split;
        [SerializeField] private Rect personalViewport = new Rect(0, 0, 1, 1);
        [SerializeField] private CharacterScreenRegion[] regions;
        [SerializeField] private GameObject divider;
        [SerializeField] private PlayerController offlinePlayer;
        [SerializeField] private int offlineSlot;
        private InputAction toggle;
        private PlayScreenMode preferredMode;
        private readonly List<KeyValuePair<object, PlayScreenMode>> overrides = new();
        public PlayScreenMode Mode => overrides.Count > 0 ? overrides[overrides.Count - 1].Value : preferredMode;
        public bool IsLocked => overrides.Count > 0;
        public bool IsSplitVisible { get; private set; }

        private void Awake() { preferredMode = initialMode; }
        private void OnEnable()
        {
            if (toggleAction != null)
            {
                toggle = toggleAction.action.Clone();
                toggle.performed += OnToggle;
                toggle.Enable();
            }
            Apply();
        }
        private void Start()
        {
            if (offlinePlayer != null && offlinePlayer.isActiveAndEnabled)
                Register(offlinePlayer.View, offlineSlot, true);
        }
        private void OnToggle(InputAction.CallbackContext context) => TryToggle();
        public bool TryToggle()
        {
            if (IsLocked) return false;
            preferredMode = preferredMode == PlayScreenMode.Split ? PlayScreenMode.Personal : PlayScreenMode.Split;
            Apply();
            return true;
        }
        // Each level/sequence owns its override; releasing one must not unlock another.
        public void SetOverride(object owner, PlayScreenMode? mode)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            overrides.RemoveAll(entry => ReferenceEquals(entry.Key, owner));
            if (mode.HasValue) overrides.Add(new KeyValuePair<object, PlayScreenMode>(owner, mode.Value));
            Apply();
        }
        public PlayerView ViewForSlot(int slot)
        {
            foreach (var region in regions) if (region.spawnSlot == slot) return region.view;
            return null;
        }
        public void Register(PlayerView view, int slot, bool local)
        {
            foreach (var region in regions)
            {
                if (region.spawnSlot != slot) continue;
                if (region.view != null && region.view != view)
                    region.view.SetPresentation(false, false, personalViewport);
                region.view = view;
                region.local = local;
                Apply();
                return;
            }
            Debug.LogError("No screen region configured for player slot " + slot, this);
        }
        public void Unregister(PlayerView view)
        {
            foreach (var region in regions)
                if (region.view == view) { region.view = null; region.local = false; }
            if (view != null) view.SetPresentation(false, false, personalViewport);
            Apply();
        }
        private void Apply()
        {
            if (regions == null) return;
            int available = 0;
            foreach (var region in regions) if (region.view != null) available++;
            IsSplitVisible = Mode == PlayScreenMode.Split && available == regions.Length && available > 1;
            if (divider != null) divider.SetActive(IsSplitVisible);
            foreach (var region in regions)
            {
                bool visible = region.view != null && (IsSplitVisible || region.local);
                Rect rect = IsSplitVisible ? region.viewport : personalViewport;
                if (region.view != null)
                    region.view.SetPresentation(visible, region.local && isActiveAndEnabled, rect);
                if (region.hudRoot == null) continue;
                region.hudRoot.gameObject.SetActive(visible);
                region.hudRoot.anchorMin = rect.min;
                region.hudRoot.anchorMax = rect.max;
                region.hudRoot.offsetMin = region.hudRoot.offsetMax = Vector2.zero;
            }
        }
        private void OnDisable()
        {
            toggle?.Disable();
            toggle?.Dispose();
            toggle = null;
            if (divider != null) divider.SetActive(false);
            if (regions == null) return;
            foreach (var region in regions)
            {
                if (region.view != null) region.view.SetPresentation(region.local, region.local, personalViewport);
                if (region.hudRoot != null) region.hudRoot.gameObject.SetActive(false);
            }
        }
    }
}
