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

#ifndef __WEATHER_MAKER_WATER_INCLUDED__
#define __WEATHER_MAKER_WATER_INCLUDED__

#define WEATHER_MAKER_SHADOWS_DEPTH_EXTERNAL_FUNC ComputeCloudShadowStrengthTexture

#include "WeatherMakerCoreShaderInclude.cginc"
#include "WeatherMakerFogShaderInclude.cginc"
#include "WeatherMakerShadowsShaderInclude.cginc"
#include "WeatherMakerCloudVolumetricShadowsShaderInclude.cginc"

#define GET_WATER_DISTANCE_REDUCER(worldPos) (1.0 - saturate(((WEATHER_MAKER_CAMERA_POS.y - worldPos.y - 100.0) * _InvFadeParameter.y) + (worldPos.w * _InvFadeParameter.w)))

// textures
uniform sampler2D _WaterTex;
uniform sampler2D _WaterBumpMap;
uniform sampler2D _WaterFoam;
//uniform sampler2D _WaterFoamBumpMap;
uniform sampler2D _WeatherMakerWaterReflectionTex;
uniform sampler2D _WeatherMakerWaterReflectionTex2;
uniform sampler2D _WaterDisplacementMap;

// water y depth...
uniform sampler2D _WeatherMakerYDepthTexture;
uniform float4 _WeatherMakerYDepthParams;

#if defined(WEATHER_MAKER_LIGHT_SPECULAR_SPARKLE)

uniform sampler2D _SparkleNoise;
uniform fixed4 _SparkleTintColor;
uniform fixed4 _SparkleScale;
uniform fixed4 _SparkleOffset;
uniform fixed4 _SparkleFade;

#endif

uniform fixed3 _WaterFogColor;
uniform fixed _WaterFogDensity;

uniform sampler3D _CausticsTexture;
uniform fixed4 _WaterCausticsTintColor;
uniform fixed4 _CausticsScale;
uniform fixed4 _CausticsVelocity;

// colors in use
uniform fixed4 _SpecularColor;
uniform fixed _SpecularIntensity;
uniform fixed4 _WaterColor;
uniform fixed _WaterRefractionStrength;

// fade params
uniform float4 _InvFadeParameter;

// specularity
uniform float _Shininess;

// fresnel, vertex & bump displacements & strength
uniform float4 _DistortParams;
uniform float _FresnelScale;
uniform float4 _BumpTiling;
uniform float4 _BumpDirection;

// Scale, intensity, depth fade, wave factor
uniform float4 _WaterFoamParam1;
uniform float4 _WaterFoamParam2;

// water floor for shadow / depth pass
uniform fixed _WaterDepthThreshold;

uniform fixed _WaterDitherLevel;
uniform fixed _WaterShadowStrength;
uniform uint _WaterUnderwater;
uniform uint _WaterReflective;
static const uint _WaterAboveWater = (_WaterUnderwater == 0);

// unused currently
uniform int _VolumetricSampleCount;
uniform fixed _VolumetricSampleMaxDistance;
uniform fixed _VolumetricSampleDither;
uniform fixed _VolumetricShadowPower;
uniform fixed _VolumetricShadowPowerFade;
uniform fixed _VolumetricShadowMinShadow;

// waves
uniform float4 _WaterWave1;
uniform float4 _WaterWave1_Precompute;
uniform float4 _WaterWave1_Params1;
uniform float4 _WaterWave2;
uniform float4 _WaterWave2_Precompute;
uniform float4 _WaterWave2_Params1;
uniform float4 _WaterWave3;
uniform float4 _WaterWave3_Precompute;
uniform float4 _WaterWave3_Params1;
uniform float4 _WaterWave4;
uniform float4 _WaterWave4_Precompute;
uniform float4 _WaterWave4_Params1;
uniform float4 _WaterWave5;
uniform float4 _WaterWave5_Precompute;
uniform float4 _WaterWave5_Params1;
uniform float4 _WaterWave6;
uniform float4 _WaterWave6_Precompute;
uniform float4 _WaterWave6_Params1;
uniform float4 _WaterWave7;
uniform float4 _WaterWave7_Precompute;
uniform float4 _WaterWave7_Params1;
uniform float4 _WaterWave8;
uniform float4 _WaterWave8_Precompute;
uniform float4 _WaterWave8_Params1;
uniform float _WaterWaveMultiplier;
uniform float4 _WeatherMakerWaterDepthMaxUV;
uniform float4 _WaterDisplacement1;
uniform float4 _WaterDisplacement2;

