//
// https://github.com/SlightlyMad/AtmosphericScattering
//  Copyright(c) 2016, Michal Skalsky
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification,
//  are permitted provided that the following conditions are met:
//
//  1. Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//
//  2. Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//
//  3. Neither the name of the copyright holder nor the names of its contributors
//     may be used to endorse or promote products derived from this software without
//     specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
//  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT
//  SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
//  TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.



Shader "WeatherMaker/WeatherMakerAtmosphericScatteringLookupShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ZTest ("ZTest", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		CGINCLUDE

		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

		#include "WeatherMakerAtmosphereShaderInclude.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
		};
		
		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 wpos : TEXCOORD1;
		};

		float2 PrecomputeParticleDensity(float3 rayStart, float3 rayDir)
		{
			static const float stepCount = 250;

			float3 planetCenter = ATMOSPHERE_PLANET_CENTER;
			float2 intersection = AtmosphereRaySphereIntersection(rayStart, rayDir, planetCenter, _WeatherMakerAtmospherePlanetRadius);

			UNITY_BRANCH
			if (intersection.x > 0)
			{
				// intersection with planet, write high density
				return 1e+20;
			}
			else
			{
				intersection = AtmosphereRaySphereIntersection(rayStart, rayDir, planetCenter, _WeatherMakerAtmospherePlanetRadius + _WeatherMakerAtmosphereHeight);
				float3 rayEnd = rayStart + rayDir * intersection.y;

				// compute density along the ray
				float3 step = (rayEnd - rayStart) / stepCount;
				float stepSize = length(step);
				float2 density = 0;

				for (float s = 0.5; s < stepCount; s += 1.0)
				{
					float3 position = rayStart + step * s;
					float height = abs(length(position - planetCenter) - _WeatherMakerAtmospherePlanetRadius);
					float2 localDensity = exp(-(height.xx / _WeatherMakerDensityScaleHeight));

					density += localDensity * stepSize;
				}

				return density;
			}
		}

		float3 PrecomputeAmbientLight(float3 lightDir)
		{
			float startHeight = 0;
			float3 rayStart = float3(WEATHER_MAKER_CAMERA_POS.x, startHeight, WEATHER_MAKER_CAMERA_POS.z);
			float3 planetCenter = ATMOSPHERE_PLANET_CENTERH(startHeight);

			float3 color = 0;

			static const int sampleCount = 255;

			UNITY_LOOP
			for (int ii = 0; ii < sampleCount; ++ii)
			{
				float3 rayDir = UNITY_SAMPLE_TEX2D_LOD(_WeatherMakerRandomVectors, float2(ii + (0.5 / 255.0), 0.5), 0.0).xyz;
				rayDir.y = abs(rayDir.y);

				float2 intersection = AtmosphereRaySphereIntersection(rayStart, rayDir, planetCenter, _WeatherMakerAtmospherePlanetRadius + _WeatherMakerAtmosphereHeight);
				float rayLength = intersection.y;

				intersection = AtmosphereRaySphereIntersection(rayStart, rayDir, planetCenter, _WeatherMakerAtmospherePlanetRadius);
				rayLength = lerp(rayLength, min(rayLength, intersection.x), intersection.x > 0);

				float sampleCount = 32;
				float3 extinction;
				float3 scattaring = IntegrateInscattering(rayStart, rayDir, rayLength, planetCenter, 1, lightDir, sampleCount, extinction, false);

				color += scattaring * dot(rayDir, float3(0, 1, 0));
			}

			return color * 2 * PI / sampleCount;
		}

		float4 PrecomputeDirLight(float3 rayDir)
		{
			float startHeight = 500;

			float3 rayStart = float3(WEATHER_MAKER_CAMERA_POS.x, startHeight, WEATHER_MAKER_CAMERA_POS.z);
			float3 planetCenter = ATMOSPHERE_PLANET_CENTERH(startHeight);

			float2 localDensity;
			float2 densityToAtmosphereTop;

			GetAtmosphereDensity(rayStart, planetCenter, -rayDir, localDensity, densityToAtmosphereTop);
			float4 color;
			color.xyz = ComputeOpticalDepth(densityToAtmosphereTop);
			color.w = 1;
			return color;
		}
		               
		ENDCG
            
		// pass 0 - precompute particle density
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
			Blend Off

			CGPROGRAM

            #pragma vertex vertQuad
            #pragma fragment particleDensityLUT
            #pragma target 3.5

            #define UNITY_HDR_ON

            struct v2p
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            struct input
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2p vertQuad(input v)
            {
                v2p o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

			float4 particleDensityLUT(v2p i) : SV_Target
			{
                float cosAngle = i.uv.x * 2.0 - 1.0;
                float sinAngle = sqrt(saturate(1 - cosAngle * cosAngle));
                float startHeight = lerp(0.0, _WeatherMakerAtmosphereHeight, i.uv.y);

                float3 rayStart = float3(0, startHeight, 0);
                float3 rayDir = float3(sinAngle, cosAngle, 0);
                
				return float4(PrecomputeParticleDensity(rayStart, rayDir), 0.0, 0.0);
			}

			ENDCG
		}
			
		// pass 1 - ambient light LUT
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
			Blend Off

			CGPROGRAM

			#pragma vertex vertQuad
			#pragma fragment fragDir
			#pragma target 3.5

			struct v2p
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			struct input
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			v2p vertQuad(input v)
			{
				v2p o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				return o;
			}

			float4 fragDir(v2f i) : SV_Target
			{
				float cosAngle = i.uv.x * 1.1 - 0.1;// *2.0 - 1.0;
                float sinAngle = sqrt(saturate(1 - cosAngle * cosAngle));
                float3 lightDir = -normalize(float3(sinAngle, cosAngle, 0));
				return fixed4(PrecomputeAmbientLight(lightDir), 1.0);
			}
			ENDCG
		}

		// pass 2 - dir light LUT
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
			Blend Off

			CGPROGRAM

			#pragma vertex vertQuad
			#pragma fragment fragDir
			#pragma target 3.5

			struct v2p
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			struct input
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			v2p vertQuad(input v)
			{
				v2p o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				return o;
			}

			float4 fragDir(v2f i) : SV_Target
			{
				float cosAngle = i.uv.x * 1.1 - 0.1;// *2.0 - 1.0;
				float sinAngle = sqrt(saturate(1 - cosAngle * cosAngle));				
				float3 rayDir = normalize(float3(sinAngle, cosAngle, 0));
				return PrecomputeDirLight(rayDir);
			}
			ENDCG
		}
	}
}
