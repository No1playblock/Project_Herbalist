using UnityEngine;
using UnityEngine.UI;

/// <summary>Renders the entire UI image with a soft halo that extends beyond its RectTransform.</summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public sealed class UIImageGlowExpanded : MonoBehaviour
{
    [SerializeField] private Color glowColor = new Color(1f, 0.97f, 0.91f, 1f);
    [SerializeField, Range(1f, 18f)] private float radius = 10f; [SerializeField, Range(0.5f, 4f)] private float glowIntensity = 3.2f;

    private const string ShaderName = "UI/Expanded Whole Image Glow";
    private static readonly int GlowColorId = Shader.PropertyToID("_GlowColor"); private static readonly int GlowIntensityId = Shader.PropertyToID("_GlowIntensity");
    private static readonly int GlowSizeId = Shader.PropertyToID("_GlowSize");
    private static readonly int PaddingLocalId = Shader.PropertyToID("_GlowPaddingLocal");
    private static readonly int PaddingUvId = Shader.PropertyToID("_GlowPaddingUV");
    private static readonly int RectCenterId = Shader.PropertyToID("_RectCenter");
    private static readonly int UvBoundsId = Shader.PropertyToID("_SpriteUVBounds");

    private Image image;
    private Material glowMaterial;
    private Material previousMaterial;

    private void OnEnable() => ApplyGlow();
    private void OnValidate() { if (isActiveAndEnabled) ApplyGlow(); }
    private void OnRectTransformDimensionsChange() { if (isActiveAndEnabled) ApplyGlow(); }

    private void ApplyGlow()
    {
        if (image == null) image = GetComponent<Image>();
        if (image == null || image.sprite == null) return;
        Shader shader = Shader.Find(ShaderName);
        if (shader == null) return;

        if (glowMaterial == null || glowMaterial.shader != shader)
        {
            ReleaseMaterial();
            previousMaterial = image.material;
            glowMaterial = new Material(shader) { name = "Expanded Whole Image Glow (Runtime)" };
            glowMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        if (image.material != glowMaterial) image.material = glowMaterial;

        Rect rect = image.rectTransform.rect;
        Rect spriteRect = image.sprite.rect;
        float localPerSpritePixelX = rect.width / Mathf.Max(spriteRect.width, 1f);
        float localPerSpritePixelY = rect.height / Mathf.Max(spriteRect.height, 1f);
        Texture texture = image.sprite.texture;
        float texturePixelsPerSpritePixelX = texture.width / Mathf.Max(spriteRect.width, 1f);
        float texturePixelsPerSpritePixelY = texture.height / Mathf.Max(spriteRect.height, 1f);

        glowMaterial.SetColor(GlowColorId, glowColor); glowMaterial.SetFloat(GlowIntensityId, glowIntensity);
        glowMaterial.SetVector(GlowSizeId, new Vector4(radius * texturePixelsPerSpritePixelX, radius * texturePixelsPerSpritePixelY, 0f, 0f));
        glowMaterial.SetVector(PaddingLocalId, new Vector4(radius * localPerSpritePixelX, radius * localPerSpritePixelY, 0f, 0f));
        glowMaterial.SetVector(PaddingUvId, new Vector4(radius / Mathf.Max(spriteRect.width, 1f), radius / Mathf.Max(spriteRect.height, 1f), 0f, 0f));
        glowMaterial.SetVector(RectCenterId, new Vector4(rect.center.x, rect.center.y, 0f, 0f));

        Vector2[] uvs = image.sprite.uv;
        Vector2 min = uvs[0];
        Vector2 max = uvs[0];
        for (int i = 1; i < uvs.Length; i++)
        {
            min = Vector2.Min(min, uvs[i]);
            max = Vector2.Max(max, uvs[i]);
        }
        glowMaterial.SetVector(UvBoundsId, new Vector4(min.x, min.y, max.x, max.y));
    }

    private void OnDisable() => RestoreMaterial();
    private void OnDestroy() => RestoreMaterial();

    private void RestoreMaterial()
    {
        if (image == null) image = GetComponent<Image>();
        if (image != null && image.material == glowMaterial) image.material = previousMaterial;
        ReleaseMaterial();
    }

    private void ReleaseMaterial()
    {
        if (glowMaterial == null) return;
        if (Application.isPlaying) Destroy(glowMaterial); else DestroyImmediate(glowMaterial);
        glowMaterial = null;
    }
}
