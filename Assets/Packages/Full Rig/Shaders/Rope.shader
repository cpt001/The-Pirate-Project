Shader "Full Sails/Rope"
{
    Properties
    {
        _Color				("Color",			Color)		= (1,1,1,1)
        _MainTex			("Albedo (RGB)",	2D)			= "white" {}
        _Glossiness			("Smoothness",		Range(0,1))	= 0.5
        _Metallic			("Metallic",		Range(0,1))	= 0.0
		_Width				("Width",			Vector)		= (1,1,1,1)
		_SwingOffset		("Swing Phase Off",	Float)		= 0.0
		_SwingAngle			("Swing Angle",		Float)		= 0.5
		_SwingFreq			("Swing Freq",		Float)		= 1.0
		_MinMax				("Min Max",			Vector)		= (0,1,0,0)
		_P0					("P0",				Vector)		= (0,0,0,0)
		_P1					("P1",				Vector)		= (0,0,0,0)
		_Apq				("Apq",				Vector)		= (0,0,0,0)
		_ArcLength			("Arc Length",		Float)		= 0
		_FlipX				("Flip X",			Float)		= 0
		_Adjust				("Adjust",			Vector)		= (0,0,0,0)
		_StretchUV			("StretchUV",		Range(0,1))	= 1
	}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 3.0

        struct Input
        {
			float2 plop;
        };

        sampler2D	_MainTex;
        half		_Glossiness;
        half		_Metallic;
        fixed4		_Color;
		float4		_Width;
		float		_SwingOffset;
		float		_SwingAngle;
		float		_SwingFreq;
		float		_ArcLength;
		float4		_MinMax;
		float4		_P0;
		float4		_P1;
		float4		_Apq;
		float		_FlipX;
		float4		_MainTex_ST;
		float4		_Adjust;
		float		_StretchUV;

        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.plop) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }

		static const float PI = 3.14159265359;

		float asinh(float x)
		{
			return log(x + sqrt(x * x + 1));
		}

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_SETUP_INSTANCE_ID(v);

			float3 wv = v.vertex.xyz;
			float alpha = (wv.x - _MinMax.x) / (_MinMax.y - _MinMax.x);

			// Calculate the delta, side direction and length vectors
			float3 shift = _P1 - _P0;
			float3 side = normalize(float3(-shift.z, 0.0, shift.x));
			float l = length(shift.xz);

			// Map local x to world x and calculate curve horizontal t between 0 to 1 Formula derived from: S(t) = a * (sinh((x - p) / a) - sinh(-p / a))

			float za = lerp(_Adjust.x, _Adjust.y, alpha);
			float ya = lerp(_Adjust.z, _Adjust.w, alpha);
			float w = lerp(_Width.x, _Width.y, alpha);

			if ( wv.z < 0 )
				wv.z += za;	//lerp(_Adjust.x, _Adjust.y, alpha);	//_AdjustZ;
			else
				wv.z -= za;	//_AdjustZ;

			if ( wv.y < 0 )
				wv.y += ya;	//_AdjustY;
			else
				wv.y -= ya;	//_AdjustY;

			//float lx = lerp((wv.x - _MinMax.x) / (_MinMax.y - _MinMax.x), 1.0 - (wv.x - _MinMax.x) / (_MinMax.y - _MinMax.x), _FlipX);
			float lx = lerp(alpha, 1.0 - alpha, _FlipX);

			float wx = (_Apq.x * asinh(lx * _ArcLength / _Apq.x - sinh(_Apq.y / _Apq.x)) + _Apq.y);

			float t = wx / l;
    
			// Estimate catenary height at two points along the curve
			float tf = t + 0.01;
			float y0 = _Apq.x * cosh((t * l - _Apq.y) / _Apq.x) + _Apq.z;
			float y1 = _Apq.x * cosh((tf * l - _Apq.y) / _Apq.x) + _Apq.z;
    
			// Calculate catenary sag
			float sag = y0 - shift.y * t;

			// Calculate swing offset
			float wave = sin(PI * _SwingFreq * _Time.y + _SwingOffset);
			if ( _FlipX )
				wave = -wave;

			float swing_xz = sin(wave * _SwingAngle * 0.5) * sag;
			float swing_y = cos(wave * _SwingAngle * 0.5) * sag;
			float3 swing = float3(swing_xz * side.x, swing_y, swing_xz * side.z);
    
			// Calculate the curve position at two points
			float3 c0 = float3(shift.x * t, y0, shift.z * t);
			float3 c1 = float3(shift.x * tf, y1, shift.z * tf);

			// Calculate the forward and up vectors
			float3 forward = normalize(c1 - c0);
			float3 up = cross(side, forward);

			// Calculate vertex world position around the catenary position
			float2 xz = side.xz * wv.z * w * (1.0 - _FlipX * 2.0);
			//float3 world = c0 + _P0 + swing + float3(xz.x, 0, xz.y) + up * wv.y * w;
			float3 world = c0 + _P0 + swing + float3(xz.x, 0, xz.y) + up * wv.y * w;

			// Transform from world space to view space
			v.vertex.xyz = world;
			//v.normal = up;// * wv.y;	//float3(1, 0, 0);
			//v.normal = normalize(swing + float3(xz.x, 0, xz.y) + up * wv.y * _Width);
			//float3 n = float3(xz.x, 0.0, xz.y) + up;
			//v.normal = float3(xz.x, 0.0, xz.y) + up;
			float4 uv = v.texcoord1;
			if ( _StretchUV > 0.5 )
			{
				uv.y *= _Width.z * _MainTex_ST.y;
			}
			o.plop = uv;
		}
        ENDCG
    }
	CustomEditor "FullRig.RopeShaderGUI"
    FallBack "Diffuse"
}
