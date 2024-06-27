//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset from a pirate site, please consider buying it from the Unity asset store at https://assetstore.unity.com/packages/slug/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

#ifndef __WEATHER_MAKER_SHADER_CONSTANTS__
#define __WEATHER_MAKER_SHADER_CONSTANTS__

#include "UnityCG.cginc" 

#if !defined(GAMMA)
#if defined(UNITY_COLORSPACE_GAMMA)
#define GAMMA 2
#define COLOR_2_GAMMA(color) color
#define COLOR_2_LINEAR(color) color * color
#define LINEAR_2_OUTPUT(color) sqrt(color)
#else
#define GAMMA 2.2
#define COLOR_2_GAMMA(color) ((unity_ColorSpaceDouble.r>2.0) ? pow(color,1.0/GAMMA) : color)
#define COLOR_2_LINEAR(color) color
#define LINEAR_2_OUTPUT(color) color
#endif
#endif

#if !defined(float4Zero)
#define float4Zero float4(0.0, 0.0, 0.0, 0.0)
#define float4One float4(1.0, 1.0, 1.0, 1.0)
#define fixed4Zero fixed4(0.0, 0.0, 0.0, 0.0)
#define fixed4One fixed4(1.0, 1.0, 1.0, 1.0)
#define float3Zero float3(0.0, 0.0, 0.0)
#define float3One float3(1.0, 1.0, 1.0)
#define fixed3Zero fixed3(0.0, 0.0, 0.0)
#define fixed3One fixed3(1.0, 1.0, 1.0)
#define float2Zero float2(0.0, 0.0)
#define float2One float2(1.0, 1.0)
#define fixed2Zero fixed2(0.0, 0.0)
#define fixed2One fixed2(1.0, 1.0)
#define ditherMagic fixed4(12.9898, 78.233, 43758.5453, 241325690.2135)
#define ditherMagic2 fixed4(2.34234, 5.123124, 1024.0, 1024.0)
#define upVector float3(0.0, 1.0, 0.0)
#endif

#if !defined(vec2)
#define vec2 float2
#endif

#if !defined(vec3)
#define vec3 float3
#endif

#if !defined(vec4)
#define vec4 float4
#endif

#if !defined(mix)
#define mix lerp
#endif

#if !defined(fract)
#define fract frac
#endif

#if !defined(WM_MIN_PIXEL_VALUE)
#define WM_MIN_PIXEL_VALUE 0.004
#endif

#if !defined(PI)
#define PI 3.14159265359
#endif

#if !defined(PI2)
#define PI2 (PI * 2.0)
#endif

#if !defined(PI4)
#define PI4 (PI * 4.0)
#endif

#if !defined(ANTI_ALIAS)
#define ANTI_ALIAS(v) abs(frac(v) - 0.5)
#endif

#if !defined(SOFTPARTICLES_ON)
#define SOFTPARTICLES_ON
#endif

#define WM_BASE_VERTEX_INPUT UNITY_VERTEX_INPUT_INSTANCE_ID
#define WM_BASE_VERTEX_TO_FRAG UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO

#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED) || defined(STEREO_INSTANCING_ON)

#define WM_STEREO_INSTANCING_ENABLED

#endif

//#define WM_INSTANCE_VERT(v, type, o) type o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); UNITY_TRANSFER_INSTANCE_ID(v, o);
#define WM_INSTANCE_VERT(v, type, o) type o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_OUTPUT(type, o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
//#define WM_INSTANCE_FRAG(i) UNITY_SETUP_INSTANCE_ID(i); UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
#define WM_INSTANCE_FRAG(i) UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

#define WM_SAMPLE_DEPTH(uv) UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, (uv).xy))
#define WM_SAMPLE_DEPTH_PROJ(uv) UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(uv)))

// // you can get depth01 as follows:
// float depth01 = Linear01Depth(UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(screenPos)))); // screenPos is float4 from ComputeScreenPos
// float depth01 = Linear01Depth(UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV))); // screenUV is float2

#if !defined(DO_ALPHA_BLEND)
#define DO_ALPHA_BLEND(fg, bg) ((fg.rgb * fg.a) + (bg.rgb * (1.0 - fg.a)))
#endif

#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))
	#if !defined(UNITY_SAMPLE_TEX2D_LOD)
	#define UNITY_SAMPLE_TEX2D_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,(coord).xy,lod)
	#endif
#else
	#if !defined(UNITY_SAMPLE_TEX2D_LOD)
	#define UNITY_SAMPLE_TEX2D_LOD(tex,coord,lod) tex2Dlod (tex,float3(coord,lod))
	#endif
#endif

// vertex...
struct wm_appdata_base
{
	float4 vertex   : POSITION;  // The vertex position in model space.
	float3 normal   : NORMAL;    // The vertex normal in model space.
	float4 texcoord : TEXCOORD0; // The first UV coordinate.
	WM_BASE_VERTEX_INPUT
};

struct wm_appdata_full
{
	float4 vertex    : POSITION;  // The vertex position in model space.
	float3 normal    : NORMAL;    // The vertex normal in model space.
	float4 texcoord  : TEXCOORD0; // The first UV coordinate.
	float4 texcoord1 : TEXCOORD1; // The second UV coordinate.
	float4 tangent   : TANGENT;   // The tangent vector in Model Space (used for normal mapping).
	float4 color     : COLOR;     // Per-vertex color
	WM_BASE_VERTEX_INPUT
};

