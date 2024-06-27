Shader "WeatherMaker/WeatherMakerCloudParticleShader"
{
	Properties
	{
		_CloudTex("Texture", 3D) = "white" {}
		_Scale("Scale", Range(0.0, 1.0)) = 0.01
		_Density("Density", Range(0.0, 1.0)) = 0.025
		_EdgeFade("Edge Fade (xyz = dir intensity, w = overall intensity)", Vector) = (1.0, 1.0, 1.0, 1.0)
		_InvFade("Soft Fade", Range(0.0, 1.0)) = 0.05
		_MarchDither("March Dither", Range(0.0, 1.0)) = 0.3
		_ColorDither("Color Dither", Range(0.0, 1.0)) = 0.005
		_MaxRaymarchSteps("Max ray march steps", Range(1, 512)) = 12
		_LodDistance("Create LOD from distance", Range(0.0, 0.001)) = 0.00005
		_LightStepSize("Light Step", Range(0.01, 1.0)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }

		CGINCLUDE

		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9

		#define NULL_ZONE_RENDER_MASK 1 // precipitation is 1
		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

		#include "WeatherMakerCloudShaderInclude.cginc"
		#include "WeatherMakerCloudVolumetricSamplingShaderInclude.cginc"
		#include "WeatherMakerCloudVolumetricLightingShaderInclude.cginc"
		#include "WeatherMakerFogShaderInclude.cginc"
		#include "WeatherMakerNoiseShaderInclude.cginc"

		struct particle_data
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			fixed4 color : COLOR;
			float4 texcoords : TEXCOORD0;
			float4 size : TEXCOORD1;
			float4 center : TEXCOORD2;
			float3 rotation : TEXCOORD3;
			WM_BASE_VERTEX_INPUT
		};

		struct v2f
		{
			fixed4 color : COLOR0;
			float4 pos : SV_POSITION;
			float3 rayDir : NORMAL; // rotated rayDir
			float4 boxSize : TEXCOORD1; // xyz = local box size, w = box fade
			float4 boxCenter : TEXCOORD2; // xyz = world space box center, w = world space box top
			float4 invBoxSize : TEXCOORD3; // xyz = inv box size, w = inv height fade
			float4 projPos : TEXCOORD4; // screen pos
			float4 rayOrigin : TEXCOORD5; // xyz = rotated ray origin, w = u
			float4 rayDirCamera : TEXCOORD6; // xyz = original ray dir, not rotated, w = v'
			float4 quaternion : TEXCOORD7;
			WM_BASE_VERTEX_TO_FRAG
		};

		uniform sampler3D _CloudTex;
		uniform fixed _Scale;
		uniform fixed _Density;
		uniform fixed4 _EdgeFade;
		uniform fixed _MarchDither;
		uniform fixed _ColorDither;
		uniform int _MaxRaymarchSteps;
		uniform float _LodDistance;
		uniform fixed _LightStepSize;

		static const float invMaxRaymarchSteps = 1.0 / float(_MaxRaymarchSteps);

		v2f vert(particle_data v)
		{
			WM_INSTANCE_VERT(v, v2f, o);

			float4 rotationQuaternion = QuaternionFromEuler(v.rotation.xyz);
			float3 worldPos = WorldSpaceVertexPos(v.vertex);
			float3 rayDir = (worldPos - WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET);
			float3 halfSize = v.size * 0.5;

			o.pos = UnityObjectToClipPos(v.vertex);
			o.color = v.color;
			o.boxCenter.xyz = v.center.xyz;
			o.boxCenter.w = o.boxCenter.y + halfSize.y;
			o.projPos = ComputeScreenPos(o.pos);
			//COMPUTE_EYEDEPTH(o.projPos.z);
			o.projPos.z = length(halfSize);
			o.rayOrigin.xyz = RotatePointZeroOriginQuaternion(WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET - v.center.xyz, rotationQuaternion);
			o.rayOrigin.w = v.texcoords.x;
			o.rayDir = RotatePointZeroOriginQuaternion(rayDir, rotationQuaternion);
			o.rayDirCamera.xyz = rayDir;
			o.rayDirCamera.w = v.texcoords.y;
			o.boxSize.xyz = halfSize;
			o.boxSize.w = 8.0 / o.projPos.z;
			o.invBoxSize.xyz = _EdgeFade.w / halfSize;
			o.invBoxSize.w = 1.0 / (halfSize.y + halfSize.y);
			o.quaternion = rotationQuaternion;
			//o.rotation = v.rotation;

			return o;
		}

		ENDCG

		Pass
		{
			ZWrite Off
			ZTest Always
			Cull Front
			Blend One OneMinusSrcAlpha

			CGPROGRAM

			#define SOFTPARTICLES_ON

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile_instancing
			
			fixed SampleCloud(float3 localPos, float3 worldPos, float3 boxSize, float3 invBoxSize, float invBoxHeight, float distanceTo, float lodAdder)
			{
				float3 extents = saturate(1.0 - (invBoxSize * abs(localPos)));
				float heightFrac = saturate((boxSize.y - localPos.y) * invBoxHeight);
				extents = lerp(1.0, extents, _EdgeFade.xyz);
				float minExtent = pow((extents.x * extents.y * extents.z), 0.5);// *heightFrac);
				float alpha = _Density;// *(1.0 - cloudColor.a);
				float noise = 0.0;
				alpha *= minExtent;
				alpha = min(1.0, alpha * 1.5);
				alpha *= alpha;

				UNITY_BRANCH
				if (alpha > 0.002)
				{
					float lod = min(4.0, lodAdder + (distanceTo * _LodDistance));
					float3 texPos = ((worldPos - _WeatherMakerCameraOriginOffset) * _Scale);
					float4 cloudSample = tex3Dlod(_CloudTex, float4(texPos, lod));
					noise = CloudNoiseSampleToCloudNoise(cloudSample, worldPos, heightFrac, float4(1.0, 0.0, 0.7, 1.0), 1.0);
					noise = saturate(4.0 * cloudSample.r * cloudSample.b * cloudSample.b * alpha);
				}

				return noise;
			}

			fixed3 DirLightTerm(float4 quaternion, float3 localPos, float3 worldPos, float3 minBox, float3 maxBox, float3 boxSize, float3 invBoxSize, float invBoxHeight, float distanceTo)
			{
				float3 lightDir = _WeatherMakerDirLightPosition[0].xyz;
				lightDir = RotatePointZeroOriginQuaternion(lightDir, quaternion);
				fixed4 lightColor = _WeatherMakerDirLightColor[0];
				float rayLength;
				fixed density = 0.0;
				float hit = RayBoxIntersect(localPos, lightDir, 1000000.0, minBox, maxBox, rayLength, distanceTo);
				float lightStepSize = rayLength * _LightStepSize * 0.333;
				float coneRadius = lightStepSize;
				float3 lightStep = lightDir * rayLength * _LightStepSize * 0.333;
				float3 samplePosWorld;
				float3 samplePosLocal;

				UNITY_LOOP
				for (int i = 0; i < 3; i++)
				{
					samplePosWorld = worldPos + (weatherMakerRandomCone[i] * coneRadius);
					samplePosLocal = localPos + (weatherMakerRandomCone[i] * coneRadius);
					density += (5.0 * SampleCloud(samplePosLocal, samplePosWorld, boxSize, invBoxSize, invBoxHeight, distanceTo, 1.0));
					worldPos += lightStep;
					localPos += lightStep;
				}

				return lightColor.rgb * lightColor.a * CloudVolumetricBeerLambert(density) * 2.0;
			}

			fixed4 frag (v2f data) : SV_Target
			{
				WM_INSTANCE_FRAG(data);

				float2 screenUV = data.projPos.xy / data.projPos.w;
				float2 uv = float2(data.rayOrigin.w, data.rayDirCamera.w);
				float depth01 = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(screenUV);
				float depth = ViewDepthFromDepth01(depth01, screenUV);
				float3 rayDir = normalize(data.rayDir);
				float3 rayDirCamera = normalize(data.rayDirCamera.xyz);
				float3 rayOrigin = data.rayOrigin.xyz;
				float rayLength;
				float distanceTo;
				float3 minBox = -data.boxSize.xyz;
				float3 maxBox = data.boxSize.xyz;
				float hit = RayBoxIntersect(rayOrigin, rayDir, depth, minBox, maxBox, rayLength, distanceTo);
				clip(hit - 0.5);
				fixed3 blueNoise = tex2Dlod(_WeatherMakerBlueNoiseTexture, float4((4.0 * screenUV), 0.0, 0.0)).xyz - 0.5;
				float stepDither = 1.0 + (_MarchDither * (blueNoise.r + blueNoise.g + blueNoise.b));
				rayLength *= stepDither;
				float step = data.projPos.z * invMaxRaymarchSteps;
				//float step = rayLength * invMaxRaymarchSteps;
				int iterations = min(_MaxRaymarchSteps, ceil(rayLength / step));
				float3 posDither = (blueNoise * _MarchDither);
				float3 marchDir = rayDir * step;
				float randomDither = (1.0 + (_MarchDither * abs(RandomFloat(rayDir))));
				marchDir += posDither;
				float3 marchDirCamera = rayDirCamera * step;
				marchDirCamera += posDither;
				float3 invBoxSize = data.invBoxSize.xyz;
				float invBoxHeight = data.invBoxSize.w;
				float3 marchPos = rayOrigin + (rayDir * distanceTo * randomDither);
				float3 marchPosCamera = WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET + (rayDirCamera * distanceTo * randomDither);
				float3 texPos;
				float3 extents;
				float minExtent;
				fixed alpha;
				fixed4 texValue;
				fixed4 cloudColor = fixed4Zero;
				fixed4 cloudSample;
				fixed lod;

				UNITY_LOOP
				for (int iteration = 0; iteration < iterations && cloudColor.a < 0.99; iteration++)
				{
					UNITY_BRANCH
					if (distanceTo > 10.0)
					{
						fixed samp = SampleCloud(marchPos, marchPosCamera, data.boxSize, invBoxSize, invBoxHeight, distanceTo, 0.0);
						samp *= min(1.0, distanceTo * 0.01);
						cloudSample.rgb = samp * (volumetricCloudAmbientColorSky + DirLightTerm(data.quaternion, marchPos, marchPosCamera, minBox, maxBox, data.boxSize, invBoxSize, invBoxHeight, distanceTo));
						cloudSample.a = samp;
						cloudColor = ((1.0 - cloudColor.a) * cloudSample) + cloudColor;
					}

					marchPos += marchDir;
					marchPosCamera += marchDirCamera;
					distanceTo += step;
				}

				cloudColor.a = saturate(cloudColor.a);

#if defined(SOFTPARTICLES_ON)

				// soft particles
				cloudColor.a *= saturate(rayLength * _InvFade);

#endif

				cloudColor *= data.color;
				cloudColor *= cloudColor.a;
				ApplyDither(cloudColor.rgb, uv, _ColorDither);
				return cloudColor;
			}
			ENDCG
		}

		// depth write pass (linear 0 - 1)
		Pass
		{
			ZWrite Off
			ZTest Always
			Cull Front
			Blend One Zero

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			float4 frag(v2f data) : SV_Target
			{
				WM_INSTANCE_FRAG(data);

				float3 rayDir = normalize(data.rayDir);
				float3 rayOrigin = data.rayOrigin.xyz;
				float intersectAmount;
				float distanceTo;
				if (RayBoxIntersect(rayOrigin, rayDir, 1000000.0, -data.boxSize.xyz, data.boxSize.xyz, intersectAmount, distanceTo))
				{
					float2 screenUV = data.projPos.xy / data.projPos.w;
					float depth01 = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(screenUV);
					float depthPos = ViewDepthFromDepth01(depth01, screenUV);
					return saturate(lerp(0.0, depth01, distanceTo / depthPos));
				}
				else
				{
					return 1.0;
				}
			}

			ENDCG
		}
	}

	Fallback Off
}
