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

Shader "WeatherMaker/WeatherMakerTemporalReprojectionShader"
{
	Properties 
	{
		_MainTex("Texture", 2D) = "white" {}
		_WeatherMakerTemporalReprojectionAlphaThreshold("Alpha redraw threshold (sharp blend only)", Range(1.0, 128.0)) = 1.0
	}
	SubShader 
	{
		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9
		
		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

		ENDCG

		Pass
		{
			Blend Off Cull Off ZWrite Off ZTest[_ZTest]
			
			CGPROGRAM

			#pragma vertex temporal_reprojection_vert_default
			#pragma fragment temporal_reprojection_fragment_custom

			#pragma multi_compile_instancing

			#include "WeatherMakerTemporalReprojectionShaderInclude.cginc"

			ENDCG
		}
	} 
}