struct wm_vertex_uv_normal
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float3 normal : NORMAL;
	WM_BASE_VERTEX_INPUT
};

struct wm_full_screen_vertex
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	WM_BASE_VERTEX_INPUT
};

// fragments...
struct wm_volumetric_data
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 projPos : TEXCOORD0;
	float3 rayDir : TEXCOORD1;
	float3 viewPos : TEXCOORD2;
	float3 worldPos : TEXCOORD3;
	float4 uv : TEXCOORD4;
	WM_BASE_VERTEX_TO_FRAG
};

struct wm_full_screen_fragment_vertex_uv
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	WM_BASE_VERTEX_TO_FRAG
};

struct wm_full_screen_fragment
{
	float4 vertex : SV_POSITION;
	float4 uv : TEXCOORD0; // uv, uv pixel pos
	float3 forwardLine : TEXCOORD1;
	float2 uvNonStereo : TEXCOORD2;
	WM_BASE_VERTEX_TO_FRAG
};

struct wm_full_screen_fragment_reflection
{
	float4 vertex : SV_POSITION;
	float4 uv : TEXCOORD0; // uv, uv pixel pos
	float3 forwardLine : TEXCOORD1;
	float4 reflectionPos : TEXCOORD2;
	float4 screenPos : TEXCOORD3;
	WM_BASE_VERTEX_TO_FRAG
};

struct wm_deferred_fragment
{
	float4 gBuffer0 : SV_Target0;
	float4 gBuffer1 : SV_Target1;
	float4 gBuffer2 : SV_Target2;
	float4 gBuffer3 : SV_Target3;
};

struct wm_frag_out_with_depth
{
	fixed4 color : COLOR;
	float depth : DEPTH;
};

SamplerState sampler_point_clamp_sampler;
SamplerState sampler_linear_clamp_sampler;
SamplerState sampler_point_mirror_sampler;
SamplerState sampler_linear_mirror_sampler;

#if defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

// https://docs.unity3d.com/Manual/SL-SamplerStates.html
// Sampler names recognized as “inline” sampler states (all case insensitive):
// “Point”, “Linear” or “Trilinear” (required) set up texture filtering mode.
// “Clamp”, “Repeat”, “Mirror” or “MirrorOnce”(required) set up texture wrap mode.
// Wrap modes can be specified per - axis(UVW), e.g.“ClampU_RepeatV”.
// “Compare”(optional) set up sampler for depth comparison; use with HLSL SamplerComparisonState type and SampleCmp / SampleCmpLevelZero functions.

#endif // WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

uniform float4 _MainTex_ST;
uniform float4 _MainTex_TexelSize;

uniform float4 _WeatherMakerTime;
uniform float4 _WeatherMakerTimeSin;
uniform float4 _WeatherMakerTimeAngle;

// contains temporal uv coordinates
uniform float4 _WeatherMakerTemporalUV;
uniform float2 _WeatherMakerTemporalUV_VertexShaderProjection;
uniform float2 _WeatherMakerTemporalUV_FragmentShader;

// inverse matrix for each eye, use unity_StereoEyeIndex to index
uniform float4x4 _WeatherMakerView[2];
uniform float4x4 _WeatherMakerInverseView[2];
uniform float4x4 _WeatherMakerInverseProj[2];
uniform uint _WeatherMakerStereoEyeIndex; // stereo eye index for multi-pass shaders like compute shaders that generate a texture for each eye
uniform float3 _WeatherMakerCameraFrustumCorners[8]; // 0-3 are left eye, 4-7 are right eye all in world space, top right, bottom right, bottom left, top left
uniform float3 _WeatherMakerCameraOriginOffset; // camera origin offset, subtract this from any noise lookup positions
uniform float3 _WeatherMakerCameraPosComputeShader; // camera pos for compute shader, left in here in non-compute shader define because it is updated in common code, ignore for non-compute shaders
uniform float _WeatherMakerAdjustFullScreenUVStereoDisable; // set to 1 if you need to disable stereo screen space uv adjust for AdjustFullScreenUV method
uniform float _WeatherMakerCameraRenderMode;
uniform float _WeatherMakerVREnabled;
uniform float _WeatherMakerDeferredShading;
uniform float _WeatherMakerDownsampleScale;
uniform float _WeatherMakerTemporalReprojectionEnabled; // 0 = none, 1 = sub frame mode, 2 = full mode

static const float weatherMakerTemporalReprojectionEnabled = (round(_WeatherMakerTemporalReprojectionEnabled));
static const float weatherMakerDownsampleScale = (round(_WeatherMakerDownsampleScale));

// this is the only place in the weather maker code where _WorldSpaceCameraPos is allowed, all other uses of camera positions
// should use either WEATHER_MAKER_CAMERA_POS for origin offset position or WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET for non-origin offset camera pos
// for working against vertex positions in world space and computing true depth, WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET can be used
// for most other cases, WEATHER_MAKER_CAMERA_POS should be used
#define WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET _WorldSpaceCameraPos

#if defined(WEATHER_MAKER_COMPUTE_SHADER)

static const float3 weatherMakerCameraPosWithOriginOffset = _WeatherMakerCameraPosComputeShader - _WeatherMakerCameraOriginOffset;

#else

static const float3 weatherMakerCameraPosWithOriginOffset = WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET - _WeatherMakerCameraOriginOffset;