static const float waterShadowStrengthSaturated = max(0.01, _WaterShadowStrength);

// shortcuts
#define PER_PIXEL_DISPLACE _DistortParams.x
#define REALTIME_DISTORTION _DistortParams.y
#define FRESNEL_POWER _DistortParams.z
#define FRESNEL_BIAS _DistortParams.w
#define DISTANCE_SCALE 0.01

struct appdata_water
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 uv : TEXCOORD0;
	WM_BASE_VERTEX_INPUT
};

struct v2fWater
{
	float4 pos : SV_POSITION;
	float4 normal : NORMAL;
	float4 rayDir : TEXCOORD0;
	float4 bumpCoords : TEXCOORD1;
	float4 reflectionPos : TEXCOORD2;
	float4 refractionPos : TEXCOORD3;
	float4 worldPos : TEXCOORD4;
	float4 viewPos : TEXCOORD5;
	float4 uv : TEXCOORD6;
	WM_BASE_VERTEX_TO_FRAG
};

struct appdata_water_shadow_cast
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 uv : TEXCOORD0;
	WM_BASE_VERTEX_INPUT
};

struct v2fs
{

#if defined(UNITY_URP)

	float4 pos : SV_POSITION;

#else

	V2F_SHADOW_CASTER;

#endif

	WM_BASE_VERTEX_TO_FRAG
};

// use camera y depth to get water depth (0-1) at uv from top water plane
float GetWaterYDepth(float2 uv)
{
	UNITY_BRANCH
	if (_WeatherMakerYDepthParams.w == 0.0)
	{
		return 1.0;
	}
	else
	{
		uv = saturate(lerp(_WeatherMakerWaterDepthMaxUV.x, _WeatherMakerWaterDepthMaxUV.y, uv));
		float yDepth = tex2Dlod(_WeatherMakerYDepthTexture, float4(uv, 0.0, 0.0)).r;

#if defined(UNITY_REVERSED_Z)

		yDepth = 1.0 - yDepth;

#endif

		yDepth = max(0.0, yDepth - _WeatherMakerYDepthParams.y);
		yDepth *= _WeatherMakerYDepthParams.w;
		return yDepth;
	}
}

// get water height from GetWaterYDepth
float GetWaterHeight(float yDepth)
{
	return _WeatherMakerYDepthParams.x * yDepth;
}

// apply normal map to vertex normal
half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength)
{
	half4 bump = (tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw)) * 0.5;
	half3 normal = UnpackNormal(bump);
	half3 worldNormal = vertexNormal + normal.xxy * bumpStrength * half3(1, 0, 1);
	return normalize(worldNormal);
}

half3 PerPixelNormalLod(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength, float lod)
{
	half4 bump = (tex2Dlod(bumpMap, float4(coords.xy, 0.0, lod)) + tex2Dlod(bumpMap, float4(coords.zw, 0.0, lod))) * 0.5;
	half3 normal = UnpackNormal(bump);
	half3 worldNormal = vertexNormal + normal.xxy * bumpStrength * half3(1, 0, 1);
	return normalize(worldNormal);
}

// get normal from bump map
half3 PerPixelNormalUnpacked(sampler2D bumpMap, half4 coords, half bumpStrength)
{
	half4 bump = (tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw)) * 0.5;
	half3 normal = UnpackNormal(bump);
	normal.xy *= bumpStrength;
	return normalize(normal);
}

