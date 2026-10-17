// Made with Amplify Shader Editor v1.9.9.12
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ToonScapes/URP/Stone"
{
	Properties
	{
		[Header(Color)][Space(8)][Toggle( _ENABLECOLORTINT_ON )] _EnableColorTint( "Enable Color Tint", Float ) = 1
		[Space(8)] _BaseTint( "Base Tint", Color ) = ( 1, 1, 1, 1 )
		[Header(Textures)][NoScaleOffset][SingleLineTexture][Space(8)] _TextureRamp( "Texture Ramp", 2D ) = "white" {}
		_RampScale( "Ramp Scale", Range( 0, 1 ) ) = 0.5
		_RampOffset( "Ramp Offset", Range( 0, 1 ) ) = 0.5
		[Space(8)][Toggle( _ENABLEMACRONORMALS_ON )] _EnableMacroNormals( "Enable Macro Normals", Float ) = 0
		[NoScaleOffset][Normal][SingleLineTexture] _MacroNormalMap( "Macro Normal Map", 2D ) = "white" {}
		_MacroNormalScale( "Macro Normal Scale", Range( 0, 5 ) ) = 1
		[NoScaleOffset][SingleLineTexture][Space (8)] _SurfaceTexture( "Surface Texture", 2D ) = "white" {}
		_SurfaceTextureTiling( "Surface Texture Tiling", Range( 0.001, 512 ) ) = 2
		[NoScaleOffset][Normal][SingleLineTexture][Space(8)] _SurfaceNormalMap( "Surface Normal Map", 2D ) = "white" {}
		_SurfaceNormalScale( "Surface Normal Scale", Range( 0, 5 ) ) = 1
		[NoScaleOffset][SingleLineTexture][Space(8)] _OverlayTexture( "Overlay Texture", 2D ) = "white" {}
		_OverlayTextureTiling( "Overlay Texture Tiling", Range( 0, 20 ) ) = 10.09175
		[NoScaleOffset][Normal][SingleLineTexture][Space(8)] _OverlayNormalMap( "Overlay Normal Map", 2D ) = "white" {}
		_OverlayNormalScale( "Overlay Normal Scale", Range( 0, 5 ) ) = 1
		[NoScaleOffset][SingleLineTexture][Space (8)] _DetailMask( "Detail Mask", 2D ) = "white" {}
		[Header(Coverage Options)][Space(8)][Toggle( _ENABLECOVERAGE_ON )] _EnableCoverage( "Enable Coverage", Float ) = 0
		[Space(8)] _OverlayCoverage( "Overlay Coverage", Range( 0, 1 ) ) = 0.001
		_CoverageFalloff( "Coverage Falloff", Range( 1, 12 ) ) = 2
		[KeywordEnum( SurfaceNormalMap,OverlayNormalMap )] _CoverageMaskSource( "Coverage Mask Source", Float ) = 0
		_TransitionStart( "Transition Start", Range( 0, 1 ) ) = 0.65
		_TransitionEnd( "Transition End", Range( 0, 1 ) ) = 0.35
		[Space(8)] _EdgeWearTint( "Edge Wear Tint", Color ) = ( 0, 0, 0, 0 )
		_EdgeWear1( "Edge Wear", Range( 0, 1 ) ) = 1
		[Space(8)] _CavityTint( "Cavity Tint", Color ) = ( 0, 0, 0, 0 )
		_Cavity( "Cavity ", Range( 0, 1 ) ) = 1
		[Header(Specular Highlights)][Space(8)] _SpecularColor( "Specular Color", Color ) = ( 0, 0, 0, 0 )
		_SpecularIntensity1( "Specular Intensity", Range( 0, 1 ) ) = 0
		_Smoothness( "Smoothness", Range( 0, 1 ) ) = 0
		[Space(8)][Toggle( _ENABLESECONDARYHIGHLIGHTS_ON )] _EnableSecondaryHighlights( "Enable Secondary Highlights", Float ) = 1
		[Space(8)] _SecondarySpecularIntensity( "Secondary Specular Intensity", Range( 0, 1 ) ) = 0.5
		_SecondarySpecularSize( "Secondary Specular Size", Range( 0, 1 ) ) = 0
		_SecondarySmoothness( "Secondary Smoothness", Range( 0.001, 1 ) ) = 1
		[Header (Emission)][Space (8)][Toggle( _ENABLEEMISSION_ON )] _EnableEmission( "Enable Emission", Float ) = 1
		[HDR][Space(8)] _EmissionColor( "Emission Color", Color ) = ( 0, 0, 0, 0 )
		_EmissionIntensity1( "Emission Intensity", Float ) = 1
		_FlickerFrequency( "Flicker Frequency", Float ) = 1
		_FlickerScale( "Flicker Scale", Float ) = 1
		_MinIntensity( "Min Intensity", Float ) = 0.75
		_MaxIntensity( "Max Intensity", Float ) = 1
		[Header(Surface Options)][Space(8)] _Occlusion1( "Occlusion", Range( 0, 1 ) ) = 1
		_AdditionalLightInfluence( "Additional Light Influence", Range( 0, 1 ) ) = 0.5
		_AdditionalLightFalloff( "Additional Light Falloff", Range( 0, 12 ) ) = 1


		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5

		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		//_InstancedTerrainNormals("Instanced Terrain Normals", Float) = 1.0

		//[ToggleOff(_SPECULARHIGHLIGHTS_OFF)] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0
		[ToggleOff] _ScreenSpaceReflections("Screen Space Reflections", Float) = 1.0
		[ToggleOff] _ScreenSpaceReflectionsContributeTransparent("Screen Space Reflections Contribute Transparent", Float) = 1.0
		[HideInInspector][ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		//[HideInInspector][ToggleUI] _AddPrecomputedVelocity("Add Precomputed Velocity", Float) = 1
		//[HideInInspector] _XRMotionVectorsPass("_XRMotionVectorsPass", Float) = 1

		//[HideInInspector] _AlphaClip("__clip", Float) = 0.0
	}

	SubShader
	{
		PackageRequirements
		{
			"com.unity.render-pipelines.universal": "[17.0,18.0]"
		}

		

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" "UniversalMaterialType"="Lit" }

	LOD 0

		Cull Back
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#define ASE_ADJUST_CLIP_POSITION( x ) x

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SPECULAR_SETUP 1
			#define _NORMAL_DROPOFF_TS 1
			#pragma shader_feature_local_fragment _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170100


			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#if ( UNITY_VERSION >= 60010000 )
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
			#endif

			#if defined(UNITY_PLATFORM_META_QUEST) && ( UNITY_VERSION >= 60050000 )
            #pragma multi_compile _ META_QUEST_LIGHTUNROLL
            #endif

			#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
			#if ( UNITY_VERSION >= 60030000 )
			#pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
			#endif
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile _ _LIGHT_LAYERS
			#pragma multi_compile_fragment _ _LIGHT_COOKIES
			#if ( UNITY_VERSION >= 60010000 )
			#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
			#else
			#pragma multi_compile _ _FORWARD_PLUS
			#endif

            #if defined(UNITY_PLATFORM_META_QUEST) && ( UNITY_VERSION >= 60050000 )
            #pragma multi_compile _ META_QUEST_ORTHO_PROJ
            #pragma multi_compile _ META_QUEST_NO_SPOTLIGHTS_LIGHT_LOOP
            #endif

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#if ( UNITY_VERSION >= 60010000 )
			#pragma multi_compile _ LIGHTMAP_BICUBIC_SAMPLING
			#endif
			#if ( UNITY_VERSION >= 60030000 )
			#pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
			#endif
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile _ USE_LEGACY_LIGHTMAPS

			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_FORWARD

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#if ( UNITY_VERSION >= 60010000 )
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
			#else
			#pragma multi_compile_fog
			#endif
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#if defined( UNITY_INSTANCING_ENABLED ) && defined( ASE_INSTANCED_TERRAIN ) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_NORMAL
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_WORLD_VIEW_DIR
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#pragma shader_feature_local _ENABLESECONDARYHIGHLIGHTS_ON
			#pragma shader_feature_local _ENABLECOVERAGE_ON
			#pragma shader_feature_local _COVERAGEMASKSOURCE_SURFACENORMALMAP _COVERAGEMASKSOURCE_OVERLAYNORMALMAP
			#pragma shader_feature_local _ENABLEMACRONORMALS_ON
			#pragma shader_feature_local _ENABLECOLORTINT_ON
			#pragma shader_feature_local _ENABLEEMISSION_ON


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			#if ( UNITY_VERSION < 60010000 )
				#define USE_CLUSTER_LIGHT_LOOP USE_FORWARD_PLUS
				#define CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
				float4 lightmapUVOrVertexSH : TEXCOORD3;
				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					half4 fogFactorAndVertexLight : TEXCOORD4;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
					float2 dynamicLightmapUV : TEXCOORD5;
				#endif
				#if defined(USE_APV_PROBE_OCCLUSION)
					float4 probeOcclusion : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord8 : TEXCOORD8;
				float4 ase_texcoord9 : TEXCOORD9;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _CavityTint;
			float4 _BaseTint;
			float4 _EdgeWearTint;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion1;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _AdditionalLightFalloff;
			float _Cavity;
			float _EdgeWear1;
			float _RampOffset;
			float _RampScale;
			float _SecondarySmoothness;
			float _MacroNormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _OverlayCoverage;
			float _OverlayNormalScale;
			float _SurfaceNormalScale;
			float _TransitionEnd;
			float _TransitionStart;
			float _OverlayTextureTiling;
			float _CoverageFalloff;
			float _SurfaceTextureTiling;
			float _MaxIntensity;
			float _MinIntensity;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _SurfaceTexture;
			sampler2D _OverlayTexture;
			sampler2D _SurfaceNormalMap;
			sampler2D _OverlayNormalMap;
			sampler2D _DetailMask;
			sampler2D _MacroNormalMap;
			sampler2D _TextureRamp;


			inline float4 TriplanarSampling537( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
			}
			
			inline float4 TriplanarSampling573( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
			}
			
			inline float3 TriplanarSampling591( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				xNorm.xyz  = half3( UnpackNormalScale( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
				yNorm.xyz  = half3( UnpackNormalScale( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				zNorm.xyz  = half3( UnpackNormalScale( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
				yNormN.xyz = half3( UnpackNormalScale( yNormN, normalScale.z ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
			}
			
			inline float3 TriplanarSampling594( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				xNorm.xyz  = half3( UnpackNormalScale( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
				yNorm.xyz  = half3( UnpackNormalScale( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				zNorm.xyz  = half3( UnpackNormalScale( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
				yNormN.xyz = half3( UnpackNormalScale( yNormN, normalScale.z ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
			}
			
			half3 ASEIndirectDiffuse( PackedVaryings input, half3 normalWS, float3 positionWS, half3 viewDirWS )
			{
			#if defined( DYNAMICLIGHTMAP_ON )
				return SAMPLE_GI( input.lightmapUVOrVertexSH.xy, input.dynamicLightmapUV.xy, 0, normalWS );
			#elif defined( LIGHTMAP_ON )
				return SAMPLE_GI( input.lightmapUVOrVertexSH.xy, 0, normalWS );
			#elif defined( PROBE_VOLUMES_L1 ) || defined( PROBE_VOLUMES_L2 )
				return SampleProbeVolumePixel( SampleSH( normalWS ), positionWS, normalWS, viewDirWS, input.positionCS.xy );
			#else
				return SampleSH( normalWS );
			#endif
			}
			
			half4 CalculateShadowMask1_g61183( half2 LightmapUV )
			{
				#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
				return SAMPLE_SHADOWMASK( LightmapUV.xy );
				#elif !defined (LIGHTMAP_ON)
				return unity_ProbesOcclusion;
				#else
				return half4( 1, 1, 1, 1 );
				#endif
			}
			
			float3 AdditionalLightsLambertMask17x( float3 WorldPosition, float2 ScreenUV, float3 WorldNormal, float4 ShadowMask )
			{
				#if ( UNITY_VERSION < 60010000 ) && !defined( USE_CLUSTER_LIGHT_LOOP )
					#define USE_CLUSTER_LIGHT_LOOP USE_FORWARD_PLUS
				#endif
				#if ( UNITY_VERSION < 60010000 ) && !defined( CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK )
					#define CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
				#endif
				float3 Color = 0;
				#if defined(_ADDITIONAL_LIGHTS)
					#if ( UNITY_VERSION >= 60010000 )
						#define CALC_LAMBERT(Light) LightingLambert( AttLightColor, Light.direction, WorldNormal )
					#else
						#define CALC_LAMBERT(Light) ( dot( Light.direction, WorldNormal ) * 0.5 + 0.5 )* AttLightColor
					#endif
					#define SUM_LIGHTLAMBERT(Light)\
						half3 AttLightColor = Light.color * ( Light.distanceAttenuation * Light.shadowAttenuation );\
						Color += CALC_LAMBERT(Light);
					InputData inputData = (InputData)0;
					inputData.normalizedScreenSpaceUV = ScreenUV;
					inputData.positionWS = WorldPosition;
					uint meshRenderingLayers = GetMeshRenderingLayer();
					uint pixelLightCount = GetAdditionalLightsCount();	
					#if USE_CLUSTER_LIGHT_LOOP
					[loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
					{
						CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					}
					#endif
					LIGHT_LOOP_BEGIN( pixelLightCount )
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					LIGHT_LOOP_END
				#endif
				return Color;
			}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 ase_bitangentOS = cross( input.normalOS, input.tangentOS.xyz ) * input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				output.ase_texcoord8.xyz = ase_bitangentOS;
				
				output.ase_texcoord7 = input.positionOS;
				output.ase_normal = input.normalOS;
				output.ase_tangent = input.tangentOS;
				output.ase_texcoord9.xy = input.texcoord.xy;
				output.ase_texcoord9.zw = input.texcoord1.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord8.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif
				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				#ifdef ASE_CUSTOM_MOTION_VECTOR
					// Declared so the Motion Vector output port surfaces on the master node; only consumed by the motion vector passes.
					float3 aseCustomMotionVector = float3(0, 0, 0);
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy);
				#if defined(DYNAMICLIGHTMAP_ON)
					output.dynamicLightmapUV.xy = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				OUTPUT_SH4(vertexInput.positionWS, normalInput.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.lightmapUVOrVertexSH.xyz, output.probeOcclusion);

				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					output.fogFactorAndVertexLight = 0;
					#if defined(ASE_FOG) && !defined(_FOG_FRAGMENT)
						output.fogFactorAndVertexLight.x = ComputeFogFactor(vertexInput.positionCS.z);
					#endif
					#ifdef _ADDITIONAL_LIGHTS_VERTEX
						half3 vertexLight = VertexLighting( vertexInput.positionWS, normalInput.normalWS );
						output.fogFactorAndVertexLight.yzw = vertexLight;
					#endif
				#endif

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					output.tangentWS.zw = input.texcoord.xy;
					output.tangentWS.xy = input.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = input.texcoord1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = input.texcoord2;
				#endif
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				#endif
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag ( PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						#if ( UNITY_VERSION >= 60020000 )
						, out uint outRenderingLayers : SV_Target1
						#else
						, out float4 outRenderingLayers : SV_Target1
						#endif
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				#if defined( _SURFACE_TYPE_TRANSPARENT )
					const bool isTransparent = true;
				#else
					const bool isTransparent = false;
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					float4 shadowCoord = TransformWorldToShadowCoord( input.positionWS );
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float3 ViewDirWS = GetWorldSpaceNormalizeViewDir( PositionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float2 sampleCoords = (input.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					NormalWS = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					TangentWS = -cross(GetObjectToWorldMatrix()._13_23_33, NormalWS);
					BitangentWS = cross(NormalWS, -TangentWS);
				#endif

				float SurfaceTextureTiling218 = ( 1.0 / _SurfaceTextureTiling );
				float2 temp_cast_0 = (SurfaceTextureTiling218).xx;
				float CoverageFalloff829 = _CoverageFalloff;
				float4 triplanar537 = TriplanarSampling537( _SurfaceTexture, float4( 1, 1, 0, 0 ), _SurfaceTexture, float4( 1, 1, 0, 0 ), _SurfaceTexture, float4( 1, 1, 0, 0 ), input.ase_texcoord7.xyz, input.ase_normal, CoverageFalloff829, temp_cast_0, float3( 1,1,1 ), float3(0,0,0) );
				float OverlayTextureTiling172 = ( 1.0 / _OverlayTextureTiling );
				float2 temp_cast_1 = (OverlayTextureTiling172).xx;
				float4 triplanar573 = TriplanarSampling573( _OverlayTexture, float4( 1, 1, 0, 0 ), _OverlayTexture, float4( 1, 1, 0, 0 ), _OverlayTexture, float4( 1, 1, 0, 0 ), PositionWS, NormalWS, CoverageFalloff829, temp_cast_1, float3( 1,1,1 ), float3(0,0,0) );
				float2 temp_cast_2 = (SurfaceTextureTiling218).xx;
				float3x3 ase_worldToTangent = float3x3( TangentWS, BitangentWS, NormalWS );
				float3 ase_bitangentOS = input.ase_texcoord8.xyz;
				float3x3 objectToTangent = float3x3(input.ase_tangent.xyz, ase_bitangentOS, input.ase_normal);
				float SurfaceNormalMapScale258 = _SurfaceNormalScale;
				float3 temp_cast_3 = (SurfaceNormalMapScale258).xxx;
				float3 triplanar591 = TriplanarSampling591( _SurfaceNormalMap, float4( 1, 1, 0, 0 ), _SurfaceNormalMap, float4( 1, 1, 0, 0 ), _SurfaceNormalMap, float4( 1, 1, 0, 0 ), input.ase_texcoord7.xyz, input.ase_normal, CoverageFalloff829, temp_cast_2, temp_cast_3, float3(0,0,0) );
				float3 tanTriplanarNormal591 = mul( objectToTangent, triplanar591 );
				float3 TriNormalMain759 = tanTriplanarNormal591;
				float2 temp_cast_4 = (OverlayTextureTiling172).xx;
				float OverlayNormalMapScale261 = _OverlayNormalScale;
				float3 temp_cast_5 = (OverlayNormalMapScale261).xxx;
				float3 triplanar594 = TriplanarSampling594( _OverlayNormalMap, float4( 1, 1, 0, 0 ), _OverlayNormalMap, float4( 1, 1, 0, 0 ), _OverlayNormalMap, float4( 1, 1, 0, 0 ), PositionWS, NormalWS, CoverageFalloff829, temp_cast_4, temp_cast_5, float3(0,0,0) );
				float3 tanTriplanarNormal594 = mul( ase_worldToTangent, triplanar594 );
				float3 TriNormalOverlay758 = tanTriplanarNormal594;
				#if defined( _COVERAGEMASKSOURCE_SURFACENORMALMAP )
				float3 staticSwitch755 = TriNormalMain759;
				#elif defined( _COVERAGEMASKSOURCE_OVERLAYNORMALMAP )
				float3 staticSwitch755 = TriNormalOverlay758;
				#else
				float3 staticSwitch755 = TriNormalMain759;
				#endif
				float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
				float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
				float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
				float3 tanNormal324 = staticSwitch755;
				float3 worldNormal324 = float3( dot( tanToWorld0, tanNormal324 ), dot( tanToWorld1, tanNormal324 ), dot( tanToWorld2, tanNormal324 ) );
				float2 uv_DetailMask329 = input.ase_texcoord9.xy;
				float smoothstepResult544 = smoothstep( _TransitionStart , _TransitionEnd , ( ( worldNormal324.y +  (-1.0 + ( _OverlayCoverage - 0.0 ) * ( 2.0 - -1.0 ) / ( 1.0 - 0.0 ) ) ) * ( 1.0 - tex2D( _DetailMask, uv_DetailMask329 ).a ) ));
				float OverlayCoverage340 = saturate( smoothstepResult544 );
				float lerpResult580 = lerp( triplanar573.w , triplanar537.a , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float staticSwitch796 = lerpResult580;
				#else
				float staticSwitch796 = triplanar537.w;
				#endif
				float BlendedAlpha496 = staticSwitch796;
				float3 temp_output_723_0 = ( _SpecularColor.rgb * BlendedAlpha496 * _SecondarySpecularIntensity );
				float3 normalizeResult4_g61171 = normalize( ( ViewDirWS + SafeNormalize( _MainLightPosition.xyz ) ) );
				float3 lerpResult590 = lerp( tanTriplanarNormal594 , tanTriplanarNormal591 , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float3 staticSwitch800 = lerpResult590;
				#else
				float3 staticSwitch800 = tanTriplanarNormal591;
				#endif
				float2 uv_MacroNormalMap806 = input.ase_texcoord9.xy;
				float MacroNormalMapScale808 = _MacroNormalScale;
				float3 unpack806 = UnpackNormalScale( tex2D( _MacroNormalMap, uv_MacroNormalMap806 ), MacroNormalMapScale808 );
				unpack806.z = lerp( 1, unpack806.z, saturate(MacroNormalMapScale808) );
				#ifdef _ENABLEMACRONORMALS_ON
				float3 staticSwitch810 = BlendNormal( staticSwitch800 , unpack806 );
				#else
				float3 staticSwitch810 = staticSwitch800;
				#endif
				float3 BlendedNormals169 = staticSwitch810;
				float3 tanNormal442 = BlendedNormals169;
				float3 worldNormal442 = normalize( float3( dot( tanToWorld0, tanNormal442 ), dot( tanToWorld1, tanNormal442 ), dot( tanToWorld2, tanNormal442 ) ) );
				float3 normalizeResult443 = normalize( worldNormal442 );
				float3 Normals444 = normalizeResult443;
				float dotResult718 = dot( normalizeResult4_g61171 , Normals444 );
				float temp_output_855_0 = ( 1.0 - _SecondarySmoothness );
				float3 temp_output_726_0 = saturate( ( saturate( (  (-1.0 + ( _SecondarySpecularSize - 0.0 ) * ( -0.5 - -1.0 ) / ( 1.0 - 0.0 ) ) + dotResult718 ) ) / ( ( 1.0 - temp_output_723_0 ) * temp_output_855_0 ) ) );
				float3 DirectSpecHighlights463 = ( (temp_output_723_0).xyz * temp_output_726_0 );
				float ase_lightIntensity = max( max( _MainLightColor.r, _MainLightColor.g ), _MainLightColor.b ) + 1e-7;
				float4 ase_lightColor = float4( _MainLightColor.rgb / ase_lightIntensity, ase_lightIntensity );
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoord );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float3 bakedGI692 = ASEIndirectDiffuse( input, NormalWS, PositionWS, ViewDirWS );
				MixRealtimeAndBakedGI( ase_mainLight, NormalWS, bakedGI692, half4( 0, 0, 0, 0 ) );
				float4 HalfLambert695 = ( ( ase_lightColor * ase_lightAtten ) + float4( bakedGI692 , 0.0 ) );
				float4 SpecularHighlights457 = ( float4( DirectSpecHighlights463 , 0.0 ) * HalfLambert695 );
				#ifdef _ENABLESECONDARYHIGHLIGHTS_ON
				float4 staticSwitch456 = SpecularHighlights457;
				#else
				float4 staticSwitch456 = float4( 0,0,0,0 );
				#endif
				float4 color450 = IsGammaSpace() ? float4( 1, 1, 1, 1 ) : float4( 1, 1, 1, 1 );
				#ifdef _ENABLECOLORTINT_ON
				float4 staticSwitch451 = _BaseTint;
				#else
				float4 staticSwitch451 = color450;
				#endif
				float dotResult684 = dot( Normals444 , SafeNormalize( _MainLightPosition.xyz ) );
				float RampScale710 = _RampScale;
				float RampOffset709 = _RampOffset;
				float CEL_Effect437 = saturate( (dotResult684*RampScale710 + RampOffset709) );
				float2 temp_cast_8 = (CEL_Effect437).xx;
				float4 lerpResult543 = lerp( triplanar573 , triplanar537 , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float4 staticSwitch795 = lerpResult543;
				#else
				float4 staticSwitch795 = triplanar537;
				#endif
				float2 uv_DetailMask525 = input.ase_texcoord9.xy;
				float4 tex2DNode525 = tex2D( _DetailMask, uv_DetailMask525 );
				float temp_output_578_0 = saturate( tex2DNode525.r );
				float4 lerpResult814 = lerp( staticSwitch795 , ( _EdgeWearTint * temp_output_578_0 ) , ( _EdgeWear1 * temp_output_578_0 ));
				float temp_output_584_0 = saturate( tex2DNode525.b );
				float4 lerpResult542 = lerp( lerpResult814 , ( _CavityTint * temp_output_584_0 ) , ( _Cavity * temp_output_584_0 ));
				float4 BlendedTextures167 = lerpResult542;
				float3 WorldPosition288_g61181 = PositionWS;
				float3 WorldPosition305_g61181 = WorldPosition288_g61181;
				float2 ScreenUV286_g61181 = (ScreenPosNorm).xy;
				float2 ScreenUV305_g61181 = ScreenUV286_g61181;
				float3 WorldNormal281_g61181 = Normals444;
				float3 WorldNormal305_g61181 = WorldNormal281_g61181;
				half2 LightmapUV1_g61183 = (input.ase_texcoord9.zw*(unity_LightmapST).xy + (unity_LightmapST).zw);
				half4 localCalculateShadowMask1_g61183 = CalculateShadowMask1_g61183( LightmapUV1_g61183 );
				float4 ShadowMask360_g61181 = localCalculateShadowMask1_g61183;
				float4 ShadowMask305_g61181 = ShadowMask360_g61181;
				float3 localAdditionalLightsLambertMask17x305_g61181 = AdditionalLightsLambertMask17x( WorldPosition305_g61181 , ScreenUV305_g61181 , WorldNormal305_g61181 , ShadowMask305_g61181 );
				float3 saferPower843 = abs( saturate( localAdditionalLightsLambertMask17x305_g61181 ) );
				float3 temp_cast_10 = ( (0.001 + ( _AdditionalLightFalloff - 0.0 ) * ( 12.0 - 0.001 ) / ( 12.0 - 0.0 ) )).xxx;
				float4 CustomLighting109 = ( staticSwitch456 + ( staticSwitch451 * ( ( ( tex2D( _TextureRamp, temp_cast_8 ) * BlendedTextures167 ) * HalfLambert695 ) + ( BlendedTextures167 * tex2D( _TextureRamp, (pow( saferPower843 , temp_cast_10 )* (0.001 + ( _AdditionalLightInfluence - 0.0 ) * ( 6.0 - 0.001 ) / ( 1.0 - 0.0 ) ) + RampOffset709).xy ) ) ) ) );
				
				float3 SpecularTint825 = _SpecularColor.rgb;
				float2 uv_SurfaceTexture847 = input.ase_texcoord9.xy;
				
				float3 EmissionColor396 = _EmissionColor.rgb;
				float2 uv_DetailMask393 = input.ase_texcoord9.xy;
				float EmissionColorAlpha398 = _EmissionColor.a;
				float mulTime782 = _TimeParameters.x * _FlickerFrequency;
				#ifdef _ENABLEEMISSION_ON
				float3 staticSwitch405 = ( ( _EmissionIntensity1 * EmissionColor396 * tex2D( _DetailMask, uv_DetailMask393 ).g * EmissionColorAlpha398 ) * ( ( sin( ( ( mulTime782 + ( PositionWS.x + PositionWS.z ) ) / _FlickerScale ) ) * ( ( _MaxIntensity - _MinIntensity ) * 0.5 ) ) + ( ( _MaxIntensity + _MinIntensity ) * 0.5 ) ) );
				#else
				float3 staticSwitch405 = float3( 0,0,0 );
				#endif
				float3 Emission788 = staticSwitch405;
				

				float3 BaseColor = CustomLighting109.rgb;
				float3 Normal = BlendedNormals169;
				float3 Specular = ( _SpecularIntensity1 * SpecularTint825 * tex2D( _SurfaceTexture, uv_SurfaceTexture847 ).a );
				float Metallic = 0;
				float Smoothness = _Smoothness;
				float Occlusion = _Occlusion1;
				float3 Emission = Emission788;
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
					float AlphaClipThresholdShadow = 0.5;
				#endif
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _CLEARCOAT
					float CoatMask = 0;
					float CoatSmoothness = 0;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_CHANGES_WORLD_POS)
					ShadowCoord = TransformWorldToShadowCoord( PositionWS );
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = PositionWS;
				inputData.positionCS = input.positionCS;
				inputData.normalizedScreenSpaceUV = ScreenPosNorm.xy;
				inputData.viewDirectionWS = ViewDirWS;
				inputData.shadowCoord = ShadowCoord;

				#ifdef _NORMALMAP
						#if _NORMAL_DROPOFF_TS
							inputData.normalWS = TransformTangentToWorld(Normal, half3x3(TangentWS, BitangentWS, NormalWS));
						#elif _NORMAL_DROPOFF_OS
							inputData.normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							inputData.normalWS = Normal;
						#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = NormalWS;
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
				#endif
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#endif

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = input.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(_SCREEN_SPACE_IRRADIANCE) && ( UNITY_VERSION >= 60030000 )
					#if ( UNITY_VERSION >= 60060000 )
						inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy, inputData.normalWS));
					#else
						inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy);
					#endif
				#elif defined(DYNAMICLIGHTMAP_ON)
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, input.dynamicLightmapUV.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
					inputData.bakedGI = SAMPLE_GI( SH, GetAbsolutePositionWS(inputData.positionWS),
						inputData.normalWS,
						inputData.viewDirectionWS,
						input.positionCS.xy,
						input.probeOcclusion,
						inputData.shadowMask );
				#else
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
					#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = input.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
					#if defined(USE_APV_PROBE_OCCLUSION)
						inputData.probeOcclusion = input.probeOcclusion;
					#endif
				#endif

				SurfaceData surfaceData;
				surfaceData.albedo              = BaseColor;
				surfaceData.metallic            = saturate(Metallic);
				surfaceData.specular            = Specular;
				surfaceData.smoothness          = saturate(Smoothness),
				surfaceData.occlusion           = Occlusion,
				surfaceData.emission            = Emission,
				surfaceData.alpha               = saturate(Alpha);
				surfaceData.normalTS            = Normal;
				surfaceData.clearCoatMask       = 0;
				surfaceData.clearCoatSmoothness = 1;

				#ifdef _CLEARCOAT
					surfaceData.clearCoatMask       = saturate(CoatMask);
					surfaceData.clearCoatSmoothness = saturate(CoatSmoothness);
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
				#endif

				#ifdef ASE_LIGHTING_SIMPLE
					half4 color = UniversalFragmentBlinnPhong( inputData, surfaceData);
				#else
					half4 color = UniversalFragmentPBR( inputData, surfaceData);
				#endif

				#ifdef ASE_TRANSMISSION
				{
					float shadow = _TransmissionShadow;

					#define SUM_LIGHT_TRANSMISSION(Light)\
						float3 atten = Light.color * Light.distanceAttenuation;\
						atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
						half3 transmission = max( 0, -dot( inputData.normalWS, Light.direction ) ) * atten * Transmission;\
						color.rgb += BaseColor * transmission;

					SUM_LIGHT_TRANSMISSION( GetMainLight( inputData.shadowCoord ) );

					#if defined(_ADDITIONAL_LIGHTS)
						uint meshRenderingLayers = GetMeshRenderingLayer();
						uint pixelLightCount = GetAdditionalLightsCount();
						#if USE_CLUSTER_LIGHT_LOOP
							[loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
							{
								CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK

								Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
								#ifdef _LIGHT_LAYERS
								if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
								#endif
								{
									SUM_LIGHT_TRANSMISSION( light );
								}
							}
						#endif
						LIGHT_LOOP_BEGIN( pixelLightCount )
							Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
							#ifdef _LIGHT_LAYERS
							if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
							#endif
							{
								SUM_LIGHT_TRANSMISSION( light );
							}
						LIGHT_LOOP_END
					#endif
				}
				#endif

				#ifdef ASE_TRANSLUCENCY
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					#define SUM_LIGHT_TRANSLUCENCY(Light)\
						float3 atten = Light.color * Light.distanceAttenuation;\
						atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
						half3 lightDir = Light.direction + inputData.normalWS * normal;\
						half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );\
						half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;\
						color.rgb += BaseColor * translucency * strength;

					SUM_LIGHT_TRANSLUCENCY( GetMainLight( inputData.shadowCoord ) );

					#if defined(_ADDITIONAL_LIGHTS)
						uint meshRenderingLayers = GetMeshRenderingLayer();
						uint pixelLightCount = GetAdditionalLightsCount();
						#if USE_CLUSTER_LIGHT_LOOP
							[loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
							{
								CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK

								Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
								#ifdef _LIGHT_LAYERS
								if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
								#endif
								{
									SUM_LIGHT_TRANSLUCENCY( light );
								}
							}
						#endif
						LIGHT_LOOP_BEGIN( pixelLightCount )
							Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
							#ifdef _LIGHT_LAYERS
							if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
							#endif
							{
								SUM_LIGHT_TRANSLUCENCY( light );
							}
						LIGHT_LOOP_END
					#endif
				}
				#endif

				#ifdef ASE_REFRACTION
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( NormalWS,0 ) ).xyz * ( 1.0 - dot( NormalWS, ViewDirWS ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3(0,0,0), inputData.fogCoord);
					#else
						color.rgb = MixFog(color.rgb, inputData.fogCoord);
					#endif
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					#if ( UNITY_VERSION >= 60020000 )
					outRenderingLayers = EncodeMeshRenderingLayer();
					#else
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
					#endif
				#endif

				#if defined( ASE_OPAQUE_KEEP_ALPHA )
					return half4( color.rgb, color.a );
				#else
					return half4( color.rgb, OutputAlpha( color.a, isTransparent ) );
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SPECULAR_SETUP 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170100


			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW // @diogo: removed _vertex for POM node

			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _CavityTint;
			float4 _BaseTint;
			float4 _EdgeWearTint;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion1;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _AdditionalLightFalloff;
			float _Cavity;
			float _EdgeWear1;
			float _RampOffset;
			float _RampScale;
			float _SecondarySmoothness;
			float _MacroNormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _OverlayCoverage;
			float _OverlayNormalScale;
			float _SurfaceNormalScale;
			float _TransitionEnd;
			float _TransitionStart;
			float _OverlayTextureTiling;
			float _CoverageFalloff;
			float _SurfaceTextureTiling;
			float _MaxIntensity;
			float _MinIntensity;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			float3 _LightDirection;
			float3 _LightPosition;

			
			PackedVaryings VertexFunction( Attributes input )
			{
				PackedVaryings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				float3 positionWS = TransformObjectToWorld( input.positionOS.xyz );
				float3 normalWS = TransformObjectToWorldDir(input.normalOS);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				float4 positionCS = ASE_ADJUST_CLIP_POSITION( TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS)) );

				//code for UNITY_REVERSED_Z is moved into Shadows.hlsl from 6000.0.22 and or higher
				positionCS = ApplyShadowClamping(positionCS);

				output.positionCS = positionCS;
				output.positionWS = positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(	PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				

				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
					float AlphaClipThresholdShadow = 0.5;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					#if defined( _ALPHATEST_SHADOW_ON )
						AlphaDiscard( Alpha, AlphaClipThresholdShadow );
					#else
						AlphaDiscard( Alpha, AlphaClipThreshold );
					#endif
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask R
			AlphaToMask Off

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SPECULAR_SETUP 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170100


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _CavityTint;
			float4 _BaseTint;
			float4 _EdgeWearTint;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion1;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _AdditionalLightFalloff;
			float _Cavity;
			float _EdgeWear1;
			float _RampOffset;
			float _RampScale;
			float _SecondarySmoothness;
			float _MacroNormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _OverlayCoverage;
			float _OverlayNormalScale;
			float _SurfaceNormalScale;
			float _TransitionEnd;
			float _TransitionStart;
			float _OverlayTextureTiling;
			float _CoverageFalloff;
			float _SurfaceTextureTiling;
			float _MaxIntensity;
			float _MinIntensity;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(	PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				

				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			#define ASE_GEOMETRY
			#define _SPECULAR_SETUP 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170100

			#pragma shader_feature EDITOR_VISUALIZATION

			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_FRAG_NORMAL
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES1
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES2
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#pragma shader_feature_local _ENABLESECONDARYHIGHLIGHTS_ON
			#pragma shader_feature_local _ENABLECOVERAGE_ON
			#pragma shader_feature_local _COVERAGEMASKSOURCE_SURFACENORMALMAP _COVERAGEMASKSOURCE_OVERLAYNORMALMAP
			#pragma shader_feature_local _ENABLEMACRONORMALS_ON
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#if UNITY_VERSION >= 60010000
			#pragma multi_compile _ LIGHTMAP_BICUBIC_SAMPLING
			#endif
			#if UNITY_VERSION >= 60010000
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
			#endif
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma shader_feature_local _ENABLECOLORTINT_ON
			#pragma shader_feature_local _ENABLEEMISSION_ON


			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				#ifdef EDITOR_VISUALIZATION
					float4 VizUV : TEXCOORD1;
					float4 LightCoord : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_texcoord8 : TEXCOORD8;
				float4 lightmapUVOrVertexSH : TEXCOORD9;
				float4 dynamicLightmapUV : TEXCOORD10;
				float4 ase_texcoord11 : TEXCOORD11;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _CavityTint;
			float4 _BaseTint;
			float4 _EdgeWearTint;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion1;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _AdditionalLightFalloff;
			float _Cavity;
			float _EdgeWear1;
			float _RampOffset;
			float _RampScale;
			float _SecondarySmoothness;
			float _MacroNormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _OverlayCoverage;
			float _OverlayNormalScale;
			float _SurfaceNormalScale;
			float _TransitionEnd;
			float _TransitionStart;
			float _OverlayTextureTiling;
			float _CoverageFalloff;
			float _SurfaceTextureTiling;
			float _MaxIntensity;
			float _MinIntensity;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _SurfaceTexture;
			sampler2D _OverlayTexture;
			sampler2D _SurfaceNormalMap;
			sampler2D _OverlayNormalMap;
			sampler2D _DetailMask;
			sampler2D _MacroNormalMap;
			sampler2D _TextureRamp;


			inline float4 TriplanarSampling537( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
			}
			
			inline float4 TriplanarSampling573( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
			}
			
			inline float3 TriplanarSampling591( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				xNorm.xyz  = half3( UnpackNormalScale( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
				yNorm.xyz  = half3( UnpackNormalScale( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				zNorm.xyz  = half3( UnpackNormalScale( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
				yNormN.xyz = half3( UnpackNormalScale( yNormN, normalScale.z ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
			}
			
			inline float3 TriplanarSampling594( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				xNorm.xyz  = half3( UnpackNormalScale( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
				yNorm.xyz  = half3( UnpackNormalScale( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				zNorm.xyz  = half3( UnpackNormalScale( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
				yNormN.xyz = half3( UnpackNormalScale( yNormN, normalScale.z ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
			}
			
			half3 ASEIndirectDiffuse( PackedVaryings input, half3 normalWS, float3 positionWS, half3 viewDirWS )
			{
			#if defined( DYNAMICLIGHTMAP_ON )
				return SAMPLE_GI( input.lightmapUVOrVertexSH.xy, input.dynamicLightmapUV.xy, 0, normalWS );
			#elif defined( LIGHTMAP_ON )
				return SAMPLE_GI( input.lightmapUVOrVertexSH.xy, 0, normalWS );
			#elif defined( PROBE_VOLUMES_L1 ) || defined( PROBE_VOLUMES_L2 )
				return SampleProbeVolumePixel( SampleSH( normalWS ), positionWS, normalWS, viewDirWS, input.positionCS.xy );
			#else
				return SampleSH( normalWS );
			#endif
			}
			
			half4 CalculateShadowMask1_g61183( half2 LightmapUV )
			{
				#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
				return SAMPLE_SHADOWMASK( LightmapUV.xy );
				#elif !defined (LIGHTMAP_ON)
				return unity_ProbesOcclusion;
				#else
				return half4( 1, 1, 1, 1 );
				#endif
			}
			
			float3 AdditionalLightsLambertMask17x( float3 WorldPosition, float2 ScreenUV, float3 WorldNormal, float4 ShadowMask )
			{
				#if ( UNITY_VERSION < 60010000 ) && !defined( USE_CLUSTER_LIGHT_LOOP )
					#define USE_CLUSTER_LIGHT_LOOP USE_FORWARD_PLUS
				#endif
				#if ( UNITY_VERSION < 60010000 ) && !defined( CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK )
					#define CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
				#endif
				float3 Color = 0;
				#if defined(_ADDITIONAL_LIGHTS)
					#if ( UNITY_VERSION >= 60010000 )
						#define CALC_LAMBERT(Light) LightingLambert( AttLightColor, Light.direction, WorldNormal )
					#else
						#define CALC_LAMBERT(Light) ( dot( Light.direction, WorldNormal ) * 0.5 + 0.5 )* AttLightColor
					#endif
					#define SUM_LIGHTLAMBERT(Light)\
						half3 AttLightColor = Light.color * ( Light.distanceAttenuation * Light.shadowAttenuation );\
						Color += CALC_LAMBERT(Light);
					InputData inputData = (InputData)0;
					inputData.normalizedScreenSpaceUV = ScreenUV;
					inputData.positionWS = WorldPosition;
					uint meshRenderingLayers = GetMeshRenderingLayer();
					uint pixelLightCount = GetAdditionalLightsCount();	
					#if USE_CLUSTER_LIGHT_LOOP
					[loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
					{
						CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					}
					#endif
					LIGHT_LOOP_BEGIN( pixelLightCount )
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					LIGHT_LOOP_END
				#endif
				return Color;
			}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord3.xyz = ase_normalWS;
				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord5.xyz = ase_tangentWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord6.xyz = ase_bitangentWS;
				float3 ase_bitangentOS = cross( input.normalOS, input.tangentOS.xyz ) * input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				output.ase_texcoord7.xyz = ase_bitangentOS;
				OUTPUT_LIGHTMAP_UV( input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy );
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				#if !defined( OUTPUT_SH4 )
				OUTPUT_SH( ase_positionWS, ase_normalWS, GetWorldSpaceNormalizeViewDir( ase_positionWS ), output.lightmapUVOrVertexSH.xyz );
				#elif UNITY_VERSION > 60000009
				OUTPUT_SH4( ase_positionWS, ase_normalWS, GetWorldSpaceNormalizeViewDir( ase_positionWS ), output.lightmapUVOrVertexSH.xyz, output.probeOcclusion );
				#else
				OUTPUT_SH4( ase_positionWS, ase_normalWS, GetWorldSpaceNormalizeViewDir( ase_positionWS ), output.lightmapUVOrVertexSH.xyz );
				#endif
				#if defined( DYNAMICLIGHTMAP_ON )
				output.dynamicLightmapUV.xy = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				float4 ase_positionCS = TransformObjectToHClip( ( input.positionOS ).xyz );
				float4 screenPos = ComputeScreenPos( ase_positionCS );
				output.ase_texcoord11 = screenPos;
				
				output.ase_texcoord4 = input.positionOS;
				output.ase_normal = input.normalOS;
				output.ase_tangent = input.tangentOS;
				output.ase_texcoord8.xy = input.texcoord.xy;
				output.ase_texcoord8.zw = input.texcoord1.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord3.w = 0;
				output.ase_texcoord5.w = 0;
				output.ase_texcoord6.w = 0;
				output.ase_texcoord7.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				#ifdef EDITOR_VISUALIZATION
					float2 VizUV = 0;
					float4 LightCoord = 0;
					UnityEditorVizData(input.positionOS.xyz, input.texcoord.xy, input.texcoord1.xy, input.texcoord2.xy, VizUV, LightCoord);
					output.VizUV = float4(VizUV, 0, 0);
					output.LightCoord = LightCoord;
				#endif

				output.positionCS = MetaVertexPosition( input.positionOS, input.texcoord1.xy, input.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );
				output.positionWS = TransformObjectToWorld( input.positionOS.xyz );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				output.texcoord1 = input.texcoord1;
				output.texcoord2 = input.texcoord2;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				output.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				output.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;

				float SurfaceTextureTiling218 = ( 1.0 / _SurfaceTextureTiling );
				float2 temp_cast_0 = (SurfaceTextureTiling218).xx;
				float CoverageFalloff829 = _CoverageFalloff;
				float3 ase_normalWS = input.ase_texcoord3.xyz;
				float4 triplanar537 = TriplanarSampling537( _SurfaceTexture, float4( 1, 1, 0, 0 ), _SurfaceTexture, float4( 1, 1, 0, 0 ), _SurfaceTexture, float4( 1, 1, 0, 0 ), input.ase_texcoord4.xyz, input.ase_normal, CoverageFalloff829, temp_cast_0, float3( 1,1,1 ), float3(0,0,0) );
				float OverlayTextureTiling172 = ( 1.0 / _OverlayTextureTiling );
				float2 temp_cast_1 = (OverlayTextureTiling172).xx;
				float4 triplanar573 = TriplanarSampling573( _OverlayTexture, float4( 1, 1, 0, 0 ), _OverlayTexture, float4( 1, 1, 0, 0 ), _OverlayTexture, float4( 1, 1, 0, 0 ), PositionWS, ase_normalWS, CoverageFalloff829, temp_cast_1, float3( 1,1,1 ), float3(0,0,0) );
				float2 temp_cast_2 = (SurfaceTextureTiling218).xx;
				float3 ase_tangentWS = input.ase_texcoord5.xyz;
				float3 ase_bitangentWS = input.ase_texcoord6.xyz;
				float3x3 ase_worldToTangent = float3x3( ase_tangentWS, ase_bitangentWS, ase_normalWS );
				float3 ase_bitangentOS = input.ase_texcoord7.xyz;
				float3x3 objectToTangent = float3x3(input.ase_tangent.xyz, ase_bitangentOS, input.ase_normal);
				float SurfaceNormalMapScale258 = _SurfaceNormalScale;
				float3 temp_cast_3 = (SurfaceNormalMapScale258).xxx;
				float3 triplanar591 = TriplanarSampling591( _SurfaceNormalMap, float4( 1, 1, 0, 0 ), _SurfaceNormalMap, float4( 1, 1, 0, 0 ), _SurfaceNormalMap, float4( 1, 1, 0, 0 ), input.ase_texcoord4.xyz, input.ase_normal, CoverageFalloff829, temp_cast_2, temp_cast_3, float3(0,0,0) );
				float3 tanTriplanarNormal591 = mul( objectToTangent, triplanar591 );
				float3 TriNormalMain759 = tanTriplanarNormal591;
				float2 temp_cast_4 = (OverlayTextureTiling172).xx;
				float OverlayNormalMapScale261 = _OverlayNormalScale;
				float3 temp_cast_5 = (OverlayNormalMapScale261).xxx;
				float3 triplanar594 = TriplanarSampling594( _OverlayNormalMap, float4( 1, 1, 0, 0 ), _OverlayNormalMap, float4( 1, 1, 0, 0 ), _OverlayNormalMap, float4( 1, 1, 0, 0 ), PositionWS, ase_normalWS, CoverageFalloff829, temp_cast_4, temp_cast_5, float3(0,0,0) );
				float3 tanTriplanarNormal594 = mul( ase_worldToTangent, triplanar594 );
				float3 TriNormalOverlay758 = tanTriplanarNormal594;
				#if defined( _COVERAGEMASKSOURCE_SURFACENORMALMAP )
				float3 staticSwitch755 = TriNormalMain759;
				#elif defined( _COVERAGEMASKSOURCE_OVERLAYNORMALMAP )
				float3 staticSwitch755 = TriNormalOverlay758;
				#else
				float3 staticSwitch755 = TriNormalMain759;
				#endif
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 tanNormal324 = staticSwitch755;
				float3 worldNormal324 = float3( dot( tanToWorld0, tanNormal324 ), dot( tanToWorld1, tanNormal324 ), dot( tanToWorld2, tanNormal324 ) );
				float2 uv_DetailMask329 = input.ase_texcoord8.xy;
				float smoothstepResult544 = smoothstep( _TransitionStart , _TransitionEnd , ( ( worldNormal324.y +  (-1.0 + ( _OverlayCoverage - 0.0 ) * ( 2.0 - -1.0 ) / ( 1.0 - 0.0 ) ) ) * ( 1.0 - tex2D( _DetailMask, uv_DetailMask329 ).a ) ));
				float OverlayCoverage340 = saturate( smoothstepResult544 );
				float lerpResult580 = lerp( triplanar573.w , triplanar537.a , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float staticSwitch796 = lerpResult580;
				#else
				float staticSwitch796 = triplanar537.w;
				#endif
				float BlendedAlpha496 = staticSwitch796;
				float3 temp_output_723_0 = ( _SpecularColor.rgb * BlendedAlpha496 * _SecondarySpecularIntensity );
				float3 ase_viewVectorWS = ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz );
				float3 ase_viewDirWS = normalize( ase_viewVectorWS );
				float3 normalizeResult4_g61171 = normalize( ( ase_viewDirWS + SafeNormalize( _MainLightPosition.xyz ) ) );
				float3 lerpResult590 = lerp( tanTriplanarNormal594 , tanTriplanarNormal591 , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float3 staticSwitch800 = lerpResult590;
				#else
				float3 staticSwitch800 = tanTriplanarNormal591;
				#endif
				float2 uv_MacroNormalMap806 = input.ase_texcoord8.xy;
				float MacroNormalMapScale808 = _MacroNormalScale;
				float3 unpack806 = UnpackNormalScale( tex2D( _MacroNormalMap, uv_MacroNormalMap806 ), MacroNormalMapScale808 );
				unpack806.z = lerp( 1, unpack806.z, saturate(MacroNormalMapScale808) );
				#ifdef _ENABLEMACRONORMALS_ON
				float3 staticSwitch810 = BlendNormal( staticSwitch800 , unpack806 );
				#else
				float3 staticSwitch810 = staticSwitch800;
				#endif
				float3 BlendedNormals169 = staticSwitch810;
				float3 tanNormal442 = BlendedNormals169;
				float3 worldNormal442 = normalize( float3( dot( tanToWorld0, tanNormal442 ), dot( tanToWorld1, tanNormal442 ), dot( tanToWorld2, tanNormal442 ) ) );
				float3 normalizeResult443 = normalize( worldNormal442 );
				float3 Normals444 = normalizeResult443;
				float dotResult718 = dot( normalizeResult4_g61171 , Normals444 );
				float temp_output_855_0 = ( 1.0 - _SecondarySmoothness );
				float3 temp_output_726_0 = saturate( ( saturate( (  (-1.0 + ( _SecondarySpecularSize - 0.0 ) * ( -0.5 - -1.0 ) / ( 1.0 - 0.0 ) ) + dotResult718 ) ) / ( ( 1.0 - temp_output_723_0 ) * temp_output_855_0 ) ) );
				float3 DirectSpecHighlights463 = ( (temp_output_723_0).xyz * temp_output_726_0 );
				float ase_lightIntensity = max( max( _MainLightColor.r, _MainLightColor.g ), _MainLightColor.b ) + 1e-7;
				float4 ase_lightColor = float4( _MainLightColor.rgb / ase_lightIntensity, ase_lightIntensity );
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoord );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float3 bakedGI692 = ASEIndirectDiffuse( input, ase_normalWS, PositionWS, ase_viewDirWS );
				MixRealtimeAndBakedGI( ase_mainLight, ase_normalWS, bakedGI692, half4( 0, 0, 0, 0 ) );
				float4 HalfLambert695 = ( ( ase_lightColor * ase_lightAtten ) + float4( bakedGI692 , 0.0 ) );
				float4 SpecularHighlights457 = ( float4( DirectSpecHighlights463 , 0.0 ) * HalfLambert695 );
				#ifdef _ENABLESECONDARYHIGHLIGHTS_ON
				float4 staticSwitch456 = SpecularHighlights457;
				#else
				float4 staticSwitch456 = float4( 0,0,0,0 );
				#endif
				float4 color450 = IsGammaSpace() ? float4( 1, 1, 1, 1 ) : float4( 1, 1, 1, 1 );
				#ifdef _ENABLECOLORTINT_ON
				float4 staticSwitch451 = _BaseTint;
				#else
				float4 staticSwitch451 = color450;
				#endif
				float dotResult684 = dot( Normals444 , SafeNormalize( _MainLightPosition.xyz ) );
				float RampScale710 = _RampScale;
				float RampOffset709 = _RampOffset;
				float CEL_Effect437 = saturate( (dotResult684*RampScale710 + RampOffset709) );
				float2 temp_cast_8 = (CEL_Effect437).xx;
				float4 lerpResult543 = lerp( triplanar573 , triplanar537 , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float4 staticSwitch795 = lerpResult543;
				#else
				float4 staticSwitch795 = triplanar537;
				#endif
				float2 uv_DetailMask525 = input.ase_texcoord8.xy;
				float4 tex2DNode525 = tex2D( _DetailMask, uv_DetailMask525 );
				float temp_output_578_0 = saturate( tex2DNode525.r );
				float4 lerpResult814 = lerp( staticSwitch795 , ( _EdgeWearTint * temp_output_578_0 ) , ( _EdgeWear1 * temp_output_578_0 ));
				float temp_output_584_0 = saturate( tex2DNode525.b );
				float4 lerpResult542 = lerp( lerpResult814 , ( _CavityTint * temp_output_584_0 ) , ( _Cavity * temp_output_584_0 ));
				float4 BlendedTextures167 = lerpResult542;
				float3 WorldPosition288_g61181 = PositionWS;
				float3 WorldPosition305_g61181 = WorldPosition288_g61181;
				float4 screenPos = input.ase_texcoord11;
				float4 ase_positionSSNorm = screenPos / screenPos.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float2 ScreenUV286_g61181 = (ase_positionSSNorm).xy;
				float2 ScreenUV305_g61181 = ScreenUV286_g61181;
				float3 WorldNormal281_g61181 = Normals444;
				float3 WorldNormal305_g61181 = WorldNormal281_g61181;
				half2 LightmapUV1_g61183 = (input.ase_texcoord8.zw*(unity_LightmapST).xy + (unity_LightmapST).zw);
				half4 localCalculateShadowMask1_g61183 = CalculateShadowMask1_g61183( LightmapUV1_g61183 );
				float4 ShadowMask360_g61181 = localCalculateShadowMask1_g61183;
				float4 ShadowMask305_g61181 = ShadowMask360_g61181;
				float3 localAdditionalLightsLambertMask17x305_g61181 = AdditionalLightsLambertMask17x( WorldPosition305_g61181 , ScreenUV305_g61181 , WorldNormal305_g61181 , ShadowMask305_g61181 );
				float3 saferPower843 = abs( saturate( localAdditionalLightsLambertMask17x305_g61181 ) );
				float3 temp_cast_10 = ( (0.001 + ( _AdditionalLightFalloff - 0.0 ) * ( 12.0 - 0.001 ) / ( 12.0 - 0.0 ) )).xxx;
				float4 CustomLighting109 = ( staticSwitch456 + ( staticSwitch451 * ( ( ( tex2D( _TextureRamp, temp_cast_8 ) * BlendedTextures167 ) * HalfLambert695 ) + ( BlendedTextures167 * tex2D( _TextureRamp, (pow( saferPower843 , temp_cast_10 )* (0.001 + ( _AdditionalLightInfluence - 0.0 ) * ( 6.0 - 0.001 ) / ( 1.0 - 0.0 ) ) + RampOffset709).xy ) ) ) ) );
				
				float3 EmissionColor396 = _EmissionColor.rgb;
				float2 uv_DetailMask393 = input.ase_texcoord8.xy;
				float EmissionColorAlpha398 = _EmissionColor.a;
				float mulTime782 = _TimeParameters.x * _FlickerFrequency;
				#ifdef _ENABLEEMISSION_ON
				float3 staticSwitch405 = ( ( _EmissionIntensity1 * EmissionColor396 * tex2D( _DetailMask, uv_DetailMask393 ).g * EmissionColorAlpha398 ) * ( ( sin( ( ( mulTime782 + ( PositionWS.x + PositionWS.z ) ) / _FlickerScale ) ) * ( ( _MaxIntensity - _MinIntensity ) * 0.5 ) ) + ( ( _MaxIntensity + _MinIntensity ) * 0.5 ) ) );
				#else
				float3 staticSwitch405 = float3( 0,0,0 );
				#endif
				float3 Emission788 = staticSwitch405;
				

				float3 BaseColor = CustomLighting109.rgb;
				float3 Emission = Emission788;
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = BaseColor;
				metaInput.Emission = Emission;
				#ifdef EDITOR_VISUALIZATION
					metaInput.VizUV = input.VizUV.xy;
					metaInput.LightCoord = input.LightCoord;
				#endif

				return UnityMetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SPECULAR_SETUP 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170100


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_FRAG_NORMAL
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#pragma shader_feature_local _ENABLESECONDARYHIGHLIGHTS_ON
			#pragma shader_feature_local _ENABLECOVERAGE_ON
			#pragma shader_feature_local _COVERAGEMASKSOURCE_SURFACENORMALMAP _COVERAGEMASKSOURCE_OVERLAYNORMALMAP
			#pragma shader_feature_local _ENABLEMACRONORMALS_ON
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#if UNITY_VERSION >= 60010000
			#pragma multi_compile _ LIGHTMAP_BICUBIC_SAMPLING
			#endif
			#if UNITY_VERSION >= 60010000
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
			#endif
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma shader_feature_local _ENABLECOLORTINT_ON


			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 lightmapUVOrVertexSH : TEXCOORD7;
				float4 dynamicLightmapUV : TEXCOORD8;
				float4 ase_texcoord9 : TEXCOORD9;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _CavityTint;
			float4 _BaseTint;
			float4 _EdgeWearTint;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion1;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _AdditionalLightFalloff;
			float _Cavity;
			float _EdgeWear1;
			float _RampOffset;
			float _RampScale;
			float _SecondarySmoothness;
			float _MacroNormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _OverlayCoverage;
			float _OverlayNormalScale;
			float _SurfaceNormalScale;
			float _TransitionEnd;
			float _TransitionStart;
			float _OverlayTextureTiling;
			float _CoverageFalloff;
			float _SurfaceTextureTiling;
			float _MaxIntensity;
			float _MinIntensity;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _SurfaceTexture;
			sampler2D _OverlayTexture;
			sampler2D _SurfaceNormalMap;
			sampler2D _OverlayNormalMap;
			sampler2D _DetailMask;
			sampler2D _MacroNormalMap;
			sampler2D _TextureRamp;


			inline float4 TriplanarSampling537( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
			}
			
			inline float4 TriplanarSampling573( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
			}
			
			inline float3 TriplanarSampling591( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				xNorm.xyz  = half3( UnpackNormalScale( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
				yNorm.xyz  = half3( UnpackNormalScale( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				zNorm.xyz  = half3( UnpackNormalScale( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
				yNormN.xyz = half3( UnpackNormalScale( yNormN, normalScale.z ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
			}
			
			inline float3 TriplanarSampling594( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				xNorm.xyz  = half3( UnpackNormalScale( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
				yNorm.xyz  = half3( UnpackNormalScale( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				zNorm.xyz  = half3( UnpackNormalScale( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
				yNormN.xyz = half3( UnpackNormalScale( yNormN, normalScale.z ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
			}
			
			half3 ASEIndirectDiffuse( PackedVaryings input, half3 normalWS, float3 positionWS, half3 viewDirWS )
			{
			#if defined( DYNAMICLIGHTMAP_ON )
				return SAMPLE_GI( input.lightmapUVOrVertexSH.xy, input.dynamicLightmapUV.xy, 0, normalWS );
			#elif defined( LIGHTMAP_ON )
				return SAMPLE_GI( input.lightmapUVOrVertexSH.xy, 0, normalWS );
			#elif defined( PROBE_VOLUMES_L1 ) || defined( PROBE_VOLUMES_L2 )
				return SampleProbeVolumePixel( SampleSH( normalWS ), positionWS, normalWS, viewDirWS, input.positionCS.xy );
			#else
				return SampleSH( normalWS );
			#endif
			}
			
			half4 CalculateShadowMask1_g61183( half2 LightmapUV )
			{
				#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
				return SAMPLE_SHADOWMASK( LightmapUV.xy );
				#elif !defined (LIGHTMAP_ON)
				return unity_ProbesOcclusion;
				#else
				return half4( 1, 1, 1, 1 );
				#endif
			}
			
			float3 AdditionalLightsLambertMask17x( float3 WorldPosition, float2 ScreenUV, float3 WorldNormal, float4 ShadowMask )
			{
				#if ( UNITY_VERSION < 60010000 ) && !defined( USE_CLUSTER_LIGHT_LOOP )
					#define USE_CLUSTER_LIGHT_LOOP USE_FORWARD_PLUS
				#endif
				#if ( UNITY_VERSION < 60010000 ) && !defined( CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK )
					#define CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
				#endif
				float3 Color = 0;
				#if defined(_ADDITIONAL_LIGHTS)
					#if ( UNITY_VERSION >= 60010000 )
						#define CALC_LAMBERT(Light) LightingLambert( AttLightColor, Light.direction, WorldNormal )
					#else
						#define CALC_LAMBERT(Light) ( dot( Light.direction, WorldNormal ) * 0.5 + 0.5 )* AttLightColor
					#endif
					#define SUM_LIGHTLAMBERT(Light)\
						half3 AttLightColor = Light.color * ( Light.distanceAttenuation * Light.shadowAttenuation );\
						Color += CALC_LAMBERT(Light);
					InputData inputData = (InputData)0;
					inputData.normalizedScreenSpaceUV = ScreenUV;
					inputData.positionWS = WorldPosition;
					uint meshRenderingLayers = GetMeshRenderingLayer();
					uint pixelLightCount = GetAdditionalLightsCount();	
					#if USE_CLUSTER_LIGHT_LOOP
					[loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
					{
						CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					}
					#endif
					LIGHT_LOOP_BEGIN( pixelLightCount )
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					LIGHT_LOOP_END
				#endif
				return Color;
			}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_TRANSFER_INSTANCE_ID( input, output );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord1.xyz = ase_normalWS;
				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord3.xyz = ase_tangentWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord4.xyz = ase_bitangentWS;
				float3 ase_bitangentOS = cross( input.normalOS, input.tangentOS.xyz ) * input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				output.ase_texcoord5.xyz = ase_bitangentOS;
				OUTPUT_LIGHTMAP_UV( input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy );
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				#if !defined( OUTPUT_SH4 )
				OUTPUT_SH( ase_positionWS, ase_normalWS, GetWorldSpaceNormalizeViewDir( ase_positionWS ), output.lightmapUVOrVertexSH.xyz );
				#elif UNITY_VERSION > 60000009
				OUTPUT_SH4( ase_positionWS, ase_normalWS, GetWorldSpaceNormalizeViewDir( ase_positionWS ), output.lightmapUVOrVertexSH.xyz, output.probeOcclusion );
				#else
				OUTPUT_SH4( ase_positionWS, ase_normalWS, GetWorldSpaceNormalizeViewDir( ase_positionWS ), output.lightmapUVOrVertexSH.xyz );
				#endif
				#if defined( DYNAMICLIGHTMAP_ON )
				output.dynamicLightmapUV.xy = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				float4 ase_positionCS = TransformObjectToHClip( ( input.positionOS ).xyz );
				float4 screenPos = ComputeScreenPos( ase_positionCS );
				output.ase_texcoord9 = screenPos;
				
				output.ase_texcoord2 = input.positionOS;
				output.ase_normal = input.normalOS;
				output.ase_tangent = input.tangentOS;
				output.ase_texcoord6.xy = input.ase_texcoord.xy;
				output.ase_texcoord6.zw = input.texcoord1.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord1.w = 0;
				output.ase_texcoord3.w = 0;
				output.ase_texcoord4.w = 0;
				output.ase_texcoord5.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.ase_texcoord = input.ase_texcoord;
				output.texcoord1 = input.texcoord1;
				output.texcoord2 = input.texcoord2;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				output.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;

				float SurfaceTextureTiling218 = ( 1.0 / _SurfaceTextureTiling );
				float2 temp_cast_0 = (SurfaceTextureTiling218).xx;
				float CoverageFalloff829 = _CoverageFalloff;
				float3 ase_normalWS = input.ase_texcoord1.xyz;
				float4 triplanar537 = TriplanarSampling537( _SurfaceTexture, float4( 1, 1, 0, 0 ), _SurfaceTexture, float4( 1, 1, 0, 0 ), _SurfaceTexture, float4( 1, 1, 0, 0 ), input.ase_texcoord2.xyz, input.ase_normal, CoverageFalloff829, temp_cast_0, float3( 1,1,1 ), float3(0,0,0) );
				float OverlayTextureTiling172 = ( 1.0 / _OverlayTextureTiling );
				float2 temp_cast_1 = (OverlayTextureTiling172).xx;
				float4 triplanar573 = TriplanarSampling573( _OverlayTexture, float4( 1, 1, 0, 0 ), _OverlayTexture, float4( 1, 1, 0, 0 ), _OverlayTexture, float4( 1, 1, 0, 0 ), PositionWS, ase_normalWS, CoverageFalloff829, temp_cast_1, float3( 1,1,1 ), float3(0,0,0) );
				float2 temp_cast_2 = (SurfaceTextureTiling218).xx;
				float3 ase_tangentWS = input.ase_texcoord3.xyz;
				float3 ase_bitangentWS = input.ase_texcoord4.xyz;
				float3x3 ase_worldToTangent = float3x3( ase_tangentWS, ase_bitangentWS, ase_normalWS );
				float3 ase_bitangentOS = input.ase_texcoord5.xyz;
				float3x3 objectToTangent = float3x3(input.ase_tangent.xyz, ase_bitangentOS, input.ase_normal);
				float SurfaceNormalMapScale258 = _SurfaceNormalScale;
				float3 temp_cast_3 = (SurfaceNormalMapScale258).xxx;
				float3 triplanar591 = TriplanarSampling591( _SurfaceNormalMap, float4( 1, 1, 0, 0 ), _SurfaceNormalMap, float4( 1, 1, 0, 0 ), _SurfaceNormalMap, float4( 1, 1, 0, 0 ), input.ase_texcoord2.xyz, input.ase_normal, CoverageFalloff829, temp_cast_2, temp_cast_3, float3(0,0,0) );
				float3 tanTriplanarNormal591 = mul( objectToTangent, triplanar591 );
				float3 TriNormalMain759 = tanTriplanarNormal591;
				float2 temp_cast_4 = (OverlayTextureTiling172).xx;
				float OverlayNormalMapScale261 = _OverlayNormalScale;
				float3 temp_cast_5 = (OverlayNormalMapScale261).xxx;
				float3 triplanar594 = TriplanarSampling594( _OverlayNormalMap, float4( 1, 1, 0, 0 ), _OverlayNormalMap, float4( 1, 1, 0, 0 ), _OverlayNormalMap, float4( 1, 1, 0, 0 ), PositionWS, ase_normalWS, CoverageFalloff829, temp_cast_4, temp_cast_5, float3(0,0,0) );
				float3 tanTriplanarNormal594 = mul( ase_worldToTangent, triplanar594 );
				float3 TriNormalOverlay758 = tanTriplanarNormal594;
				#if defined( _COVERAGEMASKSOURCE_SURFACENORMALMAP )
				float3 staticSwitch755 = TriNormalMain759;
				#elif defined( _COVERAGEMASKSOURCE_OVERLAYNORMALMAP )
				float3 staticSwitch755 = TriNormalOverlay758;
				#else
				float3 staticSwitch755 = TriNormalMain759;
				#endif
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 tanNormal324 = staticSwitch755;
				float3 worldNormal324 = float3( dot( tanToWorld0, tanNormal324 ), dot( tanToWorld1, tanNormal324 ), dot( tanToWorld2, tanNormal324 ) );
				float2 uv_DetailMask329 = input.ase_texcoord6.xy;
				float smoothstepResult544 = smoothstep( _TransitionStart , _TransitionEnd , ( ( worldNormal324.y +  (-1.0 + ( _OverlayCoverage - 0.0 ) * ( 2.0 - -1.0 ) / ( 1.0 - 0.0 ) ) ) * ( 1.0 - tex2D( _DetailMask, uv_DetailMask329 ).a ) ));
				float OverlayCoverage340 = saturate( smoothstepResult544 );
				float lerpResult580 = lerp( triplanar573.w , triplanar537.a , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float staticSwitch796 = lerpResult580;
				#else
				float staticSwitch796 = triplanar537.w;
				#endif
				float BlendedAlpha496 = staticSwitch796;
				float3 temp_output_723_0 = ( _SpecularColor.rgb * BlendedAlpha496 * _SecondarySpecularIntensity );
				float3 ase_viewVectorWS = ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz );
				float3 ase_viewDirWS = normalize( ase_viewVectorWS );
				float3 normalizeResult4_g61171 = normalize( ( ase_viewDirWS + SafeNormalize( _MainLightPosition.xyz ) ) );
				float3 lerpResult590 = lerp( tanTriplanarNormal594 , tanTriplanarNormal591 , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float3 staticSwitch800 = lerpResult590;
				#else
				float3 staticSwitch800 = tanTriplanarNormal591;
				#endif
				float2 uv_MacroNormalMap806 = input.ase_texcoord6.xy;
				float MacroNormalMapScale808 = _MacroNormalScale;
				float3 unpack806 = UnpackNormalScale( tex2D( _MacroNormalMap, uv_MacroNormalMap806 ), MacroNormalMapScale808 );
				unpack806.z = lerp( 1, unpack806.z, saturate(MacroNormalMapScale808) );
				#ifdef _ENABLEMACRONORMALS_ON
				float3 staticSwitch810 = BlendNormal( staticSwitch800 , unpack806 );
				#else
				float3 staticSwitch810 = staticSwitch800;
				#endif
				float3 BlendedNormals169 = staticSwitch810;
				float3 tanNormal442 = BlendedNormals169;
				float3 worldNormal442 = normalize( float3( dot( tanToWorld0, tanNormal442 ), dot( tanToWorld1, tanNormal442 ), dot( tanToWorld2, tanNormal442 ) ) );
				float3 normalizeResult443 = normalize( worldNormal442 );
				float3 Normals444 = normalizeResult443;
				float dotResult718 = dot( normalizeResult4_g61171 , Normals444 );
				float temp_output_855_0 = ( 1.0 - _SecondarySmoothness );
				float3 temp_output_726_0 = saturate( ( saturate( (  (-1.0 + ( _SecondarySpecularSize - 0.0 ) * ( -0.5 - -1.0 ) / ( 1.0 - 0.0 ) ) + dotResult718 ) ) / ( ( 1.0 - temp_output_723_0 ) * temp_output_855_0 ) ) );
				float3 DirectSpecHighlights463 = ( (temp_output_723_0).xyz * temp_output_726_0 );
				float ase_lightIntensity = max( max( _MainLightColor.r, _MainLightColor.g ), _MainLightColor.b ) + 1e-7;
				float4 ase_lightColor = float4( _MainLightColor.rgb / ase_lightIntensity, ase_lightIntensity );
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoord );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float3 bakedGI692 = ASEIndirectDiffuse( input, ase_normalWS, PositionWS, ase_viewDirWS );
				MixRealtimeAndBakedGI( ase_mainLight, ase_normalWS, bakedGI692, half4( 0, 0, 0, 0 ) );
				float4 HalfLambert695 = ( ( ase_lightColor * ase_lightAtten ) + float4( bakedGI692 , 0.0 ) );
				float4 SpecularHighlights457 = ( float4( DirectSpecHighlights463 , 0.0 ) * HalfLambert695 );
				#ifdef _ENABLESECONDARYHIGHLIGHTS_ON
				float4 staticSwitch456 = SpecularHighlights457;
				#else
				float4 staticSwitch456 = float4( 0,0,0,0 );
				#endif
				float4 color450 = IsGammaSpace() ? float4( 1, 1, 1, 1 ) : float4( 1, 1, 1, 1 );
				#ifdef _ENABLECOLORTINT_ON
				float4 staticSwitch451 = _BaseTint;
				#else
				float4 staticSwitch451 = color450;
				#endif
				float dotResult684 = dot( Normals444 , SafeNormalize( _MainLightPosition.xyz ) );
				float RampScale710 = _RampScale;
				float RampOffset709 = _RampOffset;
				float CEL_Effect437 = saturate( (dotResult684*RampScale710 + RampOffset709) );
				float2 temp_cast_8 = (CEL_Effect437).xx;
				float4 lerpResult543 = lerp( triplanar573 , triplanar537 , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float4 staticSwitch795 = lerpResult543;
				#else
				float4 staticSwitch795 = triplanar537;
				#endif
				float2 uv_DetailMask525 = input.ase_texcoord6.xy;
				float4 tex2DNode525 = tex2D( _DetailMask, uv_DetailMask525 );
				float temp_output_578_0 = saturate( tex2DNode525.r );
				float4 lerpResult814 = lerp( staticSwitch795 , ( _EdgeWearTint * temp_output_578_0 ) , ( _EdgeWear1 * temp_output_578_0 ));
				float temp_output_584_0 = saturate( tex2DNode525.b );
				float4 lerpResult542 = lerp( lerpResult814 , ( _CavityTint * temp_output_584_0 ) , ( _Cavity * temp_output_584_0 ));
				float4 BlendedTextures167 = lerpResult542;
				float3 WorldPosition288_g61181 = PositionWS;
				float3 WorldPosition305_g61181 = WorldPosition288_g61181;
				float4 screenPos = input.ase_texcoord9;
				float4 ase_positionSSNorm = screenPos / screenPos.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float2 ScreenUV286_g61181 = (ase_positionSSNorm).xy;
				float2 ScreenUV305_g61181 = ScreenUV286_g61181;
				float3 WorldNormal281_g61181 = Normals444;
				float3 WorldNormal305_g61181 = WorldNormal281_g61181;
				half2 LightmapUV1_g61183 = (input.ase_texcoord6.zw*(unity_LightmapST).xy + (unity_LightmapST).zw);
				half4 localCalculateShadowMask1_g61183 = CalculateShadowMask1_g61183( LightmapUV1_g61183 );
				float4 ShadowMask360_g61181 = localCalculateShadowMask1_g61183;
				float4 ShadowMask305_g61181 = ShadowMask360_g61181;
				float3 localAdditionalLightsLambertMask17x305_g61181 = AdditionalLightsLambertMask17x( WorldPosition305_g61181 , ScreenUV305_g61181 , WorldNormal305_g61181 , ShadowMask305_g61181 );
				float3 saferPower843 = abs( saturate( localAdditionalLightsLambertMask17x305_g61181 ) );
				float3 temp_cast_10 = ( (0.001 + ( _AdditionalLightFalloff - 0.0 ) * ( 12.0 - 0.001 ) / ( 12.0 - 0.0 ) )).xxx;
				float4 CustomLighting109 = ( staticSwitch456 + ( staticSwitch451 * ( ( ( tex2D( _TextureRamp, temp_cast_8 ) * BlendedTextures167 ) * HalfLambert695 ) + ( BlendedTextures167 * tex2D( _TextureRamp, (pow( saferPower843 , temp_cast_10 )* (0.001 + ( _AdditionalLightInfluence - 0.0 ) * ( 6.0 - 0.001 ) / ( 1.0 - 0.0 ) ) + RampOffset709).xy ) ) ) ) );
				

				float3 BaseColor = CustomLighting109.rgb;
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				half4 color = half4(BaseColor, Alpha );

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				return color;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			Blend One Zero
			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SPECULAR_SETUP 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170100


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
			//#define SHADERPASS SHADERPASS_DEPTHNORMALS

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#if defined( UNITY_INSTANCING_ENABLED ) && defined( ASE_INSTANCED_TERRAIN ) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#pragma shader_feature_local _ENABLEMACRONORMALS_ON
			#pragma shader_feature_local _ENABLECOVERAGE_ON
			#pragma shader_feature_local _COVERAGEMASKSOURCE_SURFACENORMALMAP _COVERAGEMASKSOURCE_OVERLAYNORMALMAP


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				half4 texcoord : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord3 : TEXCOORD3;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _CavityTint;
			float4 _BaseTint;
			float4 _EdgeWearTint;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion1;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _AdditionalLightFalloff;
			float _Cavity;
			float _EdgeWear1;
			float _RampOffset;
			float _RampScale;
			float _SecondarySmoothness;
			float _MacroNormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _OverlayCoverage;
			float _OverlayNormalScale;
			float _SurfaceNormalScale;
			float _TransitionEnd;
			float _TransitionStart;
			float _OverlayTextureTiling;
			float _CoverageFalloff;
			float _SurfaceTextureTiling;
			float _MaxIntensity;
			float _MinIntensity;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _SurfaceNormalMap;
			sampler2D _OverlayNormalMap;
			sampler2D _DetailMask;
			sampler2D _MacroNormalMap;


			inline float3 TriplanarSampling591( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				xNorm.xyz  = half3( UnpackNormalScale( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
				yNorm.xyz  = half3( UnpackNormalScale( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				zNorm.xyz  = half3( UnpackNormalScale( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
				yNormN.xyz = half3( UnpackNormalScale( yNormN, normalScale.z ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
			}
			
			inline float3 TriplanarSampling594( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				xNorm.xyz  = half3( UnpackNormalScale( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
				yNorm.xyz  = half3( UnpackNormalScale( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				zNorm.xyz  = half3( UnpackNormalScale( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
				yNormN.xyz = half3( UnpackNormalScale( yNormN, normalScale.z ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
			}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 ase_bitangentOS = cross( input.normalOS, input.tangentOS.xyz ) * input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				output.ase_texcoord3.xyz = ase_bitangentOS;
				
				output.ase_tangent = input.tangentOS;
				output.ase_normal = input.normalOS;
				output.ase_texcoord4 = input.positionOS;
				output.ase_texcoord5.xy = input.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord3.w = 0;
				output.ase_texcoord5.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					output.tangentWS.zw = input.texcoord.xy;
					output.tangentWS.xy = input.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			void frag(	PackedVaryings input
						, out half4 outNormalWS : SV_Target0
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						#if ( UNITY_VERSION >= 60020000 )
						, out uint outRenderingLayers : SV_Target1
						#else
						, out float4 outRenderingLayers : SV_Target1
						#endif
						#endif
						 )
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float2 sampleCoords = (input.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					NormalWS = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					TangentWS = -cross(GetObjectToWorldMatrix()._13_23_33, NormalWS);
					BitangentWS = cross(NormalWS, -TangentWS);
				#endif

				float SurfaceTextureTiling218 = ( 1.0 / _SurfaceTextureTiling );
				float2 temp_cast_0 = (SurfaceTextureTiling218).xx;
				float CoverageFalloff829 = _CoverageFalloff;
				float3x3 ase_worldToTangent = float3x3( TangentWS, BitangentWS, NormalWS );
				float3 ase_bitangentOS = input.ase_texcoord3.xyz;
				float3x3 objectToTangent = float3x3(input.ase_tangent.xyz, ase_bitangentOS, input.ase_normal);
				float SurfaceNormalMapScale258 = _SurfaceNormalScale;
				float3 temp_cast_1 = (SurfaceNormalMapScale258).xxx;
				float3 triplanar591 = TriplanarSampling591( _SurfaceNormalMap, float4( 1, 1, 0, 0 ), _SurfaceNormalMap, float4( 1, 1, 0, 0 ), _SurfaceNormalMap, float4( 1, 1, 0, 0 ), input.ase_texcoord4.xyz, input.ase_normal, CoverageFalloff829, temp_cast_0, temp_cast_1, float3(0,0,0) );
				float3 tanTriplanarNormal591 = mul( objectToTangent, triplanar591 );
				float OverlayTextureTiling172 = ( 1.0 / _OverlayTextureTiling );
				float2 temp_cast_2 = (OverlayTextureTiling172).xx;
				float OverlayNormalMapScale261 = _OverlayNormalScale;
				float3 temp_cast_3 = (OverlayNormalMapScale261).xxx;
				float3 triplanar594 = TriplanarSampling594( _OverlayNormalMap, float4( 1, 1, 0, 0 ), _OverlayNormalMap, float4( 1, 1, 0, 0 ), _OverlayNormalMap, float4( 1, 1, 0, 0 ), PositionWS, NormalWS, CoverageFalloff829, temp_cast_2, temp_cast_3, float3(0,0,0) );
				float3 tanTriplanarNormal594 = mul( ase_worldToTangent, triplanar594 );
				float3 TriNormalMain759 = tanTriplanarNormal591;
				float3 TriNormalOverlay758 = tanTriplanarNormal594;
				#if defined( _COVERAGEMASKSOURCE_SURFACENORMALMAP )
				float3 staticSwitch755 = TriNormalMain759;
				#elif defined( _COVERAGEMASKSOURCE_OVERLAYNORMALMAP )
				float3 staticSwitch755 = TriNormalOverlay758;
				#else
				float3 staticSwitch755 = TriNormalMain759;
				#endif
				float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
				float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
				float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
				float3 tanNormal324 = staticSwitch755;
				float3 worldNormal324 = float3( dot( tanToWorld0, tanNormal324 ), dot( tanToWorld1, tanNormal324 ), dot( tanToWorld2, tanNormal324 ) );
				float2 uv_DetailMask329 = input.ase_texcoord5.xy;
				float smoothstepResult544 = smoothstep( _TransitionStart , _TransitionEnd , ( ( worldNormal324.y +  (-1.0 + ( _OverlayCoverage - 0.0 ) * ( 2.0 - -1.0 ) / ( 1.0 - 0.0 ) ) ) * ( 1.0 - tex2D( _DetailMask, uv_DetailMask329 ).a ) ));
				float OverlayCoverage340 = saturate( smoothstepResult544 );
				float3 lerpResult590 = lerp( tanTriplanarNormal594 , tanTriplanarNormal591 , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float3 staticSwitch800 = lerpResult590;
				#else
				float3 staticSwitch800 = tanTriplanarNormal591;
				#endif
				float2 uv_MacroNormalMap806 = input.ase_texcoord5.xy;
				float MacroNormalMapScale808 = _MacroNormalScale;
				float3 unpack806 = UnpackNormalScale( tex2D( _MacroNormalMap, uv_MacroNormalMap806 ), MacroNormalMapScale808 );
				unpack806.z = lerp( 1, unpack806.z, saturate(MacroNormalMapScale808) );
				#ifdef _ENABLEMACRONORMALS_ON
				float3 staticSwitch810 = BlendNormal( staticSwitch800 , unpack806 );
				#else
				float3 staticSwitch810 = staticSwitch800;
				#endif
				float3 BlendedNormals169 = staticSwitch810;
				

				float3 Normal = BlendedNormals169;
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(NormalWS);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(TangentWS, BitangentWS, NormalWS));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = NormalWS;
					#endif
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					#if ( UNITY_VERSION >= 60020000 )
					outRenderingLayers = EncodeMeshRenderingLayer();
					#else
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
					#endif
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "GBuffer"
			Tags { "LightMode"="UniversalGBuffer" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SPECULAR_SETUP 1
			#define _NORMAL_DROPOFF_TS 1
			#pragma shader_feature_local_fragment _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#define ASE_FOG 1
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170100


			// Deferred Rendering Path does not support the OpenGL-based graphics API:
			// Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
			#pragma exclude_renderers glcore gles3 

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#if ( UNITY_VERSION >= 60000058 )
			#pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
			#endif
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
			#if ( UNITY_VERSION >= 60030000 )
			#pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
			#endif
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
			#pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
			#if ( UNITY_VERSION >= 60010000 )
			#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
			#endif

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ USE_LEGACY_LIGHTMAPS
			#pragma multi_compile _ LIGHTMAP_ON
			#if ( UNITY_VERSION >= 60010000 )
			#pragma multi_compile _ LIGHTMAP_BICUBIC_SAMPLING
			#endif
			#if ( UNITY_VERSION >= 60030000 )
			#pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
			#endif
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_GBUFFER

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if ( UNITY_VERSION >= 60030016 && UNITY_VERSION < 60040000 ) || ( UNITY_VERSION >= 60040010 )
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutputFormat.hlsl"
			#endif

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#if defined( UNITY_INSTANCING_ENABLED ) && defined( ASE_INSTANCED_TERRAIN ) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_NORMAL
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_WORLD_VIEW_DIR
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#pragma shader_feature_local _ENABLESECONDARYHIGHLIGHTS_ON
			#pragma shader_feature_local _ENABLECOVERAGE_ON
			#pragma shader_feature_local _COVERAGEMASKSOURCE_SURFACENORMALMAP _COVERAGEMASKSOURCE_OVERLAYNORMALMAP
			#pragma shader_feature_local _ENABLEMACRONORMALS_ON
			#pragma shader_feature_local _ENABLECOLORTINT_ON
			#pragma shader_feature_local _ENABLEEMISSION_ON


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
				float4 lightmapUVOrVertexSH : TEXCOORD3;
				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					half4 fogFactorAndVertexLight : TEXCOORD4;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
					float2 dynamicLightmapUV : TEXCOORD5;
				#endif
				#if defined(USE_APV_PROBE_OCCLUSION)
					float4 probeOcclusion : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord8 : TEXCOORD8;
				float4 ase_texcoord9 : TEXCOORD9;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _CavityTint;
			float4 _BaseTint;
			float4 _EdgeWearTint;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion1;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _AdditionalLightFalloff;
			float _Cavity;
			float _EdgeWear1;
			float _RampOffset;
			float _RampScale;
			float _SecondarySmoothness;
			float _MacroNormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _OverlayCoverage;
			float _OverlayNormalScale;
			float _SurfaceNormalScale;
			float _TransitionEnd;
			float _TransitionStart;
			float _OverlayTextureTiling;
			float _CoverageFalloff;
			float _SurfaceTextureTiling;
			float _MaxIntensity;
			float _MinIntensity;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _SurfaceTexture;
			sampler2D _OverlayTexture;
			sampler2D _SurfaceNormalMap;
			sampler2D _OverlayNormalMap;
			sampler2D _DetailMask;
			sampler2D _MacroNormalMap;
			sampler2D _TextureRamp;


			#if ( UNITY_VERSION >= 60010000 )
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutput.hlsl"
			#else
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
			#endif

			inline float4 TriplanarSampling537( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
			}
			
			inline float4 TriplanarSampling573( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
			}
			
			inline float3 TriplanarSampling591( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				xNorm.xyz  = half3( UnpackNormalScale( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
				yNorm.xyz  = half3( UnpackNormalScale( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				zNorm.xyz  = half3( UnpackNormalScale( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
				yNormN.xyz = half3( UnpackNormalScale( yNormN, normalScale.z ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
			}
			
			inline float3 TriplanarSampling594( sampler2D topTexMap, float4 topST, sampler2D midTexMap, float4 midST, sampler2D botTexMap, float4 botST, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
			{
				float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
				projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
				float3 nsign = sign( worldNormal );
				float negProjNormalY = max( 0, projNormal.y * -nsign.y );
				projNormal.y = max( 0, projNormal.y * nsign.y );
				half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
				xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) * midST.xy + midST.zw );
				yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * topST.xy + topST.zw );
				yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) * botST.xy + botST.zw );
				zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) * midST.xy + midST.zw );
				xNorm.xyz  = half3( UnpackNormalScale( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
				yNorm.xyz  = half3( UnpackNormalScale( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				zNorm.xyz  = half3( UnpackNormalScale( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
				yNormN.xyz = half3( UnpackNormalScale( yNormN, normalScale.z ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
				return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
			}
			
			half3 ASEIndirectDiffuse( PackedVaryings input, half3 normalWS, float3 positionWS, half3 viewDirWS )
			{
			#if defined( DYNAMICLIGHTMAP_ON )
				return SAMPLE_GI( input.lightmapUVOrVertexSH.xy, input.dynamicLightmapUV.xy, 0, normalWS );
			#elif defined( LIGHTMAP_ON )
				return SAMPLE_GI( input.lightmapUVOrVertexSH.xy, 0, normalWS );
			#elif defined( PROBE_VOLUMES_L1 ) || defined( PROBE_VOLUMES_L2 )
				return SampleProbeVolumePixel( SampleSH( normalWS ), positionWS, normalWS, viewDirWS, input.positionCS.xy );
			#else
				return SampleSH( normalWS );
			#endif
			}
			
			half4 CalculateShadowMask1_g61183( half2 LightmapUV )
			{
				#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
				return SAMPLE_SHADOWMASK( LightmapUV.xy );
				#elif !defined (LIGHTMAP_ON)
				return unity_ProbesOcclusion;
				#else
				return half4( 1, 1, 1, 1 );
				#endif
			}
			
			float3 AdditionalLightsLambertMask17x( float3 WorldPosition, float2 ScreenUV, float3 WorldNormal, float4 ShadowMask )
			{
				#if ( UNITY_VERSION < 60010000 ) && !defined( USE_CLUSTER_LIGHT_LOOP )
					#define USE_CLUSTER_LIGHT_LOOP USE_FORWARD_PLUS
				#endif
				#if ( UNITY_VERSION < 60010000 ) && !defined( CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK )
					#define CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
				#endif
				float3 Color = 0;
				#if defined(_ADDITIONAL_LIGHTS)
					#if ( UNITY_VERSION >= 60010000 )
						#define CALC_LAMBERT(Light) LightingLambert( AttLightColor, Light.direction, WorldNormal )
					#else
						#define CALC_LAMBERT(Light) ( dot( Light.direction, WorldNormal ) * 0.5 + 0.5 )* AttLightColor
					#endif
					#define SUM_LIGHTLAMBERT(Light)\
						half3 AttLightColor = Light.color * ( Light.distanceAttenuation * Light.shadowAttenuation );\
						Color += CALC_LAMBERT(Light);
					InputData inputData = (InputData)0;
					inputData.normalizedScreenSpaceUV = ScreenUV;
					inputData.positionWS = WorldPosition;
					uint meshRenderingLayers = GetMeshRenderingLayer();
					uint pixelLightCount = GetAdditionalLightsCount();	
					#if USE_CLUSTER_LIGHT_LOOP
					[loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
					{
						CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					}
					#endif
					LIGHT_LOOP_BEGIN( pixelLightCount )
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					LIGHT_LOOP_END
				#endif
				return Color;
			}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 ase_bitangentOS = cross( input.normalOS, input.tangentOS.xyz ) * input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				output.ase_texcoord8.xyz = ase_bitangentOS;
				
				output.ase_texcoord7 = input.positionOS;
				output.ase_normal = input.normalOS;
				output.ase_tangent = input.tangentOS;
				output.ase_texcoord9.xy = input.texcoord.xy;
				output.ase_texcoord9.zw = input.texcoord1.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord8.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				#ifdef ASE_CUSTOM_MOTION_VECTOR
					// Declared so the Motion Vector output port surfaces on the master node; only consumed by the motion vector passes.
					float3 aseCustomMotionVector = float3(0, 0, 0);
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy);
				#if defined(DYNAMICLIGHTMAP_ON)
					output.dynamicLightmapUV.xy = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				OUTPUT_SH4(vertexInput.positionWS, normalInput.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.lightmapUVOrVertexSH.xyz, output.probeOcclusion);

				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					output.fogFactorAndVertexLight = 0;
					#if defined(ASE_FOG) && !defined(_FOG_FRAGMENT)
						// @diogo: no fog applied in GBuffer
					#endif
					#ifdef _ADDITIONAL_LIGHTS_VERTEX
						half3 vertexLight = VertexLighting( vertexInput.positionWS, normalInput.normalWS );
						output.fogFactorAndVertexLight.yzw = vertexLight;
					#endif
				#endif

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					output.tangentWS.zw = input.texcoord.xy;
					output.tangentWS.xy = input.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = input.texcoord1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = input.texcoord2;
				#endif
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				#endif
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

		#if ( UNITY_VERSION >= 60010000 )
			GBufferFragOutput frag ( PackedVaryings input
		#else
			FragmentOutput frag ( PackedVaryings input
		#endif
								#if defined( ASE_WRITE_DEPTH )
								,out float outputDepth : ASE_SV_DEPTH
								#endif
								 )
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					float4 shadowCoord = TransformWorldToShadowCoord( input.positionWS );
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float3 ViewDirWS = GetWorldSpaceNormalizeViewDir( PositionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float2 sampleCoords = (input.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					NormalWS = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					TangentWS = -cross(GetObjectToWorldMatrix()._13_23_33, NormalWS);
					BitangentWS = cross(NormalWS, -TangentWS);
				#endif

				float SurfaceTextureTiling218 = ( 1.0 / _SurfaceTextureTiling );
				float2 temp_cast_0 = (SurfaceTextureTiling218).xx;
				float CoverageFalloff829 = _CoverageFalloff;
				float4 triplanar537 = TriplanarSampling537( _SurfaceTexture, float4( 1, 1, 0, 0 ), _SurfaceTexture, float4( 1, 1, 0, 0 ), _SurfaceTexture, float4( 1, 1, 0, 0 ), input.ase_texcoord7.xyz, input.ase_normal, CoverageFalloff829, temp_cast_0, float3( 1,1,1 ), float3(0,0,0) );
				float OverlayTextureTiling172 = ( 1.0 / _OverlayTextureTiling );
				float2 temp_cast_1 = (OverlayTextureTiling172).xx;
				float4 triplanar573 = TriplanarSampling573( _OverlayTexture, float4( 1, 1, 0, 0 ), _OverlayTexture, float4( 1, 1, 0, 0 ), _OverlayTexture, float4( 1, 1, 0, 0 ), PositionWS, NormalWS, CoverageFalloff829, temp_cast_1, float3( 1,1,1 ), float3(0,0,0) );
				float2 temp_cast_2 = (SurfaceTextureTiling218).xx;
				float3x3 ase_worldToTangent = float3x3( TangentWS, BitangentWS, NormalWS );
				float3 ase_bitangentOS = input.ase_texcoord8.xyz;
				float3x3 objectToTangent = float3x3(input.ase_tangent.xyz, ase_bitangentOS, input.ase_normal);
				float SurfaceNormalMapScale258 = _SurfaceNormalScale;
				float3 temp_cast_3 = (SurfaceNormalMapScale258).xxx;
				float3 triplanar591 = TriplanarSampling591( _SurfaceNormalMap, float4( 1, 1, 0, 0 ), _SurfaceNormalMap, float4( 1, 1, 0, 0 ), _SurfaceNormalMap, float4( 1, 1, 0, 0 ), input.ase_texcoord7.xyz, input.ase_normal, CoverageFalloff829, temp_cast_2, temp_cast_3, float3(0,0,0) );
				float3 tanTriplanarNormal591 = mul( objectToTangent, triplanar591 );
				float3 TriNormalMain759 = tanTriplanarNormal591;
				float2 temp_cast_4 = (OverlayTextureTiling172).xx;
				float OverlayNormalMapScale261 = _OverlayNormalScale;
				float3 temp_cast_5 = (OverlayNormalMapScale261).xxx;
				float3 triplanar594 = TriplanarSampling594( _OverlayNormalMap, float4( 1, 1, 0, 0 ), _OverlayNormalMap, float4( 1, 1, 0, 0 ), _OverlayNormalMap, float4( 1, 1, 0, 0 ), PositionWS, NormalWS, CoverageFalloff829, temp_cast_4, temp_cast_5, float3(0,0,0) );
				float3 tanTriplanarNormal594 = mul( ase_worldToTangent, triplanar594 );
				float3 TriNormalOverlay758 = tanTriplanarNormal594;
				#if defined( _COVERAGEMASKSOURCE_SURFACENORMALMAP )
				float3 staticSwitch755 = TriNormalMain759;
				#elif defined( _COVERAGEMASKSOURCE_OVERLAYNORMALMAP )
				float3 staticSwitch755 = TriNormalOverlay758;
				#else
				float3 staticSwitch755 = TriNormalMain759;
				#endif
				float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
				float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
				float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
				float3 tanNormal324 = staticSwitch755;
				float3 worldNormal324 = float3( dot( tanToWorld0, tanNormal324 ), dot( tanToWorld1, tanNormal324 ), dot( tanToWorld2, tanNormal324 ) );
				float2 uv_DetailMask329 = input.ase_texcoord9.xy;
				float smoothstepResult544 = smoothstep( _TransitionStart , _TransitionEnd , ( ( worldNormal324.y +  (-1.0 + ( _OverlayCoverage - 0.0 ) * ( 2.0 - -1.0 ) / ( 1.0 - 0.0 ) ) ) * ( 1.0 - tex2D( _DetailMask, uv_DetailMask329 ).a ) ));
				float OverlayCoverage340 = saturate( smoothstepResult544 );
				float lerpResult580 = lerp( triplanar573.w , triplanar537.a , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float staticSwitch796 = lerpResult580;
				#else
				float staticSwitch796 = triplanar537.w;
				#endif
				float BlendedAlpha496 = staticSwitch796;
				float3 temp_output_723_0 = ( _SpecularColor.rgb * BlendedAlpha496 * _SecondarySpecularIntensity );
				float3 normalizeResult4_g61171 = normalize( ( ViewDirWS + SafeNormalize( _MainLightPosition.xyz ) ) );
				float3 lerpResult590 = lerp( tanTriplanarNormal594 , tanTriplanarNormal591 , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float3 staticSwitch800 = lerpResult590;
				#else
				float3 staticSwitch800 = tanTriplanarNormal591;
				#endif
				float2 uv_MacroNormalMap806 = input.ase_texcoord9.xy;
				float MacroNormalMapScale808 = _MacroNormalScale;
				float3 unpack806 = UnpackNormalScale( tex2D( _MacroNormalMap, uv_MacroNormalMap806 ), MacroNormalMapScale808 );
				unpack806.z = lerp( 1, unpack806.z, saturate(MacroNormalMapScale808) );
				#ifdef _ENABLEMACRONORMALS_ON
				float3 staticSwitch810 = BlendNormal( staticSwitch800 , unpack806 );
				#else
				float3 staticSwitch810 = staticSwitch800;
				#endif
				float3 BlendedNormals169 = staticSwitch810;
				float3 tanNormal442 = BlendedNormals169;
				float3 worldNormal442 = normalize( float3( dot( tanToWorld0, tanNormal442 ), dot( tanToWorld1, tanNormal442 ), dot( tanToWorld2, tanNormal442 ) ) );
				float3 normalizeResult443 = normalize( worldNormal442 );
				float3 Normals444 = normalizeResult443;
				float dotResult718 = dot( normalizeResult4_g61171 , Normals444 );
				float temp_output_855_0 = ( 1.0 - _SecondarySmoothness );
				float3 temp_output_726_0 = saturate( ( saturate( (  (-1.0 + ( _SecondarySpecularSize - 0.0 ) * ( -0.5 - -1.0 ) / ( 1.0 - 0.0 ) ) + dotResult718 ) ) / ( ( 1.0 - temp_output_723_0 ) * temp_output_855_0 ) ) );
				float3 DirectSpecHighlights463 = ( (temp_output_723_0).xyz * temp_output_726_0 );
				float ase_lightIntensity = max( max( _MainLightColor.r, _MainLightColor.g ), _MainLightColor.b ) + 1e-7;
				float4 ase_lightColor = float4( _MainLightColor.rgb / ase_lightIntensity, ase_lightIntensity );
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoord );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float3 bakedGI692 = ASEIndirectDiffuse( input, NormalWS, PositionWS, ViewDirWS );
				MixRealtimeAndBakedGI( ase_mainLight, NormalWS, bakedGI692, half4( 0, 0, 0, 0 ) );
				float4 HalfLambert695 = ( ( ase_lightColor * ase_lightAtten ) + float4( bakedGI692 , 0.0 ) );
				float4 SpecularHighlights457 = ( float4( DirectSpecHighlights463 , 0.0 ) * HalfLambert695 );
				#ifdef _ENABLESECONDARYHIGHLIGHTS_ON
				float4 staticSwitch456 = SpecularHighlights457;
				#else
				float4 staticSwitch456 = float4( 0,0,0,0 );
				#endif
				float4 color450 = IsGammaSpace() ? float4( 1, 1, 1, 1 ) : float4( 1, 1, 1, 1 );
				#ifdef _ENABLECOLORTINT_ON
				float4 staticSwitch451 = _BaseTint;
				#else
				float4 staticSwitch451 = color450;
				#endif
				float dotResult684 = dot( Normals444 , SafeNormalize( _MainLightPosition.xyz ) );
				float RampScale710 = _RampScale;
				float RampOffset709 = _RampOffset;
				float CEL_Effect437 = saturate( (dotResult684*RampScale710 + RampOffset709) );
				float2 temp_cast_8 = (CEL_Effect437).xx;
				float4 lerpResult543 = lerp( triplanar573 , triplanar537 , OverlayCoverage340);
				#ifdef _ENABLECOVERAGE_ON
				float4 staticSwitch795 = lerpResult543;
				#else
				float4 staticSwitch795 = triplanar537;
				#endif
				float2 uv_DetailMask525 = input.ase_texcoord9.xy;
				float4 tex2DNode525 = tex2D( _DetailMask, uv_DetailMask525 );
				float temp_output_578_0 = saturate( tex2DNode525.r );
				float4 lerpResult814 = lerp( staticSwitch795 , ( _EdgeWearTint * temp_output_578_0 ) , ( _EdgeWear1 * temp_output_578_0 ));
				float temp_output_584_0 = saturate( tex2DNode525.b );
				float4 lerpResult542 = lerp( lerpResult814 , ( _CavityTint * temp_output_584_0 ) , ( _Cavity * temp_output_584_0 ));
				float4 BlendedTextures167 = lerpResult542;
				float3 WorldPosition288_g61181 = PositionWS;
				float3 WorldPosition305_g61181 = WorldPosition288_g61181;
				float2 ScreenUV286_g61181 = (ScreenPosNorm).xy;
				float2 ScreenUV305_g61181 = ScreenUV286_g61181;
				float3 WorldNormal281_g61181 = Normals444;
				float3 WorldNormal305_g61181 = WorldNormal281_g61181;
				half2 LightmapUV1_g61183 = (input.ase_texcoord9.zw*(unity_LightmapST).xy + (unity_LightmapST).zw);
				half4 localCalculateShadowMask1_g61183 = CalculateShadowMask1_g61183( LightmapUV1_g61183 );
				float4 ShadowMask360_g61181 = localCalculateShadowMask1_g61183;
				float4 ShadowMask305_g61181 = ShadowMask360_g61181;
				float3 localAdditionalLightsLambertMask17x305_g61181 = AdditionalLightsLambertMask17x( WorldPosition305_g61181 , ScreenUV305_g61181 , WorldNormal305_g61181 , ShadowMask305_g61181 );
				float3 saferPower843 = abs( saturate( localAdditionalLightsLambertMask17x305_g61181 ) );
				float3 temp_cast_10 = ( (0.001 + ( _AdditionalLightFalloff - 0.0 ) * ( 12.0 - 0.001 ) / ( 12.0 - 0.0 ) )).xxx;
				float4 CustomLighting109 = ( staticSwitch456 + ( staticSwitch451 * ( ( ( tex2D( _TextureRamp, temp_cast_8 ) * BlendedTextures167 ) * HalfLambert695 ) + ( BlendedTextures167 * tex2D( _TextureRamp, (pow( saferPower843 , temp_cast_10 )* (0.001 + ( _AdditionalLightInfluence - 0.0 ) * ( 6.0 - 0.001 ) / ( 1.0 - 0.0 ) ) + RampOffset709).xy ) ) ) ) );
				
				float3 SpecularTint825 = _SpecularColor.rgb;
				float2 uv_SurfaceTexture847 = input.ase_texcoord9.xy;
				
				float3 EmissionColor396 = _EmissionColor.rgb;
				float2 uv_DetailMask393 = input.ase_texcoord9.xy;
				float EmissionColorAlpha398 = _EmissionColor.a;
				float mulTime782 = _TimeParameters.x * _FlickerFrequency;
				#ifdef _ENABLEEMISSION_ON
				float3 staticSwitch405 = ( ( _EmissionIntensity1 * EmissionColor396 * tex2D( _DetailMask, uv_DetailMask393 ).g * EmissionColorAlpha398 ) * ( ( sin( ( ( mulTime782 + ( PositionWS.x + PositionWS.z ) ) / _FlickerScale ) ) * ( ( _MaxIntensity - _MinIntensity ) * 0.5 ) ) + ( ( _MaxIntensity + _MinIntensity ) * 0.5 ) ) );
				#else
				float3 staticSwitch405 = float3( 0,0,0 );
				#endif
				float3 Emission788 = staticSwitch405;
				

				float3 BaseColor = CustomLighting109.rgb;
				float3 Normal = BlendedNormals169;
				float3 Specular = ( _SpecularIntensity1 * SpecularTint825 * tex2D( _SurfaceTexture, uv_SurfaceTexture847 ).a );
				float Metallic = 0;
				float Smoothness = _Smoothness;
				float Occlusion = _Occlusion1;
				float3 Emission = Emission788;
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
					float AlphaClipThresholdShadow = 0.5;
				#endif
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_CHANGES_WORLD_POS)
					ShadowCoord = TransformWorldToShadowCoord( PositionWS );
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = PositionWS;
				inputData.positionCS = input.positionCS;
				inputData.normalizedScreenSpaceUV = ScreenPosNorm.xy;
				inputData.shadowCoord = ShadowCoord;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
						inputData.normalWS = TransformTangentToWorld(Normal, half3x3( TangentWS, BitangentWS, NormalWS ));
					#elif _NORMAL_DROPOFF_OS
						inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
						inputData.normalWS = Normal;
					#endif
				#else
					inputData.normalWS = NormalWS;
				#endif

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				inputData.viewDirectionWS = SafeNormalize( ViewDirWS );

				#ifdef ASE_FOG
					// @diogo: no fog applied in GBuffer
				#endif
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#endif

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = input.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(_SCREEN_SPACE_IRRADIANCE) && ( UNITY_VERSION >= 60030000 )
					#if ( UNITY_VERSION >= 60060000 )
						inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy, inputData.normalWS));
					#else
						inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy);
					#endif
				#elif defined(DYNAMICLIGHTMAP_ON)
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, input.dynamicLightmapUV.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
					inputData.bakedGI = SAMPLE_GI(SH,
						GetAbsolutePositionWS(inputData.positionWS),
						inputData.normalWS,
						inputData.viewDirectionWS,
						input.positionCS.xy,
						input.probeOcclusion,
						inputData.shadowMask);
				#else
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
						#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = input.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
					#if defined(USE_APV_PROBE_OCCLUSION)
						inputData.probeOcclusion = input.probeOcclusion;
					#endif
				#endif

				#ifdef _DBUFFER
					ApplyDecal(input.positionCS,
						BaseColor,
						Specular,
						inputData.normalWS,
						Metallic,
						Occlusion,
						Smoothness);
				#endif

				BRDFData brdfData;
				InitializeBRDFData(BaseColor, Metallic, Specular, Smoothness, Alpha, brdfData);

				Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
				half4 color;
				MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);

			#if ( UNITY_VERSION >= 60010000 )
				color.rgb = GlobalIllumination(brdfData, (BRDFData)0, 0,
                              inputData.bakedGI, Occlusion, inputData.positionWS,
                              inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV);
			#else
				color.rgb = GlobalIllumination(brdfData, inputData.bakedGI, Occlusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);
			#endif

				color.a = Alpha;

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

			#if ( UNITY_VERSION >= 60010000 )
				return PackGBuffersBRDFData(brdfData, inputData, Smoothness, Emission + color.rgb, Occlusion);
			#else
				return BRDFDataToGbuffer(brdfData, inputData, Smoothness, Emission + color.rgb, Occlusion);
			#endif
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SPECULAR_SETUP 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170100


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SCENESELECTIONPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _CavityTint;
			float4 _BaseTint;
			float4 _EdgeWearTint;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion1;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _AdditionalLightFalloff;
			float _Cavity;
			float _EdgeWear1;
			float _RampOffset;
			float _RampScale;
			float _SecondarySmoothness;
			float _MacroNormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _OverlayCoverage;
			float _OverlayNormalScale;
			float _SurfaceNormalScale;
			float _TransitionEnd;
			float _TransitionStart;
			float _OverlayTextureTiling;
			float _CoverageFalloff;
			float _SurfaceTextureTiling;
			float _MaxIntensity;
			float _MinIntensity;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction(Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag( PackedVaryings input
				#if defined( ASE_WRITE_DEPTH )
				,out float outputDepth : ASE_SV_DEPTH
				#endif
				 ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;

				

				surfaceDescription.Alpha = 1;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return half4( _ObjectId, _PassValue, 1.0, 1.0 );
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SPECULAR_SETUP 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170100


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

		    #define SCENEPICKINGPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _CavityTint;
			float4 _BaseTint;
			float4 _EdgeWearTint;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion1;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _AdditionalLightFalloff;
			float _Cavity;
			float _EdgeWear1;
			float _RampOffset;
			float _RampScale;
			float _SecondarySmoothness;
			float _MacroNormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _OverlayCoverage;
			float _OverlayNormalScale;
			float _SurfaceNormalScale;
			float _TransitionEnd;
			float _TransitionStart;
			float _OverlayTextureTiling;
			float _CoverageFalloff;
			float _SurfaceTextureTiling;
			float _MaxIntensity;
			float _MinIntensity;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag( PackedVaryings input
				#if defined( ASE_WRITE_DEPTH )
				,out float outputDepth : ASE_SV_DEPTH
				#endif
				 ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;

				

				surfaceDescription.Alpha = 1;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return unity_SelectionID;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "MotionVectors"
			Tags { "LightMode"="MotionVectors" }

			ColorMask RG

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SPECULAR_SETUP 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_TIME_BASED_MOTION_VECTORS
			#define ASE_FOG 1
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170100


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

            #define SHADERPASS SHADERPASS_MOTION_VECTORS

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
		    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
		    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
			#endif

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MotionVectorsCommon.hlsl"

			

			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			#if ( UNITY_VERSION < 60010000 )
				#define APPLICATION_SPACE_WARP_MOTION APLICATION_SPACE_WARP_MOTION
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 positionOld : TEXCOORD4;
				#if _ADD_PRECOMPUTED_VELOCITY
					float3 alembicMotionVector : TEXCOORD5;
				#endif
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 positionCSNoJitter : TEXCOORD0;
				float4 previousPositionCSNoJitter : TEXCOORD1;
				float3 positionWS : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _CavityTint;
			float4 _BaseTint;
			float4 _EdgeWearTint;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion1;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _AdditionalLightFalloff;
			float _Cavity;
			float _EdgeWear1;
			float _RampOffset;
			float _RampScale;
			float _SecondarySmoothness;
			float _MacroNormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _OverlayCoverage;
			float _OverlayNormalScale;
			float _SurfaceNormalScale;
			float _TransitionEnd;
			float _TransitionStart;
			float _OverlayTextureTiling;
			float _CoverageFalloff;
			float _SurfaceTextureTiling;
			float _MaxIntensity;
			float _MinIntensity;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			// Applies the graph's vertex stage at a given time so the motion vector pass can
			// evaluate the current frame and re-evaluate the previous frame (procedural / time-based animation).
			Attributes ASEApplyVertexModification( Attributes input, float3 timeParameters, inout PackedVaryings output, out float3 customMotionVector  )
			{
				float3 currentTimeParameters = _TimeParameters.xyz;
				_TimeParameters.xyz = timeParameters;

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				customMotionVector = float3(0, 0, 0);

				_TimeParameters.xyz = currentTimeParameters;
				return input;
			}

			PackedVaryings VertexFunction( Attributes input )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				Attributes defaultInput = input;
				float3 currentMotionVector;
				input = ASEApplyVertexModification( input, _TimeParameters.xyz, output, currentMotionVector );

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				#if defined(APPLICATION_SPACE_WARP_MOTION)
					float4 positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, input.positionOS));
					float4 positionCS = positionCSNoJitter;
				#else
					float4 positionCS = vertexInput.positionCS;
					float4 positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, input.positionOS));
				#endif

				// Custom output and automatic time-based motion are mutually exclusive.
				#if defined(ASE_CUSTOM_MOTION_VECTOR)
					float3 prevPositionOS = ( unity_MotionVectorsParams.x == 1 ) ? input.positionOld : input.positionOS.xyz;
					prevPositionOS -= currentMotionVector;
				#else
					float3 prevPositionOS = ( unity_MotionVectorsParams.x == 1 ) ? input.positionOld : defaultInput.positionOS.xyz;
					#ifdef ASE_TIME_BASED_MOTION_VECTORS
						Attributes prevInput = defaultInput;
						prevInput.positionOS.xyz = prevPositionOS;
						PackedVaryings prevOutput = (PackedVaryings)0;
						float3 prevMotionVector;
						prevInput = ASEApplyVertexModification( prevInput, _LastTimeParameters.xyz, prevOutput, prevMotionVector );
						prevPositionOS = prevInput.positionOS.xyz;
					#endif
				#endif
				#if _ADD_PRECOMPUTED_VELOCITY
					prevPositionOS -= input.alembicMotionVector;
				#endif
				float4 previousPositionCSNoJitter = mul( _PrevViewProjMatrix, mul( UNITY_PREV_MATRIX_M, float4( prevPositionOS, 1 ) ) );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( positionCS );
				output.positionCSNoJitter = ASE_ADJUST_CLIP_POSITION( positionCSNoJitter );
				output.previousPositionCSNoJitter = ASE_ADJUST_CLIP_POSITION( previousPositionCSNoJitter );
				output.positionWS = vertexInput.positionWS;

				return output;
			}

			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}

			half4 frag(	PackedVaryings input
				#if defined( ASE_WRITE_DEPTH )
				,out float outputDepth : ASE_SV_DEPTH
				#endif
				 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;

				

				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#if defined(ASE_CHANGES_WORLD_POS)
					float3 positionOS = mul( GetWorldToObjectMatrix(),  float4( PositionWS, 1.0 ) ).xyz;
					float3 previousPositionWS = mul( GetPrevObjectToWorldMatrix(),  float4( positionOS, 1.0 ) ).xyz;
					input.positionCSNoJitter = mul( _NonJitteredViewProjMatrix, float4( PositionWS, 1.0 ) );
					input.previousPositionCSNoJitter = mul( _PrevViewProjMatrix, float4( previousPositionWS, 1.0 ) );
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				#if defined(APPLICATION_SPACE_WARP_MOTION)
					return float4( CalcAswNdcMotionVectorFromCsPositions( input.positionCSNoJitter, input.previousPositionCSNoJitter ), 1 );
				#else
					return float4( CalcNdcMotionVectorFromCsPositions( input.positionCSNoJitter, input.previousPositionCSNoJitter ), 0, 0 );
				#endif
			}
			ENDHLSL
		}

	
	}
	

	

	CustomEditor "UnityEditor.ShaderGraphLitGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19912
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":612,"pos":[1888,-4096],"params":["Inherit","False","2349.958","639.8836","Normals","19","809","804","806","805","800","591","759","758","594","739","610","593","609","599","592","595","799","810","831","","0.4980392,0,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":611,"pos":[1888,-5248],"params":["Inherit","False","2386.304","1084.61","Albedo","25","542","818","819","541","814","522","816","817","815","527","584","795","578","796","519","525","538","573","537","737","532","533","520","797","830","Blended Textures","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":619,"pos":[-880,-1360],"params":["Inherit","False","3848.118","1797.501","","44","626","625","624","636","676","673","668","667","666","665","664","663","662","660","658","656","655","654","653","648","647","646","645","644","643","642","641","640","639","638","637","635","634","633","632","631","630","629","623","622","621","620","748","747","Fresnel","0.7960784,0.7215686,0,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":762,"pos":[-880,560],"params":["Inherit","False","2321.01","875.313","","19","781","780","779","778","777","776","775","774","773","772","771","770","765","764","763","393","392","403","405","Emission","1,0.1319249,0,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":349,"pos":[-880,-3776],"params":["Inherit","False","2018.011","624.8931","","15","576","575","345","329","331","338","339","544","330","348","324","746","755","760","761","Overlay Coverage","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":438,"pos":[-880,-6128],"params":["Inherit","False","710.7263","318.0039","","3","443","442","439","Normals","0.6382856,0.4745098,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":470,"pos":[-880,-1856],"params":["Inherit","False","2046.093","408.9518","Indirect","14","822","488","472","792","471","466","473","497","823","734","821","826","827","861","","1,0,0.390008,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":504,"pos":[-880,-5248],"params":["Inherit","False","2242.611","1394.968","","31","696","697","840","702","701","698","700","214","445","703","448","699","704","449","454","447","446","452","181","166","456","455","453","451","450","155","842","844","843","845","860","Custom Lighting","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":458,"pos":[-880,-3072],"params":["Inherit","False","646","316","","4","459","462","460","461","Specular Highlights","1,0,0.3882353,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":613,"pos":[272,-6128],"params":["Inherit","False","592.2708","402.95","Normal Lerp","4","617","616","615","614","","0.6392157,0.4745098,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":681,"pos":[272,-6528],"params":["Inherit","False","1166.792","305.3181","","7","688","687","686","685","684","683","682","CEL Effect","0.7960785,0.7215686,0,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":689,"pos":[-880,-6528],"params":["Inherit","False","816","304","","5","694","693","692","691","690","Half Lambert","0.8078432,0.7294118,0,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":713,"pos":[-880,-2736],"params":["Inherit","False","2048.144","858.4296","Direct","24","723","735","733","732","731","730","729","727","726","725","724","722","719","718","717","715","820","825","852","853","854","855","856","858","","1,0,0.390008,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":799,"pos":[2848,-3856],"params":["Inherit","False","228","210.8501","TriBlended","1","590","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":797,"pos":[2784,-4960],"params":["Inherit","False","228","354.8501","TriBlended","2","580","543","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":620,"pos":[-816,-160],"params":["Inherit","False","452","394.7998","Fresnel","3","657","652","651","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":621,"pos":[-816,-1280],"params":["Inherit","False","784","363.95","Half Vector","7","675","674","672","671","670","669","628","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":622,"pos":[-816,-608],"params":["Inherit","False","452","394.7998","NdotL","3","661","650","649","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":623,"pos":[-144,-128],"params":["Inherit","False","228","162.9502","InvertedViewMask","1","659","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":763,"pos":[-816,992],"params":["Inherit","False","799.4725","322.7755","Time+Offset","7","791","769","786","785","784","783","782","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":439,"pos":[-832,-6032],"params":["Inherit","False","169","BlendedNormals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor","id":442,"pos":[-560,-6032],"params":["Inherit","False","True","1","0","FLOAT3","0,0,1","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.NormalizeNode, AmplifyShaderEditor","id":443,"pos":[-352,-6032],"params":["Inherit","False","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":459,"pos":[-464,-2960],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":339,"pos":[960,-3568],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":338,"pos":[448,-3632],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":595,"pos":[1968,-4048],"params":["Inherit","False","207","OverlayNormalMap","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":592,"pos":[2000,-3808],"params":["Inherit","False","203","MainNormalMap","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":599,"pos":[2464,-3568],"params":["Inherit","False","340","OverlayCoverage","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor","id":614,"pos":[432,-6080],"params":["Inherit","False","True","1","0","FLOAT3","0,0,1","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":615,"pos":[432,-5920],"params":["Inherit","False","444","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":617,"pos":[688,-5888],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":618,"pos":[928,-5888],"params":["Inherit","False","NormalLerp","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":628,"pos":[-768,-1168],"params":["Inherit","False","618","NormalLerp","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor","id":629,"pos":[896,-1024],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.WorldSpaceCameraPos, AmplifyShaderEditor","id":630,"pos":[896,-832],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.DistanceOpNode, AmplifyShaderEditor","id":631,"pos":[1152,-1024],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":632,"pos":[1376,-880],"params":["Inherit","False","201","MainTexture","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":633,"pos":[1568,-1024],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":634,"pos":[1584,-880],"params":["Inherit","True","Property","_TextureSample5","Texture Sample 2","10","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":635,"pos":[2000,-880],"params":["Inherit","False","Property","_SpecularInfluence","Specular Influence","50","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":637,"pos":[896,-1264],"params":["Inherit","False","5","5","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","2","FLOAT3","0,0,0","False","3","FLOAT","0","False","4","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":638,"pos":[1888,-1264],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":639,"pos":[2128,-1168],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":640,"pos":[2368,-1264],"params":["Inherit","False","3","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","2","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":641,"pos":[640,-1264],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":642,"pos":[320,-1168],"params":["Inherit","False","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":643,"pos":[-816,-864],"params":["Inherit","False","618","NormalLerp","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":644,"pos":[-592,-864],"params":["Inherit","False","SRP Additional Light","-1","","61164","6c86746ad131a0a408ca599df5f40861","3,6,1,351,1,23,0","6","2","FLOAT3","0,0,0","False","11","FLOAT3","0,0,0","False","345","FLOAT3","0,0,0","False","346","FLOAT3","0,0,0","False","347","FLOAT","0.5","False","32","FLOAT4","0,0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":645,"pos":[-144,-864],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":646,"pos":[-320,-864],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":647,"pos":[0,-1040],"params":["Inherit","False","Property","_RimIntensity1","Rim Intensity","47","0","Create","True","0","0","0","False","0","False","Object","-1","","0.2","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":648,"pos":[320,-1040],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","0","False","4","FLOAT","1000","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldSpaceLightDirHlpNode, AmplifyShaderEditor","id":649,"pos":[-768,-528],"params":["Inherit","False","True","1","0","FLOAT","0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":650,"pos":[-768,-352],"params":["Inherit","False","618","NormalLerp","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ViewDirInputsCoordNode, AmplifyShaderEditor","id":651,"pos":[-768,64],"params":["Inherit","False","World","True","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":652,"pos":[-752,-80],"params":["Inherit","False","618","NormalLerp","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LightColorNode, AmplifyShaderEditor","id":653,"pos":[-592,-752],"params":["Inherit","False","0","3","COLOR","0","FLOAT3","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":654,"pos":[-320,160],"params":["Inherit","False","Property","_ViewEdgeThreshold","ViewEdgeThreshold","45","0","Create","True","0","0","0","False","1","Space (8)","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":655,"pos":[224,160],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":656,"pos":[-32,160],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","0.4","False","4","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":657,"pos":[-512,-80],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":658,"pos":[-320,-80],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":659,"pos":[-96,-80],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor","id":660,"pos":[416,-80],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":661,"pos":[-512,-448],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":662,"pos":[-320,-448],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":663,"pos":[688,-448],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":664,"pos":[416,-448],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor","id":665,"pos":[1376,-1024],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LightAttenuation, AmplifyShaderEditor","id":666,"pos":[544,-1040],"params":["Inherit","False","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.IndirectDiffuseLighting, AmplifyShaderEditor","id":667,"pos":[576,-944],"params":["Inherit","False","Tangent","1","0","FLOAT3","0,0,1","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":668,"pos":[80,-1232],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":669,"pos":[-768,-1008],"params":["Inherit","False","Property","_RimSpread","Rim Spread","48","0","Create","True","0","0","0","False","0","False","Object","-1","","0.2","0.2","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":670,"pos":[-176,-1232],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":671,"pos":[-464,-1088],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0.2","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":672,"pos":[-304,-1168],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":673,"pos":[1152,-816],"params":["Inherit","False","Property","_DistanceFade1","Distance Fade","51","0","Create","True","0","0","0","False","1","Space (8)","False","Object","-1","","12","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":674,"pos":[-768,-1088],"params":["Inherit","False","Blinn-Phong Half Vector","-1","","61166","91a149ac9d615be429126c95e20753ce","0","0","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":675,"pos":[-544,-1232],"params":["Inherit","False","Constant","_Float2","Float 0","27","0","Create","True","0","0","0","False","0","False","Object","-1","","-0.2","-0.1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":676,"pos":[-128,336],"params":["Inherit","False","Property","_ViewEdgeSoftness","ViewEdgeSoftness","46","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldSpaceLightDirHlpNode, AmplifyShaderEditor","id":682,"pos":[320,-6384],"params":["Inherit","False","True","1","0","FLOAT","0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":683,"pos":[352,-6480],"params":["Inherit","False","444","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":684,"pos":[592,-6464],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ScaleAndOffsetNode, AmplifyShaderEditor","id":685,"pos":[1008,-6464],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0.5","False","2","FLOAT","0.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":686,"pos":[1264,-6464],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":687,"pos":[704,-6320],"params":["Inherit","False","709","RampOffset","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":688,"pos":[704,-6400],"params":["Inherit","False","710","RampScale","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":437,"pos":[1488,-6464],"params":["Inherit","False","CEL Effect","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LightColorNode, AmplifyShaderEditor","id":690,"pos":[-832,-6480],"params":["Inherit","False","0","3","COLOR","0","FLOAT3","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.LightAttenuation, AmplifyShaderEditor","id":691,"pos":[-832,-6336],"params":["Inherit","False","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.IndirectDiffuseLighting, AmplifyShaderEditor","id":692,"pos":[-496,-6336],"params":["Inherit","False","Tangent","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":693,"pos":[-496,-6480],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":694,"pos":[-224,-6480],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT3","0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":695,"pos":[0,-6480],"params":["Inherit","False","HalfLambert","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":444,"pos":[-112,-6032],"params":["Inherit","False","Normals","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":155,"pos":[64,-4608],"params":["Inherit","True","Property","_TextureRampRef","Texture Ramp Ref","0","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":450,"pos":[-208,-5024],"params":["Inherit","False","Constant","_DefaultTint","Default Tint","19","0","Create","True","0","0","0","False","0","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":451,"pos":[80,-4864],"params":["Inherit","False","Property","_EnableColorTint","Enable Color Tint","0","0","Create","True","0","0","0","False","2","Header(Color)","Space(8)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","COLOR","0,0,0,0","False","0","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","3","COLOR","0,0,0,0","False","4","COLOR","0,0,0,0","False","5","COLOR","0,0,0,0","False","6","COLOR","0,0,0,0","False","7","COLOR","0,0,0,0","False","8","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":453,"pos":[400,-4992],"params":["Inherit","False","627","RimLight","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":455,"pos":[272,-5168],"params":["Inherit","False","457","SpecularHighlights","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":452,"pos":[976,-4864],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":446,"pos":[432,-4608],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":447,"pos":[624,-4608],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":715,"pos":[-768,-2080],"params":["Inherit","False","444","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":718,"pos":[-496,-2160],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":733,"pos":[-800,-2160],"params":["Inherit","False","Blinn-Phong Half Vector","-1","","61171","91a149ac9d615be429126c95e20753ce","0","0","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":463,"pos":[1216,-2432],"params":["Inherit","False","DirectSpecHighlights","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":199,"pos":[4896,-6656],"params":["Inherit","False","DetailMask","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":205,"pos":[4896,-6032],"params":["Inherit","False","OverlayTexture","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":203,"pos":[4896,-6240],"params":["Inherit","False","MainNormalMap","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":207,"pos":[4896,-5824],"params":["Inherit","False","OverlayNormalMap","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":201,"pos":[4896,-6448],"params":["Inherit","False","MainTexture","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":197,"pos":[4896,-6864],"params":["Inherit","False","TextureRamp","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":196,"pos":[4656,-6864],"params":["Inherit","True","Property","_TextureRamp","Texture Ramp","2","3","[Header]","[NoScaleOffset]","[SingleLineTexture]","Create","True","1","Textures","0","0","False","1","Space(8)","False","","None","None","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":204,"pos":[4656,-6032],"params":["Inherit","True","Property","_OverlayTexture","Overlay Texture","12","2","[NoScaleOffset]","[SingleLineTexture]","Create","True","0","0","0","False","1","Space(8)","False","","None","None","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":610,"pos":[1936,-3968],"params":["Inherit","False","261","OverlayNormalMapScale","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":739,"pos":[1968,-3888],"params":["Inherit","False","172","OverlayTextureTiling","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":243,"pos":[4112,-6640],"params":["Inherit","False","2","0","FLOAT","1","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":740,"pos":[4112,-6848],"params":["Inherit","False","2","0","FLOAT","1","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":216,"pos":[3744,-6848],"params":["Inherit","False","Property","_OverlayTextureTiling","Overlay Texture Tiling","13","0","Create","True","0","0","0","False","0","False","Object","-1","","10.09175","9.2","0","20","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":172,"pos":[4304,-6848],"params":["Inherit","False","OverlayTextureTiling","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":198,"pos":[4656,-6656],"params":["Inherit","True","Property","_DetailMask","Detail Mask","16","2","[NoScaleOffset]","[SingleLineTexture]","Create","True","0","0","0","False","1","Space (8)","False","","None","None","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":636,"pos":[320,-1264],"params":["Inherit","False","679","RimLightColor","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":454,"pos":[608,-5008],"params":["Inherit","False","Property","_EnableRimLighting1","Enable Rim Lighting","43","0","Create","True","0","0","0","False","2","Header (Rim Lighting)","Space(8)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","COLOR","0,0,0,0","False","0","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","3","COLOR","0,0,0,0","False","4","COLOR","0,0,0,0","False","5","COLOR","0,0,0,0","False","6","COLOR","0,0,0,0","False","7","COLOR","0,0,0,0","False","8","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":616,"pos":[336,-5840],"params":["Inherit","False","Property","_NormalMapInfluence","Normal Map Influence","49","0","Create","True","0","0","0","False","1","Space (8)","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":449,"pos":[-208,-4832],"params":["Inherit","False","Property","_BaseTint","Base Tint","1","0","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":758,"pos":[2832,-4048],"params":["Inherit","False","TriNormalOverlay","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":759,"pos":[2832,-3568],"params":["Inherit","False","TriNormalMain","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor","id":748,"pos":[2352,-976],"params":["Inherit","False","0","5","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":747,"pos":[2768,-1056],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":770,"pos":[-816,624],"params":["Inherit","False","Property","_EmissionIntensity1","Emission Intensity","38","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":772,"pos":[16,1024],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":392,"pos":[-816,704],"params":["Inherit","False","199","DetailMask","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":393,"pos":[-592,704],"params":["Inherit","True","Property","_TextureSample1","Texture Sample 1","18","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":405,"pos":[1120,624],"params":["Inherit","False","Property","_EnableEmission","Enable Emission","36","0","Create","True","0","0","0","False","2","Header (Emission)","Space (8)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":788,"pos":[1504,624],"params":["Inherit","False","Emission","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":774,"pos":[16,1184],"params":["Inherit","False","Property","_MinIntensity","Min Intensity","41","0","Create","True","0","0","0","False","0","False","Object","-1","","0.75","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":775,"pos":[16,1104],"params":["Inherit","False","Property","_MaxIntensity","Max Intensity","42","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":778,"pos":[464,1104],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":779,"pos":[464,1216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":780,"pos":[656,1024],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":781,"pos":[848,1024],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":771,"pos":[928,624],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":777,"pos":[240,1328],"params":["Inherit","False","Constant","_Half","Half","30","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":773,"pos":[272,1104],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":776,"pos":[272,1216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleTimeNode, AmplifyShaderEditor","id":782,"pos":[-544,1040],"params":["Inherit","False","1","0","FLOAT","1","False","5","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":783,"pos":[-336,1040],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":784,"pos":[-544,1120],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor","id":785,"pos":[-768,1120],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":786,"pos":[-768,1040],"params":["Inherit","False","Property","_FlickerFrequency","Flicker Frequency","39","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":769,"pos":[-544,1232],"params":["Inherit","False","Property","_FlickerScale","Flicker Scale","40","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":791,"pos":[-176,1040],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":340,"pos":[1184,-3568],"params":["Inherit","False","OverlayCoverage","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor","id":544,"pos":[736,-3568],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0.33","False","2","FLOAT","0.66","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor","id":324,"pos":[-128,-3728],"params":["Inherit","False","False","1","0","FLOAT3","0,0,1","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":348,"pos":[-128,-3568],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","-1","False","4","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":330,"pos":[112,-3632],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":329,"pos":[-224,-3376],"params":["Inherit","True","Property","_CovMask","Coverage Mask","13","1","[NoScaleOffset]","Create","False","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","0,0","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":345,"pos":[-464,-3376],"params":["Inherit","False","199","DetailMask","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":760,"pos":[-832,-3648],"params":["Inherit","False","758","TriNormalOverlay","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":331,"pos":[112,-3376],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":575,"pos":[384,-3520],"params":["Inherit","False","Property","_TransitionStart","Transition Start","21","0","Create","True","0","0","0","False","0","False","Object","-1","","0.65","0.65","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":576,"pos":[384,-3440],"params":["Inherit","False","Property","_TransitionEnd","Transition End","22","0","Create","True","0","0","0","False","0","False","Object","-1","","0.35","0.35","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":538,"pos":[2448,-4720],"params":["Inherit","False","340","OverlayCoverage","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":543,"pos":[2832,-4912],"params":["Inherit","False","3","0","FLOAT4","0,0,0,0","False","1","FLOAT4","0,0,0,0","False","2","FLOAT","0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":580,"pos":[2832,-4768],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":525,"pos":[2768,-4576],"params":["Inherit","True","Property","_EdgeWearMaskTextureRef1","Edge Wear Mask Texture Ref","2","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":519,"pos":[2544,-4576],"params":["Inherit","False","199","DetailMask","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":590,"pos":[2896,-3808],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":800,"pos":[3136,-4048],"params":["Inherit","False","Property","_Keyword0","Keyword 0","17","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","795","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":806,"pos":[3360,-3856],"params":["Inherit","True","Property","_TextureSample0","Texture Sample 0","46","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","True","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":206,"pos":[4656,-5824],"params":["Inherit","True","Property","_OverlayNormalMap","Overlay Normal Map","14","3","[NoScaleOffset]","[Normal]","[SingleLineTexture]","Create","True","0","0","0","False","1","Space(8)","False","","None","None","True","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":202,"pos":[4656,-6240],"params":["Inherit","True","Property","_SurfaceNormalMap","Surface Normal Map","10","3","[NoScaleOffset]","[Normal]","[SingleLineTexture]","Create","True","0","0","0","False","1","Space(8)","False","","None","None","True","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":200,"pos":[4656,-6448],"params":["Inherit","True","Property","_SurfaceTexture","Surface Texture","8","2","[NoScaleOffset]","[SingleLineTexture]","Create","True","0","0","0","False","1","Space (8)","False","","None","None","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":755,"pos":[-592,-3728],"params":["Inherit","False","Property","_CoverageMaskSource","Coverage Mask Source","20","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","KeywordEnum","2","SurfaceNormalMap","OverlayNormalMap","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":680,"pos":[4304,-5904],"params":["Inherit","False","RimLightAlpha","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":711,"pos":[3840,-5792],"params":["Inherit","False","Property","_RampScale","Ramp Scale","3","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":712,"pos":[3840,-5696],"params":["Inherit","False","Property","_RampOffset","Ramp Offset","4","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":709,"pos":[4304,-5696],"params":["Inherit","False","RampOffset","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":710,"pos":[4304,-5792],"params":["Inherit","False","RampScale","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":679,"pos":[4304,-6016],"params":["Inherit","False","RimLightColor","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":398,"pos":[4304,-6128],"params":["Inherit","False","EmissionColorAlpha","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":396,"pos":[4304,-6240],"params":["Inherit","False","EmissionColor","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":362,"pos":[3840,-6240],"params":["Inherit","False","Property","_EmissionColor","Emission Color","37","1","[HDR]","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","0,0,0,0","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":678,"pos":[3840,-6016],"params":["Inherit","False","Property","_RimLightColor","Rim Light Color","44","1","[HDR]","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":260,"pos":[3792,-6448],"params":["Inherit","False","Property","_OverlayNormalScale","Overlay Normal Scale","15","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","5","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":261,"pos":[4304,-6448],"params":["Inherit","False","OverlayNormalMapScale","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":257,"pos":[3792,-6528],"params":["Inherit","False","Property","_SurfaceNormalScale","Surface Normal Scale","11","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","5","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":807,"pos":[3792,-6368],"params":["Inherit","False","Property","_MacroNormalScale","Macro Normal Scale","7","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","5","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":808,"pos":[4304,-6368],"params":["Inherit","False","MacroNormalMapScale","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":258,"pos":[4304,-6528],"params":["Inherit","False","SurfaceNormalMapScale","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":218,"pos":[4304,-6640],"params":["Inherit","False","SurfaceTextureTiling","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":804,"pos":[3136,-3856],"params":["Inherit","False","803","MacroNormalMap","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":810,"pos":[3904,-4048],"params":["Inherit","False","Property","_EnableMacroNormals","Enable Macro Normals","5","0","Create","True","0","0","0","False","1","Space(8)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":802,"pos":[4656,-5616],"params":["Inherit","True","Property","_MacroNormalMap","Macro Normal Map","6","3","[NoScaleOffset]","[Normal]","[SingleLineTexture]","Create","True","0","0","0","False","0","False","","None","None","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":44,"pos":[3744,-6640],"params":["Inherit","False","Property","_SurfaceTextureTiling","Surface Texture Tiling","9","0","Create","True","0","0","0","False","0","False","Object","-1","","2","8.9","0.001","512","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":803,"pos":[4896,-5616],"params":["Inherit","False","MacroNormalMap","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":809,"pos":[3104,-3760],"params":["Inherit","False","808","MacroNormalMapScale","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":761,"pos":[-832,-3728],"params":["Inherit","False","759","TriNormalMain","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":578,"pos":[3104,-4528],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":795,"pos":[3072,-4976],"params":["Inherit","False","Property","_EnableCoverage","Enable Coverage","17","0","Create","True","0","0","0","False","2","Header(Coverage Options)","Space(8)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT4","0,0,0,0","False","0","FLOAT4","0,0,0,0","False","2","FLOAT4","0,0,0,0","False","3","FLOAT4","0,0,0,0","False","4","FLOAT4","0,0,0,0","False","5","FLOAT4","0,0,0,0","False","6","FLOAT4","0,0,0,0","False","7","FLOAT4","0,0,0,0","False","8","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":584,"pos":[3104,-4448],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":815,"pos":[3376,-5040],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":817,"pos":[3376,-4768],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":816,"pos":[3072,-5184],"params":["Inherit","False","Property","_EdgeWearTint","Edge Wear Tint","23","0","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","0,0,0,0","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":522,"pos":[3072,-4768],"params":["Inherit","False","Property","_EdgeWear1","Edge Wear","24","0","Create","True","1","Highlights","0","0","False","0","False","Object","-1","","1","0.792","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":814,"pos":[3584,-4976],"params":["Inherit","False","3","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","2","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":541,"pos":[3584,-4672],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":819,"pos":[3888,-5040],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":818,"pos":[3584,-5184],"params":["Inherit","False","Property","_CavityTint","Cavity Tint","25","0","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","0,0,0,0","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":542,"pos":[4080,-4768],"params":["Inherit","False","3","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","2","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":496,"pos":[4336,-4880],"params":["Inherit","False","BlendedAlpha","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":167,"pos":[4336,-4768],"params":["Inherit","False","BlendedTextures","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":464,"pos":[1216,-1728],"params":["Inherit","False","IndirectSpecHighlights","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":497,"pos":[16,-1744],"params":["Inherit","False","444","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":473,"pos":[336,-1568],"params":["Float","False","Property","_IndirectSpecularContribution","Indirect Specular Contribution","35","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":792,"pos":[16,-1584],"params":["Inherit","False","Property","_SpecularOcclusion","Specular Occlusion","34","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","12","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.IndirectSpecularLight, AmplifyShaderEditor","id":472,"pos":[336,-1696],"params":["Inherit","False","World","3","0","FLOAT3","0,0,0","False","1","FLOAT","0.5","False","2","FLOAT","1","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":466,"pos":[672,-1696],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":471,"pos":[336,-1792],"params":["Float","False","Constant","_Float5","Float 5","20","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":823,"pos":[944,-1744],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":488,"pos":[672,-1792],"params":["Inherit","False","496","BlendedAlpha","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":796,"pos":[3072,-4880],"params":["Inherit","False","Property","_EnableCoverage1","Enable Coverage","17","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","795","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":732,"pos":[-608,-1984],"params":["Inherit","False","444","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ViewDirInputsCoordNode, AmplifyShaderEditor","id":821,"pos":[-608,-1808],"params":["Inherit","False","World","True","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":825,"pos":[-176,-2688],"params":["Inherit","False","SpecularTint","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":826,"pos":[-640,-1648],"params":["Inherit","False","825","SpecularTint","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":822,"pos":[16,-1664],"params":["Inherit","False","820","SpecularSmoothness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":764,"pos":[128,624],"params":["Inherit","False","4","4","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":765,"pos":[-272,704],"params":["Inherit","False","396","EmissionColor","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":403,"pos":[-272,848],"params":["Inherit","False","398","EmissionColorAlpha","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":532,"pos":[1936,-4880],"params":["Inherit","False","218","SurfaceTextureTiling","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":527,"pos":[3072,-4672],"params":["Inherit","False","Property","_Cavity","Cavity ","26","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0.816","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":831,"pos":[2000,-3568],"params":["Inherit","False","829","CoverageFalloff","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":609,"pos":[1936,-3728],"params":["Inherit","False","258","SurfaceNormalMapScale","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":593,"pos":[1968,-3648],"params":["Inherit","False","218","SurfaceTextureTiling","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":830,"pos":[1968,-4784],"params":["Inherit","False","829","CoverageFalloff","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":520,"pos":[2000,-4976],"params":["Inherit","False","201","MainTexture","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":737,"pos":[1936,-5088],"params":["Inherit","False","172","OverlayTextureTiling","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":533,"pos":[1968,-5184],"params":["Inherit","False","205","OverlayTexture","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":829,"pos":[4304,-5600],"params":["Inherit","False","CoverageFalloff","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":828,"pos":[3840,-5600],"params":["Inherit","False","Property","_CoverageFalloff","Coverage Falloff","19","0","Create","True","0","0","0","False","0","False","Object","-1","","2","0","1","12","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TriplanarNode, AmplifyShaderEditor","id":573,"pos":[2256,-5184],"params":["Inherit","True","Cylindrical","World","False","Top Texture 3","_TopTexture3","white","-1","None","Mid Texture 3","_MidTexture3","white","-1","None","Bot Texture 3","_BotTexture3","white","-1","None","Triplanar Sampler","Tangent","10","0","SAMPLER2D","","False","5","FLOAT","1","False","1","SAMPLER2D","","False","6","FLOAT","0","False","2","SAMPLER2D","","False","7","FLOAT","0","False","9","FLOAT3","0,0,0","False","8","FLOAT3","1,1,1","False","3","FLOAT2","1,1","False","4","FLOAT","8","False","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.TriplanarNode, AmplifyShaderEditor","id":537,"pos":[2256,-4976],"params":["Inherit","True","Cylindrical","Object","False","Top Texture 2","_TopTexture2","white","-1","None","Mid Texture 2","_MidTexture2","white","-1","None","Bot Texture 2","_BotTexture2","white","-1","None","Triplanar Sampler","Tangent","10","0","SAMPLER2D","","False","5","FLOAT","1","False","1","SAMPLER2D","","False","6","FLOAT","0","False","2","SAMPLER2D","","False","7","FLOAT","0","False","9","FLOAT3","0,0,0","False","8","FLOAT3","1,1,1","False","3","FLOAT2","1,1","False","4","FLOAT","8","False","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.TriplanarNode, AmplifyShaderEditor","id":594,"pos":[2320,-4048],"params":["Inherit","True","Cylindrical","World","True","Top Texture 5","_TopTexture5","white","-1","None","Mid Texture 5","_MidTexture5","white","-1","None","Bot Texture 5","_BotTexture5","white","-1","None","Triplanar Sampler","Tangent","10","0","SAMPLER2D","","False","5","FLOAT","1","False","1","SAMPLER2D","","False","6","FLOAT","0","False","2","SAMPLER2D","","False","7","FLOAT","0","False","9","FLOAT3","0,0,0","False","8","FLOAT3","1,1,1","False","3","FLOAT2","1,1","False","4","FLOAT","8","False","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.TriplanarNode, AmplifyShaderEditor","id":591,"pos":[2320,-3808],"params":["Inherit","True","Cylindrical","Object","True","Top Texture 4","_TopTexture4","white","-1","None","Mid Texture 4","_MidTexture4","white","-1","None","Bot Texture 4","_BotTexture4","white","-1","None","Triplanar Sampler","Tangent","10","0","SAMPLER2D","","False","5","FLOAT","1","False","1","SAMPLER2D","","False","6","FLOAT","0","False","2","SAMPLER2D","","False","7","FLOAT","0","False","9","FLOAT3","0,0,0","False","8","FLOAT3","1,1,1","False","3","FLOAT2","1,1","False","4","FLOAT","8","False","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.BlendNormalsNode, AmplifyShaderEditor","id":805,"pos":[3664,-3936],"params":["Inherit","False","0","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":169,"pos":[4288,-4048],"params":["Inherit","False","BlendedNormals","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":832,"pos":[2872.281,-7127.638],"params":["Inherit","False","169","BlendedNormals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":704,"pos":[816,-4608],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":703,"pos":[624,-4400],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":746,"pos":[-448,-3536],"params":["Inherit","False","Property","_OverlayCoverage","Overlay Coverage","18","0","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","0.001","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":448,"pos":[400,-4480],"params":["Inherit","False","695","HalfLambert","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":445,"pos":[-208,-4608],"params":["Inherit","False","437","CEL Effect","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":181,"pos":[128,-4400],"params":["Inherit","False","167","BlendedTextures","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":214,"pos":[-240,-4528],"params":["Inherit","False","197","TextureRamp","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":811,"pos":[2528,-6960],"params":["Inherit","False","803","MacroNormalMap","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":813,"pos":[2528,-6880],"params":["Inherit","False","808","MacroNormalMapScale","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":812,"pos":[2800,-7008],"params":["Inherit","True","Property","_TextureSample3","Texture Sample 3","48","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","True","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":851,"pos":[2800,-6800],"params":["Inherit","False","Property","_Smoothness","Smoothness","29","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":677,"pos":[2800,-6704],"params":["Inherit","False","Property","_Occlusion1","Occlusion","52","1","[Header]","Create","True","1","Surface Options","0","0","False","1","Space(8)","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":170,"pos":[2864,-6608],"params":["Inherit","False","109","CustomLighting","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":846,"pos":[2928,-6416],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":847,"pos":[2608,-6240],"params":["Inherit","True","Property","_MainTexture2","Main Texture 1","0","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Instance","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":848,"pos":[2672,-6320],"params":["Inherit","False","825","SpecularTint","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":849,"pos":[2608,-6416],"params":["Inherit","False","Property","_SpecularIntensity1","Specular Intensity","28","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":850,"pos":[2400,-6240],"params":["Inherit","False","201","MainTexture","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":462,"pos":[-832,-2928],"params":["Inherit","False","695","HalfLambert","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":460,"pos":[-832,-2848],"params":["Inherit","False","464","IndirectSpecHighlights","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":166,"pos":[1216,-4864],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":109,"pos":[1408,-4864],"params":["Inherit","False","CustomLighting","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":457,"pos":[-128,-2960],"params":["Inherit","False","SpecularHighlights","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":735,"pos":[-528,-2480],"params":["Inherit","False","496","BlendedAlpha","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":731,"pos":[0,-2000],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":820,"pos":[384,-2000],"params":["Inherit","False","SpecularSmoothness","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":719,"pos":[-192,-2192],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","-1","False","4","FLOAT","-0.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":730,"pos":[48,-2176],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":727,"pos":[992,-2432],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":723,"pos":[-176,-2592],"params":["Inherit","False","3","3","0","FLOAT3","0,0,0","False","1","FLOAT","1","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor","id":725,"pos":[384,-2592],"params":["Inherit","False","FLOAT3","0","1","2","3","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":855,"pos":[32,-2336],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":717,"pos":[32,-2496],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":722,"pos":[272,-2496],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":724,"pos":[496,-2336],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT3","0.05,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":856,"pos":[272,-2176],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":726,"pos":[672,-2336],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":729,"pos":[848,-2336],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":734,"pos":[-368,-1792],"params":["Inherit","False","SRP Additional Light","-1","","61179","6c86746ad131a0a408ca599df5f40861","3,6,2,351,1,23,0","6","2","FLOAT3","0,0,0","False","11","FLOAT3","0,0,0","False","345","FLOAT3","0,0,0","False","346","FLOAT3","0,0,0","False","347","FLOAT","0.5","False","32","FLOAT4","0,0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":827,"pos":[-640,-1568],"params":["Inherit","False","820","SpecularSmoothness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":840,"pos":[-814,-4096],"params":["Inherit","False","Property","_AdditionalLightInfluence","Additional Light Influence","53","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","15","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":844,"pos":[-814,-4176],"params":["Inherit","False","Property","_AdditionalLightFalloff","Additional Light Falloff","54","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","12","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":842,"pos":[-526,-4096],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","0.001","False","4","FLOAT","6","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":701,"pos":[-142,-4000],"params":["Inherit","False","709","RampOffset","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":843,"pos":[-78,-4304],"params":["Inherit","False","True","2","0","FLOAT3","0,0,0","False","1","FLOAT","1","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ScaleAndOffsetNode, AmplifyShaderEditor","id":700,"pos":[98,-4256],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT","1","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":845,"pos":[-334,-4176],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","12","False","3","FLOAT","0.001","False","4","FLOAT","12","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":702,"pos":[-142,-4080],"params":["Inherit","False","710","RampScale","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":699,"pos":[320,-4304],"params":["Inherit","True","Property","_TextureRamp2","Texture Ramp 1","0","1","[Header]","Create","True","1","Textures","0","0","False","1","Space(8)","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":627,"pos":[3040,-1056],"params":["Inherit","False","RimLight","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":852,"pos":[-592,-2384],"params":["Inherit","False","Property","_SecondarySpecularIntensity","Secondary Specular Intensity","31","0","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","0.5","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":854,"pos":[-288,-2336],"params":["Float","False","Property","_SecondarySmoothness","Secondary Smoothness","33","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0.04","0.001","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":853,"pos":[-592,-2288],"params":["Float","False","Property","_SecondarySpecularSize","Secondary Specular Size","32","0","Create","True","0","0","0","False","0","False","Object","-1","","0","-0.95","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":858,"pos":[-528,-2688],"params":["Inherit","False","Property","_SpecularColor","Specular Color","27","1","[Header]","Create","True","1","Specular Highlights","0","0","False","1","Space(8)","False","Object","-1","","0,0,0,0","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":456,"pos":[544,-5184],"params":["Inherit","False","Property","_EnableSecondaryHighlights","Enable Secondary Highlights","30","0","Create","True","0","0","0","False","1","Space(8)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","COLOR","0,0,0,0","False","0","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","3","COLOR","0,0,0,0","False","4","COLOR","0,0,0,0","False","5","COLOR","0,0,0,0","False","6","COLOR","0,0,0,0","False","7","COLOR","0,0,0,0","False","8","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":461,"pos":[-832,-3008],"params":["Inherit","False","463","DirectSpecHighlights","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":789,"pos":[2896,-6512],"params":["Inherit","False","788","Emission","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":698,"pos":[-256,-4304],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":697,"pos":[-528,-4304],"params":["Inherit","False","SRP Additional Light","-1","","61181","6c86746ad131a0a408ca599df5f40861","3,6,1,351,1,23,0","6","2","FLOAT3","0,0,0","False","11","FLOAT3","0,0,0","False","345","FLOAT3","0,0,0","False","346","FLOAT3","0,0,0","False","347","FLOAT","0.5","False","32","FLOAT4","0,0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":860,"pos":[-816,-4280],"params":["Inherit","False","Shadow Mask","-1","","61183","b50f5becdd6b8504a861ba5b9b861159","0","1","3","FLOAT2","0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":696,"pos":[-752,-4368],"params":["Inherit","False","444","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":861,"pos":[-848,-1808],"params":["Inherit","False","Shadow Mask","-1","","61188","b50f5becdd6b8504a861ba5b9b861159","0","1","3","FLOAT2","0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":506,"pos":[3264,-6864],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","12","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","ExtraPrePass","0","0","ExtraPrePass","6","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","0","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","0","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":507,"pos":[3264,-6864],"params":["Float","False","True","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","ToonScapes/URP/Stone","94348b07e5e8bab40bd6c8a1e3df54cd","True","Forward","0","1","Forward","22","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=UniversalForward","False","False","0","","0","0","Standard","52","Category","0","0","  Instanced Terrain Normals","1","0","Lighting Model","0","639180069731139368","Workflow","0","0","Surface","0","0","  Keep Alpha","0","0","  Refraction Model","0","0","  Blend","0","0","Two Sided","1","0","Alpha Clipping","0","639046412434242284","  Use Shadow Threshold","0","0","Fragment Normal Space","0","0","Forward Only","0","0","Transmission","0","0","  Transmission Shadow","0.5,False,","0","Translucency","0","0","  Translucency Strength","1,False,","0","  Normal Distortion","0.5,False,","0","  Scattering","2,False,","0","  Direct","0.9,False,","0","  Ambient","0.1,False,","0","  Shadow","0.5,False,","0","Cast Shadows","1","639056809467599434","Receive Shadows","2","0","Specular Highlights","1","639180069856387606","Environment Reflections","2","0","Receive SSAO","1","639056812780020420","Motion Vectors","1","639180070355857071","  Additional Motion Vectors","1","0","  Alembic Motion Vectors","0","0","  XR Motion Vectors","0","0","GPU Instancing","1","639056812810836558","LOD CrossFade","1","639056812823351191","Built-in Fog","1","639056669117530347","_FinalColorxAlpha","0","0","Meta Pass","1","639180070442865311","Override Baked GI","0","0","Extra Pre Pass","0","0","Tessellation","0","0","  Phong","0","0","  Strength","0.5,False,","0","  Type","0","0","  Tess","16,False,","0","  Min","10,False,","0","  Max","25,False,","0","  Edge Length","16,False,","0","  Max Displacement","25,False,","0","Write Depth","0","0","  Conservative","0","0","Vertex Position","1","0","Debug Display","0","0","Clear Coat","0","0","0","12","False","True","True","True","True","True","True","True","True","True","True","False","False","","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":508,"pos":[3264,-6864],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","12","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","ShadowCaster","0","2","ShadowCaster","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","False","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=ShadowCaster","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":509,"pos":[3264,-6864],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","12","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","DepthOnly","0","3","DepthOnly","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","True","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","False","False","False","True","1","LightMode=DepthOnly","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":510,"pos":[3264,-6864],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","12","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","Meta","0","4","Meta","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Meta","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":511,"pos":[3264,-6864],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","12","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","Universal2D","0","5","Universal2D","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=Universal2D","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":512,"pos":[3264,-6864],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","12","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","DepthNormals","0","6","DepthNormals","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","0","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=DepthNormals","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":513,"pos":[3264,-6864],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","12","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","GBuffer","0","7","GBuffer","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=UniversalGBuffer","False","True","12","d3d11","gles","metal","vulkan","xboxone","xboxseries","playstation","ps4","ps5","switch","switch2","webgpu","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":514,"pos":[3264,-6864],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","12","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","SceneSelectionPass","0","8","SceneSelectionPass","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=SceneSelectionPass","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":515,"pos":[3264,-6864],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","12","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","ScenePickingPass","0","9","ScenePickingPass","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Picking","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":516,"pos":[3264,-6864],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","12","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","MotionVectors","0","10","MotionVectors","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","False","False","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=MotionVectors","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":517,"pos":[3264,-6864],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","12","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","XRMotionVectors","0","11","XRMotionVectors","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","True","1","False","","255","False","","1","False","","7","False","","3","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","False","False","False","False","True","1","LightMode=XRMotionVectors","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":624,"pos":[416,-576],"params":["Inherit","False","128","100","RimFromLight","0","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":625,"pos":[416,-208],"params":["Inherit","False","159","100","ViewEdge","0","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":626,"pos":[688,-576],"params":["Inherit","False","128","100","RimMask","0","","1,1,1,1","0","0"]}
{"wire":[442,0,439,0]}
{"wire":[443,0,442,0]}
{"wire":[459,0,461,0]}
{"wire":[459,1,462,0]}
{"wire":[339,0,544,0]}
{"wire":[338,0,330,0]}
{"wire":[338,1,331,0]}
{"wire":[617,0,614,0]}
{"wire":[617,1,615,0]}
{"wire":[617,2,616,0]}
{"wire":[618,0,617,0]}
{"wire":[631,0,629,0]}
{"wire":[631,1,630,0]}
{"wire":[633,0,665,0]}
{"wire":[634,0,632,0]}
{"wire":[637,0,641,0]}
{"wire":[637,1,666,0]}
{"wire":[637,2,667,0]}
{"wire":[637,3,663,0]}
{"wire":[637,4,642,0]}
{"wire":[638,0,637,0]}
{"wire":[638,1,633,0]}
{"wire":[639,0,638,0]}
{"wire":[639,1,634,4]}
{"wire":[640,0,638,0]}
{"wire":[640,1,639,0]}
{"wire":[640,2,635,0]}
{"wire":[641,0,636,0]}
{"wire":[641,1,648,0]}
{"wire":[642,0,668,0]}
{"wire":[644,11,643,0]}
{"wire":[645,0,646,0]}
{"wire":[645,1,653,0]}
{"wire":[646,0,644,0]}
{"wire":[648,0,647,0]}
{"wire":[655,0,656,0]}
{"wire":[655,1,676,0]}
{"wire":[656,0,654,0]}
{"wire":[657,0,652,0]}
{"wire":[657,1,651,0]}
{"wire":[658,0,657,0]}
{"wire":[659,0,658,0]}
{"wire":[660,0,659,0]}
{"wire":[660,1,656,0]}
{"wire":[660,2,655,0]}
{"wire":[661,0,649,0]}
{"wire":[661,1,650,0]}
{"wire":[662,0,661,0]}
{"wire":[663,0,664,0]}
{"wire":[663,1,660,0]}
{"wire":[664,0,662,0]}
{"wire":[665,0,631,0]}
{"wire":[665,2,673,0]}
{"wire":[668,0,670,0]}
{"wire":[668,1,645,0]}
{"wire":[670,0,675,0]}
{"wire":[670,1,672,0]}
{"wire":[671,0,674,0]}
{"wire":[671,1,669,0]}
{"wire":[672,0,628,0]}
{"wire":[672,1,671,0]}
{"wire":[684,0,683,0]}
{"wire":[684,1,682,0]}
{"wire":[685,0,684,0]}
{"wire":[685,1,688,0]}
{"wire":[685,2,687,0]}
{"wire":[686,0,685,0]}
{"wire":[437,0,686,0]}
{"wire":[693,0,690,0]}
{"wire":[693,1,691,0]}
{"wire":[694,0,693,0]}
{"wire":[694,1,692,0]}
{"wire":[695,0,694,0]}
{"wire":[444,0,443,0]}
{"wire":[155,0,214,0]}
{"wire":[155,1,445,0]}
{"wire":[451,1,450,0]}
{"wire":[451,0,449,0]}
{"wire":[452,0,451,0]}
{"wire":[452,1,704,0]}
{"wire":[446,0,155,0]}
{"wire":[446,1,181,0]}
{"wire":[447,0,446,0]}
{"wire":[447,1,448,0]}
{"wire":[718,0,733,0]}
{"wire":[718,1,715,0]}
{"wire":[463,0,727,0]}
{"wire":[199,0,198,0]}
{"wire":[205,0,204,0]}
{"wire":[203,0,202,0]}
{"wire":[207,0,206,0]}
{"wire":[201,0,200,0]}
{"wire":[197,0,196,0]}
{"wire":[243,1,44,0]}
{"wire":[740,1,216,0]}
{"wire":[172,0,740,0]}
{"wire":[454,0,453,0]}
{"wire":[758,0,594,0]}
{"wire":[759,0,591,0]}
{"wire":[747,0,640,0]}
{"wire":[747,1,748,4]}
{"wire":[772,0,791,0]}
{"wire":[393,0,392,0]}
{"wire":[405,0,771,0]}
{"wire":[788,0,405,0]}
{"wire":[778,0,773,0]}
{"wire":[778,1,777,0]}
{"wire":[779,0,776,0]}
{"wire":[779,1,777,0]}
{"wire":[780,0,772,0]}
{"wire":[780,1,778,0]}
{"wire":[781,0,780,0]}
{"wire":[781,1,779,0]}
{"wire":[771,0,764,0]}
{"wire":[771,1,781,0]}
{"wire":[773,0,775,0]}
{"wire":[773,1,774,0]}
{"wire":[776,0,775,0]}
{"wire":[776,1,774,0]}
{"wire":[782,0,786,0]}
{"wire":[783,0,782,0]}
{"wire":[783,1,784,0]}
{"wire":[784,0,785,1]}
{"wire":[784,1,785,3]}
{"wire":[791,0,783,0]}
{"wire":[791,1,769,0]}
{"wire":[340,0,339,0]}
{"wire":[544,0,338,0]}
{"wire":[544,1,575,0]}
{"wire":[544,2,576,0]}
{"wire":[324,0,755,0]}
{"wire":[348,0,746,0]}
{"wire":[330,0,324,2]}
{"wire":[330,1,348,0]}
{"wire":[329,0,345,0]}
{"wire":[331,0,329,4]}
{"wire":[543,0,573,0]}
{"wire":[543,1,537,0]}
{"wire":[543,2,538,0]}
{"wire":[580,0,573,4]}
{"wire":[580,1,537,4]}
{"wire":[580,2,538,0]}
{"wire":[525,0,519,0]}
{"wire":[590,0,594,0]}
{"wire":[590,1,591,0]}
{"wire":[590,2,599,0]}
{"wire":[800,1,591,0]}
{"wire":[800,0,590,0]}
{"wire":[806,0,804,0]}
{"wire":[806,5,809,0]}
{"wire":[755,1,761,0]}
{"wire":[755,0,760,0]}
{"wire":[680,0,678,4]}
{"wire":[709,0,712,0]}
{"wire":[710,0,711,0]}
{"wire":[679,0,678,5]}
{"wire":[398,0,362,4]}
{"wire":[396,0,362,5]}
{"wire":[261,0,260,0]}
{"wire":[808,0,807,0]}
{"wire":[258,0,257,0]}
{"wire":[218,0,243,0]}
{"wire":[810,1,800,0]}
{"wire":[810,0,805,0]}
{"wire":[803,0,802,0]}
{"wire":[578,0,525,1]}
{"wire":[795,1,537,0]}
{"wire":[795,0,543,0]}
{"wire":[584,0,525,3]}
{"wire":[815,0,816,0]}
{"wire":[815,1,578,0]}
{"wire":[817,0,522,0]}
{"wire":[817,1,578,0]}
{"wire":[814,0,795,0]}
{"wire":[814,1,815,0]}
{"wire":[814,2,817,0]}
{"wire":[541,0,527,0]}
{"wire":[541,1,584,0]}
{"wire":[819,0,818,0]}
{"wire":[819,1,584,0]}
{"wire":[542,0,814,0]}
{"wire":[542,1,819,0]}
{"wire":[542,2,541,0]}
{"wire":[496,0,796,0]}
{"wire":[167,0,542,0]}
{"wire":[464,0,823,0]}
{"wire":[472,0,497,0]}
{"wire":[472,1,822,0]}
{"wire":[472,2,792,0]}
{"wire":[466,0,471,0]}
{"wire":[466,1,472,0]}
{"wire":[466,2,473,0]}
{"wire":[823,0,488,0]}
{"wire":[823,1,466,0]}
{"wire":[796,1,537,4]}
{"wire":[796,0,580,0]}
{"wire":[825,0,858,5]}
{"wire":[764,0,770,0]}
{"wire":[764,1,765,0]}
{"wire":[764,2,393,2]}
{"wire":[764,3,403,0]}
{"wire":[829,0,828,0]}
{"wire":[573,0,533,0]}
{"wire":[573,1,533,0]}
{"wire":[573,2,533,0]}
{"wire":[573,3,737,0]}
{"wire":[573,4,830,0]}
{"wire":[537,0,520,0]}
{"wire":[537,1,520,0]}
{"wire":[537,2,520,0]}
{"wire":[537,3,532,0]}
{"wire":[537,4,830,0]}
{"wire":[594,0,595,0]}
{"wire":[594,1,595,0]}
{"wire":[594,2,595,0]}
{"wire":[594,8,610,0]}
{"wire":[594,3,739,0]}
{"wire":[594,4,831,0]}
{"wire":[591,0,592,0]}
{"wire":[591,1,592,0]}
{"wire":[591,2,592,0]}
{"wire":[591,8,609,0]}
{"wire":[591,3,593,0]}
{"wire":[591,4,831,0]}
{"wire":[805,0,800,0]}
{"wire":[805,1,806,0]}
{"wire":[169,0,810,0]}
{"wire":[704,0,447,0]}
{"wire":[704,1,703,0]}
{"wire":[703,0,181,0]}
{"wire":[703,1,699,0]}
{"wire":[812,0,811,0]}
{"wire":[812,5,813,0]}
{"wire":[846,0,849,0]}
{"wire":[846,1,848,0]}
{"wire":[846,2,847,4]}
{"wire":[847,0,850,0]}
{"wire":[166,0,456,0]}
{"wire":[166,1,452,0]}
{"wire":[109,0,166,0]}
{"wire":[457,0,459,0]}
{"wire":[731,0,734,0]}
{"wire":[820,0,855,0]}
{"wire":[719,0,853,0]}
{"wire":[730,0,719,0]}
{"wire":[730,1,718,0]}
{"wire":[727,0,725,0]}
{"wire":[727,1,726,0]}
{"wire":[723,0,858,5]}
{"wire":[723,1,735,0]}
{"wire":[723,2,852,0]}
{"wire":[725,0,723,0]}
{"wire":[855,0,854,0]}
{"wire":[717,0,723,0]}
{"wire":[722,0,717,0]}
{"wire":[722,1,855,0]}
{"wire":[724,0,856,0]}
{"wire":[724,1,722,0]}
{"wire":[856,0,730,0]}
{"wire":[726,0,724,0]}
{"wire":[729,0,726,0]}
{"wire":[729,1,731,0]}
{"wire":[734,11,732,0]}
{"wire":[734,345,821,0]}
{"wire":[734,346,826,0]}
{"wire":[734,347,827,0]}
{"wire":[734,32,861,0]}
{"wire":[842,0,840,0]}
{"wire":[843,0,698,0]}
{"wire":[843,1,845,0]}
{"wire":[700,0,843,0]}
{"wire":[700,1,842,0]}
{"wire":[700,2,701,0]}
{"wire":[845,0,844,0]}
{"wire":[699,0,214,0]}
{"wire":[699,1,700,0]}
{"wire":[627,0,747,0]}
{"wire":[456,0,455,0]}
{"wire":[698,0,697,0]}
{"wire":[697,11,696,0]}
{"wire":[697,32,860,0]}
{"wire":[507,0,170,0]}
{"wire":[507,1,832,0]}
{"wire":[507,9,846,0]}
{"wire":[507,4,851,0]}
{"wire":[507,5,677,0]}
{"wire":[507,2,789,0]}
ASEEND*/
//CHKSM=1799B05AA63277DA5A9A1C17BC5587C57A64C8FC