#endif

static const float WM_CAMERA_RENDER_MODE_NORMAL = (_WeatherMakerCameraRenderMode < 0.5);
static const float WM_CAMERA_RENDER_MODE_NOT_NORMAL = (_WeatherMakerCameraRenderMode > 0.5);
static const float WM_CAMERA_RENDER_MODE_REFLECTION = (_WeatherMakerCameraRenderMode > 0.5 && _WeatherMakerCameraRenderMode < 1.5);
static const float WM_CAMERA_RENDER_MODE_NOT_REFLECTION = (_WeatherMakerCameraRenderMode < 0.5 || _WeatherMakerCameraRenderMode > 1.5);
static const float WM_CAMERA_RENDER_MODE_CUBEMAP = (_WeatherMakerCameraRenderMode > 1.5);
static const float WM_CAMERA_RENDER_MODE_NOT_CUBEMAP = (_WeatherMakerCameraRenderMode < 1.5);

#define WEATHER_MAKER_CAMERA_POS weatherMakerCameraPosWithOriginOffset

#if defined(WEATHER_MAKER_IS_FULL_SCREEN_EFFECT) && defined(WM_STEREO_INSTANCING_ENABLED)

#define WM_DECLARE_SCREENSPACE_TEXTURE_SAMPLER UNITY_DECLARE_SCREENSPACE_TEXTURE

#else

#define WM_DECLARE_SCREENSPACE_TEXTURE_SAMPLER UNITY_DECLARE_TEX2D

#endif

#if defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraOpaqueTexture);

#if defined(WEATHER_MAKER_IS_FULL_SCREEN_EFFECT) && defined(WM_STEREO_INSTANCING_ENABLED)

uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex2);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex3);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex4);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex5);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex6);

#elif defined(WEATHER_MAKER_IS_FULL_SCREEN_EFFECT) && defined(WEATHER_MAKER_MAIN_TEX_SAMPLERS)

uniform UNITY_DECLARE_TEX2D(_MainTex);
uniform UNITY_DECLARE_TEX2D(_MainTex2);
uniform UNITY_DECLARE_TEX2D(_MainTex3);
uniform UNITY_DECLARE_TEX2D(_MainTex4);
uniform UNITY_DECLARE_TEX2D(_MainTex5);
uniform UNITY_DECLARE_TEX2D(_MainTex6);

#else

uniform sampler2D _MainTex;
uniform sampler2D _MainTex2;
uniform sampler2D _MainTex3;
uniform sampler2D _MainTex4;
uniform sampler2D _MainTex5;
uniform sampler2D _MainTex6;

#endif

uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthNormalsTexture);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_WeatherMakerAfterForwardOpaqueTexture);
uniform UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

uniform float4 _CameraDepthTexture_ST;
uniform float4 _CameraDepthTexture_TexelSize;
//uniform float4 _CameraDepthTextureOne_ST;
//uniform float4 _CameraDepthTextureOne_TexelSize;
uniform float4 _CameraDepthTextureHalf_ST;
uniform float4 _CameraDepthTextureHalf_TexelSize;
uniform float4 _CameraDepthTextureQuarter_ST;
uniform float4 _CameraDepthTextureQuarter_TexelSize;
uniform float4 _CameraDepthTextureEighth_ST;
uniform float4 _CameraDepthTextureEighth_TexelSize;
uniform float4 _CameraDepthNormalsTexture_ST;
uniform float4 _CameraDepthNormalsTexture_TexelSize;

//uniform UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTextureOne);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTextureHalf);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTextureQuarter);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTextureEighth);

uniform float4 _MainTex2_ST;
uniform float4 _MainTex2_TexelSize;
uniform float4 _MainTex3_ST;
uniform float4 _MainTex3_TexelSize;
uniform float4 _MainTex4_ST;
uniform float4 _MainTex4_TexelSize;
uniform float4 _MainTex5_ST;
uniform float4 _MainTex5_TexelSize;
uniform float4 _MainTex6_ST;
uniform float4 _MainTex6_TexelSize;
uniform float4 _CameraMotionVectorsTexture_ST;
uniform float4 _CameraMotionVectorsTexture_TexelSize;
uniform sampler2D _WeatherMakerBlueNoiseTexture;
uniform sampler2D _WeatherMakerDitherTexture;

//UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraMotionVectorsTexture);
//float2 motion = WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraMotionVectorsTexture, i.uv).xy;
//float motionLength = length(motion * _CameraMotionVectorsTexture_TexelSize.zw);

#if defined(UNITY_URP)

uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_GBuffer0);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_GBuffer1);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_GBuffer2);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_GBuffer3);

#define _CameraGBufferTexture0 _GBuffer0
#define _CameraGBufferTexture1 _GBuffer1
#define _CameraGBufferTexture2 _GBuffer2
#define _CameraGBufferTexture3 _GBuffer3

#else

uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraGBufferTexture0);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraGBufferTexture1);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraGBufferTexture2);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraGBufferTexture3);

#endif

#if defined(SOFTPARTICLES_ON)

float _InvFade;

#endif

#endif // WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

#ifndef UNITY_SAMPLE_TEX2D_SAMPLER_LOD

#define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex, samp, uv, lod) tex.SampleLevel(sampler##samp, uv, lod)

#endif

#if defined(WM_STEREO_INSTANCING_ENABLED)

