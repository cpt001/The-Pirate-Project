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

Shader "WeatherMaker/WeatherMakerCloudNoiseShader"
{
	Properties
	{
		_CloudNoiseFrame("Cloud Frame", Float) = 0.0
		_CloudNoiseType("Clout Noise Type", Range(0, 2)) = 0
		_CloudNoisePerlinParams1("Cloud Perlin Params1", Vector) = (5.0, 2.0, 1.0, 1.0) // octaves, period, brightness, 0
		_CloudNoisePerlinParams2("Cloud Perlin Params2", Vector) = (1.0, 1.0, 0.0, 0.0) // multiplier, power, 0, 0
		_CloudNoiseWorleyParams1("Cloud Worley Params1", Vector) = (5.0, 2.0, 1.0, 1.0) // octaves, period, brightness, 0
		_CloudNoiseWorleyParams2("Cloud Worley Params2", Vector) = (1.0, 1.0, 0.0, 0.0) // multiplier, power, 0, 0
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM

			#pragma target 3.5
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "WeatherMakerCloudNoiseShaderInclude.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			uniform float _CloudNoiseFrame;
			uniform int _CloudNoiseType;
			uniform float4 _CloudNoisePerlinParams1; // octaves, period, brightness, 0, 0
			uniform float4 _CloudNoisePerlinParams2; // multiplier, power, 0, 0
			uniform float4 _CloudNoiseWorleyParams1; // octaves, period, brightness, 0, 0
			uniform float4 _CloudNoiseWorleyParams2; // multiplier, power, 0, 0

			v2f vert(appdata_full v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = v.texcoord.xy;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 pos = float3(i.uv.x, i.uv.y, _CloudNoiseFrame);
				float3 noise = CloudNoise(pos, _CloudNoiseType, _CloudNoisePerlinParams1, _CloudNoisePerlinParams2, _CloudNoiseWorleyParams1, _CloudNoiseWorleyParams2);
				return float4(noise, 1.0);
			}
			ENDCG
		}
	}
}