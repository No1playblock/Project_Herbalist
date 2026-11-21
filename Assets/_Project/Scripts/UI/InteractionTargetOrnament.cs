using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    /// <summary>Displays the translucent brush-stroke sprite behind world interaction hints.</summary>
    [ExecuteAlways]
    [AddComponentMenu("UI/Interaction Target Ornament")]
    public sealed class InteractionTargetOrnament : MaskableGraphic
    {
        [SerializeField] private Sprite _brushSprite;
        [SerializeField, Min(0f)] private float _contentWidth = 150f;
        [SerializeField, Min(0f)] private float _panelHorizontalPadding = 16f;
        [SerializeField] private float _panelCenterY = 30f;
        [SerializeField, Range(0f, 1f)] private float _panelOpacity = 0.5f;

        public override Texture mainTexture =>
            _brushSprite != null ? _brushSprite.texture : s_WhiteTexture;

        public void SetContentWidth(float width)
        {
            Rect rect = rectTransform.rect;
            float maxWidth = rect.width > 0f ? Mathf.Max(24f, rect.width - 24f) : width;
            float nextWidth = Mathf.Clamp(width, 24f, maxWidth);
            if (Mathf.Abs(_contentWidth - nextWidth) < 0.5f) return;
            _contentWidth = nextWidth;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = rectTransform.rect;
            if (_brushSprite == null || rect.width <= 0f || rect.height <= 0f) return;

            Rect spriteRect = _brushSprite.rect;
            if (spriteRect.width <= 0f || spriteRect.height <= 0f) return;
            float aspect = spriteRect.width / spriteRect.height;
            float requestedWidth = _contentWidth + _panelHorizontalPadding * 2f;
            float maxWidthByRect = Mathf.Max(0f, rect.width - 16f);
            float maxWidthByHeight = Mathf.Max(0f, rect.height - 8f) * aspect;
            float width = Mathf.Min(requestedWidth, maxWidthByRect, maxWidthByHeight);
            if (width <= 0f) return;

            float halfWidth = width * 0.5f;
            float halfHeight = halfWidth / aspect;
            Vector2 center = new Vector2((rect.xMin + rect.xMax) * 0.5f, _panelCenterY);
            Vector4 uv = DataUtility.GetOuterUV(_brushSprite);
            Color tint = color;
            tint.a *= _panelOpacity;

            int first = vh.currentVertCount;
            AddVertex(vh, center + new Vector2(-halfWidth, -halfHeight), new Vector2(uv.x, uv.y), tint);
            AddVertex(vh, center + new Vector2(-halfWidth, halfHeight), new Vector2(uv.x, uv.w), tint);
            AddVertex(vh, center + new Vector2(halfWidth, halfHeight), new Vector2(uv.z, uv.w), tint);
            AddVertex(vh, center + new Vector2(halfWidth, -halfHeight), new Vector2(uv.z, uv.y), tint);
            vh.AddTriangle(first, first + 1, first + 2);
            vh.AddTriangle(first, first + 2, first + 3);
        }

        private static void AddVertex(VertexHelper vh, Vector2 position, Vector2 uv, Color tint)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.uv0 = uv;
            vertex.color = tint;
            vh.AddVert(vertex);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            _panelOpacity = Mathf.Clamp01(_panelOpacity);
            _panelHorizontalPadding = Mathf.Max(0f, _panelHorizontalPadding);
            SetVerticesDirty();
        }
    }
}