#define WM_SAMPLE_FULL_SCREEN_TEXTURE_PROJ(tex, uv) UNITY_SAMPLE_TEX2DARRAY_PROJ(tex, uv)
#define WM_SAMPLE_FULL_SCREEN_TEXTURE(tex, uv) UNITY_SAMPLE_TEX2DARRAY_LOD(tex, float3((uv).xy, (float)unity_StereoEyeIndex), 0.0)
#define WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(tex, samp, uv) UNITY_SAMPLE_TEX2DARRAY_SAMPLER_LOD(tex, samp, float3((uv).xy, (float)unity_StereoEyeIndex), 0.0)

#else

#define WM_SAMPLE_FULL_SCREEN_TEXTURE_PROJ(tex, uv) tex2Dproj(tex, uv)
#define WM_SAMPLE_FULL_SCREEN_TEXTURE(tex, uv) tex2Dlod(tex, float4((uv).xy, 0.0, 0.0))
#define WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(tex, samp, uv) WM_SAMPLE_TEX2D_SAMPLER_LOD(tex, samp, float4((uv).xy, 0.0, 0.0))

#endif

#define WM_SAMPLE_TEX2D_SAMPLER_LOD(tex, samp, coord) UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex, samp, coord.xyz, coord.w)
#define WM_SAMPLE_TEX2D_ARRAY_STEREO(tex, uv) UNITY_SAMPLE_TEX2DARRAY_LOD(tex, float3((uv).xy, unity_StereoEyeIndex), 0.0)

#define WM_LINEAR_DEPTH(depth) lerp(LinearEyeDepth(depth), 1.0, unity_OrthoParams.w)
#define WM_LINEAR_DEPTH_01(depth) lerp(Linear01Depth(depth), 1.0, unity_OrthoParams.w)
#define WM_SAMPLE_DEPTH_TEXTURE_01(tex, uv) WM_LINEAR_DEPTH_01(UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(tex, uv)))

float _WeatherMakerEnableToneMapping;

static const float4x4 _WeatherMakerBayerMatrix =
{
	00.0 / 16.0, 12.0 / 16.0, 03.0 / 16.0, 15.0 / 16.0,
	08.0 / 16.0, 04.0 / 16.0, 11.0 / 16.0, 07.0 / 16.0,
	02.0 / 16.0, 14.0 / 16.0, 01.0 / 16.0, 13.0 / 16.0,
	10.0 / 16.0, 06.0 / 16.0, 09.0 / 16.0, 05.0 / 16.0
};

float2 AdjustFullScreenUV(float2 uv)
{

#if defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

	uv = lerp(UnityStereoTransformScreenSpaceTex(uv), uv, _WeatherMakerAdjustFullScreenUVStereoDisable);
	//uv = UnityStereoScreenSpaceUVAdjust(uv, _MainTex_ST);

#if (UNITY_UV_STARTS_AT_TOP)

	if (_MainTex_TexelSize.y < 0.0)
	{
		uv.y = 1.0 - uv.y;
	}

#endif

#endif

	return uv;
}

float CalculateNoiseXZ(sampler2D noiseTex, float3 worldPos, float scale, float2 offset, float2 velocity, float multiplier, float adder)
{
	float2 noiseUV = float2(worldPos.x * scale, worldPos.z * scale);
	noiseUV += offset + velocity;
	return (tex2Dlod(noiseTex, float4(noiseUV, 0.0, 0.0)).a + adder) * multiplier;
}

float CalculateNoiseXZ(texture2D<float4> noiseTex, float3 worldPos, float scale, float2 offset, float2 velocity, float multiplier, float adder)
{
	float2 noiseUV = float2(worldPos.x * scale, worldPos.z * scale);
	noiseUV += offset + velocity;
	return (UNITY_SAMPLE_TEX2D_SAMPLER(noiseTex, _linear_clamp_sampler, noiseUV).a + adder) * multiplier;
}