// apply a single gerstner wave
void WaterApplyGerstnerWave(float4 wave, float4 wavePrecompute, float4 waveParams1, float3 baseWorldPos, inout float4 worldPos, inout float3 tangent, inout float3 binormal, float reducer)
{
	UNITY_BRANCH
	if ((wave.x != 0 || wave.y != 0.0) && wave.z > 0.0 && wave.w > 0.0)
	{
		float steepness = wave.z;
		static const float svScale = 0.1;
		float sv = 1.0 + ((1.0 + (sin((((worldPos.x + worldPos.y + _WeatherMakerTime.x) * svScale))))) * waveParams1.z);
		float wavelength = wave.w;
		//float k = 2 * UNITY_PI / wavelength;
		//float c = sqrt(9.8 / k);
		//float a = steepness / k;
		float k = wavePrecompute.x;
		float c = wavePrecompute.y;
		float a = lerp(wavePrecompute.z * reducer * sv, wavePrecompute.z, waveParams1.y) * _WaterWaveMultiplier; // reduce by height according to height reduce parameter waveParams1.y
		float2 d = (wave.xy);
		float f = k * (dot(d, baseWorldPos.xz) - c);
		float sinF;
		float cosF;
		sincos(f, sinF, cosF);
		float steepnessSinF = steepness * sinF;
		float steepnessCosF = steepness * cosF;
		float dxSteepnessSinF = d.x * steepnessSinF;
		float negDxSteepnessSinF = -d.x * steepnessSinF;
		float dySteepnessSinF = d.y * steepnessCosF;
		float negDySteepnessSinF = -d.y * dySteepnessSinF;
		float negDxDySteepnessSinF = -d.x * dySteepnessSinF;

		tangent += float3(negDxSteepnessSinF, d.x * steepnessCosF, negDxDySteepnessSinF);
		binormal += float3(negDxDySteepnessSinF, d.y * steepnessCosF, -d.y * dySteepnessSinF);
		worldPos.xyz += float3(d.x * (a * cosF), a * sinF, d.y * (a * cosF));
	}
}

// apply all gerstner waves
float4 WaterApplyWaves(inout float4 vertexPos, inout float4 normal, out float4 bumpCoords, inout float waterHeight, float depth01)
{
	// perform calculations in world space
	float4 worldPos = mul(unity_ObjectToWorld, vertexPos);
	float origY = worldPos.y;
	float lod = distance(WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET, worldPos.xyz);
	bumpCoords = (worldPos.xzxz + (_WeatherMakerTime.x * _BumpDirection.xyzw)) * _BumpTiling.xyzw;

	UNITY_BRANCH
	if (normal.w > 0.99)
	{
		if (((_WaterWave1.x != 0.0 || _WaterWave1.y != 0.0) && (_WaterWave1.z > 0.0 && _WaterWave1.w > 0.0)) ||
			((_WaterWave2.x != 0.0 || _WaterWave2.y != 0.0) && (_WaterWave2.z > 0.0 && _WaterWave2.w > 0.0)) ||
			((_WaterWave3.x != 0.0 || _WaterWave3.y != 0.0) && (_WaterWave3.z > 0.0 && _WaterWave3.w > 0.0)) ||
			((_WaterWave4.x != 0.0 || _WaterWave4.y != 0.0) && (_WaterWave4.z > 0.0 && _WaterWave4.w > 0.0)) ||
			((_WaterWave5.x != 0.0 || _WaterWave5.y != 0.0) && (_WaterWave5.z > 0.0 && _WaterWave5.w > 0.0)) ||
			((_WaterWave6.x != 0.0 || _WaterWave6.y != 0.0) && (_WaterWave6.z > 0.0 && _WaterWave6.w > 0.0)) ||
			((_WaterWave7.x != 0.0 || _WaterWave7.y != 0.0) && (_WaterWave7.z > 0.0 && _WaterWave7.w > 0.0)) ||
			((_WaterWave8.x != 0.0 || _WaterWave8.y != 0.0) && (_WaterWave8.z > 0.0 && _WaterWave8.w > 0.0)))
		{

#if defined(TESSELATION_ENABLE)

			UNITY_BRANCH
			if (_WaterDisplacement1.x > 0.0)
			{
				// slight wave based on normal
				float lod2 = clamp(lod * 0.02, 0.0, 6.0);
				float4 uv = float4((worldPos.xz + (_WeatherMakerTime.x * _WaterDisplacement1.zw)) * _WaterDisplacement1.x, 0.0, lod2);
				float displacement = tex2Dlod(_WaterDisplacementMap, uv).a * _WaterDisplacement1.y;
				if (_WaterDisplacement2.x > 0.0)
				{
					uv = float4((worldPos.xz + (_WeatherMakerTime.x * _WaterDisplacement2.zw)) * _WaterDisplacement2.x, 0.0, lod2);
					displacement += (tex2Dlod(_WaterDisplacementMap, uv).a * _WaterDisplacement2.y);
				}
				worldPos.xyz += (normal * displacement * _WaterWaveMultiplier);
			}

#endif

			float3 tangent = float3(1.0, 0.0, 0.0);
			float3 binormal = float3(0.0, 0.0, 1.0);
			float4 baseWorldPos = worldPos;
			float reducer = depth01;
			WaterApplyGerstnerWave(_WaterWave1, _WaterWave1_Precompute, _WaterWave1_Params1, baseWorldPos, worldPos, tangent, binormal, reducer);
			WaterApplyGerstnerWave(_WaterWave2, _WaterWave2_Precompute, _WaterWave2_Params1, baseWorldPos, worldPos, tangent, binormal, reducer);
			WaterApplyGerstnerWave(_WaterWave3, _WaterWave3_Precompute, _WaterWave3_Params1, baseWorldPos, worldPos, tangent, binormal, reducer);
			WaterApplyGerstnerWave(_WaterWave4, _WaterWave4_Precompute, _WaterWave4_Params1, baseWorldPos, worldPos, tangent, binormal, reducer);
			WaterApplyGerstnerWave(_WaterWave5, _WaterWave5_Precompute, _WaterWave5_Params1, baseWorldPos, worldPos, tangent, binormal, reducer);
			WaterApplyGerstnerWave(_WaterWave6, _WaterWave6_Precompute, _WaterWave6_Params1, baseWorldPos, worldPos, tangent, binormal, reducer);
			WaterApplyGerstnerWave(_WaterWave7, _WaterWave7_Precompute, _WaterWave7_Params1, baseWorldPos, worldPos, tangent, binormal, reducer);
			WaterApplyGerstnerWave(_WaterWave8, _WaterWave8_Precompute, _WaterWave8_Params1, baseWorldPos, worldPos, tangent, binormal, reducer);
			normal.xyz = normalize(cross(binormal, tangent));

			// re-assign vertex back from world space
			vertexPos = mul(unity_WorldToObject, worldPos);

			waterHeight += max(0.0, worldPos.y - origY);
		}
	}
	else
	{
		normal.xyz = float3(0.0, 1.0, 0.0);
	}

	worldPos.w = lod;
	return worldPos;
}

