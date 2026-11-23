Shader "Spotlight/WorldRing"
{
    Properties
    {
        [HDR] _Color ("Ring Color", Color) = (1,1,1,1)
        _EmissionIntensity ("Glow Intensity", Range(0,8)) = 3.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        Blend One One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float _EmissionIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return float4(_Color.rgb * _Color.a * _EmissionIntensity, _Color.a);
            }
            ENDCG
        }
    }
}