float RandomFloat(float3 v)
{
	return (frac(frac(dot(v.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453) - 0.5) * 2.0;
}

float RandomFloatHalved(float3 v)
{
	return (frac(frac(dot(v.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453) - 0.5);
}

float RandomFloat2D(float2 v)
{
	return (frac(frac(dot(v, float2(12.9898, 78.233))) * 43758.5453) - 0.5) * 2.0;
}

float RandomFloat2DHalved(float2 v)
{
	return (frac(frac(dot(v, float2(12.9898, 78.233))) * 43758.5453) - 0.5);
}

float RandomFloat01(float3 v)
{
	return (frac(frac(dot(v.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453));
}

float RandomSimple(float n)
{
	return mad(frac(sin(n) * 43758.5453123), 2.0, -1.0);
}

float3 RandomVector(float3 n)
{
	return normalize(float3(RandomSimple(n.x), RandomSimple(n.y), RandomSimple(n.z)));
}

// see https://bib.irb.hr/datoteka/949019.Final_0036470256_56.pdf
float Remap(float value, float low1, float high1, float low2, float high2)
{
	return low2 + ((value - low1) * ((high2 - low2) / (high1 - low1)));
}

// Uncharted 2 tonemap from http://filmicgames.com/archives/75
float3 FilmicTonemap(float3 x)
{
	static const float A = 0.15;
	static const float B = 0.50;
	static const float C = 0.10;
	static const float D = 0.20;
	static const float E = 0.02;
	static const float F = 0.30;
	return ((x * (A * x + C * B) + D * E) / (x *(A * x + B) + D * F)) - E / F;
}

float3 FilmicTonemapFull(float3 color, float exposure)
{
	float3 curr = FilmicTonemap(exposure * color.rgb);
	float3 whiteScale = 1.0f / FilmicTonemap(11.175);
	return curr * whiteScale;
}

float GetDepth01FromWorldSpaceRay(float3 rayDir, float dist)
{
	float3 localWorldPos = rayDir * dist;
	return -(mul((float3x3)UNITY_MATRIX_V, localWorldPos).z * _ProjectionParams.w);
}

float GetDepth(float2 uv)
{

#if !defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

	return 0.0;

#else

	return WM_LINEAR_DEPTH(WM_SAMPLE_DEPTH(uv));

#endif

}

float GetDepth01(float2 uv)
{

#if !defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

	return 0.0;

#else

	return WM_LINEAR_DEPTH_01(WM_SAMPLE_DEPTH(uv));

#endif

}

void GetDepth01AndNormal(float2 uv, out float depth, out float3 normal)
{

#if !defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

	depth = 0.0;
	normal = 0.0;

#else

	float4 samp = WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraDepthNormalsTexture, uv);
	DecodeDepthNormal(samp, depth, normal);

#endif

}

float WM_SAMPLE_DEPTH_DOWNSAMPLED_01(float2 uv)
{

#if !defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

	return 0.0;

#else

	UNITY_BRANCH
	switch (weatherMakerDownsampleScale)
	{
	default:
		return WM_LINEAR_DEPTH_01(UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv)));

	case 2:
        return WM_LINEAR_DEPTH_01(WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraDepthTextureHalf, uv));

	case 4:
        return WM_LINEAR_DEPTH_01(WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraDepthTextureQuarter, uv));

	case 8:
        return WM_LINEAR_DEPTH_01(WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraDepthTextureEighth, uv));
	}

#endif

}

float WM_SAMPLE_DEPTH_DOWNSAMPLED_TEMPORAL_REPROJECTION_01(float2 uv)
{

#if !defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

	return 0.0;

#else

	return WM_SAMPLE_DEPTH_DOWNSAMPLED_01(uv + _WeatherMakerTemporalUV_FragmentShader);

#endif

}

float WM_SAMPLE_DEPTH_DOWNSAMPLED_01_KEYWORD(float2 uv)
{

#if !defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

    return 0.0;

#elif defined(WEATHER_MAKER_DOWNSAMPLE_2)

    return WM_LINEAR_DEPTH_01(WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraDepthTextureHalf, uv));

#elif defined(WEATHER_MAKER_DOWNSAMPLE_4)

    return WM_LINEAR_DEPTH_01(WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraDepthTextureQuarter, uv));

#elif defined(WEATHER_MAKER_DOWNSAMPLE_8)

    return WM_LINEAR_DEPTH_01(WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraDepthTextureEighth, uv));

#else

    return WM_LINEAR_DEPTH_01(UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv)));

#endif

}

float WM_SAMPLE_DEPTH_DOWNSAMPLED_TEMPORAL_REPROJECTION_01_KEYWORD(float2 uv)
{

#if !defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

    return 0.0;

#else

    return WM_SAMPLE_DEPTH_DOWNSAMPLED_01_KEYWORD(uv + _WeatherMakerTemporalUV_FragmentShader);

#endif

}

// alphaThreshold is the max difference from the average alpha value to include in the blur
fixed4 GaussianBlur_Texture2D_17Tap(sampler2D samp, float2 uv, float4 offsets, float alphaThreshold, float lod)
{
	float2 uv1 = float2(uv.x + offsets.x, uv.y - offsets.w);
	float2 uv2 = float2(uv.x - offsets.y, uv.y - offsets.z);
	float2 uv3 = float2(uv.x + offsets.y, uv.y + offsets.z);
	float2 uv4 = float2(uv.x - offsets.x, uv.y + offsets.w);
	fixed4 col = tex2Dlod(samp, float4(uv, 0.0, lod));
	fixed4 col2 = tex2Dlod(samp, float4(uv1, 0.0, lod));
	fixed4 col3 = tex2Dlod(samp, float4(uv2, 0.0, lod));
	fixed4 col4 = tex2Dlod(samp, float4(uv3, 0.0, lod));
	fixed4 col5 = tex2Dlod(samp, float4(uv4, 0.0, lod));
	UNITY_BRANCH
	if (alphaThreshold < 1.0)
	{
		fixed alphaAvg = ((col.a + col2.a + col3.a + col4.a + col5.a) * 0.2);
		fixed match1 = abs(col.a - alphaAvg) < alphaThreshold;
		fixed match2 = abs(col2.a - alphaAvg) < alphaThreshold;
		fixed match3 = abs(col3.a - alphaAvg) < alphaThreshold;
		fixed match4 = abs(col4.a - alphaAvg) < alphaThreshold;
		fixed match5 = abs(col5.a - alphaAvg) < alphaThreshold;
		fixed count = match1 + match2 + match3 + match4 + match5;
		col *= match1;
		col += (match2 * col2);
		col += (match3 * col3);
		col += (match4 * col4);
		col += (match5 * col5);
		col /= max(1.0, count);
	}
	else
	{
		col = (col + col2 + col3 + col4 + col5) * 0.2;
	}
	return col;
}

