Shader "Herbalist/Maze Density Water"
{
    Properties
    {
        _BaseColor ("Water", Color) = (0.025, 0.36, 0.82, 0.94)
        _EdgeColor ("Water Rim", Color) = (0.27, 0.72, 1, 1)
        [HideInInspector] _Density ("Density", 2D) = "black" {}
        [HideInInspector] _VisibleCorners ("Wall Visibility", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "MazeDensityWater"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_Density);
            TEXTURE2D(_VisibleCorners);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EdgeColor;
                float4x4 _MazeFromObject;
                float4 _Grid;
                float4 _GridExtent;
                float4 _WetBounds;
                float _Threshold;
                float _WaterUvScale;
            CBUFFER_END

            struct Attributes { float3 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 grid : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                float2 local = mul(_MazeFromObject, float4(input.positionOS, 1)).xy;
                output.grid = (local - _Grid.xy) * _Grid.z;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 grid = input.grid;
                if (any(grid < _WetBounds.xy) || any(grid >= _WetBounds.zw)) discard;
                int2 square = (int2)floor(grid);
                int2 extent = (int2)_GridExtent.xy;
                if (any(square < 0) || any(square >= extent)) discard;

                // Each bit authorizes a bilinear tap whose entire path stays on this side of the wall.
                int2 maskPixel = (int2)floor(grid * _Grid.w);
                uint mask = (uint)round(15 * LOAD_TEXTURE2D(_VisibleCorners, maskPixel).r);
                if (mask == 0) discard;
                float2 f = frac(grid);
                float4 weights = float4((1-f.x)*(1-f.y), f.x*(1-f.y), (1-f.x)*f.y, f.x*f.y);
                weights *= float4(mask & 1u, (mask >> 1) & 1u, (mask >> 2) & 1u, (mask >> 3) & 1u);
                float4 density = float4(
                    LOAD_TEXTURE2D(_Density, square).r,
                    LOAD_TEXTURE2D(_Density, square + int2(1,0)).r,
                    LOAD_TEXTURE2D(_Density, square + int2(0,1)).r,
                    LOAD_TEXTURE2D(_Density, square + int2(1,1)).r);
                float water = dot(density, weights) / max(dot(weights, float4(1,1,1,1)), 0.00001);
                clip(water - _Threshold);

                float2 local = grid / _Grid.z;
                float ripple = sin(dot(local, float2(9, 7)) * _WaterUvScale - _Time.y * 2.1) *
                               sin(dot(local, float2(-5, 11)) * _WaterUvScale + _Time.y * 1.7);
                half rim = 1 - smoothstep(0, 0.12, water - _Threshold);
                return lerp(_BaseColor, _EdgeColor, saturate(rim * 0.7 + 0.12 + ripple * 0.06));
            }
            ENDHLSL
        }
    }
}
