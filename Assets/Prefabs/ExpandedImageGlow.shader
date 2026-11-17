Shader "UI/Expanded Whole Image Glow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1, 0.97, 0.91, 1) _GlowIntensity ("Glow Intensity", Range(0.5,4)) = 3.2
        _GlowSize ("Glow Radius", Vector) = (8,8,0,0)
        _GlowPaddingLocal ("Local Padding", Vector) = (8,8,0,0)
        _GlowPaddingUV ("UV Padding", Vector) = (0.08,0.08,0,0)
        _RectCenter ("Rect Center", Vector) = (0,0,0,0)
        _SpriteUVBounds ("Sprite UV Bounds", Vector) = (0,0,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            Name "ExpandedWholeImageGlow"
            CGPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _GlowColor; float _GlowIntensity;
            float2 _GlowSize;
            float2 _GlowPaddingLocal;
            float2 _GlowPaddingUV;
            float2 _RectCenter;
            float4 _SpriteUVBounds;
            float4 _ClipRect;
            struct appdata_t { float4 vertex : POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; float4 worldPosition : TEXCOORD1; };
            v2f vert(appdata_t v)
            {
                v2f o;
                float2 uvCenter = (_SpriteUVBounds.xy + _SpriteUVBounds.zw) * 0.5;
                v.vertex.xy += sign(v.vertex.xy - _RectCenter) * _GlowPaddingLocal;
                v.texcoord = uvCenter + (v.texcoord - uvCenter) * (1.0 + 2.0 * _GlowPaddingUV);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }
            float SampleAlpha(float2 uv)
            {
                float inside = step(_SpriteUVBounds.x, uv.x) * step(uv.x, _SpriteUVBounds.z)
                             * step(_SpriteUVBounds.y, uv.y) * step(uv.y, _SpriteUVBounds.w);
                return tex2D(_MainTex, uv).a * inside;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float centerAlpha = SampleAlpha(i.texcoord);
                float2 texel = _MainTex_TexelSize.xy * max(_GlowSize, 0.01);
                float nearSum = 0;
                float farSum = 0;
                nearSum += SampleAlpha(i.texcoord + float2(1,0) * texel);
                nearSum += SampleAlpha(i.texcoord + float2(-1,0) * texel);
                nearSum += SampleAlpha(i.texcoord + float2(0,1) * texel);
                nearSum += SampleAlpha(i.texcoord + float2(0,-1) * texel);
                nearSum += SampleAlpha(i.texcoord + float2(0.7071,0.7071) * texel);
                nearSum += SampleAlpha(i.texcoord + float2(-0.7071,0.7071) * texel);
                nearSum += SampleAlpha(i.texcoord + float2(0.7071,-0.7071) * texel);
                nearSum += SampleAlpha(i.texcoord + float2(-0.7071,-0.7071) * texel);
                farSum += SampleAlpha(i.texcoord + float2(1,0) * texel * 2.0);
                farSum += SampleAlpha(i.texcoord + float2(-1,0) * texel * 2.0);
                farSum += SampleAlpha(i.texcoord + float2(0,1) * texel * 2.0);
                farSum += SampleAlpha(i.texcoord + float2(0,-1) * texel * 2.0);
                farSum += SampleAlpha(i.texcoord + float2(0.7071,0.7071) * texel * 2.0);
                farSum += SampleAlpha(i.texcoord + float2(-0.7071,0.7071) * texel * 2.0);
                farSum += SampleAlpha(i.texcoord + float2(0.7071,-0.7071) * texel * 2.0);
                farSum += SampleAlpha(i.texcoord + float2(-0.7071,-0.7071) * texel * 2.0);
                float surroundingAlpha = (nearSum * 0.68 + farSum * 0.32) / 8.0;
                float glowAlpha = saturate(surroundingAlpha * (1.0 - centerAlpha) * _GlowColor.a * i.color.a * 4.0 * _GlowIntensity);
                #ifdef UNITY_UI_CLIP_RECT
                glowAlpha *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif
                fixed4 source = tex2D(_MainTex, i.texcoord) * i.color;
                float bodyAlpha = saturate(source.a + centerAlpha * 0.24);
                float3 emittedSource = saturate(source.rgb + (_GlowColor.rgb * i.color.rgb) * centerAlpha * 0.65);
                float combinedAlpha = bodyAlpha + glowAlpha * (1.0 - bodyAlpha);
                float3 combinedRgb = emittedSource * bodyAlpha + (_GlowColor.rgb * i.color.rgb) * glowAlpha * (1.0 - bodyAlpha);
                combinedRgb /= max(combinedAlpha, 0.0001);
                #ifdef UNITY_UI_ALPHACLIP
                clip(combinedAlpha - 0.001);
                #endif
                return fixed4(combinedRgb, combinedAlpha);
            }
            ENDCG
        }
    }
}
