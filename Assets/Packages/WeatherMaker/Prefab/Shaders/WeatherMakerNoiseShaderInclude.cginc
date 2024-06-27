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

#ifndef _WEATHER_MAKER_SIMPLE_NOISE_SHADER_INCLUDE_
#define _WEATHER_MAKER_SIMPLE_NOISE_SHADER_INCLUDE_

#include "WeatherMakerFastMathShader.cginc"

// 1 / 6
#define oneDiv6 0.16666666666666666666666666666667

// 1 / 7
#define oneDiv7 0.14285714285714285714285714285714f

// 1 / 289
#define oneDiv289 0.00346020761245674740484429065744f

// 1 / 289
#define ONE_OVER_289 oneDiv289

#define dist2(x, y, manhattanDistance) (manhattanDistance ? abs(x) + abs(y) : (x * x + y * y))
#define dist3(x, y, z, manhattanDistance) (manhattanDistance ? abs(x) + abs(y) + abs(z) : (x * x + y * y + z * z))

#define K 0.142857142857 // 1/7
#define Ko 0.428571428571 // 1/2-K/2
#define K2 0.020408163265306 // 1/(7*7)
#define Kz 0.166666666667 // 1/6
#define Kzo 0.416666666667 // 1/2-1/6*2

#define mod(x, y) ((x) - (floor((x) / (y)) * (y))) //fmod(x, y)
#define mod7(x) ((x) - (floor(x * oneDiv7) * 7.0))
#define mod289(x) ((x) - (floor((x) * oneDiv289) * 289.0))
#define mix lerp

#define _HASH(p4, swizzle) \
		p4 = frac(p4 * float4(443.897, 441.423, 437.195, 444.129)); \
		p4 += dot(p4, p4.wzxy + 19.19); \
		return frac(dot(p.xyzw, p.zwxy) * p.swizzle);

#define IDENTITY(x) x
#define WRAP(x) (frac((x) / valMax) * valMax)
 
float _hashTo1(float4 p) { _HASH(p, x); }
float2 _hashTo2(float4 p) { _HASH(p, xy); }
float3 _hashTo3(float4 p) { _HASH(p, xyz); }
float4 _hashTo4(float4 p) { _HASH(p, xyzw); }
float  hashTo1(float p) { return _hashTo1(p.xxxx); }
float  hashTo1(float2 p) { return _hashTo1(p.xyxy); }
float  hashTo1(float3 p) { return _hashTo1(p.xyzx); }
float  hashTo1(float4 p) { return _hashTo1(p); }
float2 hashTo2(float p) { return _hashTo2(p.xxxx); }
float2 hashTo2(float2 p) { return _hashTo2(p.xyxy); }
float2 hashTo2(float3 p) { return _hashTo2(p.xyzx); }
float2 hashTo2(float4 p) { return _hashTo2(p); }
float3 hashTo3(float p) { return _hashTo3(p.xxxx); }
float3 hashTo3(float2 p) { return _hashTo3(p.xyxy); }
float3 hashTo3(float3 p) { return _hashTo3(p.xyzx); }
float3 hashTo3(float4 p) { return _hashTo3(p); }
float4 hashTo4(float p) { return _hashTo4(p.xxxx); }
float4 hashTo4(float2 p) { return _hashTo4(p.xyxy); }
float4 hashTo4(float3 p) { return _hashTo4(p.xyzx); }
float4 hashTo4(float4 p) { return _hashTo4(p); }

float2 hash33(float2 p, float2 jitter = float2(0.5, 0.5))
{

#define UI0 1597334673U
#define UI1 3812015801U
#define UI2 uint2(UI0, UI1)
#define UI3 uint3(UI0, UI1, 2798796415U)
#define UIF (1.0 / float(0xffffffffU))

	uint2 q = uint2(int2(p.x, p.y)) * UI2;
	q = (q.x ^ q.y) * UI2;
	return (-1.0 + 2.0 * float2(q.x, q.y) * UIF) * jitter.x + jitter.y;
}

float3 hash33(float3 p, float2 jitter = float2(0.5, 0.5))
{

#define UI0 1597334673U
#define UI1 3812015801U
#define UI2 uint2(UI0, UI1)
#define UI3 uint3(UI0, UI1, 2798796415U)
#define UIF (1.0 / float(0xffffffffU))

	uint3 q = uint3(int3(p.x, p.y, p.z)) * UI3;
	q = (q.x ^ q.y ^ q.z) * UI3;
	return (-1.0 + 2.0 * float3(q.x, q.y, q.z) * UIF) * jitter.x + jitter.y;
}

// ( x*34.0 + 1.0 )*x =
// x*x*34.0 + x
float permute(float x) { return mod289((34.0 * x + 1.0) * x); }
float2 permute(float2 x) { return mod289((34.0 * x + 1.0) * x); }
float3 permute(float3 x) { return mod289((34.0 * x + 1.0) * x); }
float4 permute(float4 x) { return mod289((34.0 * x + 1.0) * x); }

float permute(float x, float rep) { return mod((34.0 * x + 1.0) * x, rep); }
float2 permute(float2 x, float2 rep) { return mod((34.0 * x + 1.0) * x, rep); }
float3 permute(float3 x, float3 rep) { return mod((34.0 * x + 1.0) * x, rep); }
float4 permute(float4 x, float4 rep) { return mod((34.0 * x + 1.0) * x, rep); }

float perm289(float x) { return mod289(((x * 34.0) + 1.0) * x); }
float2 perm289(float2 x) { return mod289(((x * 34.0) + 1.0) * x); }
float3 perm289(float3 x) { return mod289(((x * 34.0) + 1.0) * x); }
float4 perm289(float4 x) { return mod289(((x * 34.0) + 1.0) * x); }