#define GaussianBlur17Tap(ret, samp, uv, offsets, alphaThreshold) \
{ \
	float2 uv1 = float2(uv.x + offsets.x, uv.y - offsets.w); \
	float2 uv2 = float2(uv.x - offsets.y, uv.y - offsets.z); \
	float2 uv3 = float2(uv.x + offsets.y, uv.y + offsets.z); \
	float2 uv4 = float2(uv.x - offsets.x, uv.y + offsets.w); \
	fixed4 col = WM_SAMPLE_FULL_SCREEN_TEXTURE(samp, uv); \
	fixed4 col2 = WM_SAMPLE_FULL_SCREEN_TEXTURE(samp, uv1); \
	fixed4 col3 = WM_SAMPLE_FULL_SCREEN_TEXTURE(samp, uv2); \
	fixed4 col4 = WM_SAMPLE_FULL_SCREEN_TEXTURE(samp, uv3); \
	fixed4 col5 = WM_SAMPLE_FULL_SCREEN_TEXTURE(samp, uv4); \
	if (alphaThreshold < 1.0) \
	{ \
		fixed alphaAvg = ((col.a + col2.a + col3.a + col4.a + col5.a) * 0.2); \
		fixed match1 = abs(col.a - alphaAvg) < alphaThreshold; \
		fixed match2 = abs(col2.a - alphaAvg) < alphaThreshold; \
		fixed match3 = abs(col3.a - alphaAvg) < alphaThreshold; \
		fixed match4 = abs(col4.a - alphaAvg) < alphaThreshold; \
		fixed match5 = abs(col5.a - alphaAvg) < alphaThreshold; \
		fixed count = match1 + match2 + match3 + match4 + match5; \
		col *= match1; \
		col += (match2 * col2); \
		col += (match3 * col3); \
		col += (match4 * col4); \
		col += (match5 * col5); \
		col /= count; \
	} \
	else \
	{ \
		col = (col + col2 + col3 + col4 + col5) * 0.2; \
	} \
	ret = col; \
}

#define GaussianBlur17TapR(ret, samp, uv, offsets) \
{ \
	float2 uv1 = float2(uv.x + offsets.x, uv.y - offsets.w); \
	float2 uv2 = float2(uv.x - offsets.y, uv.y - offsets.z); \
	float2 uv3 = float2(uv.x + offsets.y, uv.y + offsets.z); \
	float2 uv4 = float2(uv.x - offsets.x, uv.y + offsets.w); \
	fixed col = WM_SAMPLE_FULL_SCREEN_TEXTURE(samp, uv).r; \
	fixed col2 = WM_SAMPLE_FULL_SCREEN_TEXTURE(samp, uv1).r; \
	fixed col3 = WM_SAMPLE_FULL_SCREEN_TEXTURE(samp, uv2).r; \
	fixed col4 = WM_SAMPLE_FULL_SCREEN_TEXTURE(samp, uv3).r; \
	fixed col5 = WM_SAMPLE_FULL_SCREEN_TEXTURE(samp, uv4).r; \
	col = (col + col2 + col3 + col4 + col5) * 0.2; \
	ret = col; \
}

// MIT license: https://github.com/tuxalin/water-shader/blob/3cf2cd4788c87d42303afbbdfb8bfe0591b20ccf/shaders/hlsl/bicubic.cginc
float4 tex2DBicubic(sampler2D tex, float2 texCoords, float lod, float4 texelSize)
{
	float4 n, s;
	float x, y, z, w;

#define cubic(v, result) \
	n = float4(1.0, 2.0, 3.0, 4.0) - v; \
	s = n * n * n; \
	x = s.x; \
	y = s.y - 4.0 * s.x; \
	z = s.z - 4.0 * s.y + 6.0 * s.x; \
	w = 6.0 - x - y - z; \
	float4 result = float4(x, y, z, w) * (1.0 / 6.0);

	texCoords = (texCoords * texelSize.zw) - 0.5;
	float2 fxy = frac(texCoords);
	texCoords -= fxy;

	cubic(fxy.x, xcubic);
	cubic(fxy.y, ycubic);

	float4 c = texCoords.xxyy + float2(-0.5, 1.5).xyxy;
	float4 t = float4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
	float4 offset = c + float4(xcubic.yw, ycubic.yw) / t;
	offset *= texelSize.xxyy;
	float4 sample0 = tex2Dlod(tex, float4(offset.xz, 0.0, lod));
	float4 sample1 = tex2Dlod(tex, float4(offset.yz, 0.0, lod));
	float4 sample2 = tex2Dlod(tex, float4(offset.xw, 0.0, lod));
	float4 sample3 = tex2Dlod(tex, float4(offset.yw, 0.0, lod));
	float sx = t.x / (t.x + t.y);
	float sy = t.z / (t.z + t.w);

	return lerp(lerp(sample3, sample2, sx), lerp(sample1, sample0, sx), sy);

#undef cubic

}

float2 AlignScreenUVWithDepthTexel(float2 uv)
{

#if defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)
	
#if (UNITY_UV_STARTS_AT_TOP)

	if (_CameraDepthTexture_TexelSize.y < 0)
	{
		uv.y = 1 - uv.y;
	}

#endif

	return (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) *
		abs(_CameraDepthTexture_TexelSize.xy);

#else

	return uv;

#endif // WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

}

