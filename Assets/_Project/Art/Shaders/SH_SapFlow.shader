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
                float filament = pow(saturate(0.5 + 0.5 * sin(phase + sin(input.uv.y * 6.28318) * 0.35)), 7.0);
                float ripple = 0.5 + 0.5 * sin(phase * 1.35 + input.uv.y * 5.0);
                float highlight = saturate(filament * 0.38 * _FlowAmount + ripple * 0.06 * _FlowAmount + fresnel * 0.72);

                float3 color = lerp(_BaseColor.rgb * 0.76, _CoreColor.rgb, highlight * 0.34);
                color += _RimColor.rgb * fresnel * 0.4;
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
                color = lerp(color, _FoamColor.rgb, foam * _FoamColor.a);
                float alpha = saturate(_BaseColor.a * (0.78 + highlight * 0.12 + fresnel * 0.1)
                    + foam * 0.08);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