// return water foam color
fixed4 WaterFoamColor(float4 worldPos, float3 viewVector, float4 bumpCoords, float waterAmount, float distanceReducer, inout float3 worldNormal)
{
	UNITY_BRANCH
	if (_WaterFoamParam1.x <= 0.0 || _WaterFoamParam1.y <= 0.0 || distanceReducer <= 0.0 || (_WaterFoamParam1.z <= 0.0 && _WaterFoamParam1.w <= 0.0))
	{
		return fixed4Zero;
	}
	else
	{
		// reduce foam by increase depth
		fixed depthFactor = saturate(1.0 - (_WaterFoamParam1.z * waterAmount));

		// increase foam by increase wave
		fixed waveFactor = saturate((worldPos.y - _WeatherMakerFogBoxMax.y) * _WaterFoamParam1.w);

		// foam is still if depth factor is greater than wave factor
		fixed foamVelFactor = max(0.0, ceil(waveFactor - depthFactor));
		float2 foamUV = (_WeatherMakerTime.x * _WaterFoamParam2.xy);

		// get foam color from both bump coords and average
		fixed4 foamColor = (tex2D(_WaterFoam, _WaterFoamParam1.x * (bumpCoords.xy + foamUV)) + tex2D(_WaterFoam, _WaterFoamParam1.x * (bumpCoords.zw + foamUV))) * 0.5;

		// apply intensity, depth factor and wave factor
		fixed factor = saturate(distanceReducer * _WaterFoamParam1.y * max(depthFactor, waveFactor));
		foamColor.a = factor * factor;
		//worldNormal = PerPixelNormal(_WaterFoamBumpMap, bumpCoords, worldNormal, PER_PIXEL_DISPLACE * foamColor.a);
		return foamColor;
	}
}