float ViewDepthFromDepth01(float depth01, float2 screenUV)
{
	float4 clipPos = float4((screenUV * 2.0) - 1.0, 1.0, 1.0);
	clipPos = mul(unity_CameraInvProjection, clipPos);
	clipPos.xyz /= clipPos.w;
	return length(clipPos.xyz);
}

fixed LerpFade(float4 lifeTime, float timeSinceLevelLoad)
{
	// the vertex will fade in, stay at full color, then fade out
	// x = creation time seconds
	// y = fade time in seconds
	// z = life time seconds

	// debug
	// return 1;

	float peakFadeIn = lifeTime.x + lifeTime.y;
	float startFadeOut = lifeTime.x + lifeTime.z - lifeTime.y;
	float endTime = lifeTime.x + lifeTime.z;
	float lerpMultiplier = saturate(ceil(timeSinceLevelLoad - peakFadeIn));
	float lerp1Scalar = saturate(((timeSinceLevelLoad - lifeTime.x + 0.000001) / max(0.000001, (peakFadeIn - lifeTime.x))));
	float lerp2Scalar = saturate(max(0, ((timeSinceLevelLoad - startFadeOut) / max(0.000001, (endTime - startFadeOut)))));
	float lerp1 = lerp1Scalar * (1.0 - lerpMultiplier);
	float lerp2 = (1.0 - lerp2Scalar) * lerpMultiplier;
	return lerp1 + lerp2;
}

float GetNearPlane()
{

#if defined(UNITY_REVERSED_Z)

	return 1.0 - UNITY_NEAR_CLIP_VALUE;

#else

	return UNITY_NEAR_CLIP_VALUE;

#endif

}

float WrapFloat(float v)
{
	if (v < 0.0f)
	{
		v += 1.0f;
	}
	else if (v > 1.0f)
	{
		v -= 1.0f;
	}
	return v;
}

void ApplyDither(inout fixed3 rgb, float2 screenUV, fixed l)
{
	fixed3 gradient = frac(cos(dot(screenUV * _WeatherMakerTime.x, ditherMagic.xy)) * ditherMagic.z) * l;
	rgb = max(0, (rgb - gradient));
}

#define BLUR_POINT_TAP_NO_ZERO_ALPHA(blurColor, tex, offsets) \
    float2 uv1 = float2(uv.z + offsets.x, uv.w + offsets.z); \
    float2 uv2 = float2(uv.z, uv.w + offsets.z); \
    float2 uv3 = float2(uv.z + offsets.y, uv.w + offsets.z); \
    float2 uv4 = float2(uv.z + offsets.x, uv.w); \
    float2 uv5 = float2(uv.z + offsets.y, uv.w); \
    float2 uv6 = float2(uv.z + offsets.x, uv.w + offsets.w); \
    float2 uv7 = float2(uv.z, uv.w + offsets.w); \
    float2 uv8 = float2(uv.z + offsets.y, uv.w + offsets.w); \
    fixed4 c1 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(tex, _point_clamp_sampler, uv1); \
    fixed4 c2 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(tex, _point_clamp_sampler, uv2); \
    fixed4 c3 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(tex, _point_clamp_sampler, uv3); \
    fixed4 c4 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(tex, _point_clamp_sampler, uv4); \
    fixed4 c5 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(tex, _point_clamp_sampler, uv5); \
    fixed4 c6 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(tex, _point_clamp_sampler, uv6); \
    fixed4 c7 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(tex, _point_clamp_sampler, uv7); \
    fixed4 c8 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(tex, _point_clamp_sampler, uv8); \
    fixed c1a = ceil(c1.a); \
    fixed c2a = ceil(c2.a); \
    fixed c3a = ceil(c3.a); \
    fixed c4a = ceil(c4.a); \
    fixed c5a = ceil(c5.a); \
    fixed c6a = ceil(c6.a); \
    fixed c7a = ceil(c7.a); \
    fixed c8a = ceil(c8.a); \
    fixed blurCount = 1.0 + c1a + c2a + c3a + c4a + c5a + c6a + c7a + c8a; \
    blurColor += (c1 * c1a); \
    blurColor += (c2 * c2a); \
    blurColor += (c3 * c3a); \
    blurColor += (c4 * c4a); \
    blurColor += (c5 * c5a); \
    blurColor += (c6 * c6a); \
    blurColor += (c7 * c7a); \
    blurColor += (c8 * c8a); \
    blurColor /= blurCount;

void ApplyDitherNoTime(inout fixed3 rgb, float2 screenUV, fixed l)
{
	fixed3 gradient = frac(cos(dot(screenUV, ditherMagic.xy)) * ditherMagic.z) * l;
	rgb = max(0, (rgb - gradient));
}

float4 UnityObjectToClipPosFarPlane(float4 vertex)
{
	float4 pos = UnityObjectToClipPos(float4(vertex.xyz, 1.0));
	if (UNITY_NEAR_CLIP_VALUE == 1.0)
	{
		// DX
		pos.z = 0.0;
	}
	else
	{
		// OpenGL
		pos.z = pos.w;
	}
	return pos;
}

float3 WorldSpaceVertexPosNear(float3 vertex)
{
	return mul(unity_ObjectToWorld, float4(vertex.xyz, 0.0)).xyz;
}

float3 WorldSpaceVertexPosFar(float3 vertex)
{
	return mul(unity_ObjectToWorld, float4(vertex.xyz, 1.0)).xyz;
}

