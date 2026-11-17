Shader "UI/SpotlightCutout"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Dim Overlay Color", Color) = (0.018,0.016,0.012,1)
        _Center ("Spotlight Center", Vector) = (0.51,0.59,0,0)
        _Radii ("Spotlight Radii", Vector) = (0.06,0.045,0,0)
        _Feather ("Cutout Edge Feather", Range(0.001,0.2)) = 0.075
        _DimAlpha ("Background Dim", Range(0,1)) = 0.74
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float4 _Center;
            float4 _Radii;
            float _Feather;
            float _DimAlpha;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 radii = max(_Radii.xy, float2(0.001, 0.001));
                float2 p = (i.uv - _Center.xy) / radii;
                float d = length(p);
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 tint = tex * i.color * _Color;
                float outside = smoothstep(1.0 - _Feather, 1.0 + _Feather, d);
                return fixed4(tint.rgb, outside * _DimAlpha * tint.a);
            }
            ENDCG
        }
    }
}
