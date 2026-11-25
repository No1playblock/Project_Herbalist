Shader "Herbalist/Ready Sap Volume"
{
    Properties
    {
        _DeepColor ("Deep Water", Color) = (0.017, 0.28, 0.67, 1)
        _ShallowColor ("Shallow Water", Color) = (0.04, 0.78, 0.96, 1)
        _ReflectionColor ("Reflected Light", Color) = (0.70, 0.95, 1, 1)
        _Opacity ("Body Opacity", Range(0, 1)) = 0.88
        _HighlightStrength ("Highlight Strength", Range(0, 2)) = 1.22
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Cull Back
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "ReadySapVolume"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half4 _ReflectionColor;
                half _Opacity;
                half _HighlightStrength;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor = ComputeFogFactor(position.positionCS.z);
                output.positionOS = input.positionOS;
                return output;
            }

            half BubbleHighlight(float3 surface, float3 ray, float3 center, float radius)
            {
                float3 toCenter = center - surface;
                float depth = dot(toCenter, ray);
                float3 closest = toCenter - ray * depth;
                half radial = length(closest) / radius;
                half edge = smoothstep(0.45h, 0.78h, radial) * (1 - smoothstep(0.78h, 1.04h, radial));
                return edge * smoothstep(0.015h, 0.10h, depth);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 normal = NormalizeNormalPerPixel(input.normalWS);
                half3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half facing = saturate(dot(normal, view));
                half grazing = 1 - facing;
                half fresnel = grazing * grazing * grazing;
                half3 reflection = reflect(-view, normal);
                half sky = smoothstep(0.38h, 0.77h, reflection.y);
                half lightThroughEdge = saturate(grazing * 0.78h + sky * 0.24h);
                half3 body = lerp(_DeepColor.rgb, _ShallowColor.rgb, lightThroughEdge);

                // The mobile pipeline can render the same tint without the optional opaque color buffer.
                if (_CameraOpaqueTexture_TexelSize.z > 32 && _CameraOpaqueTexture_TexelSize.w > 32)
                {
                    float2 screen = GetNormalizedScreenSpaceUV(input.positionCS);
                    float2 bendScreen = mul((float3x3)UNITY_MATRIX_V, normal).xy;
                    float bendScale = 0.075 / max(2, distance(GetCameraPositionWS(), input.positionWS));
                    half3 throughWater = SampleSceneColor(screen - bendScreen * bendScale * (1 - fresnel));
                    body = lerp(body, throughWater * half3(0.18h, 0.78h, 1), 0.22h * facing);
                }

                // Uneven light along the lower shoulder gives the blue interior depth.
                float3 transmitted = TransformWorldToObjectDir(refract(-view, normal, 0.75));
                float3 innerPosition = input.positionOS + transmitted * 0.16;
                half bend = 0.035h * sin(innerPosition.x * 4.8 + innerPosition.z * 3.3 - _Time.y * 0.65);
                half band = 1 - smoothstep(0.016h, 0.075h, abs(innerPosition.y + 0.085h - bend));
                half lowerShoulder = 1 - smoothstep(0.22h, 0.77h, facing);
                half innerLight = band * lowerShoulder * 0.38h;
                body += _ShallowColor.rgb * innerLight;
                half bubbles = BubbleHighlight(input.positionOS, transmitted, float3(0.12, 0.05, 0.10), 0.052) * 0.52h;
                bubbles += BubbleHighlight(input.positionOS, transmitted, float3(0.055, -0.015, -0.10), 0.034) * 0.34h;
                body += _ReflectionColor.rgb * bubbles * facing;

                Light sun = GetMainLight();
                half spot = saturate(dot(reflection, sun.direction));
                half glint = smoothstep(0.956h, 0.989h, spot);
                half sheen = pow(spot, 18) * 0.24h;
                half skyPatch = sky * smoothstep(-0.12h, 0.28h, reflection.z) * 0.15h;
                half3 reflectedColor = _ReflectionColor.rgb * (fresnel * (0.16h + sky * 0.32h) + skyPatch);
                reflectedColor += _ReflectionColor.rgb * sun.color * (glint + sheen) * _HighlightStrength;

                half opacity = saturate(_Opacity * lerp(0.71h, 1, facing) + fresnel * 0.12h);
                half3 color = MixFog(body + reflectedColor, input.fogFactor);
                return half4(color * opacity, opacity);
            }
            ENDHLSL
        }
    }
}
