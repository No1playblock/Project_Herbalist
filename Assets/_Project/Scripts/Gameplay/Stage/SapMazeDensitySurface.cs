using UnityEngine;

namespace Herbalist.Levels
{
    // Optional presentation for an authored maze. The density never feeds gameplay.
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class SapMazeDensitySurface : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Texture2D _visibleCorners;
        public const int MaskSamplesPerCell = 4;

        private static readonly int DensityId = Shader.PropertyToID("_Density");
        private static readonly int MaskId = Shader.PropertyToID("_VisibleCorners");
        private static readonly int MazeFromObjectId = Shader.PropertyToID("_MazeFromObject");
        private static readonly int GridId = Shader.PropertyToID("_Grid");
        private static readonly int ExtentId = Shader.PropertyToID("_GridExtent");
        private static readonly int WetBoundsId = Shader.PropertyToID("_WetBounds");
        private static readonly int ThresholdId = Shader.PropertyToID("_Threshold");
        private static readonly int UvScaleId = Shader.PropertyToID("_WaterUvScale");

        private Texture2D _density;
        private MaterialPropertyBlock _properties;
        private readonly System.Collections.Generic.List<int> _written = new();

        public bool CanPresent(SapMazeFluidField field) =>
            _renderer != null && _renderer.sharedMaterial != null && _renderer.sharedMaterial.shader.isSupported &&
            _visibleCorners != null && _visibleCorners.width == (field.Width - 1) * MaskSamplesPerCell &&
            _visibleCorners.height == (field.Count / field.Width - 1) * MaskSamplesPerCell &&
            SystemInfo.SupportsTextureFormat(TextureFormat.R16);

        public void SetVisible(bool visible)
        {
            if (_renderer != null) _renderer.enabled = visible;
        }

        public void Upload(SapMazeFluidField field, SapMazeSurface surface, SapMazeFluidOutline outline,
            float threshold, float uvScale)
        {
            int height = field.Count / field.Width;
            if (_density == null || _density.width != field.Width || _density.height != height)
            {
                ReleaseDensity();
                _density = new Texture2D(field.Width, height, TextureFormat.R16, false, true)
                {
                    name = "Maze visual density",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                // Explicitly clear the dry texels before the first upload.
                var initial = _density.GetRawTextureData<ushort>();
                for (int i = 0; i < initial.Length; i++) initial[i] = 0;
            }

            var pixels = _density.GetRawTextureData<ushort>();
            foreach (int cell in _written) pixels[cell] = 0;
            _written.Clear();
            int minX = field.Width, minY = height, maxX = -1, maxY = -1;
            foreach (int cell in outline.Cells)
            {
                int value = Mathf.RoundToInt(Mathf.Clamp01(outline.Value(cell)) * ushort.MaxValue);
                if (value == 0) continue;
                pixels[cell] = (ushort)value;
                _written.Add(cell);
                int x = cell % field.Width, y = cell / field.Width;
                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
            _density.Apply(false);

            _properties ??= new MaterialPropertyBlock();
            _properties.SetTexture(DensityId, _density);
            _properties.SetTexture(MaskId, _visibleCorners);
            _properties.SetMatrix(MazeFromObjectId, surface.transform.worldToLocalMatrix * transform.localToWorldMatrix);
            Vector2 origin = surface.GridPoint(0);
            _properties.SetVector(GridId, new Vector4(origin.x, origin.y, 1f / field.CellSize, MaskSamplesPerCell));
            _properties.SetVector(ExtentId, new Vector4(field.Width - 1, height - 1, 0, 0));
            _properties.SetVector(WetBoundsId, new Vector4(minX - 1, minY - 1, maxX + 1, maxY + 1));
            // At zero volume there is no threshold contour to draw.
            _properties.SetFloat(ThresholdId, threshold > 0f ? threshold : 2f);
            _properties.SetFloat(UvScaleId, uvScale);
            _renderer.SetPropertyBlock(_properties);
            SetVisible(threshold > 0f);
        }

        private void OnDisable() => SetVisible(false);
        private void OnDestroy() => ReleaseDensity();

        private void ReleaseDensity()
        {
            if (_density != null)
            {
                if (Application.isPlaying) Destroy(_density);
                else DestroyImmediate(_density);
            }
            _density = null;
            _written.Clear();
        }
    }
}