float3 WorldSpaceVertexPos(float4 vertex)
{
	return mul(unity_ObjectToWorld, vertex).xyz;
}

float3 GetFullScreenRayDir(float3 rayDir)
{
	if (WM_CAMERA_RENDER_MODE_CUBEMAP)
	{
		// cubemaps cannot calculate the rayDir in vertex shader, each pixel needs to re-calculate the rayDir from clip pos
		// see https://github.com/chriscummings100/worldspaceposteffect/blob/master/Assets/WorldSpacePostEffect/WorldSpacePostEffect.shader

#if defined(UNITY_REVERSED_Z)

		float4 clipPos = float4(rayDir.xy, GetNearPlane(), 1.0);

#else

		// WTF OpenGL requires -1.0 w and flipping the x???
		float4 clipPos = float4(rayDir.xy, GetNearPlane(), -1.0);
		clipPos.x *= -1;

#endif

		// cannot use combined view*proj matrix with cubemap, not sure why...
		clipPos = mul(_WeatherMakerInverseProj[unity_StereoEyeIndex], clipPos);
		clipPos.xyz = clipPos.xyz / clipPos.w;
		return normalize(mul((float3x3)_WeatherMakerInverseView[unity_StereoEyeIndex], clipPos.xyz));
	}
	else
	{
		return normalize(rayDir);
	}
}

wm_volumetric_data GetVolumetricData(wm_appdata_base v)
{
	WM_INSTANCE_VERT(v, wm_volumetric_data, o);
	o.vertex = UnityObjectToClipPosFarPlane(v.vertex);
	o.normal = UnityObjectToWorldNormal(v.normal);
	o.projPos = ComputeScreenPos(o.vertex);
	o.worldPos = WorldSpaceVertexPos(v.vertex);
	o.rayDir = (o.worldPos - WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET);// -WorldSpaceViewDir(v.vertex);
	o.viewPos = UnityObjectToViewPos(v.vertex);
	o.uv = v.texcoord;
	return o;
}

wm_full_screen_fragment full_screen_vertex_shader(wm_full_screen_vertex v)
{
	WM_INSTANCE_VERT(v, wm_full_screen_fragment, o);
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv.xy = AdjustFullScreenUV(v.uv);
	float2 cameraUV = (2.0 * v.uv.xy) - 1.0;
	if (WM_CAMERA_RENDER_MODE_CUBEMAP)
	{
		// cube map must get the ray in the fragment due to being 360 degrees view
		// temporal reprojection not supported for cubemap
		o.forwardLine = float3(cameraUV, 1.0);
	}
	else if (unity_OrthoParams.w == 0.0)
	{
		float4 clipPos = float4(cameraUV + _WeatherMakerTemporalUV_VertexShaderProjection, 1.0, 1.0);
		clipPos = mul(_WeatherMakerInverseProj[unity_StereoEyeIndex], clipPos);
		clipPos.xyz /= clipPos.w;
		o.forwardLine = mul((float3x3)_WeatherMakerInverseView[unity_StereoEyeIndex], clipPos.xyz);
	}
	else
	{
		float2 m = float2(_ScreenParams.x * 0.001, _ScreenParams.y * 0.001);
		o.forwardLine = float3(m.x * (o.uv.x - 0.5), m.y * (o.uv.y - 0.25), 1.0);
	}

	o.uvNonStereo = ComputeNonStereoScreenPos(o.vertex);
	o.uv.zw = round(o.uv.xy) * _MainTex_TexelSize.zw;
	return o;
}

wm_full_screen_fragment_reflection full_screen_vertex_shader_refl(wm_full_screen_vertex v)
{
	WM_INSTANCE_VERT(v, wm_full_screen_fragment_reflection, o);
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv.xy = AdjustFullScreenUV(v.uv);
	float2 cameraUV = (2.0 * v.uv.xy) - 1.0;
	if (WM_CAMERA_RENDER_MODE_CUBEMAP)
	{
		// cube map must get the ray in the fragment due to being 360 degrees view
		// temporal reprojection not supported for cubemap
		o.forwardLine = float3(cameraUV, 1.0);
	}
	else if (unity_OrthoParams.w == 0.0)
	{
		float4 clipPos = float4(cameraUV + _WeatherMakerTemporalUV_VertexShaderProjection, 1.0, 1.0);
		clipPos = mul(_WeatherMakerInverseProj[unity_StereoEyeIndex], clipPos);
		clipPos.xyz /= clipPos.w;
		o.forwardLine = mul((float3x3)_WeatherMakerInverseView[unity_StereoEyeIndex], clipPos.xyz);
	}
	else
	{
		float2 m = float2(_ScreenParams.x * 0.001, _ScreenParams.y * 0.001);
		o.forwardLine = float3(m.x * (o.uv.x - 0.5), m.y * (o.uv.y - 0.25), 1.0);
	}

	o.uv.zw = round(o.uv.xy) * _MainTex_TexelSize.zw;
	o.reflectionPos = ComputeNonStereoScreenPos(o.vertex);
	o.screenPos = ComputeScreenPos(o.vertex);

	return o;
}

wm_full_screen_fragment_vertex_uv full_screen_vertex_shader_vertex_uv(wm_full_screen_vertex v)
{
	WM_INSTANCE_VERT(v, wm_full_screen_fragment_vertex_uv, o);
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv.xy = AdjustFullScreenUV(v.uv);
	return o;
}

#endif
