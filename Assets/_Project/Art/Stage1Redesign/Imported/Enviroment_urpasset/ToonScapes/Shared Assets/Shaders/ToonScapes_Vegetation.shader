// Made with Amplify Shader Editor v1.9.9.12
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ToonScapes/URP/Vegetation"
{
	Properties
	{
		[Header(Color)][Space(8)][Toggle( _ENABLECOLORTINT_ON )] _EnableColorTint( "Enable Color Tint", Float ) = 1
		[Space(8)] _BaseTint( "Base Tint", Color ) = ( 1, 1, 1, 1 )
		[Header(Textures)][NoScaleOffset][SingleLineTexture][Space (8)] _TextureRamp( "Texture Ramp", 2D ) = "white" {}
		_RampScale( "Ramp Scale", Range( 0, 1 ) ) = 0.5
		_RampOffset( "Ramp Offset", Range( 0, 1 ) ) = 0.5
		[NoScaleOffset][SingleLineTexture][Space (8)] _MainTexture( "Main Texture", 2D ) = "white" {}
		_AlphaClipThreshold( "Alpha Clip Threshold", Range( 0, 1 ) ) = 0
		[NoScaleOffset][Normal][SingleLineTexture][Space (8)] _NormalMap( "Normal Map", 2D ) = "white" {}
		_NormalScale( "Normal Scale", Range( 0, 5 ) ) = 1
		[NoScaleOffset][SingleLineTexture][Space (8)] _EmissionMap( "Emission Map", 2D ) = "white" {}
		[Header(Specular Highlights)][Space(8)] _SpecularColor( "Specular Color", Color ) = ( 1, 1, 1, 1 )
		_SpecularIntensity1( "Specular Intensity", Range( 0, 1 ) ) = 0
		_Smoothness( "Smoothness", Range( 0, 1 ) ) = 0
		[Space(8)][Toggle( _ENABLESECONDARYHIGHLIGHTS_ON )] _EnableSecondaryHighlights( "Enable Secondary Highlights", Float ) = 1
		[Space(8)] _SecondarySpecularIntensity( "Secondary Specular Intensity", Range( 0, 1 ) ) = 0
		_SecondarySpecularSize( "Secondary Specular Size", Range( 0, 1 ) ) = 0
		_SecondarySmoothness( "Secondary Smoothness", Range( 0.001, 1 ) ) = 0.01
		[Header(Subsurface Distortion)][Space(8)][Toggle( _ENABLESUBSURFACEDISTORTION_ON )] _EnableSubsurfaceDistortion( "Enable Subsurface Distortion", Float ) = 1
		[Space (8)] _SubsurfaceTint( "Subsurface Tint", Color ) = ( 1, 1, 1, 1 )
		_DistortionScale( "Distortion Scale", Range( 0, 1 ) ) = 0.5
		_DistortionAmount( "Distortion Amount", Range( 0, 1 ) ) = 0
		[Header (Emission)][Space(8)][Toggle( _ENABLEEMISSION_ON )] _EnableEmission( "Enable Emission", Float ) = 1
		[HDR][Space(8)] _EmissionColor( "Emission Color", Color ) = ( 1, 1, 1, 1 )
		_EmissionIntensity1( "Emission Intensity", Float ) = 1
		_FlickerFrequency( "Flicker Frequency", Float ) = 1
		_FlickerScale( "Flicker Scale", Float ) = 1
		_MinIntensity( "Min Intensity", Float ) = 0.75
		_MaxIntensity( "Max Intensity", Float ) = 1
		[Header(Wind)][Space(8)][Toggle( _ENABLEWIND_ON )] _EnableWind( "Enable Wind", Float ) = 1
		_DirectionBias( "Direction Bias", Range( 0, 1 ) ) = 0
		[Header(Surface Options)][Space(8)][Toggle] _EnableTopHighlights( "Enable Top Highlights", Float ) = 1
		[HDR][Space(8)] _TopHighlightsColor( "Top Highlights Color", Color ) = ( 1, 1, 1, 1 )
		[Space (8)] _Occlusion( "Occlusion", Range( 0, 1 ) ) = 1
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

		//[HideInInspector] _AlphaClip("__clip", Float) = 1.0
	}

	SubShader
	{
		PackageRequirements
		{
			"com.unity.render-pipelines.universal": "[17.0,18.0]"
		}

		

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" "UniversalMaterialType"="Lit" }

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
			#define _ALPHATEST_ON
			#define _NORMAL_DROPOFF_TS 1
			#pragma shader_feature_local_fragment _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
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

			#define ASE_NEEDS_FRAG_WORLD_VIEW_DIR
			#define ASE_NEEDS_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#pragma shader_feature_local _ENABLEWIND_ON
			#pragma shader_feature_local _ENABLESUBSURFACEDISTORTION_ON
			#pragma shader_feature_local _ENABLESECONDARYHIGHLIGHTS_ON
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
				float4 ase_color : COLOR;
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
				float4 ase_color : COLOR;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceTint;
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _BaseTint;
			float4 _TopHighlightsColor;
			float _DirectionBias;
			float _MaxIntensity;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _RampOffset;
			float _MinIntensity;
			float _RampScale;
			float _EnableTopHighlights;
			float _SecondarySmoothness;
			float _NormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _DistortionAmount;
			float _DistortionScale;
			float _AdditionalLightFalloff;
			float _AlphaClipThreshold;
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

			float3 ToonScapesGlobalWindDirection;
			sampler2D ToonScapesGlobalNoiseTexture;
			float ToonScapesGlobalWindJitter;
			float ToonScapesGlobalWindSpeed;
			float ToonScapesGlobalWindScale;
			float ToonScapesGlobalWindStrength;
			sampler2D _EmissionMap;
			sampler2D _NormalMap;
			sampler2D _MainTexture;
			sampler2D _TextureRamp;


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
			
			half4 CalculateShadowMask1_g61192( half2 LightmapUV )
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

				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_276_0 = float3( (WindDirection255).xz ,  0.0 );
				float GlobalJitter410 = ToonScapesGlobalWindJitter;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				float2 appendResult284 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner286 = ( 1.0 * _Time.y * ( temp_output_276_0 *  (0.0 + ( GlobalJitter410 - 0.0 ) * ( 40.0 - 0.0 ) / ( 10.0 - 0.0 ) ) ).xy + appendResult284);
				float4 tex2DNode293 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner286 / float2( 2,2 ) ), 0, 0.0) );
				float4 lerpResult405 = lerp( tex2DNode293 , ( ( tex2DNode293 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindJitter296 = lerpResult405;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner287 / float2( 150,150 ) ), 0, 0.0) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 transform310 = mul(GetWorldToObjectMatrix(),( float4( ( WindDirection255 * 0.1 ) , 0.0 ) * ( ( WindJitter296 * input.ase_color.r * 5.0 ) + ( input.ase_color.g * WindSway244 *  (0.0 + ( ToonScapesGlobalWindStrength - 0.0 ) * ( 3.5 - 0.0 ) / ( 1.0 - 0.0 ) ) ) ) ));
				float4 VertexOffset254 = transform310;
				#ifdef _ENABLEWIND_ON
				float4 staticSwitch620 = VertexOffset254;
				#else
				float4 staticSwitch620 = float4( 0,0,0,0 );
				#endif
				
				output.ase_color = input.ase_color;
				output.ase_texcoord7.xy = input.texcoord.xy;
				output.ase_texcoord7.zw = input.texcoord1.xy;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = staticSwitch620.xyz;

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
				float4 ase_color : COLOR;

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
				output.ase_color = input.ase_color;
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
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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

				float3 normalizedWorldNormal = normalize( NormalWS );
				float dotResult349 = dot( ViewDirWS , -( SafeNormalize( _MainLightPosition.xyz ) + ( normalizedWorldNormal *  (2.0 + ( _DistortionScale - 0.0 ) * ( 0.0 - 2.0 ) / ( 1.0 - 0.0 ) ) ) ) );
				float dotResult368 = dot( dotResult349 ,  (0.0 + ( _DistortionAmount - 0.0 ) * ( 2.0 - 0.0 ) / ( 1.0 - 0.0 ) ) );
				float ase_lightIntensity = max( max( _MainLightColor.r, _MainLightColor.g ), _MainLightColor.b ) + 1e-7;
				float4 ase_lightColor = float4( _MainLightColor.rgb / ase_lightIntensity, ase_lightIntensity );
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoord );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float4 SubsurfaceDistortion359 = ( ( ( ( _SubsurfaceTint * saturate( dotResult368 ) ) * ase_lightColor ) * input.ase_color.b ) * ase_lightAtten );
				#ifdef _ENABLESUBSURFACEDISTORTION_ON
				float4 staticSwitch618 = SubsurfaceDistortion359;
				#else
				float4 staticSwitch618 = float4( 0,0,0,0 );
				#endif
				float2 uv_EmissionMap600 = input.ase_texcoord7.xy;
				float3 temp_output_561_0 = ( _SpecularColor.rgb * tex2D( _EmissionMap, uv_EmissionMap600 ).a * _SecondarySpecularIntensity );
				float3 normalizeResult4_g61177 = normalize( ( ViewDirWS + SafeNormalize( _MainLightPosition.xyz ) ) );
				float2 uv_NormalMap470 = input.ase_texcoord7.xy;
				float3 unpack470 = UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap470 ), _NormalScale );
				unpack470.z = lerp( 1, unpack470.z, saturate(_NormalScale) );
				float3 tex2DNode470 = unpack470;
				float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
				float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
				float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
				float3 tanNormal442 = tex2DNode470;
				float3 worldNormal442 = normalize( float3( dot( tanToWorld0, tanNormal442 ), dot( tanToWorld1, tanNormal442 ), dot( tanToWorld2, tanNormal442 ) ) );
				float3 normalizeResult444 = normalize( worldNormal442 );
				float3 Normals594 = normalizeResult444;
				float dotResult559 = dot( normalizeResult4_g61177 , Normals594 );
				float temp_output_672_0 = ( 1.0 - _SecondarySmoothness );
				float3 temp_output_564_0 = saturate( ( saturate( (  (-1.0 + ( _SecondarySpecularSize - 0.0 ) * ( -0.5 - -1.0 ) / ( 1.0 - 0.0 ) ) + dotResult559 ) ) / ( ( 1.0 - temp_output_561_0 ) * temp_output_672_0 ) ) );
				float3 DirectSpecHighlights574 = ( (temp_output_561_0).xyz * temp_output_564_0 );
				float3 bakedGI540 = ASEIndirectDiffuse( input, NormalWS, PositionWS, ViewDirWS );
				MixRealtimeAndBakedGI( ase_mainLight, NormalWS, bakedGI540, half4( 0, 0, 0, 0 ) );
				float4 HalfLambert543 = ( ( ase_lightColor * ase_lightAtten ) + float4( bakedGI540 , 0.0 ) );
				float4 SpecularHighlights578 = ( float4( DirectSpecHighlights574 , 0.0 ) * HalfLambert543 );
				#ifdef _ENABLESECONDARYHIGHLIGHTS_ON
				float4 staticSwitch573 = SpecularHighlights578;
				#else
				float4 staticSwitch573 = float4( 0,0,0,0 );
				#endif
				float4 color452 = IsGammaSpace() ? float4( 1, 1, 1, 1 ) : float4( 1, 1, 1, 1 );
				#ifdef _ENABLECOLORTINT_ON
				float4 staticSwitch586 = _BaseTint;
				#else
				float4 staticSwitch586 = color452;
				#endif
				float2 uv_MainTexture468 = input.ase_texcoord7.xy;
				float4 tex2DNode468 = tex2D( _MainTexture, uv_MainTexture468 );
				float dotResult533 = dot( Normals594 , SafeNormalize( _MainLightPosition.xyz ) );
				float RampScale553 = _RampScale;
				float RampOffset552 = _RampOffset;
				float CEL_Effect473 = saturate( (dotResult533*RampScale553 + RampOffset552) );
				float2 temp_cast_2 = (CEL_Effect473).xx;
				float4 FinalLighting611 = ( ( tex2DNode468 * tex2D( _TextureRamp, temp_cast_2 ) ) * HalfLambert543 );
				float3 HighlightsColor609 = _TopHighlightsColor.rgb;
				float4 blendOpSrc332 = float4( HighlightsColor609 , 0.0 );
				float4 blendOpDest332 = FinalLighting611;
				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(PositionWS.x , PositionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2D( ToonScapesGlobalNoiseTexture, ( panner287 / float2( 150,150 ) ) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 lerpBlendMode332 = lerp(blendOpDest332, (( blendOpSrc332 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpSrc332 - 0.5 ) ) * ( 1.0 - blendOpDest332 ) ) : ( 2.0 * blendOpSrc332 * blendOpDest332 ) ),( WindSway244 * input.ase_color.g * ase_lightAtten ).r);
				float4 FinalLightingHighlights335 = (( _EnableTopHighlights )?( ( saturate( lerpBlendMode332 )) ):( FinalLighting611 ));
				float3 WorldPosition288_g61181 = PositionWS;
				float3 WorldPosition305_g61181 = WorldPosition288_g61181;
				float2 ScreenUV286_g61181 = (ScreenPosNorm).xy;
				float2 ScreenUV305_g61181 = ScreenUV286_g61181;
				float3 WorldNormal281_g61181 = Normals594;
				float3 WorldNormal305_g61181 = WorldNormal281_g61181;
				half2 LightmapUV1_g61192 = (input.ase_texcoord7.zw*(unity_LightmapST).xy + (unity_LightmapST).zw);
				half4 localCalculateShadowMask1_g61192 = CalculateShadowMask1_g61192( LightmapUV1_g61192 );
				float4 ShadowMask360_g61181 = localCalculateShadowMask1_g61192;
				float4 ShadowMask305_g61181 = ShadowMask360_g61181;
				float3 localAdditionalLightsLambertMask17x305_g61181 = AdditionalLightsLambertMask17x( WorldPosition305_g61181 , ScreenUV305_g61181 , WorldNormal305_g61181 , ShadowMask305_g61181 );
				float3 saferPower684 = abs( saturate( localAdditionalLightsLambertMask17x305_g61181 ) );
				float3 temp_cast_7 = ( (0.001 + ( _AdditionalLightFalloff - 0.0 ) * ( 12.0 - 0.001 ) / ( 12.0 - 0.0 ) )).xxx;
				
				float3 NormalInput660 = tex2DNode470;
				
				float3 SpecularTint669 = _SpecularColor.rgb;
				float2 uv_EmissionMap666 = input.ase_texcoord7.xy;
				
				float3 EmissionColor461 = _EmissionColor.rgb;
				float2 uv_EmissionMap625 = input.ase_texcoord7.xy;
				float EmissionAlpha462 = _EmissionColor.a;
				float mulTime648 = _TimeParameters.x * _FlickerFrequency;
				#ifdef _ENABLEEMISSION_ON
				float3 staticSwitch589 = ( ( _EmissionIntensity1 * EmissionColor461 * tex2D( _EmissionMap, uv_EmissionMap625 ).rgb * EmissionAlpha462 ) * ( ( sin( ( ( mulTime648 + ( PositionWS.x + PositionWS.z ) ) / _FlickerScale ) ) * ( ( _MaxIntensity - _MinIntensity ) * 0.5 ) ) + ( ( _MaxIntensity + _MinIntensity ) * 0.5 ) ) );
				#else
				float3 staticSwitch589 = float3( 0,0,0 );
				#endif
				float3 Emission467 = staticSwitch589;
				
				float Alpha614 = tex2DNode468.a;
				

				float3 BaseColor = ( staticSwitch618 + ( staticSwitch573 + ( staticSwitch586 * ( FinalLightingHighlights335 + ( tex2DNode468 * tex2D( _TextureRamp, (pow( saferPower684 , temp_cast_7 )* (0.001 + ( _AdditionalLightInfluence - 0.0 ) * ( 6.0 - 0.001 ) / ( 1.0 - 0.0 ) ) + RampOffset552).xy ) ) ) ) ) ).rgb;
				float3 Normal = NormalInput660;
				float3 Specular = ( _SpecularIntensity1 * SpecularTint669 * tex2D( _EmissionMap, uv_EmissionMap666 ).a );
				float Metallic = 0;
				float Smoothness = _Smoothness;
				float Occlusion = _Occlusion;
				float3 Emission = Emission467;
				float Alpha = Alpha614;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _AlphaClipThreshold;
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
			#define _ALPHATEST_ON
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
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

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#pragma shader_feature_local _ENABLEWIND_ON


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
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceTint;
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _BaseTint;
			float4 _TopHighlightsColor;
			float _DirectionBias;
			float _MaxIntensity;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _RampOffset;
			float _MinIntensity;
			float _RampScale;
			float _EnableTopHighlights;
			float _SecondarySmoothness;
			float _NormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _DistortionAmount;
			float _DistortionScale;
			float _AdditionalLightFalloff;
			float _AlphaClipThreshold;
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

			float3 ToonScapesGlobalWindDirection;
			sampler2D ToonScapesGlobalNoiseTexture;
			float ToonScapesGlobalWindJitter;
			float ToonScapesGlobalWindSpeed;
			float ToonScapesGlobalWindScale;
			float ToonScapesGlobalWindStrength;
			sampler2D _MainTexture;


			float3 _LightDirection;
			float3 _LightPosition;

			
			PackedVaryings VertexFunction( Attributes input )
			{
				PackedVaryings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_276_0 = float3( (WindDirection255).xz ,  0.0 );
				float GlobalJitter410 = ToonScapesGlobalWindJitter;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				float2 appendResult284 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner286 = ( 1.0 * _Time.y * ( temp_output_276_0 *  (0.0 + ( GlobalJitter410 - 0.0 ) * ( 40.0 - 0.0 ) / ( 10.0 - 0.0 ) ) ).xy + appendResult284);
				float4 tex2DNode293 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner286 / float2( 2,2 ) ), 0, 0.0) );
				float4 lerpResult405 = lerp( tex2DNode293 , ( ( tex2DNode293 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindJitter296 = lerpResult405;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner287 / float2( 150,150 ) ), 0, 0.0) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 transform310 = mul(GetWorldToObjectMatrix(),( float4( ( WindDirection255 * 0.1 ) , 0.0 ) * ( ( WindJitter296 * input.ase_color.r * 5.0 ) + ( input.ase_color.g * WindSway244 *  (0.0 + ( ToonScapesGlobalWindStrength - 0.0 ) * ( 3.5 - 0.0 ) / ( 1.0 - 0.0 ) ) ) ) ));
				float4 VertexOffset254 = transform310;
				#ifdef _ENABLEWIND_ON
				float4 staticSwitch620 = VertexOffset254;
				#else
				float4 staticSwitch620 = float4( 0,0,0,0 );
				#endif
				
				output.ase_texcoord1.xy = input.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord1.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = staticSwitch620.xyz;
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
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

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
				output.ase_color = input.ase_color;
				output.ase_texcoord = input.ase_texcoord;
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
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

				float2 uv_MainTexture468 = input.ase_texcoord1.xy;
				float4 tex2DNode468 = tex2D( _MainTexture, uv_MainTexture468 );
				float Alpha614 = tex2DNode468.a;
				

				float Alpha = Alpha614;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _AlphaClipThreshold;
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
			#define _ALPHATEST_ON
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
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

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#pragma shader_feature_local _ENABLEWIND_ON


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
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceTint;
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _BaseTint;
			float4 _TopHighlightsColor;
			float _DirectionBias;
			float _MaxIntensity;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _RampOffset;
			float _MinIntensity;
			float _RampScale;
			float _EnableTopHighlights;
			float _SecondarySmoothness;
			float _NormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _DistortionAmount;
			float _DistortionScale;
			float _AdditionalLightFalloff;
			float _AlphaClipThreshold;
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

			float3 ToonScapesGlobalWindDirection;
			sampler2D ToonScapesGlobalNoiseTexture;
			float ToonScapesGlobalWindJitter;
			float ToonScapesGlobalWindSpeed;
			float ToonScapesGlobalWindScale;
			float ToonScapesGlobalWindStrength;
			sampler2D _MainTexture;


			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_276_0 = float3( (WindDirection255).xz ,  0.0 );
				float GlobalJitter410 = ToonScapesGlobalWindJitter;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				float2 appendResult284 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner286 = ( 1.0 * _Time.y * ( temp_output_276_0 *  (0.0 + ( GlobalJitter410 - 0.0 ) * ( 40.0 - 0.0 ) / ( 10.0 - 0.0 ) ) ).xy + appendResult284);
				float4 tex2DNode293 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner286 / float2( 2,2 ) ), 0, 0.0) );
				float4 lerpResult405 = lerp( tex2DNode293 , ( ( tex2DNode293 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindJitter296 = lerpResult405;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner287 / float2( 150,150 ) ), 0, 0.0) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 transform310 = mul(GetWorldToObjectMatrix(),( float4( ( WindDirection255 * 0.1 ) , 0.0 ) * ( ( WindJitter296 * input.ase_color.r * 5.0 ) + ( input.ase_color.g * WindSway244 *  (0.0 + ( ToonScapesGlobalWindStrength - 0.0 ) * ( 3.5 - 0.0 ) / ( 1.0 - 0.0 ) ) ) ) ));
				float4 VertexOffset254 = transform310;
				#ifdef _ENABLEWIND_ON
				float4 staticSwitch620 = VertexOffset254;
				#else
				float4 staticSwitch620 = float4( 0,0,0,0 );
				#endif
				
				output.ase_texcoord1.xy = input.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord1.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = staticSwitch620.xyz;

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
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

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
				output.ase_color = input.ase_color;
				output.ase_texcoord = input.ase_texcoord;
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
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

				float2 uv_MainTexture468 = input.ase_texcoord1.xy;
				float4 tex2DNode468 = tex2D( _MainTexture, uv_MainTexture468 );
				float Alpha614 = tex2DNode468.a;
				

				float Alpha = Alpha614;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _AlphaClipThreshold;
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
			#define _ALPHATEST_ON
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
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
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES1
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES2
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#pragma shader_feature_local _ENABLEWIND_ON
			#pragma shader_feature_local _ENABLESUBSURFACEDISTORTION_ON
			#pragma shader_feature_local _ENABLESECONDARYHIGHLIGHTS_ON
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
				float4 ase_color : COLOR;
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
				float4 ase_color : COLOR;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 lightmapUVOrVertexSH : TEXCOORD7;
				float4 dynamicLightmapUV : TEXCOORD8;
				float4 ase_texcoord9 : TEXCOORD9;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceTint;
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _BaseTint;
			float4 _TopHighlightsColor;
			float _DirectionBias;
			float _MaxIntensity;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _RampOffset;
			float _MinIntensity;
			float _RampScale;
			float _EnableTopHighlights;
			float _SecondarySmoothness;
			float _NormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _DistortionAmount;
			float _DistortionScale;
			float _AdditionalLightFalloff;
			float _AlphaClipThreshold;
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

			float3 ToonScapesGlobalWindDirection;
			sampler2D ToonScapesGlobalNoiseTexture;
			float ToonScapesGlobalWindJitter;
			float ToonScapesGlobalWindSpeed;
			float ToonScapesGlobalWindScale;
			float ToonScapesGlobalWindStrength;
			sampler2D _EmissionMap;
			sampler2D _NormalMap;
			sampler2D _MainTexture;
			sampler2D _TextureRamp;


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
			
			half4 CalculateShadowMask1_g61192( half2 LightmapUV )
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

				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_276_0 = float3( (WindDirection255).xz ,  0.0 );
				float GlobalJitter410 = ToonScapesGlobalWindJitter;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				float2 appendResult284 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner286 = ( 1.0 * _Time.y * ( temp_output_276_0 *  (0.0 + ( GlobalJitter410 - 0.0 ) * ( 40.0 - 0.0 ) / ( 10.0 - 0.0 ) ) ).xy + appendResult284);
				float4 tex2DNode293 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner286 / float2( 2,2 ) ), 0, 0.0) );
				float4 lerpResult405 = lerp( tex2DNode293 , ( ( tex2DNode293 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindJitter296 = lerpResult405;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner287 / float2( 150,150 ) ), 0, 0.0) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 transform310 = mul(GetWorldToObjectMatrix(),( float4( ( WindDirection255 * 0.1 ) , 0.0 ) * ( ( WindJitter296 * input.ase_color.r * 5.0 ) + ( input.ase_color.g * WindSway244 *  (0.0 + ( ToonScapesGlobalWindStrength - 0.0 ) * ( 3.5 - 0.0 ) / ( 1.0 - 0.0 ) ) ) ) ));
				float4 VertexOffset254 = transform310;
				#ifdef _ENABLEWIND_ON
				float4 staticSwitch620 = VertexOffset254;
				#else
				float4 staticSwitch620 = float4( 0,0,0,0 );
				#endif
				
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord3.xyz = ase_normalWS;
				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord5.xyz = ase_tangentWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord6.xyz = ase_bitangentWS;
				OUTPUT_LIGHTMAP_UV( input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy );
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
				
				output.ase_color = input.ase_color;
				output.ase_texcoord4.xy = input.texcoord.xy;
				output.ase_texcoord4.zw = input.texcoord1.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord3.w = 0;
				output.ase_texcoord5.w = 0;
				output.ase_texcoord6.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = staticSwitch620.xyz;

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
				float4 ase_color : COLOR;

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
				output.ase_color = input.ase_color;
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
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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

				float3 ase_viewVectorWS = ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz );
				float3 ase_viewDirWS = normalize( ase_viewVectorWS );
				float3 ase_normalWS = input.ase_texcoord3.xyz;
				float3 normalizedWorldNormal = normalize( ase_normalWS );
				float dotResult349 = dot( ase_viewDirWS , -( SafeNormalize( _MainLightPosition.xyz ) + ( normalizedWorldNormal *  (2.0 + ( _DistortionScale - 0.0 ) * ( 0.0 - 2.0 ) / ( 1.0 - 0.0 ) ) ) ) );
				float dotResult368 = dot( dotResult349 ,  (0.0 + ( _DistortionAmount - 0.0 ) * ( 2.0 - 0.0 ) / ( 1.0 - 0.0 ) ) );
				float ase_lightIntensity = max( max( _MainLightColor.r, _MainLightColor.g ), _MainLightColor.b ) + 1e-7;
				float4 ase_lightColor = float4( _MainLightColor.rgb / ase_lightIntensity, ase_lightIntensity );
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoord );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float4 SubsurfaceDistortion359 = ( ( ( ( _SubsurfaceTint * saturate( dotResult368 ) ) * ase_lightColor ) * input.ase_color.b ) * ase_lightAtten );
				#ifdef _ENABLESUBSURFACEDISTORTION_ON
				float4 staticSwitch618 = SubsurfaceDistortion359;
				#else
				float4 staticSwitch618 = float4( 0,0,0,0 );
				#endif
				float2 uv_EmissionMap600 = input.ase_texcoord4.xy;
				float3 temp_output_561_0 = ( _SpecularColor.rgb * tex2D( _EmissionMap, uv_EmissionMap600 ).a * _SecondarySpecularIntensity );
				float3 normalizeResult4_g61177 = normalize( ( ase_viewDirWS + SafeNormalize( _MainLightPosition.xyz ) ) );
				float2 uv_NormalMap470 = input.ase_texcoord4.xy;
				float3 unpack470 = UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap470 ), _NormalScale );
				unpack470.z = lerp( 1, unpack470.z, saturate(_NormalScale) );
				float3 tex2DNode470 = unpack470;
				float3 ase_tangentWS = input.ase_texcoord5.xyz;
				float3 ase_bitangentWS = input.ase_texcoord6.xyz;
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 tanNormal442 = tex2DNode470;
				float3 worldNormal442 = normalize( float3( dot( tanToWorld0, tanNormal442 ), dot( tanToWorld1, tanNormal442 ), dot( tanToWorld2, tanNormal442 ) ) );
				float3 normalizeResult444 = normalize( worldNormal442 );
				float3 Normals594 = normalizeResult444;
				float dotResult559 = dot( normalizeResult4_g61177 , Normals594 );
				float temp_output_672_0 = ( 1.0 - _SecondarySmoothness );
				float3 temp_output_564_0 = saturate( ( saturate( (  (-1.0 + ( _SecondarySpecularSize - 0.0 ) * ( -0.5 - -1.0 ) / ( 1.0 - 0.0 ) ) + dotResult559 ) ) / ( ( 1.0 - temp_output_561_0 ) * temp_output_672_0 ) ) );
				float3 DirectSpecHighlights574 = ( (temp_output_561_0).xyz * temp_output_564_0 );
				float3 bakedGI540 = ASEIndirectDiffuse( input, ase_normalWS, PositionWS, ase_viewDirWS );
				MixRealtimeAndBakedGI( ase_mainLight, ase_normalWS, bakedGI540, half4( 0, 0, 0, 0 ) );
				float4 HalfLambert543 = ( ( ase_lightColor * ase_lightAtten ) + float4( bakedGI540 , 0.0 ) );
				float4 SpecularHighlights578 = ( float4( DirectSpecHighlights574 , 0.0 ) * HalfLambert543 );
				#ifdef _ENABLESECONDARYHIGHLIGHTS_ON
				float4 staticSwitch573 = SpecularHighlights578;
				#else
				float4 staticSwitch573 = float4( 0,0,0,0 );
				#endif
				float4 color452 = IsGammaSpace() ? float4( 1, 1, 1, 1 ) : float4( 1, 1, 1, 1 );
				#ifdef _ENABLECOLORTINT_ON
				float4 staticSwitch586 = _BaseTint;
				#else
				float4 staticSwitch586 = color452;
				#endif
				float2 uv_MainTexture468 = input.ase_texcoord4.xy;
				float4 tex2DNode468 = tex2D( _MainTexture, uv_MainTexture468 );
				float dotResult533 = dot( Normals594 , SafeNormalize( _MainLightPosition.xyz ) );
				float RampScale553 = _RampScale;
				float RampOffset552 = _RampOffset;
				float CEL_Effect473 = saturate( (dotResult533*RampScale553 + RampOffset552) );
				float2 temp_cast_2 = (CEL_Effect473).xx;
				float4 FinalLighting611 = ( ( tex2DNode468 * tex2D( _TextureRamp, temp_cast_2 ) ) * HalfLambert543 );
				float3 HighlightsColor609 = _TopHighlightsColor.rgb;
				float4 blendOpSrc332 = float4( HighlightsColor609 , 0.0 );
				float4 blendOpDest332 = FinalLighting611;
				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(PositionWS.x , PositionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2D( ToonScapesGlobalNoiseTexture, ( panner287 / float2( 150,150 ) ) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 lerpBlendMode332 = lerp(blendOpDest332, (( blendOpSrc332 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpSrc332 - 0.5 ) ) * ( 1.0 - blendOpDest332 ) ) : ( 2.0 * blendOpSrc332 * blendOpDest332 ) ),( WindSway244 * input.ase_color.g * ase_lightAtten ).r);
				float4 FinalLightingHighlights335 = (( _EnableTopHighlights )?( ( saturate( lerpBlendMode332 )) ):( FinalLighting611 ));
				float3 WorldPosition288_g61181 = PositionWS;
				float3 WorldPosition305_g61181 = WorldPosition288_g61181;
				float4 screenPos = input.ase_texcoord9;
				float4 ase_positionSSNorm = screenPos / screenPos.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float2 ScreenUV286_g61181 = (ase_positionSSNorm).xy;
				float2 ScreenUV305_g61181 = ScreenUV286_g61181;
				float3 WorldNormal281_g61181 = Normals594;
				float3 WorldNormal305_g61181 = WorldNormal281_g61181;
				half2 LightmapUV1_g61192 = (input.ase_texcoord4.zw*(unity_LightmapST).xy + (unity_LightmapST).zw);
				half4 localCalculateShadowMask1_g61192 = CalculateShadowMask1_g61192( LightmapUV1_g61192 );
				float4 ShadowMask360_g61181 = localCalculateShadowMask1_g61192;
				float4 ShadowMask305_g61181 = ShadowMask360_g61181;
				float3 localAdditionalLightsLambertMask17x305_g61181 = AdditionalLightsLambertMask17x( WorldPosition305_g61181 , ScreenUV305_g61181 , WorldNormal305_g61181 , ShadowMask305_g61181 );
				float3 saferPower684 = abs( saturate( localAdditionalLightsLambertMask17x305_g61181 ) );
				float3 temp_cast_7 = ( (0.001 + ( _AdditionalLightFalloff - 0.0 ) * ( 12.0 - 0.001 ) / ( 12.0 - 0.0 ) )).xxx;
				
				float3 EmissionColor461 = _EmissionColor.rgb;
				float2 uv_EmissionMap625 = input.ase_texcoord4.xy;
				float EmissionAlpha462 = _EmissionColor.a;
				float mulTime648 = _TimeParameters.x * _FlickerFrequency;
				#ifdef _ENABLEEMISSION_ON
				float3 staticSwitch589 = ( ( _EmissionIntensity1 * EmissionColor461 * tex2D( _EmissionMap, uv_EmissionMap625 ).rgb * EmissionAlpha462 ) * ( ( sin( ( ( mulTime648 + ( PositionWS.x + PositionWS.z ) ) / _FlickerScale ) ) * ( ( _MaxIntensity - _MinIntensity ) * 0.5 ) ) + ( ( _MaxIntensity + _MinIntensity ) * 0.5 ) ) );
				#else
				float3 staticSwitch589 = float3( 0,0,0 );
				#endif
				float3 Emission467 = staticSwitch589;
				
				float Alpha614 = tex2DNode468.a;
				

				float3 BaseColor = ( staticSwitch618 + ( staticSwitch573 + ( staticSwitch586 * ( FinalLightingHighlights335 + ( tex2DNode468 * tex2D( _TextureRamp, (pow( saferPower684 , temp_cast_7 )* (0.001 + ( _AdditionalLightInfluence - 0.0 ) * ( 6.0 - 0.001 ) / ( 1.0 - 0.0 ) ) + RampOffset552).xy ) ) ) ) ) ).rgb;
				float3 Emission = Emission467;
				float Alpha = Alpha614;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _AlphaClipThreshold;
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
			#define _ALPHATEST_ON
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
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
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#pragma shader_feature_local _ENABLEWIND_ON
			#pragma shader_feature_local _ENABLESUBSURFACEDISTORTION_ON
			#pragma shader_feature_local _ENABLESECONDARYHIGHLIGHTS_ON
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
				float4 ase_color : COLOR;
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
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 lightmapUVOrVertexSH : TEXCOORD5;
				float4 dynamicLightmapUV : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceTint;
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _BaseTint;
			float4 _TopHighlightsColor;
			float _DirectionBias;
			float _MaxIntensity;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _RampOffset;
			float _MinIntensity;
			float _RampScale;
			float _EnableTopHighlights;
			float _SecondarySmoothness;
			float _NormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _DistortionAmount;
			float _DistortionScale;
			float _AdditionalLightFalloff;
			float _AlphaClipThreshold;
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

			float3 ToonScapesGlobalWindDirection;
			sampler2D ToonScapesGlobalNoiseTexture;
			float ToonScapesGlobalWindJitter;
			float ToonScapesGlobalWindSpeed;
			float ToonScapesGlobalWindScale;
			float ToonScapesGlobalWindStrength;
			sampler2D _EmissionMap;
			sampler2D _NormalMap;
			sampler2D _MainTexture;
			sampler2D _TextureRamp;


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
			
			half4 CalculateShadowMask1_g61192( half2 LightmapUV )
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

				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_276_0 = float3( (WindDirection255).xz ,  0.0 );
				float GlobalJitter410 = ToonScapesGlobalWindJitter;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				float2 appendResult284 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner286 = ( 1.0 * _Time.y * ( temp_output_276_0 *  (0.0 + ( GlobalJitter410 - 0.0 ) * ( 40.0 - 0.0 ) / ( 10.0 - 0.0 ) ) ).xy + appendResult284);
				float4 tex2DNode293 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner286 / float2( 2,2 ) ), 0, 0.0) );
				float4 lerpResult405 = lerp( tex2DNode293 , ( ( tex2DNode293 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindJitter296 = lerpResult405;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner287 / float2( 150,150 ) ), 0, 0.0) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 transform310 = mul(GetWorldToObjectMatrix(),( float4( ( WindDirection255 * 0.1 ) , 0.0 ) * ( ( WindJitter296 * input.ase_color.r * 5.0 ) + ( input.ase_color.g * WindSway244 *  (0.0 + ( ToonScapesGlobalWindStrength - 0.0 ) * ( 3.5 - 0.0 ) / ( 1.0 - 0.0 ) ) ) ) ));
				float4 VertexOffset254 = transform310;
				#ifdef _ENABLEWIND_ON
				float4 staticSwitch620 = VertexOffset254;
				#else
				float4 staticSwitch620 = float4( 0,0,0,0 );
				#endif
				
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord1.xyz = ase_normalWS;
				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord3.xyz = ase_tangentWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord4.xyz = ase_bitangentWS;
				OUTPUT_LIGHTMAP_UV( input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy );
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
				output.ase_texcoord7 = screenPos;
				
				output.ase_color = input.ase_color;
				output.ase_texcoord2.xy = input.ase_texcoord.xy;
				output.ase_texcoord2.zw = input.texcoord1.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord1.w = 0;
				output.ase_texcoord3.w = 0;
				output.ase_texcoord4.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = staticSwitch620.xyz;

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
				float4 ase_color : COLOR;
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
				output.ase_color = input.ase_color;
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
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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

				float3 ase_viewVectorWS = ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz );
				float3 ase_viewDirWS = normalize( ase_viewVectorWS );
				float3 ase_normalWS = input.ase_texcoord1.xyz;
				float3 normalizedWorldNormal = normalize( ase_normalWS );
				float dotResult349 = dot( ase_viewDirWS , -( SafeNormalize( _MainLightPosition.xyz ) + ( normalizedWorldNormal *  (2.0 + ( _DistortionScale - 0.0 ) * ( 0.0 - 2.0 ) / ( 1.0 - 0.0 ) ) ) ) );
				float dotResult368 = dot( dotResult349 ,  (0.0 + ( _DistortionAmount - 0.0 ) * ( 2.0 - 0.0 ) / ( 1.0 - 0.0 ) ) );
				float ase_lightIntensity = max( max( _MainLightColor.r, _MainLightColor.g ), _MainLightColor.b ) + 1e-7;
				float4 ase_lightColor = float4( _MainLightColor.rgb / ase_lightIntensity, ase_lightIntensity );
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoord );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float4 SubsurfaceDistortion359 = ( ( ( ( _SubsurfaceTint * saturate( dotResult368 ) ) * ase_lightColor ) * input.ase_color.b ) * ase_lightAtten );
				#ifdef _ENABLESUBSURFACEDISTORTION_ON
				float4 staticSwitch618 = SubsurfaceDistortion359;
				#else
				float4 staticSwitch618 = float4( 0,0,0,0 );
				#endif
				float2 uv_EmissionMap600 = input.ase_texcoord2.xy;
				float3 temp_output_561_0 = ( _SpecularColor.rgb * tex2D( _EmissionMap, uv_EmissionMap600 ).a * _SecondarySpecularIntensity );
				float3 normalizeResult4_g61177 = normalize( ( ase_viewDirWS + SafeNormalize( _MainLightPosition.xyz ) ) );
				float2 uv_NormalMap470 = input.ase_texcoord2.xy;
				float3 unpack470 = UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap470 ), _NormalScale );
				unpack470.z = lerp( 1, unpack470.z, saturate(_NormalScale) );
				float3 tex2DNode470 = unpack470;
				float3 ase_tangentWS = input.ase_texcoord3.xyz;
				float3 ase_bitangentWS = input.ase_texcoord4.xyz;
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 tanNormal442 = tex2DNode470;
				float3 worldNormal442 = normalize( float3( dot( tanToWorld0, tanNormal442 ), dot( tanToWorld1, tanNormal442 ), dot( tanToWorld2, tanNormal442 ) ) );
				float3 normalizeResult444 = normalize( worldNormal442 );
				float3 Normals594 = normalizeResult444;
				float dotResult559 = dot( normalizeResult4_g61177 , Normals594 );
				float temp_output_672_0 = ( 1.0 - _SecondarySmoothness );
				float3 temp_output_564_0 = saturate( ( saturate( (  (-1.0 + ( _SecondarySpecularSize - 0.0 ) * ( -0.5 - -1.0 ) / ( 1.0 - 0.0 ) ) + dotResult559 ) ) / ( ( 1.0 - temp_output_561_0 ) * temp_output_672_0 ) ) );
				float3 DirectSpecHighlights574 = ( (temp_output_561_0).xyz * temp_output_564_0 );
				float3 bakedGI540 = ASEIndirectDiffuse( input, ase_normalWS, PositionWS, ase_viewDirWS );
				MixRealtimeAndBakedGI( ase_mainLight, ase_normalWS, bakedGI540, half4( 0, 0, 0, 0 ) );
				float4 HalfLambert543 = ( ( ase_lightColor * ase_lightAtten ) + float4( bakedGI540 , 0.0 ) );
				float4 SpecularHighlights578 = ( float4( DirectSpecHighlights574 , 0.0 ) * HalfLambert543 );
				#ifdef _ENABLESECONDARYHIGHLIGHTS_ON
				float4 staticSwitch573 = SpecularHighlights578;
				#else
				float4 staticSwitch573 = float4( 0,0,0,0 );
				#endif
				float4 color452 = IsGammaSpace() ? float4( 1, 1, 1, 1 ) : float4( 1, 1, 1, 1 );
				#ifdef _ENABLECOLORTINT_ON
				float4 staticSwitch586 = _BaseTint;
				#else
				float4 staticSwitch586 = color452;
				#endif
				float2 uv_MainTexture468 = input.ase_texcoord2.xy;
				float4 tex2DNode468 = tex2D( _MainTexture, uv_MainTexture468 );
				float dotResult533 = dot( Normals594 , SafeNormalize( _MainLightPosition.xyz ) );
				float RampScale553 = _RampScale;
				float RampOffset552 = _RampOffset;
				float CEL_Effect473 = saturate( (dotResult533*RampScale553 + RampOffset552) );
				float2 temp_cast_2 = (CEL_Effect473).xx;
				float4 FinalLighting611 = ( ( tex2DNode468 * tex2D( _TextureRamp, temp_cast_2 ) ) * HalfLambert543 );
				float3 HighlightsColor609 = _TopHighlightsColor.rgb;
				float4 blendOpSrc332 = float4( HighlightsColor609 , 0.0 );
				float4 blendOpDest332 = FinalLighting611;
				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(PositionWS.x , PositionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2D( ToonScapesGlobalNoiseTexture, ( panner287 / float2( 150,150 ) ) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 lerpBlendMode332 = lerp(blendOpDest332, (( blendOpSrc332 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpSrc332 - 0.5 ) ) * ( 1.0 - blendOpDest332 ) ) : ( 2.0 * blendOpSrc332 * blendOpDest332 ) ),( WindSway244 * input.ase_color.g * ase_lightAtten ).r);
				float4 FinalLightingHighlights335 = (( _EnableTopHighlights )?( ( saturate( lerpBlendMode332 )) ):( FinalLighting611 ));
				float3 WorldPosition288_g61181 = PositionWS;
				float3 WorldPosition305_g61181 = WorldPosition288_g61181;
				float4 screenPos = input.ase_texcoord7;
				float4 ase_positionSSNorm = screenPos / screenPos.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float2 ScreenUV286_g61181 = (ase_positionSSNorm).xy;
				float2 ScreenUV305_g61181 = ScreenUV286_g61181;
				float3 WorldNormal281_g61181 = Normals594;
				float3 WorldNormal305_g61181 = WorldNormal281_g61181;
				half2 LightmapUV1_g61192 = (input.ase_texcoord2.zw*(unity_LightmapST).xy + (unity_LightmapST).zw);
				half4 localCalculateShadowMask1_g61192 = CalculateShadowMask1_g61192( LightmapUV1_g61192 );
				float4 ShadowMask360_g61181 = localCalculateShadowMask1_g61192;
				float4 ShadowMask305_g61181 = ShadowMask360_g61181;
				float3 localAdditionalLightsLambertMask17x305_g61181 = AdditionalLightsLambertMask17x( WorldPosition305_g61181 , ScreenUV305_g61181 , WorldNormal305_g61181 , ShadowMask305_g61181 );
				float3 saferPower684 = abs( saturate( localAdditionalLightsLambertMask17x305_g61181 ) );
				float3 temp_cast_7 = ( (0.001 + ( _AdditionalLightFalloff - 0.0 ) * ( 12.0 - 0.001 ) / ( 12.0 - 0.0 ) )).xxx;
				
				float Alpha614 = tex2DNode468.a;
				

				float3 BaseColor = ( staticSwitch618 + ( staticSwitch573 + ( staticSwitch586 * ( FinalLightingHighlights335 + ( tex2DNode468 * tex2D( _TextureRamp, (pow( saferPower684 , temp_cast_7 )* (0.001 + ( _AdditionalLightInfluence - 0.0 ) * ( 6.0 - 0.001 ) / ( 1.0 - 0.0 ) ) + RampOffset552).xy ) ) ) ) ) ).rgb;
				float Alpha = Alpha614;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _AlphaClipThreshold;
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
			#define _ALPHATEST_ON
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
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

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#pragma shader_feature_local _ENABLEWIND_ON


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
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceTint;
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _BaseTint;
			float4 _TopHighlightsColor;
			float _DirectionBias;
			float _MaxIntensity;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _RampOffset;
			float _MinIntensity;
			float _RampScale;
			float _EnableTopHighlights;
			float _SecondarySmoothness;
			float _NormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _DistortionAmount;
			float _DistortionScale;
			float _AdditionalLightFalloff;
			float _AlphaClipThreshold;
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

			float3 ToonScapesGlobalWindDirection;
			sampler2D ToonScapesGlobalNoiseTexture;
			float ToonScapesGlobalWindJitter;
			float ToonScapesGlobalWindSpeed;
			float ToonScapesGlobalWindScale;
			float ToonScapesGlobalWindStrength;
			sampler2D _NormalMap;
			sampler2D _MainTexture;


			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_276_0 = float3( (WindDirection255).xz ,  0.0 );
				float GlobalJitter410 = ToonScapesGlobalWindJitter;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				float2 appendResult284 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner286 = ( 1.0 * _Time.y * ( temp_output_276_0 *  (0.0 + ( GlobalJitter410 - 0.0 ) * ( 40.0 - 0.0 ) / ( 10.0 - 0.0 ) ) ).xy + appendResult284);
				float4 tex2DNode293 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner286 / float2( 2,2 ) ), 0, 0.0) );
				float4 lerpResult405 = lerp( tex2DNode293 , ( ( tex2DNode293 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindJitter296 = lerpResult405;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner287 / float2( 150,150 ) ), 0, 0.0) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 transform310 = mul(GetWorldToObjectMatrix(),( float4( ( WindDirection255 * 0.1 ) , 0.0 ) * ( ( WindJitter296 * input.ase_color.r * 5.0 ) + ( input.ase_color.g * WindSway244 *  (0.0 + ( ToonScapesGlobalWindStrength - 0.0 ) * ( 3.5 - 0.0 ) / ( 1.0 - 0.0 ) ) ) ) ));
				float4 VertexOffset254 = transform310;
				#ifdef _ENABLEWIND_ON
				float4 staticSwitch620 = VertexOffset254;
				#else
				float4 staticSwitch620 = float4( 0,0,0,0 );
				#endif
				
				output.ase_texcoord3.xy = input.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord3.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = staticSwitch620.xyz;

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
				float4 ase_color : COLOR;

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
				output.ase_color = input.ase_color;
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
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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

				float2 uv_NormalMap470 = input.ase_texcoord3.xy;
				float3 unpack470 = UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap470 ), _NormalScale );
				unpack470.z = lerp( 1, unpack470.z, saturate(_NormalScale) );
				float3 tex2DNode470 = unpack470;
				float3 NormalInput660 = tex2DNode470;
				
				float2 uv_MainTexture468 = input.ase_texcoord3.xy;
				float4 tex2DNode468 = tex2D( _MainTexture, uv_MainTexture468 );
				float Alpha614 = tex2DNode468.a;
				

				float3 Normal = NormalInput660;
				float Alpha = Alpha614;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _AlphaClipThreshold;
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
			#define _ALPHATEST_ON
			#define _NORMAL_DROPOFF_TS 1
			#pragma shader_feature_local_fragment _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
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

			#define ASE_NEEDS_FRAG_WORLD_VIEW_DIR
			#define ASE_NEEDS_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#pragma shader_feature_local _ENABLEWIND_ON
			#pragma shader_feature_local _ENABLESUBSURFACEDISTORTION_ON
			#pragma shader_feature_local _ENABLESECONDARYHIGHLIGHTS_ON
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
				float4 ase_color : COLOR;
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
				float4 ase_color : COLOR;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceTint;
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _BaseTint;
			float4 _TopHighlightsColor;
			float _DirectionBias;
			float _MaxIntensity;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _RampOffset;
			float _MinIntensity;
			float _RampScale;
			float _EnableTopHighlights;
			float _SecondarySmoothness;
			float _NormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _DistortionAmount;
			float _DistortionScale;
			float _AdditionalLightFalloff;
			float _AlphaClipThreshold;
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

			float3 ToonScapesGlobalWindDirection;
			sampler2D ToonScapesGlobalNoiseTexture;
			float ToonScapesGlobalWindJitter;
			float ToonScapesGlobalWindSpeed;
			float ToonScapesGlobalWindScale;
			float ToonScapesGlobalWindStrength;
			sampler2D _EmissionMap;
			sampler2D _NormalMap;
			sampler2D _MainTexture;
			sampler2D _TextureRamp;


			#if ( UNITY_VERSION >= 60010000 )
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutput.hlsl"
			#else
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
			#endif

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
			
			half4 CalculateShadowMask1_g61192( half2 LightmapUV )
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

				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_276_0 = float3( (WindDirection255).xz ,  0.0 );
				float GlobalJitter410 = ToonScapesGlobalWindJitter;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				float2 appendResult284 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner286 = ( 1.0 * _Time.y * ( temp_output_276_0 *  (0.0 + ( GlobalJitter410 - 0.0 ) * ( 40.0 - 0.0 ) / ( 10.0 - 0.0 ) ) ).xy + appendResult284);
				float4 tex2DNode293 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner286 / float2( 2,2 ) ), 0, 0.0) );
				float4 lerpResult405 = lerp( tex2DNode293 , ( ( tex2DNode293 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindJitter296 = lerpResult405;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner287 / float2( 150,150 ) ), 0, 0.0) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 transform310 = mul(GetWorldToObjectMatrix(),( float4( ( WindDirection255 * 0.1 ) , 0.0 ) * ( ( WindJitter296 * input.ase_color.r * 5.0 ) + ( input.ase_color.g * WindSway244 *  (0.0 + ( ToonScapesGlobalWindStrength - 0.0 ) * ( 3.5 - 0.0 ) / ( 1.0 - 0.0 ) ) ) ) ));
				float4 VertexOffset254 = transform310;
				#ifdef _ENABLEWIND_ON
				float4 staticSwitch620 = VertexOffset254;
				#else
				float4 staticSwitch620 = float4( 0,0,0,0 );
				#endif
				
				output.ase_color = input.ase_color;
				output.ase_texcoord7.xy = input.texcoord.xy;
				output.ase_texcoord7.zw = input.texcoord1.xy;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = staticSwitch620.xyz;

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
				float4 ase_color : COLOR;

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
				output.ase_color = input.ase_color;
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
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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

				float3 normalizedWorldNormal = normalize( NormalWS );
				float dotResult349 = dot( ViewDirWS , -( SafeNormalize( _MainLightPosition.xyz ) + ( normalizedWorldNormal *  (2.0 + ( _DistortionScale - 0.0 ) * ( 0.0 - 2.0 ) / ( 1.0 - 0.0 ) ) ) ) );
				float dotResult368 = dot( dotResult349 ,  (0.0 + ( _DistortionAmount - 0.0 ) * ( 2.0 - 0.0 ) / ( 1.0 - 0.0 ) ) );
				float ase_lightIntensity = max( max( _MainLightColor.r, _MainLightColor.g ), _MainLightColor.b ) + 1e-7;
				float4 ase_lightColor = float4( _MainLightColor.rgb / ase_lightIntensity, ase_lightIntensity );
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoord );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float4 SubsurfaceDistortion359 = ( ( ( ( _SubsurfaceTint * saturate( dotResult368 ) ) * ase_lightColor ) * input.ase_color.b ) * ase_lightAtten );
				#ifdef _ENABLESUBSURFACEDISTORTION_ON
				float4 staticSwitch618 = SubsurfaceDistortion359;
				#else
				float4 staticSwitch618 = float4( 0,0,0,0 );
				#endif
				float2 uv_EmissionMap600 = input.ase_texcoord7.xy;
				float3 temp_output_561_0 = ( _SpecularColor.rgb * tex2D( _EmissionMap, uv_EmissionMap600 ).a * _SecondarySpecularIntensity );
				float3 normalizeResult4_g61177 = normalize( ( ViewDirWS + SafeNormalize( _MainLightPosition.xyz ) ) );
				float2 uv_NormalMap470 = input.ase_texcoord7.xy;
				float3 unpack470 = UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap470 ), _NormalScale );
				unpack470.z = lerp( 1, unpack470.z, saturate(_NormalScale) );
				float3 tex2DNode470 = unpack470;
				float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
				float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
				float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
				float3 tanNormal442 = tex2DNode470;
				float3 worldNormal442 = normalize( float3( dot( tanToWorld0, tanNormal442 ), dot( tanToWorld1, tanNormal442 ), dot( tanToWorld2, tanNormal442 ) ) );
				float3 normalizeResult444 = normalize( worldNormal442 );
				float3 Normals594 = normalizeResult444;
				float dotResult559 = dot( normalizeResult4_g61177 , Normals594 );
				float temp_output_672_0 = ( 1.0 - _SecondarySmoothness );
				float3 temp_output_564_0 = saturate( ( saturate( (  (-1.0 + ( _SecondarySpecularSize - 0.0 ) * ( -0.5 - -1.0 ) / ( 1.0 - 0.0 ) ) + dotResult559 ) ) / ( ( 1.0 - temp_output_561_0 ) * temp_output_672_0 ) ) );
				float3 DirectSpecHighlights574 = ( (temp_output_561_0).xyz * temp_output_564_0 );
				float3 bakedGI540 = ASEIndirectDiffuse( input, NormalWS, PositionWS, ViewDirWS );
				MixRealtimeAndBakedGI( ase_mainLight, NormalWS, bakedGI540, half4( 0, 0, 0, 0 ) );
				float4 HalfLambert543 = ( ( ase_lightColor * ase_lightAtten ) + float4( bakedGI540 , 0.0 ) );
				float4 SpecularHighlights578 = ( float4( DirectSpecHighlights574 , 0.0 ) * HalfLambert543 );
				#ifdef _ENABLESECONDARYHIGHLIGHTS_ON
				float4 staticSwitch573 = SpecularHighlights578;
				#else
				float4 staticSwitch573 = float4( 0,0,0,0 );
				#endif
				float4 color452 = IsGammaSpace() ? float4( 1, 1, 1, 1 ) : float4( 1, 1, 1, 1 );
				#ifdef _ENABLECOLORTINT_ON
				float4 staticSwitch586 = _BaseTint;
				#else
				float4 staticSwitch586 = color452;
				#endif
				float2 uv_MainTexture468 = input.ase_texcoord7.xy;
				float4 tex2DNode468 = tex2D( _MainTexture, uv_MainTexture468 );
				float dotResult533 = dot( Normals594 , SafeNormalize( _MainLightPosition.xyz ) );
				float RampScale553 = _RampScale;
				float RampOffset552 = _RampOffset;
				float CEL_Effect473 = saturate( (dotResult533*RampScale553 + RampOffset552) );
				float2 temp_cast_2 = (CEL_Effect473).xx;
				float4 FinalLighting611 = ( ( tex2DNode468 * tex2D( _TextureRamp, temp_cast_2 ) ) * HalfLambert543 );
				float3 HighlightsColor609 = _TopHighlightsColor.rgb;
				float4 blendOpSrc332 = float4( HighlightsColor609 , 0.0 );
				float4 blendOpDest332 = FinalLighting611;
				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(PositionWS.x , PositionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2D( ToonScapesGlobalNoiseTexture, ( panner287 / float2( 150,150 ) ) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 lerpBlendMode332 = lerp(blendOpDest332, (( blendOpSrc332 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpSrc332 - 0.5 ) ) * ( 1.0 - blendOpDest332 ) ) : ( 2.0 * blendOpSrc332 * blendOpDest332 ) ),( WindSway244 * input.ase_color.g * ase_lightAtten ).r);
				float4 FinalLightingHighlights335 = (( _EnableTopHighlights )?( ( saturate( lerpBlendMode332 )) ):( FinalLighting611 ));
				float3 WorldPosition288_g61181 = PositionWS;
				float3 WorldPosition305_g61181 = WorldPosition288_g61181;
				float2 ScreenUV286_g61181 = (ScreenPosNorm).xy;
				float2 ScreenUV305_g61181 = ScreenUV286_g61181;
				float3 WorldNormal281_g61181 = Normals594;
				float3 WorldNormal305_g61181 = WorldNormal281_g61181;
				half2 LightmapUV1_g61192 = (input.ase_texcoord7.zw*(unity_LightmapST).xy + (unity_LightmapST).zw);
				half4 localCalculateShadowMask1_g61192 = CalculateShadowMask1_g61192( LightmapUV1_g61192 );
				float4 ShadowMask360_g61181 = localCalculateShadowMask1_g61192;
				float4 ShadowMask305_g61181 = ShadowMask360_g61181;
				float3 localAdditionalLightsLambertMask17x305_g61181 = AdditionalLightsLambertMask17x( WorldPosition305_g61181 , ScreenUV305_g61181 , WorldNormal305_g61181 , ShadowMask305_g61181 );
				float3 saferPower684 = abs( saturate( localAdditionalLightsLambertMask17x305_g61181 ) );
				float3 temp_cast_7 = ( (0.001 + ( _AdditionalLightFalloff - 0.0 ) * ( 12.0 - 0.001 ) / ( 12.0 - 0.0 ) )).xxx;
				
				float3 NormalInput660 = tex2DNode470;
				
				float3 SpecularTint669 = _SpecularColor.rgb;
				float2 uv_EmissionMap666 = input.ase_texcoord7.xy;
				
				float3 EmissionColor461 = _EmissionColor.rgb;
				float2 uv_EmissionMap625 = input.ase_texcoord7.xy;
				float EmissionAlpha462 = _EmissionColor.a;
				float mulTime648 = _TimeParameters.x * _FlickerFrequency;
				#ifdef _ENABLEEMISSION_ON
				float3 staticSwitch589 = ( ( _EmissionIntensity1 * EmissionColor461 * tex2D( _EmissionMap, uv_EmissionMap625 ).rgb * EmissionAlpha462 ) * ( ( sin( ( ( mulTime648 + ( PositionWS.x + PositionWS.z ) ) / _FlickerScale ) ) * ( ( _MaxIntensity - _MinIntensity ) * 0.5 ) ) + ( ( _MaxIntensity + _MinIntensity ) * 0.5 ) ) );
				#else
				float3 staticSwitch589 = float3( 0,0,0 );
				#endif
				float3 Emission467 = staticSwitch589;
				
				float Alpha614 = tex2DNode468.a;
				

				float3 BaseColor = ( staticSwitch618 + ( staticSwitch573 + ( staticSwitch586 * ( FinalLightingHighlights335 + ( tex2DNode468 * tex2D( _TextureRamp, (pow( saferPower684 , temp_cast_7 )* (0.001 + ( _AdditionalLightInfluence - 0.0 ) * ( 6.0 - 0.001 ) / ( 1.0 - 0.0 ) ) + RampOffset552).xy ) ) ) ) ) ).rgb;
				float3 Normal = NormalInput660;
				float3 Specular = ( _SpecularIntensity1 * SpecularTint669 * tex2D( _EmissionMap, uv_EmissionMap666 ).a );
				float Metallic = 0;
				float Smoothness = _Smoothness;
				float Occlusion = _Occlusion;
				float3 Emission = Emission467;
				float Alpha = Alpha614;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _AlphaClipThreshold;
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
			#define _ALPHATEST_ON
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
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

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#pragma shader_feature_local _ENABLEWIND_ON


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
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceTint;
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _BaseTint;
			float4 _TopHighlightsColor;
			float _DirectionBias;
			float _MaxIntensity;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _RampOffset;
			float _MinIntensity;
			float _RampScale;
			float _EnableTopHighlights;
			float _SecondarySmoothness;
			float _NormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _DistortionAmount;
			float _DistortionScale;
			float _AdditionalLightFalloff;
			float _AlphaClipThreshold;
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

			float3 ToonScapesGlobalWindDirection;
			sampler2D ToonScapesGlobalNoiseTexture;
			float ToonScapesGlobalWindJitter;
			float ToonScapesGlobalWindSpeed;
			float ToonScapesGlobalWindScale;
			float ToonScapesGlobalWindStrength;
			sampler2D _MainTexture;


			
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

				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_276_0 = float3( (WindDirection255).xz ,  0.0 );
				float GlobalJitter410 = ToonScapesGlobalWindJitter;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				float2 appendResult284 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner286 = ( 1.0 * _Time.y * ( temp_output_276_0 *  (0.0 + ( GlobalJitter410 - 0.0 ) * ( 40.0 - 0.0 ) / ( 10.0 - 0.0 ) ) ).xy + appendResult284);
				float4 tex2DNode293 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner286 / float2( 2,2 ) ), 0, 0.0) );
				float4 lerpResult405 = lerp( tex2DNode293 , ( ( tex2DNode293 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindJitter296 = lerpResult405;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner287 / float2( 150,150 ) ), 0, 0.0) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 transform310 = mul(GetWorldToObjectMatrix(),( float4( ( WindDirection255 * 0.1 ) , 0.0 ) * ( ( WindJitter296 * input.ase_color.r * 5.0 ) + ( input.ase_color.g * WindSway244 *  (0.0 + ( ToonScapesGlobalWindStrength - 0.0 ) * ( 3.5 - 0.0 ) / ( 1.0 - 0.0 ) ) ) ) ));
				float4 VertexOffset254 = transform310;
				#ifdef _ENABLEWIND_ON
				float4 staticSwitch620 = VertexOffset254;
				#else
				float4 staticSwitch620 = float4( 0,0,0,0 );
				#endif
				
				output.ase_texcoord1.xy = input.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord1.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = staticSwitch620.xyz;

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
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

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
				output.ase_color = input.ase_color;
				output.ase_texcoord = input.ase_texcoord;
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
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

				float2 uv_MainTexture468 = input.ase_texcoord1.xy;
				float4 tex2DNode468 = tex2D( _MainTexture, uv_MainTexture468 );
				float Alpha614 = tex2DNode468.a;
				

				surfaceDescription.Alpha = Alpha614;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _AlphaClipThreshold;
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
			#define _ALPHATEST_ON
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
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

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#pragma shader_feature_local _ENABLEWIND_ON


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
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceTint;
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _BaseTint;
			float4 _TopHighlightsColor;
			float _DirectionBias;
			float _MaxIntensity;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _RampOffset;
			float _MinIntensity;
			float _RampScale;
			float _EnableTopHighlights;
			float _SecondarySmoothness;
			float _NormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _DistortionAmount;
			float _DistortionScale;
			float _AdditionalLightFalloff;
			float _AlphaClipThreshold;
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

			float3 ToonScapesGlobalWindDirection;
			sampler2D ToonScapesGlobalNoiseTexture;
			float ToonScapesGlobalWindJitter;
			float ToonScapesGlobalWindSpeed;
			float ToonScapesGlobalWindScale;
			float ToonScapesGlobalWindStrength;
			sampler2D _MainTexture;


			
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

				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_276_0 = float3( (WindDirection255).xz ,  0.0 );
				float GlobalJitter410 = ToonScapesGlobalWindJitter;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				float2 appendResult284 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner286 = ( 1.0 * _Time.y * ( temp_output_276_0 *  (0.0 + ( GlobalJitter410 - 0.0 ) * ( 40.0 - 0.0 ) / ( 10.0 - 0.0 ) ) ).xy + appendResult284);
				float4 tex2DNode293 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner286 / float2( 2,2 ) ), 0, 0.0) );
				float4 lerpResult405 = lerp( tex2DNode293 , ( ( tex2DNode293 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindJitter296 = lerpResult405;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner287 / float2( 150,150 ) ), 0, 0.0) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 transform310 = mul(GetWorldToObjectMatrix(),( float4( ( WindDirection255 * 0.1 ) , 0.0 ) * ( ( WindJitter296 * input.ase_color.r * 5.0 ) + ( input.ase_color.g * WindSway244 *  (0.0 + ( ToonScapesGlobalWindStrength - 0.0 ) * ( 3.5 - 0.0 ) / ( 1.0 - 0.0 ) ) ) ) ));
				float4 VertexOffset254 = transform310;
				#ifdef _ENABLEWIND_ON
				float4 staticSwitch620 = VertexOffset254;
				#else
				float4 staticSwitch620 = float4( 0,0,0,0 );
				#endif
				
				output.ase_texcoord1.xy = input.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord1.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = staticSwitch620.xyz;

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
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

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
				output.ase_color = input.ase_color;
				output.ase_texcoord = input.ase_texcoord;
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
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

				float2 uv_MainTexture468 = input.ase_texcoord1.xy;
				float4 tex2DNode468 = tex2D( _MainTexture, uv_MainTexture468 );
				float Alpha614 = tex2DNode468.a;
				

				surfaceDescription.Alpha = Alpha614;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _AlphaClipThreshold;
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
			#define _ALPHATEST_ON
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_TIME_BASED_MOTION_VECTORS
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
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

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#pragma shader_feature_local _ENABLEWIND_ON


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
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 positionCSNoJitter : TEXCOORD0;
				float4 previousPositionCSNoJitter : TEXCOORD1;
				float3 positionWS : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SubsurfaceTint;
			float4 _SpecularColor;
			float4 _EmissionColor;
			float4 _BaseTint;
			float4 _TopHighlightsColor;
			float _DirectionBias;
			float _MaxIntensity;
			float _FlickerScale;
			float _FlickerFrequency;
			float _EmissionIntensity1;
			float _Occlusion;
			float _Smoothness;
			float _SpecularIntensity1;
			float _AdditionalLightInfluence;
			float _RampOffset;
			float _MinIntensity;
			float _RampScale;
			float _EnableTopHighlights;
			float _SecondarySmoothness;
			float _NormalScale;
			float _SecondarySpecularSize;
			float _SecondarySpecularIntensity;
			float _DistortionAmount;
			float _DistortionScale;
			float _AdditionalLightFalloff;
			float _AlphaClipThreshold;
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

			float3 ToonScapesGlobalWindDirection;
			sampler2D ToonScapesGlobalNoiseTexture;
			float ToonScapesGlobalWindJitter;
			float ToonScapesGlobalWindSpeed;
			float ToonScapesGlobalWindScale;
			float ToonScapesGlobalWindStrength;
			sampler2D _MainTexture;


			
			// Applies the graph's vertex stage at a given time so the motion vector pass can
			// evaluate the current frame and re-evaluate the previous frame (procedural / time-based animation).
			Attributes ASEApplyVertexModification( Attributes input, float3 timeParameters, inout PackedVaryings output, out float3 customMotionVector  )
			{
				float3 currentTimeParameters = _TimeParameters.xyz;
				_TimeParameters.xyz = timeParameters;

				float3 WindDirection255 = ToonScapesGlobalWindDirection;
				float3 temp_output_276_0 = float3( (WindDirection255).xz ,  0.0 );
				float GlobalJitter410 = ToonScapesGlobalWindJitter;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				float2 appendResult284 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner286 = ( 1.0 * _Time.y * ( temp_output_276_0 *  (0.0 + ( GlobalJitter410 - 0.0 ) * ( 40.0 - 0.0 ) / ( 10.0 - 0.0 ) ) ).xy + appendResult284);
				float4 tex2DNode293 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner286 / float2( 2,2 ) ), 0, 0.0) );
				float4 lerpResult405 = lerp( tex2DNode293 , ( ( tex2DNode293 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindJitter296 = lerpResult405;
				float3 temp_output_275_0 = float3( (WindDirection255).xz ,  0.0 );
				float2 appendResult280 = (float2(ase_positionWS.x , ase_positionWS.z));
				float2 panner287 = ( -1.0 * _Time.y * ( temp_output_275_0 *  (0.0 + ( ToonScapesGlobalWindSpeed - 0.0 ) * ( 25.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ).xy + ( appendResult280 * ( 1.0 /  (0.005 + ( ToonScapesGlobalWindScale - 0.0 ) * ( 0.5 - 0.005 ) / ( 4.0 - 0.0 ) ) ) ));
				float4 tex2DNode292 = tex2Dlod( ToonScapesGlobalNoiseTexture, float4( ( panner287 / float2( 150,150 ) ), 0, 0.0) );
				float4 lerpResult401 = lerp( tex2DNode292 , ( ( tex2DNode292 * 2.0 ) + -1.0 ) , _DirectionBias);
				float4 WindSway244 = lerpResult401;
				float4 transform310 = mul(GetWorldToObjectMatrix(),( float4( ( WindDirection255 * 0.1 ) , 0.0 ) * ( ( WindJitter296 * input.ase_color.r * 5.0 ) + ( input.ase_color.g * WindSway244 *  (0.0 + ( ToonScapesGlobalWindStrength - 0.0 ) * ( 3.5 - 0.0 ) / ( 1.0 - 0.0 ) ) ) ) ));
				float4 VertexOffset254 = transform310;
				#ifdef _ENABLEWIND_ON
				float4 staticSwitch620 = VertexOffset254;
				#else
				float4 staticSwitch620 = float4( 0,0,0,0 );
				#endif
				
				output.ase_texcoord3.xy = input.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord3.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = staticSwitch620.xyz;

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

				float2 uv_MainTexture468 = input.ase_texcoord3.xy;
				float4 tex2DNode468 = tex2D( _MainTexture, uv_MainTexture468 );
				float Alpha614 = tex2DNode468.a;
				

				float Alpha = Alpha614;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _AlphaClipThreshold;
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
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":265,"pos":[-2864,3920],"params":["Inherit","False","2415.957","740.3464","Jiter","17","293","291","290","286","284","283","281","277","276","270","267","403","404","405","406","407","412","","0.05098039,0,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":264,"pos":[-2864,4688],"params":["Inherit","False","2417.851","756.4428","Sway","22","401","399","397","400","398","292","289","288","287","282","285","278","275","280","279","271","269","273","272","266","268","402","","0.05098039,0,1,1","0","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":274,"pos":[-1680,-3216],"params":["Float","False","Global","ToonScapesGlobalWindJitter","ToonScapesGlobalWindJitter","9","0","Create","True","0","0","0","False","0","False","Object","-1","","0.4","2.2","0","10","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":410,"pos":[-1376,-3216],"params":["Inherit","False","GlobalJitter","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":267,"pos":[-2688,4320],"params":["Inherit","False","255","WindDirection","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":268,"pos":[-2784,4944],"params":["Inherit","False","Global","ToonScapesGlobalWindScale","ToonScapesGlobalWindScale","8","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0.6","0.1","10","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":266,"pos":[-2688,5088],"params":["Inherit","False","255","WindDirection","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor","id":270,"pos":[-2464,4320],"params":["Inherit","False","FLOAT2","0","2","2","3","1","0","FLOAT3","0,0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":412,"pos":[-2656,4464],"params":["Inherit","False","410","GlobalJitter","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor","id":272,"pos":[-2480,4736],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":273,"pos":[-2480,4896],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","4","False","3","FLOAT","0.005","False","4","FLOAT","0.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":271,"pos":[-2688,5232],"params":["Float","False","Global","ToonScapesGlobalWindSpeed","ToonScapesGlobalWindSpeed","9","0","Create","True","0","0","0","False","0","False","Object","-1","","0.4","10.2","0","10","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor","id":269,"pos":[-2464,5088],"params":["Inherit","False","FLOAT2","0","2","2","3","1","0","FLOAT3","0,0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.TransformDirectionNode, AmplifyShaderEditor","id":276,"pos":[-2304,4320],"params":["Inherit","False","World","World","True","Fast","False","1","0","FLOAT3","0,0,0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor","id":277,"pos":[-2304,4144],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":281,"pos":[-2368,4464],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","10","False","3","FLOAT","0","False","4","FLOAT","40","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":279,"pos":[-2224,4960],"params":["Inherit","False","2","0","FLOAT","1","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":280,"pos":[-2256,4784],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":278,"pos":[-2368,5232],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","0","False","4","FLOAT","25","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TransformDirectionNode, AmplifyShaderEditor","id":275,"pos":[-2304,5088],"params":["Inherit","False","World","World","True","Fast","False","1","0","FLOAT3","0,0,0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":283,"pos":[-2032,4368],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":284,"pos":[-2048,4240],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":262,"pos":[-1712,-3120],"params":["Inherit","True","Global","ToonScapesGlobalNoiseTexture","ToonScapesGlobalNoiseTexture","46","0","Create","True","0","0","0","False","0","False","","None","None","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":285,"pos":[-2032,4944],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":282,"pos":[-2032,5136],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.PannerNode, AmplifyShaderEditor","id":286,"pos":[-1808,4288],"params":["Inherit","False","3","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":398,"pos":[-1168,5216],"params":["Inherit","False","Constant","_Double","Double","30","0","Create","True","0","0","0","False","0","False","Object","-1","","2","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":263,"pos":[-1376,-3120],"params":["Inherit","False","NoiseTexture","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":406,"pos":[-1168,4448],"params":["Inherit","False","Constant","_Double1","Double","30","0","Create","True","0","0","0","False","0","False","Object","-1","","2","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PannerNode, AmplifyShaderEditor","id":287,"pos":[-1728,5056],"params":["Inherit","False","3","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","1","FLOAT","-1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":290,"pos":[-1600,4208],"params":["Inherit","False","263","NoiseTexture","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":291,"pos":[-1504,4288],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT2","2,2","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":397,"pos":[-976,5104],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":400,"pos":[-1168,5296],"params":["Inherit","False","Constant","_ReCenter","Re-Center","30","0","Create","True","0","0","0","False","0","False","Object","-1","","-1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":407,"pos":[-1168,4528],"params":["Inherit","False","Constant","_ReCenter1","Re-Center","30","0","Create","True","0","0","0","False","0","False","Object","-1","","-1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":403,"pos":[-976,4336],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":288,"pos":[-1504,5056],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT2","150,150","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":289,"pos":[-1600,4976],"params":["Inherit","False","263","NoiseTexture","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":293,"pos":[-1360,4256],"params":["Inherit","True","Property","_NoiseTexture2","NoiseTexture","15","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":399,"pos":[-800,5104],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":402,"pos":[-1008,4752],"params":["Inherit","False","Property","_DirectionBias","Direction Bias","40","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":404,"pos":[-800,4336],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":292,"pos":[-1360,5024],"params":["Inherit","True","Property","_NoiseTexture1","NoiseTexture","15","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":294,"pos":[-2864,3168],"params":["Inherit","False","1604","723","","13","310","309","308","307","306","305","304","302","301","300","299","298","413","Wind","0.0492959,0,1,1","0","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":401,"pos":[-624,5024],"params":["Inherit","False","3","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","2","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":405,"pos":[-624,4256],"params":["Inherit","False","3","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","2","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor","id":256,"pos":[-1712,-3376],"params":["Inherit","False","Global","ToonScapesGlobalWindDirection","ToonScapesGlobalWindDirection","12","0","Create","True","0","0","0","False","0","False","Object","-1","","0,0,0","0.1,0,0.1","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":296,"pos":[-384,4256],"params":["Inherit","False","WindJitter","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":244,"pos":[-384,5024],"params":["Inherit","False","WindSway","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":298,"pos":[-2800,3776],"params":["Inherit","False","Global","ToonScapesGlobalWindStrength","ToonScapesGlobalWindStrength","8","0","Create","True","0","0","0","False","0","False","Object","-1","","1","2.9","0","10","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":301,"pos":[-2464,3632],"params":["Inherit","False","244","WindSway","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":299,"pos":[-2432,3264],"params":["Inherit","False","296","WindJitter","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor","id":300,"pos":[-2720,3392],"params":["Inherit","False","0","5","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":302,"pos":[-2464,3712],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","0","False","4","FLOAT","3.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":413,"pos":[-2320,3520],"params":["Inherit","False","Constant","_Scalar","Scalar","30","0","Create","True","0","0","0","False","0","False","Object","-1","","5","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":255,"pos":[-1376,-3376],"params":["Inherit","False","WindDirection","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":304,"pos":[-2112,3440],"params":["Inherit","False","3","3","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":305,"pos":[-2112,3648],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","COLOR","0,0,0,0","False","2","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":306,"pos":[-2144,3312],"params":["Inherit","False","255","WindDirection","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":577,"pos":[-2864,-3168],"params":["Inherit","True","Property","_MainTexture","Main Texture","5","2","[NoScaleOffset]","[SingleLineTexture]","Create","True","0","0","0","False","1","Space (8)","False","","None","None","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":307,"pos":[-1872,3488],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.ScaleNode, AmplifyShaderEditor","id":308,"pos":[-1888,3280],"params":["Inherit","False","0.1","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":443,"pos":[-2624,-3168],"params":["Inherit","False","MainTexture","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":309,"pos":[-1680,3408],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.WorldToObjectTransfNode, AmplifyShaderEditor","id":310,"pos":[-1472,3504],"params":["Inherit","False","1","0","FLOAT4","0,0,0,1","False","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":469,"pos":[608,-1312],"params":["Inherit","False","443","MainTexture","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":426,"pos":[-2864,1264],"params":["Inherit","False","3821.917","1840.894","","44","608","607","606","604","603","528","527","524","519","518","517","516","515","514","513","511","509","507","506","505","504","499","498","497","496","495","494","493","492","491","490","489","488","487","486","485","484","483","482","481","435","433","431","430","Fresnel","0.7960784,0.7215686,0,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":621,"pos":[-2864,304],"params":["Inherit","False","2321.01","875.313","","20","640","639","638","637","636","635","634","633","632","631","630","629","628","626","625","624","623","622","589","647","Emission","1,0.1319249,0,1","0","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":254,"pos":[-1216,3504],"params":["Inherit","False","VertexOffset","-1","True","1","0","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":468,"pos":[832,-1312],"params":["Inherit","True","Property","_MainTexture1","Main Texture 1","0","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Instance","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":613,"pos":[1712,-1312],"params":["Inherit","False","324","290.95","","2","611","612","Top Highlights","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":341,"pos":[1248,1264],"params":["Inherit","False","2371.633","718.8586","","21","357","354","379","367","365","358","355","368","376","349","369","348","347","343","377","342","371","344","345","378","656","Subsurface Distortion","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":340,"pos":[-256,320],"params":["Inherit","False","1252","371","","8","331","334","338","332","333","327","328","329","Top Highlights","0.7021739,1,0.3160377,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":427,"pos":[-1712,-2432],"params":["Inherit","False","1166.792","305.3181","","7","595","537","536","535","534","533","532","CEL Effect","0.7960785,0.7215686,0,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":428,"pos":[-1472,-2032],"params":["Inherit","False","592.2708","402.95","Normal Lerp","4","478","477","476","475","","0.6392157,0.4745098,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":429,"pos":[-2864,-2032],"params":["Inherit","False","1092","325","","6","571","470","444","442","440","660","Normals","0.6382856,0.4745098,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":432,"pos":[-2864,-1264],"params":["Inherit","False","2236.144","877.4296","Direct","25","602","601","600","599","598","597","596","581","569","568","567","565","564","563","562","561","560","559","558","557","669","672","673","677","678","","1,0,0.390008,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":434,"pos":[-2864,-368],"params":["Inherit","False","2236.144","596.6401","Indirect","15","582","579","556","474","447","446","445","655","570","670","671","674","675","676","691","","1,0,0.390008,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":436,"pos":[-2864,-2432],"params":["Inherit","False","816","304","","5","542","541","540","539","538","Half Lambert","0.8078432,0.7294118,0,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":437,"pos":[-2864,-1600],"params":["Inherit","False","1044","323","","4","580","471","448","575","Specular Highlights","1,0,0.3882353,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":430,"pos":[-2800,2464],"params":["Inherit","False","452","394.7998","Fresnel","3","508","503","502","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":431,"pos":[-2800,1344],"params":["Inherit","False","784","363.95","Half Vector","7","526","525","523","522","521","520","480","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":433,"pos":[-2800,2016],"params":["Inherit","False","452","394.7998","NdotL","3","512","501","500","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":435,"pos":[-2128,2496],"params":["Inherit","False","228","162.9502","InvertedViewMask","1","510","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":647,"pos":[-2800,736],"params":["Inherit","False","799.4725","322.7755","Time+Offset","7","654","653","652","651","650","649","648","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":619,"pos":[2528,-1280],"params":["Inherit","False","254","VertexOffset","1","0","OBJECT","","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":614,"pos":[1328,-912],"params":["Inherit","False","Alpha","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":345,"pos":[1264,1872],"params":["Inherit","False","Property","_DistortionScale","Distortion Scale","21","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","0.8","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor","id":344,"pos":[1568,1616],"params":["Inherit","False","True","1","0","FLOAT3","0,0,1","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":371,"pos":[1568,1792],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","2","False","4","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldSpaceLightDirHlpNode, AmplifyShaderEditor","id":342,"pos":[1312,1360],"params":["Inherit","False","False","1","0","FLOAT","0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":377,"pos":[1824,1648],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":343,"pos":[2000,1600],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.NegateNode, AmplifyShaderEditor","id":347,"pos":[2144,1600],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ViewDirInputsCoordNode, AmplifyShaderEditor","id":348,"pos":[2000,1424],"params":["Inherit","False","World","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":369,"pos":[1856,1840],"params":["Inherit","False","Property","_DistortionAmount","Distortion Amount","22","0","Create","True","0","0","0","False","0","False","Object","-1","","0","1","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":349,"pos":[2336,1536],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":376,"pos":[2240,1744],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","0","False","4","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":368,"pos":[2496,1584],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor","id":328,"pos":[-192,368],"params":["Inherit","False","0","5","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.LightAttenuation, AmplifyShaderEditor","id":329,"pos":[-192,560],"params":["Inherit","False","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":331,"pos":[256,528],"params":["Inherit","False","3","3","0","COLOR","1,1,1,0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":355,"pos":[2800,1488],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":334,"pos":[224,448],"params":["Inherit","False","611","FinalLighting","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":439,"pos":[-2624,-2752],"params":["Inherit","False","NormalMap","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":440,"pos":[-2768,-1984],"params":["Inherit","False","439","NormalMap","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":441,"pos":[-2624,-2960],"params":["Inherit","False","TextureRamp","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":446,"pos":[-1600,64],"params":["Float","False","Property","_IndirectSpecularContribution","Indirect Specular Contribution","18","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":447,"pos":[-1216,-80],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":457,"pos":[-2624,-3376],"params":["Inherit","False","Emission Map","-1","True","1","0","SAMPLER2D","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":461,"pos":[-2000,-3376],"params":["Inherit","False","EmissionColor","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":462,"pos":[-2000,-3264],"params":["Inherit","False","EmissionAlpha","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":470,"pos":[-2464,-1936],"params":["Inherit","True","Property","_Normal","Normal","2","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","True","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":473,"pos":[-480,-2368],"params":["Inherit","False","CEL Effect","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor","id":475,"pos":[-1312,-1984],"params":["Inherit","False","True","1","0","FLOAT3","0,0,1","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":476,"pos":[-1312,-1824],"params":["Inherit","False","594","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":477,"pos":[-1408,-1744],"params":["Inherit","False","Property","_NormalMapInfluence","Normal Map Influence","36","0","Create","True","0","0","0","False","1","Space (8)","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":478,"pos":[-1056,-1792],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":479,"pos":[-816,-1792],"params":["Inherit","False","NormalLerp","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":530,"pos":[-2000,-3056],"params":["Inherit","False","RimLightAlpha","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldSpaceLightDirHlpNode, AmplifyShaderEditor","id":532,"pos":[-1664,-2288],"params":["Inherit","False","True","1","0","FLOAT","0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":533,"pos":[-1392,-2368],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ScaleAndOffsetNode, AmplifyShaderEditor","id":534,"pos":[-976,-2368],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0.5","False","2","FLOAT","0.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":535,"pos":[-720,-2368],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":536,"pos":[-1280,-2224],"params":["Inherit","False","552","RampOffset","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":537,"pos":[-1280,-2304],"params":["Inherit","False","553","RampScale","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LightColorNode, AmplifyShaderEditor","id":538,"pos":[-2816,-2384],"params":["Inherit","False","0","3","COLOR","0","FLOAT3","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.LightAttenuation, AmplifyShaderEditor","id":539,"pos":[-2816,-2240],"params":["Inherit","False","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.IndirectDiffuseLighting, AmplifyShaderEditor","id":540,"pos":[-2480,-2240],"params":["Inherit","False","Tangent","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":541,"pos":[-2480,-2384],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":542,"pos":[-2208,-2384],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT3","0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":543,"pos":[-1984,-2384],"params":["Inherit","False","HalfLambert","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":552,"pos":[-2000,-2864],"params":["Inherit","False","RampOffset","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":553,"pos":[-2000,-2960],"params":["Inherit","False","RampScale","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":554,"pos":[-2320,-2960],"params":["Inherit","False","Property","_RampScale","Ramp Scale","3","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":555,"pos":[-2320,-2864],"params":["Inherit","False","Property","_RampOffset","Ramp Offset","4","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":556,"pos":[-1552,-144],"params":["Float","False","Constant","_Float5","Float 5","20","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":558,"pos":[-1792,-1008],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":560,"pos":[-1600,-864],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":561,"pos":[-1984,-1104],"params":["Inherit","False","3","3","0","FLOAT3","0,0,0","False","1","FLOAT","1","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":562,"pos":[-1408,-864],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT3","0.05,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor","id":563,"pos":[-1600,-1104],"params":["Inherit","False","FLOAT3","0","1","2","3","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":571,"pos":[-2816,-1872],"params":["Inherit","False","Property","_NormalScale","Normal Scale","8","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","5","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":572,"pos":[-2864,-3376],"params":["Inherit","True","Property","_EmissionMap","Emission Map","9","2","[NoScaleOffset]","[SingleLineTexture]","Create","True","0","0","0","False","1","Space (8)","False","","None","None","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":574,"pos":[-544,-960],"params":["Inherit","False","DirectSpecHighlights","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":576,"pos":[-2864,-2752],"params":["Inherit","True","Property","_NormalMap","Normal Map","7","3","[NoScaleOffset]","[Normal]","[SingleLineTexture]","Create","True","0","0","0","False","1","Space (8)","False","","None","None","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor","id":583,"pos":[-2864,-2960],"params":["Inherit","True","Property","_TextureRamp","Texture Ramp","2","3","[Header]","[NoScaleOffset]","[SingleLineTexture]","Create","True","1","Textures","0","0","False","1","Space (8)","False","","None","None","False","white","Auto","Texture2D","False","-1","0","2","SAMPLER2D","0","SAMPLERSTATE","1"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":592,"pos":[-2320,-3376],"params":["Inherit","False","Property","_EmissionColor","Emission Color","24","1","[HDR]","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":595,"pos":[-1632,-2384],"params":["Inherit","False","594","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":599,"pos":[-2672,-992],"params":["Inherit","False","457","Emission Map","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":600,"pos":[-2432,-992],"params":["Inherit","True","Property","_TextureSample1","Texture Sample 1","10","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":480,"pos":[-2752,1456],"params":["Inherit","False","479","NormalLerp","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor","id":481,"pos":[-1088,1600],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.WorldSpaceCameraPos, AmplifyShaderEditor","id":482,"pos":[-1088,1792],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.DistanceOpNode, AmplifyShaderEditor","id":483,"pos":[-832,1600],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":484,"pos":[-608,1744],"params":["Inherit","False","443","MainTexture","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":485,"pos":[-416,1600],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":486,"pos":[-400,1744],"params":["Inherit","True","Property","_TextureSample5","Texture Sample 2","10","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":487,"pos":[16,1744],"params":["Inherit","False","Property","_SpecularInfluence","Specular Influence","37","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":488,"pos":[-1088,1360],"params":["Inherit","False","5","5","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","2","FLOAT3","0,0,0","False","3","FLOAT","0","False","4","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":489,"pos":[-96,1360],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":490,"pos":[144,1456],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":491,"pos":[384,1360],"params":["Inherit","False","3","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","2","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":492,"pos":[-1344,1360],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":493,"pos":[-1664,1456],"params":["Inherit","False","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":494,"pos":[-2800,1760],"params":["Inherit","False","479","NormalLerp","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":495,"pos":[-2576,1760],"params":["Inherit","False","SRP Additional Light","-1","","61172","6c86746ad131a0a408ca599df5f40861","3,6,1,351,1,23,0","6","2","FLOAT3","0,0,0","False","11","FLOAT3","0,0,0","False","345","FLOAT3","0,0,0","False","346","FLOAT3","0,0,0","False","347","FLOAT","0.5","False","32","FLOAT4","0,0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":496,"pos":[-2128,1760],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":497,"pos":[-2304,1760],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":498,"pos":[-1984,1584],"params":["Inherit","False","Property","_RimIntensity1","Rim Intensity","34","0","Create","True","0","0","0","False","0","False","Object","-1","","0.2","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":499,"pos":[-1664,1584],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","0","False","4","FLOAT","1000","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldSpaceLightDirHlpNode, AmplifyShaderEditor","id":500,"pos":[-2752,2096],"params":["Inherit","False","True","1","0","FLOAT","0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":501,"pos":[-2752,2272],"params":["Inherit","False","479","NormalLerp","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ViewDirInputsCoordNode, AmplifyShaderEditor","id":502,"pos":[-2752,2688],"params":["Inherit","False","World","True","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":503,"pos":[-2736,2544],"params":["Inherit","False","479","NormalLerp","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LightColorNode, AmplifyShaderEditor","id":504,"pos":[-2576,1872],"params":["Inherit","False","0","3","COLOR","0","FLOAT3","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":505,"pos":[-2304,2784],"params":["Inherit","False","Property","_ViewEdgeThreshold","ViewEdgeThreshold","32","0","Create","True","0","0","0","False","1","Space (8)","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":506,"pos":[-1760,2784],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":507,"pos":[-2016,2784],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","0.4","False","4","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":508,"pos":[-2496,2544],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":509,"pos":[-2304,2544],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":510,"pos":[-2080,2544],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor","id":511,"pos":[-1568,2544],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":512,"pos":[-2496,2176],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":513,"pos":[-2304,2176],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":514,"pos":[-1296,2176],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":515,"pos":[-1568,2176],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor","id":516,"pos":[-608,1600],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LightAttenuation, AmplifyShaderEditor","id":517,"pos":[-1440,1584],"params":["Inherit","False","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.IndirectDiffuseLighting, AmplifyShaderEditor","id":518,"pos":[-1408,1680],"params":["Inherit","False","Tangent","1","0","FLOAT3","0,0,1","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":519,"pos":[-1904,1392],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":520,"pos":[-2752,1616],"params":["Inherit","False","Property","_RimSpread","Rim Spread","35","0","Create","True","0","0","0","False","0","False","Object","-1","","0.2","0.2","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":521,"pos":[-2160,1392],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":522,"pos":[-2448,1536],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0.2","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":523,"pos":[-2288,1456],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":524,"pos":[-832,1808],"params":["Inherit","False","Property","_DistanceFade1","Distance Fade","38","0","Create","True","0","0","0","False","1","Space (8)","False","Object","-1","","12","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":525,"pos":[-2752,1536],"params":["Inherit","False","Blinn-Phong Half Vector","-1","","61174","91a149ac9d615be429126c95e20753ce","0","0","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":526,"pos":[-2528,1392],"params":["Inherit","False","Constant","_Float0","Float 0","27","0","Create","True","0","0","0","False","0","False","Object","-1","","-0.2","-0.1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":527,"pos":[-2112,2960],"params":["Inherit","False","Property","_ViewEdgeSoftness","ViewEdgeSoftness","33","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":528,"pos":[-1664,1360],"params":["Inherit","False","529","RimLightColor","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor","id":603,"pos":[368,1728],"params":["Inherit","False","0","5","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":452,"pos":[1296,-1760],"params":["Inherit","False","Constant","_DefaultTint","Default Tint","19","0","Create","True","0","0","0","False","0","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":338,"pos":[224,368],"params":["Inherit","False","609","HighlightsColor","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":529,"pos":[-2000,-3168],"params":["Inherit","False","RimLightColor","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":593,"pos":[-2320,-3168],"params":["Inherit","False","Property","_RimLightColor","Rim Light Color","31","1","[HDR]","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":609,"pos":[-2000,-2752],"params":["Inherit","False","HighlightsColor","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":591,"pos":[1296,-1536],"params":["Inherit","False","Property","_BaseTint","Base Tint","1","0","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":455,"pos":[1584,-1760],"params":["Inherit","False","578","SpecularHighlights","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":617,"pos":[1584,-1888],"params":["Inherit","False","359","SubsurfaceDistortion","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":618,"pos":[1872,-1888],"params":["Inherit","False","Property","_EnableSubsurfaceDistortion","Enable Subsurface Distortion","19","0","Create","True","0","0","0","False","2","Header(Subsurface Distortion)","Space(8)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","COLOR","0,0,0,0","False","0","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","3","COLOR","0,0,0,0","False","4","COLOR","0,0,0,0","False","5","COLOR","0,0,0,0","False","6","COLOR","0,0,0,0","False","7","COLOR","0,0,0,0","False","8","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":367,"pos":[2432,1328],"params":["Inherit","False","Property","_SubsurfaceTint","Subsurface Tint","20","0","Create","True","0","0","0","False","1","Space (8)","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":610,"pos":[-2320,-2752],"params":["Inherit","False","Property","_TopHighlightsColor","Top Highlights Color","42","1","[HDR]","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ToggleSwitchNode, AmplifyShaderEditor","id":333,"pos":[768,448],"params":["Inherit","False","Property","_EnableTopHighlights","Enable Top Highlights","41","0","Create","True","0","0","0","False","2","Header(Surface Options)","Space(8)","False","","1","True","Create","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":327,"pos":[-16,368],"params":["Inherit","False","244","WindSway","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":467,"pos":[-496,368],"params":["Inherit","False","Emission","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":622,"pos":[-2016,368],"params":["Inherit","False","4","4","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":623,"pos":[-2800,448],"params":["Inherit","False","461","EmissionColor","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":624,"pos":[-2800,528],"params":["Inherit","False","457","Emission Map","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":625,"pos":[-2576,528],"params":["Inherit","True","Property","_TextureSample4","Texture Sample 1","18","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":626,"pos":[-2288,640],"params":["Inherit","False","462","EmissionAlpha","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":628,"pos":[-1824,640],"params":["Inherit","False","Constant","_ReCenter2","Re-Center","30","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":629,"pos":[-2800,368],"params":["Inherit","False","Property","_EmissionIntensity1","Emission Intensity","25","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":630,"pos":[-1072,368],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":631,"pos":[-1968,768],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":632,"pos":[-1504,848],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":633,"pos":[-1792,928],"params":["Inherit","False","Property","_MinIntensity","Min Intensity","28","0","Create","True","0","0","0","False","0","False","Object","-1","","0.75","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":634,"pos":[-1792,848],"params":["Inherit","False","Property","_MaxIntensity","Max Intensity","29","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":635,"pos":[-1504,960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":636,"pos":[-1504,1072],"params":["Inherit","False","Constant","_Half","Half","30","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":637,"pos":[-1344,848],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":638,"pos":[-1344,960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":639,"pos":[-1152,768],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":640,"pos":[-960,768],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":589,"pos":[-848,368],"params":["Inherit","False","Property","_EnableEmission","Enable Emission","23","0","Create","True","0","0","0","False","2","Header (Emission)","Space(8)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleTimeNode, AmplifyShaderEditor","id":648,"pos":[-2528,784],"params":["Inherit","False","1","0","FLOAT","1","False","5","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":649,"pos":[-2320,784],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":650,"pos":[-2528,864],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor","id":651,"pos":[-2752,864],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":652,"pos":[-2752,784],"params":["Inherit","False","Property","_FlickerFrequency","Flicker Frequency","26","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":653,"pos":[-2528,976],"params":["Inherit","False","Property","_FlickerScale","Flicker Scale","27","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":654,"pos":[-2160,784],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.IndirectSpecularLight, AmplifyShaderEditor","id":582,"pos":[-1600,-64],"params":["Inherit","False","World","3","0","FLOAT3","0,0,0","False","1","FLOAT","0.5","False","2","FLOAT","1","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LightColorNode, AmplifyShaderEditor","id":379,"pos":[2496,1744],"params":["Inherit","False","0","3","COLOR","0","FLOAT3","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":354,"pos":[2624,1584],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":357,"pos":[2992,1488],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor","id":358,"pos":[2912,1696],"params":["Inherit","False","0","5","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":365,"pos":[3216,1488],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":656,"pos":[3440,1488],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.LightAttenuation, AmplifyShaderEditor","id":378,"pos":[3200,1696],"params":["Inherit","False","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":359,"pos":[3664,1488],"params":["Inherit","False","SubsurfaceDistortion","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":588,"pos":[2848,-1632],"params":["Inherit","False","467","Emission","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":616,"pos":[2752,-1472],"params":["Inherit","False","Property","_AlphaClipThreshold","Alpha Clip Threshold","6","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":615,"pos":[2848,-1552],"params":["Inherit","False","614","Alpha","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":620,"pos":[2784,-1280],"params":["Inherit","False","Property","_EnableWind","Enable Wind","39","0","Create","True","0","0","0","False","2","Header(Wind)","Space(8)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT4","0,0,0,0","False","0","FLOAT4","0,0,0,0","False","2","FLOAT4","0,0,0,0","False","3","FLOAT4","0,0,0,0","False","4","FLOAT4","0,0,0,0","False","5","FLOAT4","0,0,0,0","False","6","FLOAT4","0,0,0,0","False","7","FLOAT4","0,0,0,0","False","8","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":586,"pos":[1872,-1504],"params":["Inherit","False","Property","_EnableColorTint","Enable Color Tint","0","0","Create","True","0","0","0","False","2","Header(Color)","Space(8)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","COLOR","0,0,0,0","False","0","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","3","COLOR","0,0,0,0","False","4","COLOR","0,0,0,0","False","5","COLOR","0,0,0,0","False","6","COLOR","0,0,0,0","False","7","COLOR","0,0,0,0","False","8","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":658,"pos":[2768,-1888],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":460,"pos":[2608,-1760],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":456,"pos":[2256,-1504],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":449,"pos":[832,-1120],"params":["Inherit","True","Property","_TextureRamp1","Texture Ramp 1","0","1","[Header]","Create","True","1","Textures","0","0","False","1","Space(8)","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":450,"pos":[1344,-1312],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":451,"pos":[1312,-1184],"params":["Inherit","False","543","HalfLambert","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":453,"pos":[1536,-1312],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":458,"pos":[1344,-1088],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":472,"pos":[576,-1120],"params":["Inherit","False","441","TextureRamp","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":544,"pos":[576,-1040],"params":["Inherit","False","473","CEL Effect","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":548,"pos":[832,-928],"params":["Inherit","True","Property","_TextureRamp2","Texture Ramp 1","0","1","[Header]","Create","True","1","Textures","0","0","False","1","Space(8)","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":612,"pos":[1760,-1136],"params":["Inherit","False","335","FinalLightingHighlights","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":459,"pos":[2096,-1312],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":454,"pos":[1648,-2128],"params":["Inherit","False","605","RimLight","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":587,"pos":[1872,-2128],"params":["Inherit","False","Property","_EnableRimLighting","Enable Rim Lighting","30","0","Create","True","0","0","0","False","2","Header (Rim Lighting)","Space(8)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","COLOR","0,0,0,0","False","0","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","3","COLOR","0,0,0,0","False","4","COLOR","0,0,0,0","False","5","COLOR","0,0,0,0","False","6","COLOR","0,0,0,0","False","7","COLOR","0,0,0,0","False","8","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor","id":442,"pos":[-2144,-1872],"params":["Inherit","False","True","1","0","FLOAT3","0,0,1","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.NormalizeNode, AmplifyShaderEditor","id":444,"pos":[-1952,-1872],"params":["Inherit","False","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":594,"pos":[-1712,-1872],"params":["Inherit","False","Normals","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":660,"pos":[-2144,-1984],"params":["Inherit","False","NormalInput","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":661,"pos":[2848,-1984],"params":["Inherit","False","660","NormalInput","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":662,"pos":[2752,-1792],"params":["Inherit","False","Property","_Smoothness","Smoothness","12","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":590,"pos":[2752,-1712],"params":["Inherit","False","Property","_Occlusion","Occlusion","43","1","[Header]","Create","True","0","0","0","False","1","Space (8)","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":663,"pos":[2880,-1136],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":669,"pos":[-1984,-1200],"params":["Inherit","False","SpecularTint","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":581,"pos":[-1968,-512],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":568,"pos":[-2672,-496],"params":["Inherit","False","594","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ViewDirInputsCoordNode, AmplifyShaderEditor","id":671,"pos":[-2672,-320],"params":["Inherit","False","World","True","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":670,"pos":[-2672,-160],"params":["Inherit","False","669","SpecularTint","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":672,"pos":[-1792,-800],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":598,"pos":[-2064,-800],"params":["Float","False","Property","_SecondarySmoothness","Secondary Smoothness","16","0","Create","True","0","0","0","False","0","False","Object","-1","","0.01","0.04","0.001","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":673,"pos":[-1600,-528],"params":["Inherit","False","SpecularSmoothness","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":445,"pos":[-1312,-320],"params":["Inherit","True","Property","_TextureSample2","Texture Sample 2","10","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":579,"pos":[-1568,-320],"params":["Inherit","False","457","Emission Map","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":674,"pos":[-2672,-48],"params":["Inherit","False","673","SpecularSmoothness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":474,"pos":[-1952,-64],"params":["Inherit","False","594","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":675,"pos":[-2016,32],"params":["Inherit","False","673","SpecularSmoothness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":655,"pos":[-2048,128],"params":["Inherit","False","Property","_SpecularOcclusion","Specular Occlusion","17","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","12","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":676,"pos":[-944,-160],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":531,"pos":[-544,-160],"params":["Inherit","False","IndirectSpecHighlights","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":570,"pos":[-2352,-304],"params":["Inherit","False","SRP Additional Light","-1","","61175","6c86746ad131a0a408ca599df5f40861","3,6,2,351,1,23,0","6","2","FLOAT3","0,0,0","False","11","FLOAT3","0,0,0","False","345","FLOAT3","0,0,0","False","346","FLOAT3","0,0,0","False","347","FLOAT","0.5","False","32","FLOAT4","0,0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":596,"pos":[-2000,-720],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","-1","False","4","FLOAT","-0.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":567,"pos":[-1792,-720],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":557,"pos":[-2752,-608],"params":["Inherit","False","594","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":559,"pos":[-2480,-688],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":569,"pos":[-2784,-688],"params":["Inherit","False","Blinn-Phong Half Vector","-1","","61177","91a149ac9d615be429126c95e20753ce","0","0","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":677,"pos":[-1600,-720],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":564,"pos":[-1232,-864],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":678,"pos":[-1056,-864],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":565,"pos":[-848,-960],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":448,"pos":[-2816,-1456],"params":["Inherit","False","543","HalfLambert","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":580,"pos":[-2432,-1488],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":471,"pos":[-2816,-1536],"params":["Inherit","False","574","DirectSpecHighlights","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":575,"pos":[-2816,-1376],"params":["Inherit","False","531","IndirectSpecHighlights","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":578,"pos":[-1760,-1488],"params":["Inherit","False","SpecularHighlights","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":666,"pos":[2560,-960],"params":["Inherit","True","Property","_MainTexture2","Main Texture 1","0","0","Create","True","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Instance","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":667,"pos":[2352,-960],"params":["Inherit","False","457","Emission Map","1","0","OBJECT","","False","1","SAMPLER2D","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":665,"pos":[2624,-1040],"params":["Inherit","False","669","SpecularTint","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":664,"pos":[2560,-1136],"params":["Inherit","False","Property","_SpecularIntensity1","Specular Intensity","11","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":573,"pos":[1872,-1760],"params":["Inherit","False","Property","_EnableSecondaryHighlights","Enable Secondary Highlights","13","0","Create","True","0","0","0","False","1","Space(8)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","COLOR","0,0,0,0","False","0","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","3","COLOR","0,0,0,0","False","4","COLOR","0,0,0,0","False","5","COLOR","0,0,0,0","False","6","COLOR","0,0,0,0","False","7","COLOR","0,0,0,0","False","8","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":601,"pos":[-2368,-1200],"params":["Float","False","Property","_SpecularColor","Specular Color","10","1","[Header]","Create","True","1","Specular Highlights","0","0","False","1","Space(8)","False","Object","-1","","1,1,1,1","0.5773503,0.5773503,0.5773503,1","False","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":602,"pos":[-2368,-800],"params":["Inherit","False","Property","_SecondarySpecularIntensity","Secondary Specular Intensity","14","0","Create","True","0","0","0","False","1","Space(8)","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":597,"pos":[-2352,-720],"params":["Float","False","Property","_SecondarySpecularSize","Secondary Specular Size","15","0","Create","True","0","0","0","False","0","False","Object","-1","","0","-0.95","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":679,"pos":[-336,-640],"params":["Inherit","False","Property","_AdditionalLightInfluence","Additional Light Influence","44","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","15","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":682,"pos":[-48,-640],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","0.001","False","4","FLOAT","6","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":683,"pos":[336,-544],"params":["Inherit","False","552","RampOffset","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":684,"pos":[400,-848],"params":["Inherit","False","True","2","0","FLOAT3","0,0,0","False","1","FLOAT","1","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ScaleAndOffsetNode, AmplifyShaderEditor","id":685,"pos":[576,-800],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT","1","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor","id":686,"pos":[144,-720],"params":["Inherit","False","5","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","12","False","3","FLOAT","0.001","False","4","FLOAT","12","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":689,"pos":[336,-624],"params":["Inherit","False","553","RampScale","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":680,"pos":[208,-848],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":688,"pos":[-48,-848],"params":["Inherit","False","SRP Additional Light","-1","","61181","6c86746ad131a0a408ca599df5f40861","3,6,1,351,1,23,0","6","2","FLOAT3","0,0,0","False","11","FLOAT3","0,0,0","False","345","FLOAT3","0,0,0","False","346","FLOAT3","0,0,0","False","347","FLOAT","0.5","False","32","FLOAT4","0,0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":687,"pos":[-272,-896],"params":["Inherit","False","594","Normals","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":691,"pos":[-2672,32],"params":["Inherit","False","Shadow Mask","-1","","61190","b50f5becdd6b8504a861ba5b9b861159","0","1","3","FLOAT2","0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":690,"pos":[-336,-824],"params":["Inherit","False","Shadow Mask","-1","","61192","b50f5becdd6b8504a861ba5b9b861159","0","1","3","FLOAT2","0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":611,"pos":[1760,-1264],"params":["Inherit","False","FinalLighting","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.BlendOpsNode, AmplifyShaderEditor","id":332,"pos":[512,512],"params":["Inherit","False","HardLight","True","3","0","FLOAT3","0,0,0","False","1","COLOR","0,0,0,0","False","2","FLOAT","1","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":604,"pos":[784,1648],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":605,"pos":[1008,1648],"params":["Inherit","False","RimLight","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":335,"pos":[1056,448],"params":["Inherit","False","FinalLightingHighlights","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":681,"pos":[-336,-720],"params":["Inherit","False","Property","_AdditionalLightFalloff","Additional Light Falloff","45","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","12","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":414,"pos":[2784,-1760],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","1","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","ExtraPrePass","0","0","ExtraPrePass","6","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","0","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","0","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":416,"pos":[2784,-1760],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","1","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","ShadowCaster","0","2","ShadowCaster","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","False","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=ShadowCaster","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":417,"pos":[2784,-1760],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","1","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","DepthOnly","0","3","DepthOnly","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","True","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","False","False","False","True","1","LightMode=DepthOnly","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":418,"pos":[2784,-1760],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","1","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","Meta","0","4","Meta","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Meta","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":419,"pos":[2784,-1760],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","1","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","Universal2D","0","5","Universal2D","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=Universal2D","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":420,"pos":[2784,-1760],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","1","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","DepthNormals","0","6","DepthNormals","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","0","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=DepthNormals","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":421,"pos":[2784,-1760],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","1","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","GBuffer","0","7","GBuffer","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=UniversalGBuffer","False","True","12","d3d11","gles","metal","vulkan","xboxone","xboxseries","playstation","ps4","ps5","switch","switch2","webgpu","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":422,"pos":[2784,-1760],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","1","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","SceneSelectionPass","0","8","SceneSelectionPass","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=SceneSelectionPass","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":423,"pos":[2784,-1760],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","1","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","ScenePickingPass","0","9","ScenePickingPass","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Picking","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":424,"pos":[2784,-1760],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","1","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","MotionVectors","0","10","MotionVectors","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","False","False","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=MotionVectors","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":425,"pos":[2784,-1760],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","1","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","XRMotionVectors","0","11","XRMotionVectors","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","True","1","False","","255","False","","1","False","","7","False","","3","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","False","False","False","False","True","1","LightMode=XRMotionVectors","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":415,"pos":[3200,-1888],"params":["Float","False","True","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","ToonScapes/URP/Vegetation","94348b07e5e8bab40bd6c8a1e3df54cd","True","Forward","0","1","Forward","22","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=TransparentCutout=RenderType","Queue=AlphaTest=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=UniversalForward","False","False","0","","0","0","Standard","52","Category","0","0","  Instanced Terrain Normals","1","0","Lighting Model","0","639179993577154035","Workflow","0","0","Surface","0","639047939055015101","  Keep Alpha","0","639047938997806218","  Refraction Model","0","0","  Blend","0","0","Two Sided","1","0","Alpha Clipping","1","0","  Use Shadow Threshold","0","0","Fragment Normal Space","0","0","Forward Only","0","0","Transmission","0","0","  Transmission Shadow","0.5,False,","0","Translucency","0","0","  Translucency Strength","1,False,","0","  Normal Distortion","0.5,False,","0","  Scattering","2,False,","0","  Direct","0.9,False,","0","  Ambient","0.1,False,","0","  Shadow","0.5,False,","0","Cast Shadows","1","0","Receive Shadows","2","0","Specular Highlights","1","639180017487783039","Environment Reflections","2","0","Receive SSAO","1","0","Motion Vectors","1","0","  Additional Motion Vectors","1","0","  Alembic Motion Vectors","0","0","  XR Motion Vectors","0","0","GPU Instancing","1","0","LOD CrossFade","1","0","Built-in Fog","1","0","_FinalColorxAlpha","0","0","Meta Pass","1","0","Override Baked GI","0","0","Extra Pre Pass","0","0","Tessellation","0","0","  Phong","0","0","  Strength","0.5,False,","0","  Type","0","0","  Tess","16,False,","0","  Min","10,False,","0","  Max","25,False,","0","  Edge Length","16,False,","0","  Max Displacement","25,False,","0","Write Depth","0","0","  Conservative","0","0","Vertex Position","1","0","Debug Display","1","0","Clear Coat","0","0","0","12","False","True","True","True","True","True","True","True","True","True","True","False","False","","False","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":606,"pos":[-1568,2048],"params":["Inherit","False","128","100","RimFromLight","0","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":607,"pos":[-1568,2416],"params":["Inherit","False","159","100","ViewEdge","0","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":608,"pos":[-1296,2048],"params":["Inherit","False","128","100","RimMask","0","","1,1,1,1","0","0"]}
{"wire":[410,0,274,0]}
{"wire":[270,0,267,0]}
{"wire":[273,0,268,0]}
{"wire":[269,0,266,0]}
{"wire":[276,0,270,0]}
{"wire":[281,0,412,0]}
{"wire":[279,1,273,0]}
{"wire":[280,0,272,1]}
{"wire":[280,1,272,3]}
{"wire":[278,0,271,0]}
{"wire":[275,0,269,0]}
{"wire":[283,0,276,0]}
{"wire":[283,1,281,0]}
{"wire":[284,0,277,1]}
{"wire":[284,1,277,3]}
{"wire":[285,0,280,0]}
{"wire":[285,1,279,0]}
{"wire":[282,0,275,0]}
{"wire":[282,1,278,0]}
{"wire":[286,0,284,0]}
{"wire":[286,2,283,0]}
{"wire":[263,0,262,0]}
{"wire":[287,0,285,0]}
{"wire":[287,2,282,0]}
{"wire":[291,0,286,0]}
{"wire":[397,0,292,0]}
{"wire":[397,1,398,0]}
{"wire":[403,0,293,0]}
{"wire":[403,1,406,0]}
{"wire":[288,0,287,0]}
{"wire":[293,0,290,0]}
{"wire":[293,1,291,0]}
{"wire":[399,0,397,0]}
{"wire":[399,1,400,0]}
{"wire":[404,0,403,0]}
{"wire":[404,1,407,0]}
{"wire":[292,0,289,0]}
{"wire":[292,1,288,0]}
{"wire":[401,0,292,0]}
{"wire":[401,1,399,0]}
{"wire":[401,2,402,0]}
{"wire":[405,0,293,0]}
{"wire":[405,1,404,0]}
{"wire":[405,2,402,0]}
{"wire":[296,0,405,0]}
{"wire":[244,0,401,0]}
{"wire":[302,0,298,0]}
{"wire":[255,0,256,0]}
{"wire":[304,0,299,0]}
{"wire":[304,1,300,1]}
{"wire":[304,2,413,0]}
{"wire":[305,0,300,2]}
{"wire":[305,1,301,0]}
{"wire":[305,2,302,0]}
{"wire":[307,0,304,0]}
{"wire":[307,1,305,0]}
{"wire":[308,0,306,0]}
{"wire":[443,0,577,0]}
{"wire":[309,0,308,0]}
{"wire":[309,1,307,0]}
{"wire":[310,0,309,0]}
{"wire":[254,0,310,0]}
{"wire":[468,0,469,0]}
{"wire":[614,0,468,4]}
{"wire":[371,0,345,0]}
{"wire":[377,0,344,0]}
{"wire":[377,1,371,0]}
{"wire":[343,0,342,0]}
{"wire":[343,1,377,0]}
{"wire":[347,0,343,0]}
{"wire":[349,0,348,0]}
{"wire":[349,1,347,0]}
{"wire":[376,0,369,0]}
{"wire":[368,0,349,0]}
{"wire":[368,1,376,0]}
{"wire":[331,0,327,0]}
{"wire":[331,1,328,2]}
{"wire":[331,2,329,0]}
{"wire":[355,0,367,0]}
{"wire":[355,1,354,0]}
{"wire":[439,0,576,0]}
{"wire":[441,0,583,0]}
{"wire":[447,0,556,0]}
{"wire":[447,1,582,0]}
{"wire":[447,2,446,0]}
{"wire":[457,0,572,0]}
{"wire":[461,0,592,5]}
{"wire":[462,0,592,4]}
{"wire":[470,0,440,0]}
{"wire":[470,5,571,0]}
{"wire":[473,0,535,0]}
{"wire":[478,0,475,0]}
{"wire":[478,1,476,0]}
{"wire":[478,2,477,0]}
{"wire":[479,0,478,0]}
{"wire":[530,0,593,4]}
{"wire":[533,0,595,0]}
{"wire":[533,1,532,0]}
{"wire":[534,0,533,0]}
{"wire":[534,1,537,0]}
{"wire":[534,2,536,0]}
{"wire":[535,0,534,0]}
{"wire":[541,0,538,0]}
{"wire":[541,1,539,0]}
{"wire":[542,0,541,0]}
{"wire":[542,1,540,0]}
{"wire":[543,0,542,0]}
{"wire":[552,0,555,0]}
{"wire":[553,0,554,0]}
{"wire":[558,0,561,0]}
{"wire":[560,0,558,0]}
{"wire":[560,1,672,0]}
{"wire":[561,0,601,5]}
{"wire":[561,1,600,4]}
{"wire":[561,2,602,0]}
{"wire":[562,0,677,0]}
{"wire":[562,1,560,0]}
{"wire":[563,0,561,0]}
{"wire":[574,0,565,0]}
{"wire":[600,0,599,0]}
{"wire":[483,0,481,0]}
{"wire":[483,1,482,0]}
{"wire":[485,0,516,0]}
{"wire":[486,0,484,0]}
{"wire":[488,0,492,0]}
{"wire":[488,1,517,0]}
{"wire":[488,2,518,0]}
{"wire":[488,3,514,0]}
{"wire":[488,4,493,0]}
{"wire":[489,0,488,0]}
{"wire":[489,1,485,0]}
{"wire":[490,0,489,0]}
{"wire":[490,1,486,4]}
{"wire":[491,0,489,0]}
{"wire":[491,1,490,0]}
{"wire":[491,2,487,0]}
{"wire":[492,0,528,0]}
{"wire":[492,1,499,0]}
{"wire":[493,0,519,0]}
{"wire":[495,11,494,0]}
{"wire":[496,0,497,0]}
{"wire":[496,1,504,0]}
{"wire":[497,0,495,0]}
{"wire":[499,0,498,0]}
{"wire":[506,0,507,0]}
{"wire":[506,1,527,0]}
{"wire":[507,0,505,0]}
{"wire":[508,0,503,0]}
{"wire":[508,1,502,0]}
{"wire":[509,0,508,0]}
{"wire":[510,0,509,0]}
{"wire":[511,0,510,0]}
{"wire":[511,1,507,0]}
{"wire":[511,2,506,0]}
{"wire":[512,0,500,0]}
{"wire":[512,1,501,0]}
{"wire":[513,0,512,0]}
{"wire":[514,0,515,0]}
{"wire":[514,1,511,0]}
{"wire":[515,0,513,0]}
{"wire":[516,0,483,0]}
{"wire":[516,2,524,0]}
{"wire":[519,0,521,0]}
{"wire":[519,1,496,0]}
{"wire":[521,0,526,0]}
{"wire":[521,1,523,0]}
{"wire":[522,0,525,0]}
{"wire":[522,1,520,0]}
{"wire":[523,0,480,0]}
{"wire":[523,1,522,0]}
{"wire":[529,0,593,5]}
{"wire":[609,0,610,5]}
{"wire":[618,0,617,0]}
{"wire":[333,0,334,0]}
{"wire":[333,1,332,0]}
{"wire":[467,0,589,0]}
{"wire":[622,0,629,0]}
{"wire":[622,1,623,0]}
{"wire":[622,2,625,5]}
{"wire":[622,3,626,0]}
{"wire":[625,0,624,0]}
{"wire":[630,0,622,0]}
{"wire":[630,1,640,0]}
{"wire":[631,0,654,0]}
{"wire":[632,0,634,0]}
{"wire":[632,1,633,0]}
{"wire":[635,0,634,0]}
{"wire":[635,1,633,0]}
{"wire":[637,0,632,0]}
{"wire":[637,1,636,0]}
{"wire":[638,0,635,0]}
{"wire":[638,1,636,0]}
{"wire":[639,0,631,0]}
{"wire":[639,1,637,0]}
{"wire":[640,0,639,0]}
{"wire":[640,1,638,0]}
{"wire":[589,0,630,0]}
{"wire":[648,0,652,0]}
{"wire":[649,0,648,0]}
{"wire":[649,1,650,0]}
{"wire":[650,0,651,1]}
{"wire":[650,1,651,3]}
{"wire":[654,0,649,0]}
{"wire":[654,1,653,0]}
{"wire":[582,0,474,0]}
{"wire":[582,1,675,0]}
{"wire":[582,2,655,0]}
{"wire":[354,0,368,0]}
{"wire":[357,0,355,0]}
{"wire":[357,1,379,0]}
{"wire":[365,0,357,0]}
{"wire":[365,1,358,3]}
{"wire":[656,0,365,0]}
{"wire":[656,1,378,0]}
{"wire":[359,0,656,0]}
{"wire":[620,0,619,0]}
{"wire":[586,1,452,0]}
{"wire":[586,0,591,0]}
{"wire":[658,0,618,0]}
{"wire":[658,1,460,0]}
{"wire":[460,0,573,0]}
{"wire":[460,1,456,0]}
{"wire":[456,0,586,0]}
{"wire":[456,1,459,0]}
{"wire":[449,0,472,0]}
{"wire":[449,1,544,0]}
{"wire":[450,0,468,0]}
{"wire":[450,1,449,0]}
{"wire":[453,0,450,0]}
{"wire":[453,1,451,0]}
{"wire":[458,0,468,0]}
{"wire":[458,1,548,0]}
{"wire":[548,0,472,0]}
{"wire":[548,1,685,0]}
{"wire":[459,0,612,0]}
{"wire":[459,1,458,0]}
{"wire":[587,0,454,0]}
{"wire":[442,0,470,0]}
{"wire":[444,0,442,0]}
{"wire":[594,0,444,0]}
{"wire":[660,0,470,0]}
{"wire":[663,0,664,0]}
{"wire":[663,1,665,0]}
{"wire":[663,2,666,4]}
{"wire":[669,0,601,5]}
{"wire":[581,0,570,0]}
{"wire":[672,0,598,0]}
{"wire":[673,0,672,0]}
{"wire":[445,0,579,0]}
{"wire":[676,0,445,4]}
{"wire":[676,1,447,0]}
{"wire":[531,0,676,0]}
{"wire":[570,11,568,0]}
{"wire":[570,345,671,0]}
{"wire":[570,346,670,0]}
{"wire":[570,347,674,0]}
{"wire":[570,32,691,0]}
{"wire":[596,0,597,0]}
{"wire":[567,0,596,0]}
{"wire":[567,1,559,0]}
{"wire":[559,0,569,0]}
{"wire":[559,1,557,0]}
{"wire":[677,0,567,0]}
{"wire":[564,0,562,0]}
{"wire":[678,0,564,0]}
{"wire":[678,1,581,0]}
{"wire":[565,0,563,0]}
{"wire":[565,1,564,0]}
{"wire":[580,0,471,0]}
{"wire":[580,1,448,0]}
{"wire":[578,0,580,0]}
{"wire":[666,0,667,0]}
{"wire":[573,0,455,0]}
{"wire":[682,0,679,0]}
{"wire":[684,0,680,0]}
{"wire":[684,1,686,0]}
{"wire":[685,0,684,0]}
{"wire":[685,1,682,0]}
{"wire":[685,2,683,0]}
{"wire":[686,0,681,0]}
{"wire":[680,0,688,0]}
{"wire":[688,11,687,0]}
{"wire":[688,32,690,0]}
{"wire":[611,0,453,0]}
{"wire":[332,0,338,0]}
{"wire":[332,1,334,0]}
{"wire":[332,2,331,0]}
{"wire":[604,0,491,0]}
{"wire":[604,1,603,4]}
{"wire":[605,0,604,0]}
{"wire":[335,0,333,0]}
{"wire":[415,0,658,0]}
{"wire":[415,1,661,0]}
{"wire":[415,9,663,0]}
{"wire":[415,4,662,0]}
{"wire":[415,5,590,0]}
{"wire":[415,2,588,0]}
{"wire":[415,6,615,0]}
{"wire":[415,7,616,0]}
{"wire":[415,8,620,0]}
ASEEND*/
//CHKSM=837C0FF88117BA954B49C0FA3D44AF6A76862D3D