// get color behind water with refraction and fog
float3 ColorBehindWater(float4 worldPos, float3 rayDir, float4 screenPos, float3 viewPos, float2 uvOffset, inout float atten, out float waterAmount, out float fogFactor, out float sceneZ, out float3 depthPos)
{
	float div = 1.0 / max(0.0001, screenPos.w);

	// account for perpective distortion
	float2 baseUV = screenPos.xy * div;

	UNITY_BRANCH
	if (_WaterRefractionStrength <= 0.0)
	{
		waterAmount = 20.0;
		fogFactor = 0.1;
		sceneZ = LinearEyeDepth(WM_SAMPLE_DEPTH(baseUV));
		depthPos = worldPos;
		return _WaterColor.rgb;
	}
	else
	{
		uvOffset *= div;

		// fractional pixel removal
		uvOffset.y *= _CameraDepthTexture_TexelSize.z * abs(_CameraDepthTexture_TexelSize.y);

		// get depth value at fully distorted pixel
		float2 uv = AlignScreenUVWithDepthTexel(baseUV + uvOffset);
		float depth = LinearEyeDepth(WM_SAMPLE_DEPTH(uv));

		// water depth
		float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);

		// difference
		float depthDifference = depth - surfaceDepth;

		// scale down if negative depth or close to 0 depth
		uvOffset *= saturate(depthDifference);

		// get new uv
		uv = AlignScreenUVWithDepthTexel(baseUV + uvOffset);

		// set depth to corrected value
		depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));

		// set sceneZ
		sceneZ = length(depth / viewPos.z);

		if (_WaterUnderwater)
		{
			// in the water, use depth or distance to edge of water, whichever is smaller
			waterAmount = min(sceneZ, worldPos.w);
			depthPos = WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET + (rayDir * waterAmount);
		}
		else
		{
			// set depth to non-refracted value
			float nonRefractionDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, baseUV));
			float nonRefractionSceneZ = length(nonRefractionDepth / viewPos.z);
			float3 rayDistance = (rayDir * nonRefractionSceneZ);

			// compute depth with non-origin offset pos
			depthPos = WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET + rayDistance;
			waterAmount = distance(worldPos.xyz, depthPos);

			// now set to world pos with origin offset
			depthPos = WEATHER_MAKER_CAMERA_POS + rayDistance;
		}

		fogFactor = saturate(exp(-_WaterFogDensity * waterAmount));
		fixed3 backgroundColor;

		// fetch background at uv
		backgroundColor = WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraOpaqueTexture, uv).rgb * _WaterRefractionStrength;

		if (_WaterUnderwater)
		{
			// TODO: Note, this assumes the sun intensity is being reduced as the player goes lower into the water
			// TODO: Light water fog by additional lights? volumetric light? shadows?
			atten = CalculateDirLightDepthShadowPower(depthPos, 0);
			return backgroundColor;
		}
		else
		{
			atten = CalculateDirLightScreenShadowPower(baseUV);
			static const float3 dirLightColor = (_WeatherMakerDirLightColor[0].rgb * _WeatherMakerDirLightColor[0].a) + (_WeatherMakerDirLightColor[1].rgb * _WeatherMakerDirLightColor[1].a);
			return lerp(_WaterFogColor * dirLightColor * atten, backgroundColor, fogFactor);
		}
	}
}

// get fresnel factor (spreads specular light out)
half Fresnel(half3 viewVector, half3 worldNormal, half bias, half power)
{
	half facing = clamp(1.0 - max(dot(-viewVector, worldNormal), 0.0), 0.0, 1.0);
	half refl2Refr = saturate(bias + (1.0 - bias) * pow(facing, power));
	return refl2Refr;
}

// calculate caustics (little wavy lines at bottom of pools)
fixed3 ComputeWaterCaustics(float4 worldPos, float3 rayDir, float3 depthPos, float waterAmount, float2 distortOffset, fixed reducer, float2 screenUV)
{
	// reduce with tiny water height, fade linear, z value is shallow water fade factor
	float height = (_WeatherMakerFogBoxMax.y - depthPos.y);
	reducer *= saturate(height * _CausticsScale.z);

	// reduce with increased water height, fade linear
	reducer *= saturate(16.0 / height);

	// reduce with increased distance to water surface from eye, fade linear
	reducer /= max(1.0, _WaterAboveWater * worldPos.w * 0.01);

	UNITY_BRANCH
	if (reducer <= 0.0)
	{
		return fixed3Zero;
	}
	else
	{
		// calculate shadow, reduce caustics by shadow and sun light intensity
		float shadowAmount = CalculateDirLightScreenShadowPower(screenUV);
		reducer *= pow(shadowAmount * min(1.0, _WeatherMakerDirLightColor[0].a), 4.0);

		// if rotating to light, no need to swap z and y as the rotation does it
		float3 samplePos = RotatePointZeroOriginQuaternion(depthPos, _WeatherMakerDirLightQuaternion[0]);

		// adjust z (depth) animation
		float3 velocity = _CausticsVelocity * _WeatherMakerTime.x;
		samplePos = float3(samplePos.xy * _CausticsScale.x, velocity.y);

		// adjust texture lookup animation
		samplePos.xy -= (velocity.xz * _BumpDirection.xy);

		// adjust distortion animation
		samplePos.xy += (distortOffset * _CausticsScale.w);

		// take sample pos and lookup caustics
		fixed causticsLookupValue = tex3D(_CausticsTexture, samplePos).a * _CausticsScale.y;

		return (causticsLookupValue * reducer * _WeatherMakerDirLightPower[0].z) * _WaterCausticsTintColor * _WeatherMakerDirLightColor[0].rgb;
	}
}

