Shader "WeatherMaker/WeatherMakerSkyPlaneShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry+1" "RenderType" = "Background" "PreviewType" = "Skybox" }
		Cull Off ZWrite Off ZTest LEqual Fog { Mode Off } Blend Off

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma glsl_no_auto_normalization
		#pragma multi_compile_instancing

		ENDCG

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

			// note sky plane is always procedural Unity style
			#include "WeatherMakerSkyShaderInclude.cginc"

			v2fSkyLerp vert (wm_appdata_base v)
			{
				WM_INSTANCE_VERT(v, v2fSkyLerp, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy; // TRANSFORM_TEX not supported
				o.ray = lerp(float3(0.0, 0.0, 1.0), float3(0.0, 1.0, 0.0), max(0.0, o.uv.y - _WeatherMakerSkyYOffset2D));
				procedural_sky_info i = CalculateScatteringCoefficients(_WeatherMakerSunDirectionDown2D, _WeatherMakerSunColor.rgb * pow(_WeatherMakerSunColor.a, 0.5), 1.0, normalize(o.ray));
				o.inScatter = i.inScatter;
				o.outScatter = i.outScatter;
				o.normal = float3(0.0, 0.0, 0.0);
                return o;
			}
			
			fixed4 frag (v2fSkyLerp i) : SV_Target
			{
				static const fixed3 sunColor = _WeatherMakerSunColor.rgb * _WeatherMakerSunColor.a;

				WM_INSTANCE_FRAG(i);
				i.ray = normalize(i.ray);
				procedural_sky_info p = CalculateScatteringColor(_WeatherMakerSunDirectionDown2D, sunColor, 0.0, i.ray, i.inScatter, i.outScatter, true);
				fixed nightReducer = min(1.0, _NightDuskMultiplier * max(p.skyColor.r, max(p.skyColor.g, p.skyColor.b)));
				fixed3 nightColor = GetNightColor(i.ray, i.uv, nightReducer);
				fixed3 result = (((p.inScatter + p.outScatter) * skyTintColor)) + nightColor;
				ApplyDither(result.rgb, i.uv, _WeatherMakerSkyDitherLevel);
				result.rgb += _WeatherMakerSkyAddColor;
				return float4(result.rgb, 1.0);
			}

			ENDCG
		}
	}
}
