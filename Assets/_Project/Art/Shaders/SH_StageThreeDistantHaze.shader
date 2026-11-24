Shader "Herbalist/Environment/Stage Three Distant Haze"
{
    Properties
    {
        _UpperColor ("Upper Haze", Color) = (0.15, 0.20, 0.25, 1)
        _DeepColor ("Deep Haze", Color) = (0.028, 0.046, 0.074, 1)
        _MistColor ("Mist Highlights", Color) = (0.27, 0.35, 0.41, 1)
        _LightColor ("Blue Mist Light", Color) = (0.24, 0.43, 0.49, 1)
        _PearlColor ("Pearl Mist Light", Color) = (0.41, 0.48, 0.45, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _UpperColor;
                half4 _DeepColor;
                half4 _MistColor;
                half4 _LightColor;
                half4 _PearlColor;
            CBUFFER_END

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(float3 positionOS : POSITION)
            {
                Varyings output;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(positionOS);
                output.positionCS.z = output.positionCS.w * UNITY_RAW_FAR_CLIP_VALUE;
                output.direction = positionOS;
                return output;
            }

            float Hash(float3 cell)
            {
                cell = frac(cell * 0.3183099 + float3(0.11, 0.31, 0.73));
                return frac(dot(cell, cell.yzx + 19.19) * (cell.x + cell.z));
            }

            float MistNoise(float3 position)
            {
                float3 cell = floor(position);
                float3 weight = frac(position);
                weight = weight * weight * (3.0 - 2.0 * weight);
                float a = lerp(Hash(cell), Hash(cell + float3(1, 0, 0)), weight.x);
                float b = lerp(Hash(cell + float3(0, 1, 0)), Hash(cell + float3(1, 1, 0)), weight.x);
                float c = lerp(Hash(cell + float3(0, 0, 1)), Hash(cell + float3(1, 0, 1)), weight.x);
                float d = lerp(Hash(cell + float3(0, 1, 1)), Hash(cell + float3(1, 1, 1)), weight.x);
                return lerp(lerp(a, b, weight.y), lerp(c, d, weight.y), weight.z);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 direction = normalize(input.direction);
                float depth = smoothstep(0.18, 0.94, -direction.y);
                half3 baseColor = lerp(_UpperColor.rgb, _DeepColor.rgb, depth);

                float motion = _Time.y * 0.013;
                float billows = MistNoise(direction * float3(4.5, 15.0, 4.5) + float3(motion, 2.8, 0.6));
                float wisps = MistNoise(direction * float3(9.0, 34.0, 9.0) + float3(-motion * 0.6, 0.4, 4.2));
                float mist = smoothstep(0.33, 0.78, billows * 0.78 + wisps * 0.22);
                half3 color = lerp(baseColor, _MistColor.rgb, mist * lerp(0.42, 0.16, depth));
                // Match the dim upper cave rim while letting the lower haze catch more light.
                float rimFade = lerp(0.64, 1.0, smoothstep(0.14, 0.34, -direction.y));
                color *= rimFade;

                float forward = smoothstep(0.58, 0.86, direction.z);
                float2 opening = (direction.xy - float2(-0.025, -0.39)) * float2(7.2, 5.7);
                float glow = exp(-dot(opening, opening)) * forward;
                color = lerp(color, _LightColor.rgb, glow * lerp(0.42, 0.83, mist));

                float2 pearlCenter = (direction.xy - float2(0.022, -0.47)) * float2(11.0, 9.5);
                float pearl = exp(-dot(pearlCenter, pearlCenter)) * forward;
                float illuminatedWisps = lerp(0.19, 0.42, smoothstep(0.25, 0.85, wisps));
                color = lerp(color, _PearlColor.rgb, pearl * illuminatedWisps);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