// compute water surface color from all lights in one function, no need for more than one pass
void ComputeWaterColorAllLight(float3 worldPos, float3 worldNormal, float3 viewVector, float2 screenUV,
	fixed shadowPowerReducer, float distanceToWorldPosSquared, inout fixed4 baseColor, fixed4 specColor)
{
	wm_world_space_light_params p;
	p.worldPos = worldPos;
	p.worldNormal = worldNormal;
	p.diffuseColor = baseColor;
	p.ambientColor = _WeatherMakerAmbientLightColorGround;
	p.shadowStrength = waterShadowStrengthSaturated;

#if !defined(WEATHER_MAKER_LIGHT_NO_SPECULAR)

	p.rayDir = viewVector;
	p.specularColor = specColor.rgb * specColor.a;
	p.specularPower = _Shininess * lerp(32.0, 2.0, _WaterUnderwater);

#if defined(WEATHER_MAKER_LIGHT_SPECULAR_SPARKLE)

	//p.sparkleNoise = _SparkleNoise;
	p.sparkleTintColor = _SparkleTintColor.rgb * _SparkleTintColor.a * specColor.a;
	p.sparkleScale = _SparkleScale;
	p.sparkleOffset = _SparkleOffset;
	p.sparkleFade = _SparkleFade;
	p.distanceToWorldPosSquared = distanceToWorldPosSquared;

#endif

#endif

	baseColor.rgb = CalculateLightColorWorldSpace(p);
}

