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

// *****************
// *** IMPORTANT ***
// *****************
// To enable this shader to apply to Unity speed trees, swap the comment up one line (move the starting '//' to line 8 instead of line 9).
// Once enabled, you must reload the scene, and if that fails, restart Unity.
// *****************

Shader "WeatherMaker/TerrainEngine/BillboardTree"
// Shader "Hidden/TerrainEngine/BillboardTree"

{
	Properties
	{
		_MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
		_DirectionalLightMultiplier("Dir light multiplier", Float) = 1.0
		_PointSpotLightMultiplier("Point/spot light multiplier", Float) = 1.0
		_AmbientLightMultiplier("Ambient light multiplier", Float) = 1.0
	}

	SubShader
	{
		// TODO: If find a way to write to depth buffer, use AlphaTest+49 queue and remove hacky fog logic
		Tags { "Queue" = "AlphaTest+51" "IgnoreProjector" = "True" "RenderType" = "Transparent" "LightMode" = "Vertex" }

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9
		
		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

		ENDCG

		Pass
		{

			ColorMask rgb
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off Lighting Off ZWrite Off ZTest LEqual Fog { Mode Off }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#define WEATHER_MAKER_DEPTH_SHADOWS_OFF
			#define WEATHER_MAKER_LIGHT_NO_NORMALS
			#define WEATHER_MAKER_LIGHT_NO_SPECULAR
			#define FOG_DISTANCE_MULTIPLIER 2.5 // change to tase to make billboard trees fade out slower/faster in fog
			#define ALPHA_CUTOFF 0.2

			#include "WeatherMakerFogShaderInclude.cginc"
			#include "TerrainEngine.cginc"

			struct v2fTree
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR0;
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				//float4 screenPos : TEXCOORD2;
			};

			v2fTree vert(appdata_tree_billboard v)
			{
				v2fTree o;
				TerrainBillboardTree(v.vertex, v.texcoord1.xy, v.texcoord.y);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv.x = v.texcoord.x;
				o.uv.y = v.texcoord.y > 0;
				o.color = v.color;
				//o.screenPos = ComputeScreenPos(o.pos);
				o.worldPos = WorldSpaceVertexPos(v.vertex);
				return o;
			}

			fixed4 frag(v2fTree input) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, input.uv);
				col.rgb *= input.color.rgb;

				// HACK: Handle weather maker full screen fog
				wm_world_space_light_params p;
				p.worldPos = input.worldPos;
				p.diffuseColor = col.rgb;
				p.ambientColor = _WeatherMakerAmbientLightColorGround;
				p.shadowStrength = 0.0;

				// high quality lighting, but these billboards may not be worth it
				//col = fixed4(CalculateLightColorWorldSpace(p), col.a);

				// fast lighting, just use first dir light
				col.rgb *= unity_LightColor[0].rgb * unity_LightColor[0].a * (1.0 - unity_LightPosition[0].w);

				// make more fog around these abomination of billboards so they just fade out into the distance faster
				// unfortunately there is no way that I know of to get this shader to write to the depth buffer
				// if there was the weather maker full screen fog shader could nicely fade on top
				float depth = (FOG_DISTANCE_MULTIPLIER * distance(WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET, input.worldPos));
				float fog = min(1.0, CalculateFogFactor(depth));
				col.a *= (col.a > ALPHA_CUTOFF) * (1.0 - fog);
				return col;
			}

			ENDCG
		}
	}

	Fallback Off
}
