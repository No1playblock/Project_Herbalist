Shader "Herbalist/Sap Flow"
{
    Properties
    {
        _BaseColor ("Water Color", Color) = (0.035, 0.32, 0.98, 0.88)
        _CoreColor ("Flow Highlight", Color) = (0.28, 0.72, 1, 1)
        _RimColor ("Wet Edge", Color) = (0.05, 0.5, 1, 0.8)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        _FlowSpeed ("Flow Speed", Float) = 2
        _FlowAmount ("Flow Pattern Strength", Range(0, 1)) = 1
        _FresnelPower ("Edge Falloff", Range(0.5, 8)) = 3
        _PatternScale ("Flow Pattern Scale", Float) = 18
        _FoamColor ("Foam Color", Color) = (0.88, 0.96, 1, 1)
        _FoamAmount ("Foam Strength", Range(0, 1)) = 0
        _FoamCoverage ("Foam Coverage", Range(0, 1)) = 0.5
        _FoamScale ("Foam Pattern Scale", Range(1, 40)) = 12
        _BeamPattern ("Water Beam Pattern", 2D) = "gray" {}
        _BeamMask ("Water Beam Mask", 2D) = "gray" {}
        _BeamPatternStrength ("Water Beam Pattern Strength", Range(0, 1)) = 0.55
        _BeamPatternTiling ("Water Beam Lengthwise Tiling", Range(1, 32)) = 8
        _GlowStrength ("Water Beam Glow", Range(1, 4)) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull [_Cull]

        Pass
        {
            Name "SapFlow"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BeamPattern);
            SAMPLER(sampler_BeamPattern);
            TEXTURE2D(_BeamMask);
            SAMPLER(sampler_BeamMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _CoreColor;
                float4 _RimColor;
                float _FlowSpeed;
                float _FlowAmount;
                float _FresnelPower;
                float _PatternScale;
                float4 _FoamColor;
                float _FoamAmount;
                float _FoamCoverage;
                float _FoamScale;
                float _BeamPatternStrength;
                float _BeamPatternTiling;
                float _GlowStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
                float3 view = normalize(GetWorldSpaceViewDir(input.positionWS));
                float fresnel = pow(1.0 - saturate(dot(normal, view)), _FresnelPower);

                float phase = input.uv.x * _PatternScale - _Time.y * _FlowSpeed;
                float around = input.uv.y * 6.28318;
                float streakWave = sin(around * 3.0 + phase * 0.38
                    + sin(phase * 1.6 + around * 3.0) * 0.75
                    + sin(phase * 3.3 - around * 5.0) * 0.3);
                float tornEdge = sin(phase * 5.5 + around * 9.0) * 0.15
                    + sin(phase * 9.1 - around * 7.0) * 0.08;
                float filament = smoothstep(0.28, 0.55, streakWave + tornEdge);
                float ripple = 0.5 + 0.5 * sin(phase * 1.35 + around * 3.0);
                float2 beamUV = float2(input.uv.x * _BeamPatternTiling - _Time.y * 2.0, input.uv.y);
                float3 beamPattern = SAMPLE_TEXTURE2D(_BeamPattern, sampler_BeamPattern, beamUV).rgb;
                float3 beamMask = SAMPLE_TEXTURE2D(_BeamMask, sampler_BeamMask, beamUV).rgb;
                float pattern = dot(beamPattern, float3(0.299, 0.587, 0.114));
                float mask = dot(beamMask, float3(0.299, 0.587, 0.114));
                float textureFlow = saturate(pattern * 0.7 + mask * 0.3);
                float beamDetail = (textureFlow - 0.35) * _BeamPatternStrength;
                float whiteStreak = filament * _FlowAmount * lerp(0.72, 1.0, textureFlow);
                float highlight = saturate(whiteStreak * 1.35 + ripple * 0.08 + beamDetail);

                float3 color = lerp(_BaseColor.rgb, _CoreColor.rgb, highlight * 0.95);
                color += _RimColor.rgb * fresnel * 1.8;
                float2 foamUV = input.uv * _FoamScale;
                float foamTime = _Time.y * _FlowSpeed;
                float swirl = sin(foamUV.x * 1.7 - foamTime * 1.2
                    + sin(foamUV.y * 2.3 + foamTime * 0.4) * 1.3);
                float brokenFlow = sin(foamUV.y * 1.2 + foamTime * 0.75
                    + sin(foamUV.x * 0.73) * 1.1);
                float flecks = sin(foamUV.x * 2.9 + foamUV.y * 1.8 - foamTime * 1.6);
                float turbulence = (swirl * brokenFlow * 0.5 + 0.5) * 0.7
                    + (flecks * 0.5 + 0.5) * 0.3;
                float foamThreshold = lerp(0.88, 0.55, _FoamCoverage);
                float foam = smoothstep(foamThreshold, foamThreshold + 0.12, turbulence) * _FoamAmount
                    * (0.65 + fresnel * 0.35);
                color = lerp(color, _FoamColor.rgb, foam * _FoamColor.a * 0.45);
                float alpha = saturate(_BaseColor.a * (0.94 + highlight * 0.06));
                return half4(color * _GlowStrength, alpha);
            }
            ENDHLSL
        }
    }
}