// return water color for the given worldPos
fixed4 ComputeWaterColor
(
	float4 bumpCoords,
	float4 normal,
	float4 rayDir,
	float4 reflectionPos,
	float4 refractionPos,
	float4 viewPos,
	float4 worldPos,
	float4 uv,
	float2 screenUV,
	float atten,
	out float3 worldNormal
)
{
	//worldNormal = float3(0, 0, 0); return fixed4(uv.zzz, 1.0);

	// normalize view pos
	viewPos.xyz = normalize(viewPos.xyz);

	// distance reducer, water farther away smooths out
	fixed distanceReducer = rayDir.w;

	// ray direction
	float3 viewVector = normalize(rayDir.xyz);

	// used to fade out sparkles
	float distanceToWorldPosSquared = viewPos.w;

	// output parameters
	float waterAmount;
	float fogFactor;
	float sceneZ;
	float3 depthPos;
	float2 distortOffset;
	fixed waterHeight;
	float showCaustics;
	float noCaustics;

	UNITY_BRANCH
	if (_WaterUnderwater)
	{
		// if underwater, do not show caustics when looking up through water surface
		showCaustics = (normal.w < 0.99);
		noCaustics = (1.0 - showCaustics);

		// get water normal, no water normal for non-water surface
		worldNormal = (noCaustics * PerPixelNormal(_WaterBumpMap, bumpCoords, normal.xyz, PER_PIXEL_DISPLACE)) + (normal.xyz * showCaustics);
		worldNormal = lerp(float3(0.0, 1.0, 0.0), worldNormal, distanceReducer);
		distortOffset = worldNormal.xz * REALTIME_DISTORTION * noCaustics;
		waterHeight = 0.0;
	}
	else
	{
		// caustics always show if above water
		showCaustics = 1.0;
		noCaustics = 0.0;

		// use water height to show shoreline
		waterHeight = uv.w;
		worldNormal = PerPixelNormal(_WaterBumpMap, bumpCoords, normal.xyz, PER_PIXEL_DISPLACE);
		worldNormal = lerp(float3(0.0, 1.0, 0.0), worldNormal, distanceReducer);
		distortOffset = worldNormal.xz * REALTIME_DISTORTION;
	}

	// calculate reflection and refraction
	fixed reflectionDistortReducer = min(1.0, worldPos.w * 0.02);
	float2 reflectionUV = (reflectionPos.xy + (distortOffset * reflectionDistortReducer)) / max(0.001, reflectionPos.w);
	fixed3 colorBehindWater = ColorBehindWater(worldPos, viewVector, refractionPos, viewPos, distortOffset, atten, waterAmount, fogFactor, sceneZ, depthPos);
	fixed4 rtRefractions = fixed4(colorBehindWater, 1.0);
	fixed foamSpecularReducer;
	fixed inFrontOfDepthBuffer;
	fixed minWaterAmount;
	fixed4 foamColor;

	UNITY_BRANCH
	if (_WaterUnderwater)
	{
		// if the water is beyond the depth buffer, use a down normal to not light it
		inFrontOfDepthBuffer = (worldPos.w < sceneZ);
		worldNormal = lerp(float3(0.0, -1.0, 0.0), worldNormal, inFrontOfDepthBuffer);
		foamSpecularReducer = 1.0;
		minWaterAmount = 1.0;
		foamColor = fixed4Zero;
	}
	else
	{
		// above water, calculate foam
		inFrontOfDepthBuffer = 1.0;
		minWaterAmount = min(waterHeight, waterAmount);
		foamColor = WaterFoamColor(worldPos, viewVector, bumpCoords, minWaterAmount, distanceReducer, worldNormal);
		foamSpecularReducer = 1.0 - min(1.0, (_WaterFoamParam2.z * foamColor.a));
	}

	// shading for fresnel term
	worldNormal.xz *= _FresnelScale;
	fixed shadowPowerReducer;
	fixed4 rtReflections;
	fixed reflectionStrength;

	UNITY_BRANCH
	if (_WaterReflective)
	{
		// compute reflection color
		rtReflections = lerp(tex2D(_WeatherMakerWaterReflectionTex, reflectionUV),
			tex2D(_WeatherMakerWaterReflectionTex2, reflectionUV), reflectionPos.z);
		rtReflections.a = 1.0;
		reflectionStrength = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER) * (FRESNEL_BIAS > -100.0) * foamSpecularReducer * _InvFadeParameter.z;
		shadowPowerReducer = (1.0 - reflectionStrength);
	}
	else
	{
		rtReflections = rtRefractions;
		shadowPowerReducer = 1.0;
		reflectionStrength = 0.0;
	}

	if (_WaterCausticsTintColor.a > 0.0 && _CausticsScale.x > 0.0 && _CausticsScale.y > 0.0 && showCaustics == 1.0)
	{
		fixed reducer = shadowPowerReducer * foamSpecularReducer * atten * fogFactor * saturate(3.0 - (REALTIME_DISTORTION + PER_PIXEL_DISPLACE));
		rtRefractions.rgb += ComputeWaterCaustics(worldPos, viewVector, depthPos, waterAmount, distortOffset, reducer, screenUV);
	}

	// base, depth & reflection colors
	fixed4 baseColor = _WaterColor;
	baseColor *= lerp(fixed4One, ((tex2D(_WaterTex, bumpCoords.xy) + tex2D(_WaterTex, bumpCoords.zw)) * 0.5), distanceReducer * inFrontOfDepthBuffer);
	rtRefractions *= (1.0 - baseColor.a);

	// fade / alpha
	if (_WaterUnderwater)
	{
		baseColor.a = 1.0;
	}
	else
	{
		// fade shoreline if above water
		fixed invFade = saturate(minWaterAmount * _InvFadeParameter.x);
		baseColor.rgb = DO_ALPHA_BLEND(foamColor, baseColor);
		baseColor.a = invFade;
	}

	// lighting
	fixed4 specColor = _SpecularColor;
	specColor.a *= _SpecularIntensity * foamSpecularReducer;
	float3 lightPos = lerp(worldPos, depthPos, _WaterUnderwater);

	// compure water color
	ComputeWaterColorAllLight(lightPos, worldNormal, viewVector, screenUV, shadowPowerReducer, distanceToWorldPosSquared, baseColor, specColor);

	fixed3 emissive = lerp(rtRefractions.rgb, rtReflections.rgb, reflectionStrength * _WaterReflective);

	// add emissive color (refraction and/or reflection)
	baseColor.rgb += lerp(emissive, emissive * _WaterColor.rgb, _WaterColor.a);

	if (_WaterUnderwater)
	{
		// fog
		fixed4 fogColor = fixed4(_WaterFogColor * _WeatherMakerDirLightColor[0].rgb, 1.0 - fogFactor);
		baseColor.rgb = DO_ALPHA_BLEND(fogColor, baseColor);
	}

	// dither
	ApplyDither(baseColor.rgb, worldPos.xz + worldPos.yy, _WaterDitherLevel);

	return baseColor;
}

