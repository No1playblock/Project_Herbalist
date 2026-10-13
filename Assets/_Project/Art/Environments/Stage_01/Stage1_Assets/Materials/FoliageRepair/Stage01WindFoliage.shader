Shader "Herbalist/Stage01/Wind Foliage"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Tint", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.4
        _WindStrength ("Wind Strength (m)", Range(0,1)) = 0.15
        _WindSpeed ("Wind Speed", Range(0,5)) = 1.2
        _WindFrequency ("Wind Frequency", Range(0,5)) = 0.8
        _WindBaseY ("Mesh Base Y", Float) = 0
        _WindHeight ("Mesh Height", Float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        Cull Off
        ZWrite On

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _Cutoff;
            float _WindStrength;
            float _WindSpeed;
            float _WindFrequency;
            float _WindBaseY;
            float _WindHeight;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float3 positionWS : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        Varyings WindVertex(Attributes input)
        {
            Varyings output;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
            float anchor = saturate((input.positionOS.y - _WindBaseY) / max(_WindHeight, 0.001));
            anchor *= anchor;
            float phase = (positionWS.x + positionWS.z * 0.73) * _WindFrequency + _Time.y * _WindSpeed;
            float wave = sin(phase) * 0.75 + sin(phase * 1.73 + 1.3) * 0.25;
            positionWS.xz += float2(1, 0.45) * (wave * _WindStrength * anchor);
            output.positionWS = positionWS;
            output.positionCS = TransformWorldToHClip(positionWS);
            output.normalWS = TransformObjectToWorldNormal(input.normalOS);
            output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
            return output;
        }

        half4 FoliageColor(Varyings input)
        {
            half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
            clip(color.a - _Cutoff);
            return color;
        }
        ENDHLSL

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex WindVertex
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 color = FoliageColor(input);
                Light light = GetMainLight();
                half diffuse = saturate(dot(normalize(input.normalWS), light.direction));
                // Two-sided cards remain legible when viewed from the back.
                diffuse = max(diffuse, 0.35h);
                half3 ambient = SampleSH(input.normalWS);
                // Thin foliage cards should keep their authored tint in deep shade.
                half3 lighting = ambient + light.color * diffuse * light.distanceAttenuation;
                color.rgb *= max(half3(1.05h, 1.05h, 1.05h), lighting);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ColorMask 0
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex WindVertex
            #pragma fragment ShadowFrag
            #pragma multi_compile_instancing
            half4 ShadowFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                FoliageColor(input);
                return 0;
            }
            ENDHLSL
        }
    }
}