float fade(float t) { return t * t*t*(t*(t*6.0 - 15.0) + 10.0); }
float2 fade(float2 t) { return t * t*t*(t*(t*6.0 - 15.0) + 10.0); }
float3 fade(float3 t) { return t * t*t*(t*(t*6.0 - 15.0) + 10.0); }
float4 fade(float4 t) { return t * t*t*(t*(t*6.0 - 15.0) + 10.0); }

float taylorInvSqrt(float r)
{
	return 1.79284291400159 - 0.85373472095314 * r;
}

float2 taylorInvSqrt(float2 r)
{
	return 1.79284291400159 - 0.85373472095314 * r;
}

float3 taylorInvSqrt(float3 r)
{
	return 1.79284291400159 - 0.85373472095314 * r;
}

float4 taylorInvSqrt(float4 r)
{
	return 1.79284291400159 - 0.85373472095314 * r;
}

float4 grad(float j, float4 ip)
{
	const float4 ones = float4(1.0, 1.0, 1.0, -1.0);
	float4 p, s;
	p.xyz = floor(frac(j * ip.xyz) * 7.0) * ip.z - 1.0;
	p.w = 1.5 - dot(abs(p.xyz), ones.xyz);

	// GLSL: lessThan(x, y) = x < y
	// HLSL: 1 - step(y, x) = x < y
	s = float4(
		1 - step(0.0, p)
		);

	// Optimization hint Dolkar
	// p.xyz = p.xyz + (s.xyz * 2 - 1) * s.www;
	p.xyz -= sign(p.xyz) * (p.w < 0);

	return p;
}

float3 hash(float3 p)
{
	return frac(sin(float3(dot(p, float3(1.0, 57.0, 113.0)), dot(p, float3(57.0, 113.0, 1.0)), dot(p, float3(113.0, 1.0, 57.0)))) * 43758.5453);
}

#define permutation permute

// ----------------------------------------------------------------------------

float genericNoise3D(float3 p)
{
	float3 a = floor(p);
	float3 d = p - a;
	d = d * d * (3.0 - 2.0 * d);

	float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
	float4 k1 = perm289(b.xyxy);
	float4 k2 = perm289(k1.xyxy + b.zzww);

	float4 c = k2 + a.zzzz;
	float4 k3 = perm289(c);
	float4 k4 = perm289(c + 1.0);

	float4 o1 = frac(k3 * (1.0 / 41.0));
	float4 o2 = frac(k4 * (1.0 / 41.0));

	float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
	float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);

	return o4.y * d.y + o4.x * (1.0 - d.y);
}

float simplexNoiseFast3D(float3 x)
{

#define simplex_hash(n) (frac(sin(n) * 43758.5453))

	// The noise function returns a value in the range 0 to 1

	float3 p = floor(x);
	float3 f = frac(x);

	f = f * f * (3.0 - 2.0 * f);
	float n = p.x + p.y * 57.0 + 113.0 * p.z;

	return lerp
	(
		lerp
		(
			lerp(simplex_hash(n + 0.0), simplex_hash(n + 1.0), f.x),
			lerp(simplex_hash(n + 57.0), simplex_hash(n + 58.0), f.x),
			f.y
		),
		lerp
		(
			lerp(simplex_hash(n + 113.0), simplex_hash(n + 114.0), f.x),
			lerp(simplex_hash(n + 170.0), simplex_hash(n + 171.0), f.x),
			f.y
		),
		f.z
	);

#undef simplex_hash

}

float optimizeWorleyNoise2D(float2 st)
{
	// st *= rep;

	float2 i_st = floor(st);
	float2 f_st = frac(st);
	float m_dist = 9999999.0;

	UNITY_UNROLL
	for (int y = -1; y < 2; y++)
	{
		UNITY_UNROLL
		for (int x = -1; x < 2; x++)
		{
			// Neighbor place in the grid
			float2 neighbor = float2(float(x), float(y));

			// Random position from current + neighbor place in the grid
			//float2 pt = hash33(mod(i_st + neighbor, rep));
			float2 pt = hash33(i_st + neighbor);

			// Vector between the pixel and the point
			float2 diff = neighbor + pt - f_st;

			// Distance to the point
			float dist = dot(diff, diff);

			// Keep the closer distance
			m_dist = min(m_dist, dist);
		}
	}

	return fastSqrtNR0(m_dist);
}

float optimizeWorleyNoise3D(float3 st, float3 rep, float offset)
{
	st += offset;
	st *= rep;

	float3 i_st = floor(st);
	float3 f_st = frac(st);
	float m_dist = 9999999.0;

	UNITY_UNROLL
	for (int z = -1; z < 2; z++)
	{
		UNITY_UNROLL
		for (int y = -1; y < 2; y++)
		{
			UNITY_UNROLL
			for (int x = -1; x < 2; x++)
			{
				// Neighbor place in the grid
				float3 neighbor = float3(float(x), float(y), float(z));

				// Random position from current + neighbor place in the grid
				float3 pt = hash33(mod(i_st + neighbor, rep));

				// Vector between the pixel and the point
				float3 diff = neighbor + pt - f_st;

				// Distance to the point
				float dist = dot(diff, diff);

				// Keep the closer distance
				m_dist = min(m_dist, dist);
			}
		}
	}

	return fastSqrtNR0(m_dist);
}

#endif // _WEATHER_MAKER_NOISE_SHADER_INCLUDE_