v2fWater vertWater(appdata_water v)
{
	WM_INSTANCE_VERT(v, v2fWater, o);

	// tesselation can change uv.w to non-zero to clip out
	UNITY_BRANCH
	if (v.uv.w > 0.0)
	{
		float yDepth = GetWaterYDepth(v.uv);
		o.uv = float4(v.uv.xy, yDepth, GetWaterHeight(yDepth));
		o.normal = float4(v.normal, v.normal.y);
		o.worldPos = WaterApplyWaves(v.vertex, o.normal, o.bumpCoords, o.uv.w, yDepth);
		o.pos = UnityObjectToClipPos(v.vertex);
		o.viewPos.xyz = UnityObjectToViewPos(v.vertex);
		o.viewPos.w = o.worldPos.w * o.worldPos.w;
		o.rayDir.xyz = -WorldSpaceViewDir(v.vertex);
		o.rayDir.w = GET_WATER_DISTANCE_REDUCER(o.worldPos);
		o.reflectionPos = ComputeNonStereoScreenPos(o.pos);
		o.refractionPos = ComputeScreenPos(o.pos);

#if defined(UNITY_SINGLE_PASS_STEREO)

		o.reflectionPos.z = unity_StereoEyeIndex;

#else

		// When not using single pass stereo rendering, eye index must be determined by testing the
		// sign of the horizontal skew of the projection matrix.
		o.reflectionPos.z = (unity_CameraProjection[0][2] > 0.0);

#endif

		if (_WaterUnderwater)
		{
			// recompute to far plane position, underwater in a huge water volume can have clip holes
			//  where the camera far plane does not reach the edge of the water
			o.pos = UnityObjectToClipPosFarPlane(v.vertex);
		}
	}
	else
	{
		o.pos = -999999.0;
		o.uv = -999999.0;
		o.normal = 0.0;
		o.worldPos = 0.0;
		o.viewPos = 0.0;
		o.rayDir = 0.0;
		o.reflectionPos = 0.0;
		o.refractionPos = 0.0;
		o.bumpCoords = 0.0;
	}

	return o;
}

fixed4 fragWaterForward(v2fWater i) : SV_Target
{
	WM_INSTANCE_FRAG(i);

	clip(i.uv.w);

	UNITY_BRANCH
	if (_WaterAboveWater)
	{
		// block precipitation, fog, etc. from the water
		ClipWorldPosNullZones(i.worldPos);
	}

	float3 worldNormal;

	// all lights in one pass
	return ComputeWaterColor(i.bumpCoords, i.normal, i.rayDir, i.reflectionPos, i.refractionPos, i.viewPos, i.worldPos, i.uv, i.refractionPos.xy / i.refractionPos.w, -1.0, worldNormal);
}

v2fs vertWaterShadow(appdata_water_shadow_cast v)
{
	WM_INSTANCE_VERT(v, v2fs, o);

	float yDepth = GetWaterYDepth(v.uv);
	float4 normal = float4(v.normal, v.normal.y);
	float4 bumpCoords;
	float waterHeight = 0.0;
	WaterApplyWaves(v.vertex, normal, bumpCoords, waterHeight, yDepth);
	v.vertex.y -= _WaterDepthThreshold;
	v.normal = normal.xyz;

#if defined(UNITY_URP)

	o.pos = UnityObjectToClipPos(v.vertex);// mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1.0)));

#else

	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

#endif

	return o;
}

float4 fragWaterShadow(v2fs i, float facing : VFACE) : SV_Target
{
	WM_INSTANCE_FRAG(i);

#if defined(UNITY_URP)

	return 0.0;

#else
	
	float isFrontFace = (facing >= 0 ? 1 : 0);
	float faceSign = (facing >= 0 ? 1 : -1);
	
	SHADOW_CASTER_FRAGMENT(i)

#endif

}

#endif
