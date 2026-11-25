Shader "Herbalist/Surface Tension Sap"
{
    Properties
    {
        _WaterColor ("Body Color", Color) = (0.025, 0.42, 0.76, 1)
        _EdgeColor ("Shallow Color", Color) = (0.10, 0.72, 0.94, 1)
        _ReflectionColor ("Reflected Light", Color) = (0.78, 0.95, 1, 1)
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
            Name "SurfaceTensionSap"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _WaterColor;
                half4 _EdgeColor;
                half4 _ReflectionColor;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                half3 normalOS : NORMAL;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half depth : TEXCOORD1;
                half fogFactor : TEXCOORD2;
                half3 normalWS : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.depth = input.color.r;
                output.fogFactor = ComputeFogFactor(position.positionCS.z);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half depth = saturate(input.depth);
                half3 normal = NormalizeNormalPerPixel(input.normalWS);
                half3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half3 reflection = reflect(-view, normal);
                half grazing = 1 - saturate(dot(normal, view));
                half fresnel = grazing * grazing * grazing;
                half sky = smoothstep(0.22h, 0.78h, reflection.y);
                half reflectedSky = sky * (0.12h + 0.16h * smoothstep(-0.25h, 0.3h, reflection.z));
                half glint = smoothstep(0.955h, 0.991h, dot(reflection, GetMainLight().direction));
                half tintStrength = smoothstep(0.03h, 0.80h, depth) * (1 - fresnel * 0.55h);
                half3 tint = lerp(_EdgeColor.rgb, _WaterColor.rgb, tintStrength);

                if (_CameraOpaqueTexture_TexelSize.z > 32 && _CameraOpaqueTexture_TexelSize.w > 32)
                {
                    float2 bend = mul((float3x3)UNITY_MATRIX_V, normal).xy;
                    float distanceToCamera = max(2, distance(GetCameraPositionWS(), input.positionWS));
                    float2 uv = GetNormalizedScreenSpaceUV(input.positionCS) - bend * depth * (0.085 / distanceToCamera);
                    half3 wetSurface = SampleSceneColor(uv);
                    tint = lerp(tint, wetSurface * half3(0.22h, 0.76h, 1), 0.27h * (1 - fresnel));
                }

                tint += _ReflectionColor.rgb * (0.33h * fresnel + reflectedSky + glint * 1.2h);
                half opacity = lerp(0.48h, 0.88h, smoothstep(0, 0.40h, depth));
                opacity = saturate(opacity + fresnel * 0.15h);
                return half4(MixFog(tint, input.fogFactor) * opacity, opacity);
            }
            ENDHLSL
        }
    }
}
