Shader "Herbalist/Branch Sap"
{
    Properties
    {
        _WaterColor ("Water Body", Color) = (0.028, 0.39, 0.74, 1)
        _GlintColor ("Water Reflection", Color) = (0.54, 0.88, 1, 1)
        _FlowRate ("Flow Speed", Range(0.1, 2)) = 0.6
        _DripInterval ("Time Between Drips", Range(1, 5)) = 2.6
        _FallDistance ("Drop Fall Distance", Range(0.1, 2)) = 0.9
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "BranchSap"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _WaterColor;
                half4 _GlintColor;
                float _FlowRate;
                float _DripInterval;
                float _FallDistance;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                half3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half3 kindCycleOpacity : TEXCOORD3;
                half fogFactor : TEXCOORD4;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                // Red marks the attached and falling meshes; blue selects the falling drop.
                float cycle = frac(_Time.y / _DripInterval + input.color.g);
                float falling = saturate((cycle - 0.27) / 0.57);
                float3 world = TransformObjectToWorld(input.positionOS);
                world.y -= input.color.b * falling * falling * _FallDistance;
                output.positionWS = world;
                output.positionCS = TransformWorldToHClip(world);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.kindCycleOpacity = half3(input.color.r, lerp(-1, cycle, input.color.b), input.color.a);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
            {
                half3 normal = NormalizeNormalPerPixel(input.normalWS) * IS_FRONT_VFACE(face, 1, -1);
                half3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half grazing = 1 - saturate(dot(normal, view));
                half fresnel = grazing * grazing * grazing;
                half3 reflected = reflect(-view, normal);
                half sheen = smoothstep(0.35h, 0.83h, reflected.y) * 0.22h;
                half glint = smoothstep(0.955h, 0.991h, dot(reflected, GetMainLight().direction));

                half center = 1 - abs(input.uv.y * 2 - 1);
                half ribbon = smoothstep(0, 0.48h, center) * smoothstep(0, 0.11h, input.uv.x);
                half phase = frac(input.uv.x * 1.65 - _Time.y * _FlowRate);
                half flow = smoothstep(0.04h, 0.22h, phase) * (1 - smoothstep(0.30h, 0.65h, phase));
                half drop = input.kindCycleOpacity.x;
                half body = lerp(0.12h + flow * 0.52h, 0.26h, drop);
                half brightness = saturate(body + fresnel * 0.38h);
                half3 tint = lerp(_WaterColor.rgb, _GlintColor.rgb, brightness);
                tint += _GlintColor.rgb * (sheen + glint * lerp(0.45h, 1.22h, drop));

                half opacity = lerp(ribbon * (0.48h + flow * 0.26h), 0.84h + fresnel * 0.10h, drop);
                if (input.kindCycleOpacity.y >= 0)
                {
                    half cycle = input.kindCycleOpacity.y;
                    opacity *= smoothstep(0.27h, 0.36h, cycle) * (1 - smoothstep(0.76h, 0.86h, cycle));
                }
                opacity *= input.kindCycleOpacity.z;
                return half4(MixFog(tint, input.fogFactor) * opacity, opacity);
            }
            ENDHLSL
        }
    }
}
