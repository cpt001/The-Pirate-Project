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

Shader "WeatherMaker/WeatherMakerWeatherMapShader"
{
	Properties
	{
		[Header(Cloud Coverage)]
		_CloudCoverageNoiseType("Noise type", Range(0.0, 1.0)) = 0.0
		_CloudCoverageFrequency("Frequency", Range(0.1, 64.0)) = 6.0
		_CloudCoverageRotation("Rotation", Vector) = (0.0, 0.0, 0.0, 0.0)
		_CloudCoverageVelocity("Velocity", Vector) = (0.01, 0.01, 0.01)
		_CloudCoverageOffset("Offset", Vector) = (0.0, 0.0, 0.0)
		_CloudCoverageAdder("Adder", Range(-1.0, 1.0)) = 0.0
		_CloudCoveragePower("Power", Range(0.0, 16.0)) = 1.0
		[NoScaleOffset] _CloudCoverageTexture("Coverage Texture", 2D) = "black" {} // additional cloud coverage
		_CloudCoverageTextureMultiplier("Coverage texture multiplier", Range(0.0, 1.0)) = 0.0
		_CloudCoverageTextureScale("Coverage texture scale", Range(0.0, 1.0)) = 1.0
		_CloudCoverageWarpScale("Coverage warp scale (xy), multiplier (zw)", Vector) = (0.0, 0.0, 0.0, 0.0)

		[Header(Cloud Coverage Negation)]
		_CloudCoverageNegationFrequency("Frequency", Range(0.1, 64.0)) = 6.0
		_CloudCoverageNegationRotation("Rotation", Vector) = (0.0, 0.0, 0.0, 0.0)
		_CloudCoverageNegationVelocity("Velocity", Vector) = (0.01, 0.01, 0.01)
		_CloudCoverageNegationOffset("Offset", Vector) = (0.0, 0.0, 0.0)
		_CloudCoverageNegationMultiplier("Multiplier", Range(0.0, 100.0)) = 1.0
		_CloudCoverageNegationAdder("Adder", Range(-1.0, 1.0)) = 0.0
		_CloudCoverageNegationPower("Power", Range(0.0, 16.0)) = 1.0
		_CloudCoverageNegationWarpScale("Coverage warp scale (xy), multiplier (zw)", Vector) = (0.0, 0.0, 0.0, 0.0)

		[Header(Cloud Density)]
		_CloudDensityFrequency("Frequency", Range(0.1, 64.0)) = 6.0
		_CloudDensityRotation("Rotation", Vector) = (0.0, 0.0, 0.0, 0.0)
		_CloudDensityVelocity("Velocity", Vector) = (0.01, 0.01, 0.01)
		_CloudDensityOffset("Offset", Vector) = (0.0, 0.0, 0.0)
		_CloudDensityAdder("Adder", Range(-1.0, 1.0)) = 0.0
		_CloudDensityPower("Power", Range(0.0, 16.0)) = 1.0
		_CloudDensityProfileInfluence("Profile influence", Range(0.0, 1.0)) = 1.0
		_CloudDensityCoveragePower("Coverage Power", Range(0.0, 1.0)) = 0.3
		[NoScaleOffset] _CloudDensityTexture("Density Texture", 2D) = "black" {} // additional cloud Density
		_CloudDensityTextureMultiplier("Density texture multiplier", Range(0.0, 1.0)) = 0.0
		_CloudDensityTextureScale("Density texture scale", Range(0.0, 1.0)) = 1.0
		_CloudDensityWarpScale("Density warp scale (xy), multiplier (zw)", Vector) = (0.0, 0.0, 0.0, 0.0)

		[Header(Cloud Density)]
		_CloudDensityNegationFrequency("Frequency", Range(0.1, 64.0)) = 6.0
		_CloudDensityNegationRotation("Rotation", Vector) = (0.0, 0.0, 0.0, 0.0)
		_CloudDensityNegationVelocity("Velocity", Vector) = (0.01, 0.01, 0.01)
		_CloudDensityNegationOffset("Offset", Vector) = (0.0, 0.0, 0.0)
		_CloudDensityNegationMultiplier("Multiplier", Range(0.0, 100.0)) = 1.0
		_CloudDensityNegationAdder("Adder", Range(-1.0, 1.0)) = 0.0
		_CloudDensityNegationPower("Power", Range(0.0, 16.0)) = 1.0
		_CloudDensityNegationWarpScale("Density warp scale (xy), multiplier (zw)", Vector) = (0.0, 0.0, 0.0, 0.0)

		[Header(Cloud Type)]
		_CloudTypeFrequency("Frequency", Range(0.1, 64.0)) = 6.0
		_CloudTypeRotation("Rotation", Vector) = (0.0, 0.0, 0.0, 0.0)
		_CloudTypeVelocity("Velocity", Vector) = (0.01, 0.01, 0.01)
		_CloudTypeOffset("Offset", Vector) = (0.0, 0.0, 0.0)
		_CloudTypeAdder("Adder", Range(-1.0, 1.0)) = 0.0
		_CloudTypePower("Power", Range(0.0, 16.0)) = 1.0
		_CloudTypeProfileInfluence("Profile influence", Range(0.0, 1.0)) = 1.0
		_CloudTypeCoveragePower("Coverage Power", Range(0.0, 1.0)) = 0.3
		[NoScaleOffset] _CloudTypeTexture("Type Texture", 2D) = "black" {} // additional cloud type
		_CloudTypeTextureMultiplier("Type texture multiplier", Range(0.0, 1.0)) = 0.0
		_CloudTypeTextureScale("Type texture scale", Range(0.0, 1.0)) = 1.0
		_CloudTypeWarpScale("Type warp scale (xy), multiplier (zw)", Vector) = (0.0, 0.0, 0.0, 0.0)

		[Header(Cloud Type Negation)]
		_CloudTypeNegationFrequency("Frequency", Range(0.1, 64.0)) = 6.0
		_CloudTypeNegationRotation("Rotation", Vector) = (0.0, 0.0, 0.0, 0.0)
		_CloudTypeNegationVelocity("Velocity", Vector) = (0.01, 0.01, 0.01)
		_CloudTypeNegationOffset("Offset", Vector) = (0.0, 0.0, 0.0)
		_CloudTypeNegationMultiplier("Multiplier", Range(0.0, 100.0)) = 1.0
		_CloudTypeNegationAdder("Adder", Range(-1.0, 1.0)) = 0.0
		_CloudTypeNegationPower("Power", Range(0.0, 16.0)) = 1.0
		_CloudTypeNegationWarpScale("Type warp scale (xy), multiplier (zw)", Vector) = (0.0, 0.0, 0.0, 0.0)
	}
	SubShader
	{
		Tags { }
		LOD 100
		Blend One Zero
		Fog { Mode Off }
		ZWrite On
		ZTest Always

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			return o;
		}

		ENDCG

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "WeatherMakerCloudVolumetricUniformsShaderInclude.cginc"
			#include "WeatherMakerCloudNoiseShaderInclude.cginc"

			uniform float2 _WeatherMakerWeatherMapMaskOffset;
			uniform sampler2D _WeatherMakerWeatherMapMaskTexture;

#define noiseFunc1 perlinNoise2D

			static const float weights2[2] = { 0.65, 0.35 };
			static const float weights3[3] = { 0.5, 0.35, 0.15 };
			static const float weights4[4] = { 0.5, 0.25, 0.15, 0.1 };

			float WeatherMapNoisePerlin1(float2 P)
			{
				float sum = noiseFunc1(P);
				sum = (sum + 1.0) * 0.5;
				return sum;
			}

			float WeatherMapNoisePerlin2(float2 P)
			{
				float sum = 0.0;
				sum += (noiseFunc1(P) * weights2[0]);
				sum += (noiseFunc1(P * 2.0) * weights2[1]);
				sum = (sum + 1.0) * 0.5;
				return sum;
			}

			float WeatherMapNoisePerlin3(float2 P)
			{
				float sum = 0.0;
				sum += (noiseFunc1(P) * weights3[0]);
				sum += (noiseFunc1(P * 2.0) * weights3[1]);
				sum += (noiseFunc1(P * 4.0) * weights3[2]);
				sum = (sum + 1.0) * 0.5;
				return sum;
			}

			float WeatherMapNoisePerlin4(float2 P)
			{
				float sum = 0.0;
				sum += (noiseFunc1(P) * weights4[0]);
				sum += (noiseFunc1(P * 2.0) * weights4[1]);
				sum += (noiseFunc1(P * 4.0) * weights4[2]);
				sum += (noiseFunc1(P * 8.0) * weights4[3]);
				sum = (sum + 1.0) * 0.5;
				return sum;
			}

			float WeatherMapNoiseWorley1(float2 P)
			{
				float sum = saturate(1.0 - optimizeWorleyNoise2D(P));
				return sum;
			}

			float WeatherMapNoiseWorley2(float2 P)
			{
				static const float weights[2] = { 0.65, 0.35 };
				float sum = 0.0;
				sum += (optimizeWorleyNoise2D(P) * weights2[0]);
				sum += (optimizeWorleyNoise2D(P * 2.0) * weights2[1]);
				sum = saturate(1.0 - sum);
				return sum;
			}

			float WeatherMapNoiseWorley3(float2 P)
			{
				float sum = 0.0;
				sum += (optimizeWorleyNoise2D(P) * weights3[0]);
				sum += (optimizeWorleyNoise2D(P * 2.0) * weights3[1]);
				sum += (optimizeWorleyNoise2D(P * 4.0) * weights3[2]);
				sum = saturate(1.0 - sum);
				return sum;
			}

			float WeatherMapNoiseWorley4(float2 P)
			{
				float sum = 0.0;
				sum += (optimizeWorleyNoise2D(P) * weights4[0]);
				sum += (optimizeWorleyNoise2D(P * 2.0) * weights4[1]);
				sum += (optimizeWorleyNoise2D(P * 4.0) * weights4[2]);
				sum += (optimizeWorleyNoise2D(P * 8.0) * weights4[3]);
				sum = saturate(1.0 - sum);
				return sum;
			}

			float NoiseFunction(float2 P, float noiseType, float noiseTypeInv)
			{
				float noise;

				UNITY_BRANCH
				if (noiseType <= 0.0)
				{
					noise = WeatherMapNoisePerlin2(P);
				}
				else if (noiseType >= 1.0)
				{
					noise = WeatherMapNoiseWorley2(P);
				}
				else
				{
					noise = (WeatherMapNoisePerlin2(P) * noiseTypeInv) + (WeatherMapNoiseWorley2(P) * noiseType);
				}

				return noise;
			}

			// weather map space sample pos, velocity is already scaled
			float3 GetSamplePos(float2 uv, float3 velocity, float freq, float2 rotation)
			{
				uv -= 0.5;
				uv = RotateUV(uv, rotation.x, rotation.y);
				float2 xyPos = uv + RotateUV(weatherMapCameraPos.xz, rotation.x, rotation.y);
				float3 pos = velocity + float3(xyPos * _WeatherMakerWeatherMapScale.xy * freq, 0.0);
				return pos;
			}

			float SampleTexture(sampler2D samp, float2 uv, float2 rotation, float scale, float multiplier)
			{
				UNITY_BRANCH
				if (scale == 0.0f || multiplier <= 0.0f)
				{
					return 0.0f;
				}
				else
				{
					uv = RotateUV(uv, rotation.x, rotation.y) * scale;
					return tex2Dlod(samp, float4(uv, 0.0, 0.0)).a * multiplier;
				}
			}

			float GetCloudType(float2 uv)
			{
				static bool hasTypeNegation = (_CloudTypeNegationFrequency > 0.0 && _CloudTypeNegationPower > 0.0 && _CloudTypeNegationAdder > -1.0);
				static bool hasTypeWarp = (_CloudTypeWarpScale.x > 0.0 && _CloudTypeWarpScale.y > 0.0 && _CloudTypeWarpScale.z > 0.0 && _CloudTypeWarpScale.w > 0.0);
				static bool hasTypeWarpNegation = _CloudTypeNegationWarpScale.x > 0.0 && _CloudTypeNegationWarpScale.y > 0.0 && _CloudTypeNegationWarpScale.z > 0.0 && _CloudTypeNegationWarpScale.w > 0.0;
				static bool maxType = (!hasTypeNegation && _CloudTypeAdder >= 1.0 && cloudTypeInfluence >= 1.0);

				UNITY_BRANCH
				if (maxType)
				{
					// optimization, type always >= 1
					return 1.0;
				}

				float2 uvOrig = uv;
				float typeNoise;

				// compute cloud type noise
				// apply warp if desired
				UNITY_BRANCH
				if (hasTypeWarp)
				{
					float2 typeOffsetNoisePos = (uv * float2(_CloudTypeWarpScale.x, _CloudTypeWarpScale.y));
					float typeOffset = WeatherMapNoisePerlin3(typeOffsetNoisePos);
					uv.x += (_CloudTypeWarpScale.z * typeOffset);
					uv.y += (_CloudTypeWarpScale.w * typeOffset);
				}

				float3 samp = GetSamplePos(uv, cloudTypeVelocity, _CloudTypeFrequency, _CloudTypeRotation);
				typeNoise = SampleTexture(_CloudTypeTexture, uv, _CloudTypeRotation, cloudTypeTextureScale, cloudTypeTextureMultiplier);
				float typeNoiseOrig = NoiseFunction(samp, _CloudTypeNoiseType, _CloudTypeNoiseTypeInv);
				typeNoise = saturate((typeNoiseOrig + typeNoise + _CloudTypeAdder) * cloudTypeInfluence);

				// apply subtraction if desired
				UNITY_BRANCH
				if (hasTypeNegation)
				{
					// apply warp if desired
					UNITY_BRANCH
					if (hasTypeWarpNegation)
					{
						uv = uvOrig;
						float2 typeNegationOffsetNoisePos = (uv * float2(_CloudTypeNegationWarpScale.x, _CloudTypeNegationWarpScale.y));
						float typeNegationOffset = WeatherMapNoisePerlin3(typeNegationOffsetNoisePos);
						uv.x += (_CloudTypeNegationWarpScale.z * typeNegationOffset);
						uv.y += (_CloudTypeNegationWarpScale.w * typeNegationOffset);
					}

					float3 sampTypeNegation = GetSamplePos(uv, cloudTypeNegationVelocity, _CloudTypeNegationFrequency, _CloudTypeNegationRotation);
					float typeNegation = saturate((WeatherMapNoisePerlin3(sampTypeNegation) + _CloudTypeNegationAdder) * _CloudTypeNegationPower);
					typeNoise = typeNoise * (1.0 - typeNegation);
				}

				return typeNoise;
			}

			float GetCloudDensity(float2 uv)
			{
				static bool hasDensityNegation = (_CloudDensityNegationFrequency > 0.0 && _CloudDensityNegationPower > 0.0 && _CloudDensityNegationAdder > -1.0);
				static bool hasDensityWarp = (_CloudDensityWarpScale.x > 0.0 && _CloudDensityWarpScale.y > 0.0 && _CloudDensityWarpScale.z > 0.0 && _CloudDensityWarpScale.w > 0.0);
				static bool hasDensityWarpNegation = _CloudDensityNegationWarpScale.x > 0.0 && _CloudDensityNegationWarpScale.y > 0.0 && _CloudDensityNegationWarpScale.z > 0.0 && _CloudDensityNegationWarpScale.w > 0.0;
				static bool maxDensity = (!hasDensityNegation && _CloudDensityAdder >= 1.0 && cloudDensityInfluence >= 1.0);

				UNITY_BRANCH
				if (maxDensity)
				{
					// optimization, Density always >= 1
					return 1.0;
				}

				float2 uvOrig = uv;
				float densityNoise;

				// compute cloud Density noise
				// apply warp if desired
				UNITY_BRANCH
				if (hasDensityWarp)
				{
					float2 densityOffsetNoisePos = (uv * float2(_CloudDensityWarpScale.x, _CloudDensityWarpScale.y));
					float densityOffset = WeatherMapNoisePerlin3(densityOffsetNoisePos);
					uv.x += (_CloudDensityWarpScale.z * densityOffset);
					uv.y += (_CloudDensityWarpScale.w * densityOffset);
				}

				float3 sampDensity = GetSamplePos(uv, cloudDensityVelocity, _CloudDensityFrequency, _CloudDensityRotation);
				densityNoise = SampleTexture(_CloudDensityTexture, uv, _CloudDensityRotation, cloudDensityTextureScale, cloudDensityTextureMultiplier);
				float densityNoiseOrig = NoiseFunction(sampDensity, _CloudDensityNoiseType, _CloudDensityNoiseTypeInv);
				densityNoise = saturate((densityNoiseOrig + densityNoise + _CloudDensityAdder) * cloudDensityInfluence);

				// apply subtraction if desired
				UNITY_BRANCH
				if (hasDensityNegation)
				{
					// apply warp if desired
					UNITY_BRANCH
					if (hasDensityWarpNegation)
					{
						uv = uvOrig;
						float2 densityNegationOffsetNoisePos = (uv * float2(_CloudDensityNegationWarpScale.x, _CloudDensityNegationWarpScale.y));
						float densityNegationOffset = WeatherMapNoisePerlin3(densityNegationOffsetNoisePos);
						uv.x += (_CloudDensityNegationWarpScale.z * densityNegationOffset);
						uv.y += (_CloudDensityNegationWarpScale.w * densityNegationOffset);
					}

					float3 sampDensityNegation = GetSamplePos(uv, cloudDensityNegationVelocity, _CloudDensityNegationFrequency, _CloudDensityNegationRotation);
					float densityNegation = saturate((WeatherMapNoisePerlin3(sampDensityNegation) + _CloudDensityNegationAdder) * _CloudDensityNegationPower);
					densityNoise = densityNoise * (1.0 - densityNegation);
				}

				return densityNoise;
			}

			float GetCloudCoverage(float2 uv)
			{
				static bool hasCoverageNegation = (_CloudCoverageNegationFrequency > 0.0 && _CloudCoverageNegationPower > 0.0 && _CloudCoverageNegationAdder > -1.0);
				static bool hasCoverageWarp = (_CloudCoverageWarpScale.x > 0.0 && _CloudCoverageWarpScale.y > 0.0 && _CloudCoverageWarpScale.z > 0.0 && _CloudCoverageWarpScale.w > 0.0);
				static bool hasCoverageWarpNegation = _CloudCoverageNegationWarpScale.x > 0.0 && _CloudCoverageNegationWarpScale.y > 0.0 && _CloudCoverageNegationWarpScale.z > 0.0 && _CloudCoverageNegationWarpScale.w > 0.0;
				static bool maxCoverage = (!hasCoverageNegation && _CloudCoverageAdder >= 1.0 && cloudCoverageInfluence >= 1.0);

				UNITY_BRANCH
				if (maxCoverage)
				{
					// optimization, coverage always >= 1
					return 1.0;
				}

				float2 uvOrig = uv;
				float coverageNoise;

				// compute cloud coverage noise
				// apply warp if desired
				UNITY_BRANCH
				if (hasCoverageWarp)
				{
					float2 coverageOffsetNoisePos = (uv * float2(_CloudCoverageWarpScale.x, _CloudCoverageWarpScale.y));
					float coverageOffset = WeatherMapNoisePerlin3(coverageOffsetNoisePos);
					uv.x += (_CloudCoverageWarpScale.z * coverageOffset);
					uv.y += (_CloudCoverageWarpScale.w * coverageOffset);
				}

				float3 sampCoverage = GetSamplePos(uv, cloudCoverageVelocity, _CloudCoverageFrequency, _CloudCoverageRotation);
				coverageNoise = SampleTexture(_CloudCoverageTexture, uv, _CloudCoverageRotation, cloudCoverageTextureScale, cloudCoverageTextureMultiplier);
				float coverageNoiseOrig = NoiseFunction(sampCoverage, _CloudCoverageNoiseType, _CloudCoverageNoiseTypeInv);
				coverageNoise = saturate((coverageNoiseOrig + coverageNoise + _CloudCoverageAdder) * cloudCoverageInfluence);

				// apply subtraction if desired
				UNITY_BRANCH
				if (hasCoverageNegation)
				{
					// apply warp if desired
					UNITY_BRANCH
					if (hasCoverageWarpNegation)
					{
						uv = uvOrig;
						float2 coverageNegationOffsetNoisePos = (uv * float2(_CloudCoverageNegationWarpScale.x, _CloudCoverageNegationWarpScale.y));
						float coverageNegationOffset = WeatherMapNoisePerlin3(coverageNegationOffsetNoisePos);
						uv.x += (_CloudCoverageNegationWarpScale.z * coverageNegationOffset);
						uv.y += (_CloudCoverageNegationWarpScale.w * coverageNegationOffset);
					}

					float3 sampCoverageNegation = GetSamplePos(uv, cloudCoverageNegationVelocity, _CloudCoverageNegationFrequency, _CloudCoverageNegationRotation);
					float coverageNegation = saturate((WeatherMapNoisePerlin3(sampCoverageNegation) + _CloudCoverageNegationAdder) * _CloudCoverageNegationPower);
					coverageNoise = coverageNoise * (1.0 - coverageNegation);
				}

				return coverageNoise;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// mask, if none will be a 1x1 white pixel texture
				fixed mask = tex2Dlod(_WeatherMakerWeatherMapMaskTexture, float4(i.uv.xy + _WeatherMakerWeatherMapMaskOffset, 0.0, 0.0)).a;

				float coverageNoise = GetCloudCoverage(i.uv);
				float densityNoise = GetCloudDensity(i.uv);
				float typeNoise = GetCloudType(i.uv);

				// r = cloud coverage
				// g = density
				// b = cloud type
				// a = sdf (nearest non-empty pixel distance)
				fixed4 result = fixed4(coverageNoise, densityNoise, typeNoise, 1.0);
				result.rgb *= mask;
				return result;
			}

			ENDCG
		}

		// blit with 0 alpha pass
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D _MainTex;

			fixed4 frag (v2f input) : SV_Target
			{
				fixed4 col = tex2Dlod(_MainTex, float4(input.uv, 0.0, 0.0));
				col.a = 1.0;
				return col;
			}

			ENDCG
		}
	}

	Fallback Off
}
