using Herbalist.Player;
using Herbalist.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    [DefaultExecutionOrder(200)]
    public sealed class CharacterSelectHover : MonoBehaviour
    {
        [SerializeField] private string playerOneName = "Player";
        [SerializeField] private string playerTwoName = "Player_Sodam";
        [SerializeField, Min(1f)] private float hoverScaleMultiplier = 1.04f;
        [SerializeField, Min(0.01f)] private float transitionSpeed = 10f;
        [SerializeField] private LayerMask raycastMask = ~0;
        [SerializeField, Min(1f)] private float maxRayDistance = 10000f;
        [SerializeField, Range(0f, 20f)] private float headLiftDegrees = 10f;
        [SerializeField, Min(0f)] private float deselectHoverPause = 1f;
        [SerializeField, Min(0.05f)] private float hoverShineDuration = 0.7f;
        [SerializeField, Range(-45f, 45f)] private float hoverShineAngle = -24f;
        [SerializeField, Range(0f, 1f)] private float hoverShineOpacity = 0.68f;
        [SerializeField, Min(1f)] private float hoverShineWidth = 42f;
        [SerializeField, Range(1f, 1.2f)] private float startButtonClickScale = 1.06f;
        [SerializeField, Min(0.05f)] private float startButtonClickDuration = 0.24f;
        [SerializeField, Range(0f, 0.12f)] private float startButtonPlateLeadTime = 0.05f;
        [SerializeField] private Sprite selectedPlateSprite;

        private readonly GameObject[] nameplateRoots = new GameObject[2];
        private readonly Image[] nameplateImages = new Image[2];
        private readonly Image[] selectedPlateImages = new Image[2];
        private readonly Sprite[] normalPlateSprites = new Sprite[2];
        private readonly TMP_Text[] nameLabels = new TMP_Text[2];
        private readonly Material[] normalNameMaterials = new Material[2];
        private readonly Material[] selectedOutlineMaterials = new Material[2];
        private readonly HoverShine[] hoverShines = new HoverShine[3];
        private int selectedIndex = -1;
        private int hoverSuppressedIndex = -1;
        private float hoverSuppressedUntil;
        private int previousHoveredIndex = -1;
        private bool previousStartButtonHovered;
        private RectTransform startButtonRectTransform;
        private RectTransform startButtonPlateRectTransform;
        private RectTransform startButtonBackdropRectTransform;
        private RectTransform settingsGearRectTransform;
        private RectTransform backButtonRectTransform;
        private Vector3 startButtonBaseScale;
        private Vector3 startButtonPlateBaseScale;
        private Vector3 startButtonBackdropBaseScale;
        private Vector3 settingsGearBaseScale;
        private Vector3 backButtonBaseScale;
        private bool startButtonPulseActive;
        private float startButtonPulseElapsed;
        private bool settingsGearPulseActive;
        private float settingsGearPulseElapsed;
        private bool backButtonPulseActive;
        private float backButtonPulseElapsed;
        private Sprite hoverShineSprite;
        private Texture2D hoverShineTexture;

        private sealed class HoverShine
        {
            public Image Graphic;
            public Image MaskGraphic;
            public RectTransform Rect;
            public float Progress;
            public float TravelDistance;
            public bool IsPlaying;
        }

        private sealed class HoverTarget
        {
            public PlayerController Player;
            public PlayerCharacterPresentation Presentation;
            public Transform Visual;
            public Renderer[] Renderers;
            public Vector3 RestScale;
            public float Progress;
        }

        private readonly HoverTarget[] targets = new HoverTarget[2];
        private readonly RaycastHit[] raycastHits = new RaycastHit[128];
        private Camera sceneCamera;

        private void Start()
        {
            sceneCamera = ResolveSceneCamera();
            ResolveTargets();
            hoverShineSprite = CreateHoverShineSprite();
            ResolveNameUi();

            GameplayCursor cursor = GameplayCursor.Instance;
            if (cursor != null)
                cursor.Release();
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private Camera ResolveSceneCamera()
        {
            Camera taggedCamera = Camera.main;
            if (taggedCamera != null && taggedCamera.isActiveAndEnabled)
                return taggedCamera;

            Camera fallback = null;
            Camera[] cameras = Camera.allCameras;
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera candidate = cameras[i];
                if (candidate == null || !candidate.isActiveAndEnabled)
                    continue;

                if (candidate.name == "Main Camera")
                    return candidate;

                if (candidate.cameraType != CameraType.Game || candidate.targetTexture != null)
                    continue;

                if (fallback == null || candidate.depth > fallback.depth)
                    fallback = candidate;
            }

            return fallback;
        }

        private void ResolveTargets()
        {
            string playerOneKey = NormalizeName(playerOneName);
            string playerTwoKey = NormalizeName(playerTwoName);
            PlayerController[] players = FindObjectsByType<PlayerController>();

            for (int i = 0; i < players.Length; i++)
            {
                PlayerController player = players[i];
                if (player == null)
                    continue;

                string playerKey = NormalizeName(player.gameObject.name);
                if (targets[0] == null && playerKey == playerOneKey)
                    targets[0] = CreateTarget(player);
                else if (targets[1] == null && playerKey == playerTwoKey)
                    targets[1] = CreateTarget(player);
            }
        }

        private static string NormalizeName(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("(Clone)", string.Empty)
                    .Replace("_", string.Empty)
                    .Replace(" ", string.Empty)
                    .ToUpperInvariant();
        }

        private HoverTarget CreateTarget(PlayerController player)
        {
            PlayerCharacterPresentation presentation = player.GetComponent<PlayerCharacterPresentation>();
            Transform visual = ResolveVisual(player, presentation);

            return new HoverTarget
            {
                Player = player,
                Presentation = presentation,
                Visual = visual,
                Renderers = visual.GetComponentsInChildren<Renderer>(true),
                RestScale = visual.localScale
            };
        }

        private static Transform ResolveVisual(PlayerController player, PlayerCharacterPresentation presentation)
        {
            GameObject activeCharacterRoot = presentation != null ? presentation.ActiveCharacterRoot : null;
            if (activeCharacterRoot != null)
                return activeCharacterRoot.transform;

            Animator animator = presentation != null ? presentation.ActiveAnimator : null;
            if (animator == null)
                animator = player.GetComponentInChildren<Animator>(true);

            return animator != null ? animator.transform : player.transform;
        }

        private void RefreshVisual(HoverTarget target)
        {
            if (target == null || target.Player == null)
                return;

            Transform currentVisual = ResolveVisual(target.Player, target.Presentation);
            if (currentVisual == null || currentVisual == target.Visual)
                return;

            if (target.Visual != null)
                target.Visual.localScale = target.RestScale;

            target.Visual = currentVisual;
            target.Renderers = currentVisual.GetComponentsInChildren<Renderer>(true);
            target.RestScale = currentVisual.localScale;
        }

        private void LateUpdate()
        {
            if (headLiftDegrees <= 0f)
                return;

            for (int i = 0; i < targets.Length; i++)
            {
                HoverTarget target = targets[i];
                Animator animator = target != null && target.Presentation != null
                    ? target.Presentation.ActiveAnimator
                    : null;
                if (animator == null || !animator.isActiveAndEnabled)
                    continue;

                Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (head == null)
                    continue;

                head.rotation = Quaternion.AngleAxis(
                    -headLiftDegrees,
                    animator.transform.right) * head.rotation;
            }
        }

        private void Update()
        {
            if (sceneCamera == null)
                sceneCamera = ResolveSceneCamera();

            for (int i = 0; i < targets.Length; i++)
                RefreshVisual(targets[i]);

            bool startButtonHovered = IsPointerOverStartButton();
            bool settingsGearHovered = IsPointerOverSettingsGear();
            bool backButtonHovered = IsPointerOverBackButton();
            int hoveredIndex = startButtonHovered || settingsGearHovered || backButtonHovered
                ? -1
                : FindHoveredTarget();
            Mouse mouse = Mouse.current;
            if (mouse != null && Application.isFocused &&
                mouse.leftButton.wasPressedThisFrame && hoveredIndex >= 0)
            {
                if (selectedIndex == hoveredIndex)
                {
                    selectedIndex = -1;
                    hoverSuppressedIndex = hoveredIndex;
                    hoverSuppressedUntil = Time.unscaledTime + deselectHoverPause;
                }
                else
                {
                    selectedIndex = hoveredIndex;
                    hoverSuppressedIndex = -1;
                    hoverSuppressedUntil = 0f;
                }
            }

            if (mouse != null && Application.isFocused &&
                mouse.leftButton.wasPressedThisFrame && startButtonHovered)
            {
                StartStartButtonClickPulse();
            }

            if (mouse != null && Application.isFocused &&
                mouse.leftButton.wasPressedThisFrame && settingsGearHovered)
            {
                StartSettingsGearClickPulse();
            }

            if (mouse != null && Application.isFocused &&
                mouse.leftButton.wasPressedThisFrame && backButtonHovered)
            {
                StartBackButtonClickPulse();
            }

            bool sameCharacterHoverPaused = hoveredIndex == hoverSuppressedIndex &&
                                            Time.unscaledTime < hoverSuppressedUntil;
            int visibleHoveredIndex = sameCharacterHoverPaused ? -1 : hoveredIndex;
            UpdateNameUi(visibleHoveredIndex);

            if (visibleHoveredIndex != previousHoveredIndex)
            {
                if (visibleHoveredIndex >= 0 && visibleHoveredIndex != selectedIndex)
                    StartHoverShine(visibleHoveredIndex);

                previousHoveredIndex = visibleHoveredIndex;
            }

            if (startButtonHovered && !previousStartButtonHovered)
                StartHoverShine(2);
            previousStartButtonHovered = startButtonHovered;

            UpdateHoverShines();
            UpdateStartButtonClickPulse();
            UpdateSettingsGearClickPulse();
            UpdateBackButtonClickPulse();

            for (int i = 0; i < targets.Length; i++)
            {
                HoverTarget target = targets[i];
                if (target == null || target.Visual == null)
                    continue;

                bool isHoveredOrSelected = i == visibleHoveredIndex || i == selectedIndex;
                float desired = isHoveredOrSelected ? 1f : 0f;
                target.Progress = Mathf.MoveTowards(
                    target.Progress,
                    desired,
                    Time.unscaledDeltaTime * transitionSpeed);

                float eased = Mathf.SmoothStep(0f, 1f, target.Progress);
                float scale = Mathf.Lerp(1f, hoverScaleMultiplier, eased);
                target.Visual.localScale = target.RestScale * scale;
            }
        }

        private void ResolveNameUi()
        {
            nameplateRoots[0] = FindChildObject("DuyeongNameplate (1)") ?? FindChildObject("DuyeongNameplate");
            nameplateRoots[1] = FindChildObject("SodamNameplate");
            nameplateImages[0] = FindChildComponent<Image>("DuyeongNameplate (1)") ?? FindChildComponent<Image>("DuyeongNameplate");
            nameplateImages[1] = FindChildComponent<Image>("SodamNameplate");
            selectedPlateImages[0] = FindChildComponent<Image>("DuyeongNamegoldplate");
            selectedPlateImages[1] = FindChildComponent<Image>("SodamNamegoldplate");
            nameLabels[0] = FindChildComponent<TMP_Text>("DuyeongName");
            nameLabels[1] = FindChildComponent<TMP_Text>("SodamName");

            for (int i = 0; i < nameLabels.Length; i++)
            {
                if (nameplateImages[i] != null)
                {
                    normalPlateSprites[i] = nameplateImages[i].sprite;
                    nameplateImages[i].enabled = true;
                }

                if (selectedPlateImages[i] != null)
                {
                    selectedPlateImages[i].transform.SetAsFirstSibling();
                    selectedPlateImages[i].gameObject.SetActive(false);
                }

                hoverShines[i] = CreateHoverShine(nameplateRoots[i], selectedPlateImages[i], normalPlateSprites[i]);

                TMP_Text label = nameLabels[i];
                if (label != null)
                {
                    normalNameMaterials[i] = label.fontSharedMaterial;
                    selectedOutlineMaterials[i] = CreateSelectedOutlineMaterial(normalNameMaterials[i], i);
                }

                if (nameplateRoots[i] != null)
                    nameplateRoots[i].SetActive(false);
            }

            Image startButtonImage = FindChildComponent<Image>("StartButtonVisual");
            if (startButtonImage != null)
            {
                startButtonRectTransform = startButtonImage.rectTransform;
                startButtonBaseScale = startButtonRectTransform.localScale;

                Image startButtonPlateImage = FindChildComponent<Image>("StartButtonplate");
                if (startButtonPlateImage != null)
                {
                    startButtonPlateRectTransform = startButtonPlateImage.rectTransform;
                    startButtonPlateBaseScale = startButtonPlateRectTransform.localScale;
                    hoverShines[2] = CreateHoverShine(
                        startButtonPlateImage.gameObject,
                        null,
                        startButtonPlateImage.sprite);
                }
            }

            Image startButtonBackdropImage = FindChildComponent<Image>("StartButtonVisual (1)");
            if (startButtonBackdropImage != null)
            {
                startButtonBackdropRectTransform = startButtonBackdropImage.rectTransform;
                startButtonBackdropBaseScale = startButtonBackdropRectTransform.localScale;
            }

            Image settingsGearImage = FindChildComponent<Image>("SettingsGearIcon");
            if (settingsGearImage != null)
            {
                settingsGearRectTransform = settingsGearImage.rectTransform;
                settingsGearBaseScale = settingsGearRectTransform.localScale;
            }

            GameObject backButton = FindChildObject("back_button") ?? FindChildObject("Back_button") ?? FindChildObject("BackButton");
            if (backButton != null)
            {
                backButtonRectTransform = backButton.GetComponent<RectTransform>();
                if (backButtonRectTransform != null)
                    backButtonBaseScale = backButtonRectTransform.localScale;
            }
        }

        private bool IsPointerOverStartButton()
        {
            return IsPointerOverRect(startButtonRectTransform);
        }

        private bool IsPointerOverSettingsGear()
        {
            return IsPointerOverRect(settingsGearRectTransform);
        }

        private bool IsPointerOverBackButton()
        {
            return IsPointerOverRect(backButtonRectTransform);
        }

        private bool IsPointerOverRect(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return false;

            Mouse mouse = Mouse.current;
            if (mouse == null || !Application.isFocused)
                return false;

            Vector2 pointer = mouse.position.ReadValue();
            if (pointer.x < 0f || pointer.y < 0f ||
                pointer.x > Screen.width || pointer.y > Screen.height)
                return false;

            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            return RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                pointer,
                uiCamera);
        }

        private void StartStartButtonClickPulse()
        {
            if (startButtonRectTransform == null)
                return;

            startButtonPulseElapsed = 0f;
            startButtonPulseActive = true;
        }

        private void StartSettingsGearClickPulse()
        {
            if (settingsGearRectTransform == null)
                return;

            settingsGearPulseElapsed = 0f;
            settingsGearPulseActive = true;
        }

        private void StartBackButtonClickPulse()
        {
            if (backButtonRectTransform == null)
                return;

            backButtonPulseElapsed = 0f;
            backButtonPulseActive = true;
        }

        private void UpdateStartButtonClickPulse()
        {
            if (!startButtonPulseActive)
                return;

            float duration = Mathf.Max(0.05f, startButtonClickDuration);
            startButtonPulseElapsed += Time.unscaledDeltaTime;
            float leadTime = Mathf.Clamp(startButtonPlateLeadTime, 0f, duration * 0.5f);
            float buttonProgress = Mathf.Clamp01((startButtonPulseElapsed - leadTime) / duration);
            float plateProgress = Mathf.Clamp01(startButtonPulseElapsed / duration);
            const float growFraction = 0.3f;

            float buttonScaleMultiplier = EvaluateClickPulseScale(buttonProgress, growFraction);
            float plateScaleMultiplier = EvaluateClickPulseScale(plateProgress, growFraction);

            SetStartButtonScale(buttonScaleMultiplier, plateScaleMultiplier);

            if (startButtonPulseElapsed >= duration + leadTime)
            {
                startButtonPulseActive = false;
                SetStartButtonScale(1f, 1f);
            }
        }

        private float EvaluateClickPulseScale(float progress, float growFraction)
        {
            if (progress < growFraction)
            {
                float growProgress = Mathf.SmoothStep(0f, 1f, progress / growFraction);
                return Mathf.Lerp(1f, startButtonClickScale, growProgress);
            }

            float returnProgress = Mathf.SmoothStep(0f, 1f, (progress - growFraction) / (1f - growFraction));
            return Mathf.Lerp(startButtonClickScale, 1f, returnProgress);
        }

        private void UpdateSettingsGearClickPulse()
        {
            if (!settingsGearPulseActive || settingsGearRectTransform == null)
                return;

            float duration = Mathf.Max(0.05f, startButtonClickDuration);
            settingsGearPulseElapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(settingsGearPulseElapsed / duration);
            float multiplier = EvaluateClickPulseScale(progress, 0.3f);
            settingsGearRectTransform.localScale = settingsGearBaseScale * multiplier;

            if (progress >= 1f)
            {
                settingsGearPulseActive = false;
                settingsGearRectTransform.localScale = settingsGearBaseScale;
            }
        }

        private void UpdateBackButtonClickPulse()
        {
            if (!backButtonPulseActive || backButtonRectTransform == null)
                return;

            float duration = Mathf.Max(0.05f, startButtonClickDuration);
            backButtonPulseElapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(backButtonPulseElapsed / duration);
            float multiplier = EvaluateClickPulseScale(progress, 0.3f);
            backButtonRectTransform.localScale = backButtonBaseScale * multiplier;

            if (progress >= 1f)
            {
                backButtonPulseActive = false;
                backButtonRectTransform.localScale = backButtonBaseScale;
            }
        }

        private void SetStartButtonScale(float buttonMultiplier, float plateMultiplier)
        {
            if (startButtonRectTransform != null)
                startButtonRectTransform.localScale = startButtonBaseScale * buttonMultiplier;

            if (startButtonPlateRectTransform != null)
            {
                float localPlateMultiplier = buttonMultiplier > Mathf.Epsilon
                    ? plateMultiplier / buttonMultiplier
                    : 1f;
                startButtonPlateRectTransform.localScale = startButtonPlateBaseScale * localPlateMultiplier;
            }

            if (startButtonBackdropRectTransform != null)
                startButtonBackdropRectTransform.localScale = startButtonBackdropBaseScale * buttonMultiplier;
        }

        private GameObject FindChildObject(string objectName)
        {
            Transform[] children = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
                if (children[i].name == objectName)
                    return children[i].gameObject;

            return null;
        }

        private T FindChildComponent<T>(string objectName) where T : Component
        {
            GameObject child = FindChildObject(objectName);
            return child != null ? child.GetComponent<T>() : null;
        }

        private HoverShine CreateHoverShine(GameObject nameplateRoot, Image selectedPlate, Sprite normalPlateSprite)
        {
            if (nameplateRoot == null || hoverShineSprite == null)
                return null;

            GameObject maskObject = new GameObject("HoverShineMask", typeof(RectTransform), typeof(Image), typeof(Mask));
            RectTransform maskRect = maskObject.GetComponent<RectTransform>();
            maskRect.SetParent(nameplateRoot.transform, false);
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;

            Image maskGraphic = maskObject.GetComponent<Image>();
            maskGraphic.sprite = normalPlateSprite;
            maskGraphic.raycastTarget = false;
            maskGraphic.maskable = true;

            Mask mask = maskObject.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            if (selectedPlate != null)
                maskRect.SetSiblingIndex(selectedPlate.transform.GetSiblingIndex() + 1);
            else
                maskRect.SetAsFirstSibling();

            GameObject streakObject = new GameObject("DiagonalShine", typeof(RectTransform), typeof(Image));
            RectTransform streakRect = streakObject.GetComponent<RectTransform>();
            streakRect.SetParent(maskRect, false);
            streakRect.anchorMin = new Vector2(0.5f, 0.5f);
            streakRect.anchorMax = new Vector2(0.5f, 0.5f);
            streakRect.pivot = new Vector2(0.5f, 0.5f);
            streakRect.sizeDelta = new Vector2(hoverShineWidth, 84f);
            streakRect.localScale = Vector3.one;
            streakRect.localRotation = Quaternion.Euler(0f, 0f, hoverShineAngle);

            Image graphic = streakObject.GetComponent<Image>();
            graphic.sprite = hoverShineSprite;
            graphic.color = new Color(1f, 0.78f, 0.32f, hoverShineOpacity);
            graphic.raycastTarget = false;
            graphic.maskable = true;
            graphic.enabled = false;

            RectTransform rootRect = nameplateRoot.transform as RectTransform;
            float plateWidth = rootRect != null && rootRect.rect.width > 0f
                ? rootRect.rect.width
                : rootRect != null ? rootRect.sizeDelta.x : 132f;
            float radians = hoverShineAngle * Mathf.Deg2Rad;
            float projectedHalfWidth = Mathf.Abs(Mathf.Cos(radians)) * hoverShineWidth * 0.5f +
                                       Mathf.Abs(Mathf.Sin(radians)) * streakRect.sizeDelta.y * 0.5f;

            return new HoverShine
            {
                Graphic = graphic,
                MaskGraphic = maskGraphic,
                Rect = streakRect,
                TravelDistance = plateWidth * 0.5f + projectedHalfWidth + 8f
            };
        }

        private Sprite CreateHoverShineSprite()
        {
            const int width = 64;
            const int height = 128;
            hoverShineTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, true)
            {
                name = "Character Select Hover Shine Texture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float vertical = (float)y / (height - 1);
                float edgeDistance = Mathf.Min(vertical, 1f - vertical);
                float verticalFade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(edgeDistance / 0.28f));

                for (int x = 0; x < width; x++)
                {
                    float horizontal = (x - (width - 1) * 0.5f) / (width * 0.5f);
                    float core = Mathf.Exp(-horizontal * horizontal * 32f);
                    float halo = Mathf.Exp(-horizontal * horizontal * 7f);
                    float alpha = Mathf.Clamp01((core * 0.85f + halo * 0.18f) * verticalFade);
                    pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            hoverShineTexture.SetPixels(pixels);
            hoverShineTexture.Apply(false, true);
            return Sprite.Create(
                hoverShineTexture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        private void StartHoverShine(int index)
        {
            if (index < 0 || index >= hoverShines.Length)
                return;

            for (int i = 0; i < hoverShines.Length; i++)
            {
                if (i == index)
                    continue;

                HoverShine other = hoverShines[i];
                if (other == null)
                    continue;

                other.IsPlaying = false;
                if (other.Graphic != null)
                    other.Graphic.enabled = false;
            }

            HoverShine shine = hoverShines[index];
            if (shine == null || shine.Graphic == null || shine.Rect == null)
                return;

            shine.Progress = 0f;
            shine.IsPlaying = true;
            shine.Graphic.enabled = true;
            shine.Rect.anchoredPosition = new Vector2(-shine.TravelDistance, 0f);
        }

        private void UpdateHoverShines()
        {
            float duration = Mathf.Max(0.05f, hoverShineDuration);
            for (int i = 0; i < hoverShines.Length; i++)
            {
                HoverShine shine = hoverShines[i];
                if (shine == null || !shine.IsPlaying || shine.Graphic == null || shine.Rect == null)
                    continue;

                shine.Progress += Time.unscaledDeltaTime / duration;
                if (shine.Progress >= 1f)
                {
                    shine.Progress = 1f;
                    shine.IsPlaying = false;
                    shine.Graphic.enabled = false;
                    continue;
                }

                float eased = Mathf.SmoothStep(0f, 1f, shine.Progress);
                float x = Mathf.Lerp(-shine.TravelDistance, shine.TravelDistance, eased);
                shine.Rect.anchoredPosition = new Vector2(x, 0f);
            }
        }

        private void StopHoverShines()
        {
            previousHoveredIndex = -1;
            for (int i = 0; i < hoverShines.Length; i++)
            {
                HoverShine shine = hoverShines[i];
                if (shine == null)
                    continue;

                shine.IsPlaying = false;
                if (shine.Graphic != null)
                    shine.Graphic.enabled = false;
            }
        }

        private static Material CreateSelectedOutlineMaterial(Material source, int index)
        {
            if (source == null)
                return null;

            Material outline = new Material(source)
            {
                name = source.name + (index == 0 ? " Duyeong Selected Black Outline" : " Sodam Selected Black Outline")
            };

            outline.DisableKeyword("GLOW_ON");
            if (outline.HasProperty("_GlowColor"))
            {
                Color glowColor = outline.GetColor("_GlowColor");
                glowColor.a = 0f;
                outline.SetColor("_GlowColor", glowColor);
            }

            if (outline.HasProperty("_GlowInner"))
                outline.SetFloat("_GlowInner", 0f);
            if (outline.HasProperty("_GlowOuter"))
                outline.SetFloat("_GlowOuter", 0f);
            if (outline.HasProperty("_OutlineColor"))
                outline.SetColor("_OutlineColor", Color.black);
            if (outline.HasProperty("_OutlineWidth"))
                outline.SetFloat("_OutlineWidth", 0.1f);

            return outline;
        }

        private void UpdateNameUi(int hoveredIndex)
        {
            for (int i = 0; i < nameplateRoots.Length; i++)
            {
                bool isSelected = i == selectedIndex;
                bool shouldShow = isSelected || i == hoveredIndex;

                if (nameplateRoots[i] != null && nameplateRoots[i].activeSelf != shouldShow)
                    nameplateRoots[i].SetActive(shouldShow);

                Image plate = nameplateImages[i];
                Image selectedPlate = selectedPlateImages[i];
                HoverShine shine = hoverShines[i];
                if (shine != null && shine.MaskGraphic != null)
                {
                    Sprite maskSprite = isSelected
                        ? (selectedPlate != null ? selectedPlate.sprite : selectedPlateSprite)
                        : normalPlateSprites[i];
                    if (maskSprite != null && shine.MaskGraphic.sprite != maskSprite)
                        shine.MaskGraphic.sprite = maskSprite;
                }

                if (selectedPlate != null)
                {
                    if (plate != null && plate.enabled != !isSelected)
                        plate.enabled = !isSelected;

                    if (selectedPlate.gameObject.activeSelf != isSelected)
                        selectedPlate.gameObject.SetActive(isSelected);
                }
                else
                {
                    Sprite desiredSprite = isSelected && selectedPlateSprite != null
                        ? selectedPlateSprite
                        : normalPlateSprites[i];
                    if (plate != null && desiredSprite != null && plate.sprite != desiredSprite)
                        plate.sprite = desiredSprite;
                }

                TMP_Text label = nameLabels[i];
                Material desiredMaterial = isSelected
                    ? selectedOutlineMaterials[i]
                    : normalNameMaterials[i];

                if (label != null && desiredMaterial != null && label.fontSharedMaterial != desiredMaterial)
                    label.fontSharedMaterial = desiredMaterial;
            }
        }

        private int FindHoveredTarget()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || sceneCamera == null || !Application.isFocused)
                return -1;

            Vector2 pointer = mouse.position.ReadValue();
            if (pointer.x < 0f || pointer.y < 0f ||
                pointer.x > Screen.width || pointer.y > Screen.height)
                return -1;

            Ray ray = sceneCamera.ScreenPointToRay(pointer);
            int hitCount = Physics.RaycastNonAlloc(
                ray,
                raycastHits,
                maxRayDistance,
                raycastMask,
                QueryTriggerInteraction.Ignore);

            int nearestTarget = -1;
            float nearestDistance = float.PositiveInfinity;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = raycastHits[i];
                if (hit.collider == null || hit.distance >= nearestDistance)
                    continue;

                PlayerController hitPlayer = hit.collider.GetComponentInParent<PlayerController>();
                int targetIndex = FindTargetIndex(hitPlayer);
                if (targetIndex < 0 || !IsPointerInsideTightScreenBounds(targetIndex, pointer))
                    continue;

                nearestTarget = targetIndex;
                nearestDistance = hit.distance;
            }

            return nearestTarget >= 0 ? nearestTarget : FindTargetByScreenBounds(pointer);
        }

        private int FindTargetByScreenBounds(Vector2 pointer)
        {
            int nearestTarget = -1;
            float nearestDepth = float.PositiveInfinity;

            for (int i = 0; i < targets.Length; i++)
            {
                if (!TryGetTightScreenBounds(targets[i], out Rect screenBounds, out float depth) ||
                    !screenBounds.Contains(pointer) || depth >= nearestDepth)
                    continue;

                nearestTarget = i;
                nearestDepth = depth;
            }

            return nearestTarget;
        }

        private bool IsPointerInsideTightScreenBounds(int targetIndex, Vector2 pointer)
        {
            return targetIndex >= 0 && targetIndex < targets.Length &&
                   TryGetTightScreenBounds(targets[targetIndex], out Rect screenBounds, out _) &&
                   screenBounds.Contains(pointer);
        }

        private bool TryGetTightScreenBounds(HoverTarget target, out Rect screenBounds, out float depth)
        {
            screenBounds = default;
            depth = float.PositiveInfinity;
            if (target == null || target.Renderers == null || sceneCamera == null)
                return false;

            Bounds bounds = default;
            bool hasBounds = false;
            for (int i = 0; i < target.Renderers.Length; i++)
            {
                Renderer renderer = target.Renderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                    continue;

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
                return false;

            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 world = new Vector3(
                    (corner & 1) == 0 ? bounds.min.x : bounds.max.x,
                    (corner & 2) == 0 ? bounds.min.y : bounds.max.y,
                    (corner & 4) == 0 ? bounds.min.z : bounds.max.z);
                Vector3 screen = sceneCamera.WorldToScreenPoint(world);
                if (screen.z <= 0f)
                    continue;

                minX = Mathf.Min(minX, screen.x);
                minY = Mathf.Min(minY, screen.y);
                maxX = Mathf.Max(maxX, screen.x);
                maxY = Mathf.Max(maxY, screen.y);
                depth = Mathf.Min(depth, screen.z);
            }

            if (float.IsInfinity(minX) || float.IsInfinity(minY) ||
                float.IsInfinity(maxX) || float.IsInfinity(maxY))
                return false;

            const float widthMultiplier = 0.55f;
            const float heightMultiplier = 0.9f;
            float centerX = (minX + maxX) * 0.5f;
            float centerY = (minY + maxY) * 0.5f;
            float halfWidth = (maxX - minX) * 0.5f * widthMultiplier;
            float halfHeight = (maxY - minY) * 0.5f * heightMultiplier;
            screenBounds = Rect.MinMaxRect(
                centerX - halfWidth,
                centerY - halfHeight,
                centerX + halfWidth,
                centerY + halfHeight);
            return true;
        }

        private int FindTargetIndex(PlayerController player)
        {
            if (player == null)
                return -1;

            for (int i = 0; i < targets.Length; i++)
                if (targets[i] != null && targets[i].Player == player)
                    return i;

            return -1;
        }

        private void OnDisable()
        {
            selectedIndex = -1;
            hoverSuppressedIndex = -1;
            hoverSuppressedUntil = 0f;
            previousStartButtonHovered = false;
            startButtonPulseActive = false;
            settingsGearPulseActive = false;
            backButtonPulseActive = false;
            SetStartButtonScale(1f, 1f);
            if (settingsGearRectTransform != null)
                settingsGearRectTransform.localScale = settingsGearBaseScale;
            if (backButtonRectTransform != null)
                backButtonRectTransform.localScale = backButtonBaseScale;
            StopHoverShines();
            UpdateNameUi(-1);

            for (int i = 0; i < targets.Length; i++)
            {
                HoverTarget target = targets[i];
                if (target != null && target.Visual != null)
                    target.Visual.localScale = target.RestScale;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < selectedOutlineMaterials.Length; i++)
                if (selectedOutlineMaterials[i] != null)
                    Destroy(selectedOutlineMaterials[i]);

            if (hoverShineSprite != null)
                Destroy(hoverShineSprite);
            if (hoverShineTexture != null)
                Destroy(hoverShineTexture);
        }
    }
}
