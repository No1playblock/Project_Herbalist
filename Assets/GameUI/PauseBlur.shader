Shader "Herbalist/UI/PauseBlur" {
Properties { _MainTex("Texture",2D)="white"{} _Radius("Blur radius",Float)=5 }
SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"} Cull Off ZWrite Off ZTest Always
Pass { CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
sampler2D _MainTex;float4 _MainTex_TexelSize;float _Radius;
v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
fixed4 frag(v2f i):SV_Target{
float2 d=_MainTex_TexelSize.xy*_Radius;
fixed4 c=tex2D(_MainTex,i.uv)*4;
c+=tex2D(_MainTex,i.uv+float2(d.x,0))*2+tex2D(_MainTex,i.uv-float2(d.x,0))*2;
c+=tex2D(_MainTex,i.uv+float2(0,d.y))*2+tex2D(_MainTex,i.uv-float2(0,d.y))*2;
c+=tex2D(_MainTex,i.uv+d)+tex2D(_MainTex,i.uv-d);
c+=tex2D(_MainTex,i.uv+float2(d.x,-d.y))+tex2D(_MainTex,i.uv+float2(-d.x,d.y));
return c/16*i.color;
}
ENDCG } } }
