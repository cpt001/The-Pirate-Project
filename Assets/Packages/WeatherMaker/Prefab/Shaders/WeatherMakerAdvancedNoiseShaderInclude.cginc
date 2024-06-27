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

#ifndef _WEATHER_MAKER_ADVANCED_NOISE_SHADER_INCLUDE_
#define _WEATHER_MAKER_ADVANCED_NOISE_SHADER_INCLUDE_

#include "WeatherMakerNoiseShaderInclude.cginc"
#include "WeatherMakerCoreShaderInclude.cginc"

#define _Worley_Jitter 1.2

static const float3 cellVariance = float3(0.5, 0.5, 0.5);

uniform int _Seamless_Tiling;
uniform float _EndFrame;
uniform float _Frame;

uniform float _Worley_Octaves;
uniform float _Worley_Frequency;
uniform float _Worley_Lacunarity;
uniform float _Worley_Start_Weight;
uniform float _Worley_Decay;
uniform float _Worley_Amp;
uniform float _Worley_Power;
uniform float _Worley_Inverter;

uniform float _Perlin_Octaves;
uniform float _Perlin_Frequency;
uniform float _Perlin_Lacunarity;
uniform float _Perlin_Start_Weight;
uniform float _Perlin_Decay;
uniform float _Perlin_Amp;
uniform float _Perlin_Power;

uniform float _Worley_Perlin_Factor;
uniform float _Worley_Perlin_Factor2;

#define PERLIN_NOISE_SEAMLESS_TILE_START 0.5
#define PERLIN_NOISE_SEAMLESS_TILE_END (1.0 - PERLIN_NOISE_SEAMLESS_TILE_START)
#define PERLIN_NOISE_SEAMLESS_TILE_POWER 1.5

#define WORLEY_NOISE_SEAMLESS_TILE_START 0.5
#define WORLEY_NOISE_SEAMLESS_TILE_END (1.0 - WORLEY_NOISE_SEAMLESS_TILE_START)
#define WORLEY_NOISE_SEAMLESS_TILE_POWER 1.5

// ----------------------------------- 3D -------------------------------------

float snoise(float3 v)
{
	const float2 C = float2(
		0.166666666666666667, // 1/6
		0.333333333333333333  // 1/3
		);
	const float4 D = float4(0.0, 0.5, 1.0, 2.0);

	// First corner
	float3 i = floor(v + dot(v, C.yyy));
	float3 x0 = v - i + dot(i, C.xxx);

	// Other corners
	float3 g = step(x0.yzx, x0.xyz);
	float3 l = 1 - g;
	float3 i1 = min(g.xyz, l.zxy);
	float3 i2 = max(g.xyz, l.zxy);

	float3 x1 = x0 - i1 + C.xxx;
	float3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
	float3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

// Permutations
	i = mod289(i);
	float4 p = permute(
		permute(
			permute(
				i.z + float4(0.0, i1.z, i2.z, 1.0)
			) + i.y + float4(0.0, i1.y, i2.y, 1.0)
		) + i.x + float4(0.0, i1.x, i2.x, 1.0)
	);

	// Gradients: 7x7 points over a square, mapped onto an octahedron.
	// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
	float n_ = 0.142857142857; // 1/7
	float3 ns = n_ * D.wyz - D.xzx;

	float4 j = p - 49.0 * floor(p * ns.z * ns.z); // mod(p,7*7)

	float4 x_ = floor(j * ns.z);
	float4 y_ = floor(j - 7.0 * x_); // mod(j,N)

	float4 x = x_ * ns.x + ns.yyyy;
	float4 y = y_ * ns.x + ns.yyyy;
	float4 h = 1.0 - abs(x) - abs(y);

	float4 b0 = float4(x.xy, y.xy);
	float4 b1 = float4(x.zw, y.zw);

	//float4 s0 = float4(lessThan(b0,0.0))*2.0 - 1.0;
	//float4 s1 = float4(lessThan(b1,0.0))*2.0 - 1.0;
	float4 s0 = floor(b0)*2.0 + 1.0;
	float4 s1 = floor(b1)*2.0 + 1.0;
	float4 sh = -step(h, 0.0);

	float4 a0 = b0.xzyw + s0.xzyw*sh.xxyy;
	float4 a1 = b1.xzyw + s1.xzyw*sh.zzww;

	float3 p0 = float3(a0.xy, h.x);
	float3 p1 = float3(a0.zw, h.y);
	float3 p2 = float3(a1.xy, h.z);
	float3 p3 = float3(a1.zw, h.w);

	//Normalise gradients
	float4 norm = taylorInvSqrt(float4(
		dot(p0, p0),
		dot(p1, p1),
		dot(p2, p2),
		dot(p3, p3)
		));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;

	// Mix final noise value
	float4 m = max(
		0.6 - float4(
			dot(x0, x0),
			dot(x1, x1),
			dot(x2, x2),
			dot(x3, x3)
			),
		0.0
	);
	m = m * m;
	return 42.0 * dot(
		m*m,
		float4(
			dot(p0, x0),
			dot(p1, x1),
			dot(p2, x2),
			dot(p3, x3)
			)
	);
}

// ----------------------------------- 4D -------------------------------------

float snoise(float4 v)
{
	const float4 C = float4(
		0.138196601125011, // (5 - sqrt(5))/20 G4
		0.276393202250021, // 2 * G4
		0.414589803375032, // 3 * G4
		-0.447213595499958  // -1 + 4 * G4
		);

	// First corner
	float4 i = floor(
		v +
		dot(
			v,
			0.309016994374947451 // (sqrt(5) - 1) / 4
		)
	);
	float4 x0 = v - i + dot(i, C.xxxx);

	// Other corners

	// Rank sorting originally contributed by Bill Licea-Kane, AMD (formerly ATI)
	float4 i0;
	float3 isX = step(x0.yzw, x0.xxx);
	float3 isYZ = step(x0.zww, x0.yyz);
	i0.x = isX.x + isX.y + isX.z;
	i0.yzw = 1.0 - isX;
	i0.y += isYZ.x + isYZ.y;
	i0.zw += 1.0 - isYZ.xy;
	i0.z += isYZ.z;
	i0.w += 1.0 - isYZ.z;

	// i0 now contains the unique values 0,1,2,3 in each channel
	float4 i3 = saturate(i0);
	float4 i2 = saturate(i0 - 1.0);
	float4 i1 = saturate(i0 - 2.0);

	//    x0 = x0 - 0.0 + 0.0 * C.xxxx
	//    x1 = x0 - i1  + 1.0 * C.xxxx
	//    x2 = x0 - i2  + 2.0 * C.xxxx
	//    x3 = x0 - i3  + 3.0 * C.xxxx
	//    x4 = x0 - 1.0 + 4.0 * C.xxxx
	float4 x1 = x0 - i1 + C.xxxx;
	float4 x2 = x0 - i2 + C.yyyy;
	float4 x3 = x0 - i3 + C.zzzz;
	float4 x4 = x0 + C.wwww;

	// Permutations
	i = mod289(i);
	float j0 = permute(
		permute(
			permute(
				permute(i.w) + i.z
			) + i.y
		) + i.x
	);
	float4 j1 = permute(
		permute(
			permute(
				permute(
					i.w + float4(i1.w, i2.w, i3.w, 1.0)
				) + i.z + float4(i1.z, i2.z, i3.z, 1.0)
			) + i.y + float4(i1.y, i2.y, i3.y, 1.0)
		) + i.x + float4(i1.x, i2.x, i3.x, 1.0)
	);

	// Gradients: 7x7x6 points over a cube, mapped onto a 4-cross polytope
	// 7*7*6 = 294, which is close to the ring size 17*17 = 289.
	const float4 ip = float4(
		0.003401360544217687075, // 1/294
		0.020408163265306122449, // 1/49
		0.142857142857142857143, // 1/7
		0.0
		);

	float4 p0 = grad(j0, ip);
	float4 p1 = grad(j1.x, ip);
	float4 p2 = grad(j1.y, ip);
	float4 p3 = grad(j1.z, ip);
	float4 p4 = grad(j1.w, ip);

	// Normalise gradients
	float4 norm = taylorInvSqrt(float4(
		dot(p0, p0),
		dot(p1, p1),
		dot(p2, p2),
		dot(p3, p3)
		));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;
	p4 *= taylorInvSqrt(dot(p4, p4));

	// Mix contributions from the five corners
	float3 m0 = max(
		0.6 - float3(
			dot(x0, x0),
			dot(x1, x1),
			dot(x2, x2)
			),
		0.0
	);
	float2 m1 = max(
		0.6 - float2(
			dot(x3, x3),
			dot(x4, x4)
			),
		0.0
	);
	m0 = m0 * m0;
	m1 = m1 * m1;

	return 49.0 * (
		dot(
			m0*m0,
			float3(
				dot(p0, x0),
				dot(p1, x1),
				dot(p2, x2)
				)
		) + dot(
			m1*m1,
			float2(
				dot(p3, x3),
				dot(p4, x4)
				)
		)
		);
}

float2 wnoise(float2 P)
{
	float2 Pi = mod(floor(P), 289.0);
	float2 Pf = frac(P);
	float3 oi = float3(-1.0, 0.0, 1.0);
	float3 of = float3(-0.5, 0.5, 1.5);
	float3 px = permute(Pi.x + oi);
	float3 p = permute(px.x + Pi.y + oi); // p11, p12, p13
	float3 ox = frac(p*K) - Ko;
	float3 oy = mod(floor(p*K), 7.0)*K - Ko;
	float3 dx = Pf.x + 0.5 + _Worley_Jitter * ox;
	float3 dy = Pf.y - of + _Worley_Jitter * oy;
	float3 d1 = dist2(dx, dy, false); // d11, d12 and d13, squared
	p = permute(px.y + Pi.y + oi); // p21, p22, p23
	ox = frac(p*K) - Ko;
	oy = mod(floor(p*K), 7.0)*K - Ko;
	dx = Pf.x - 0.5 + _Worley_Jitter * ox;
	dy = Pf.y - of + _Worley_Jitter * oy;
	float3 d2 = dist2(dx, dy, false); // d21, d22 and d23, squared
	p = permute(px.z + Pi.y + oi); // p31, p32, p33
	ox = frac(p*K) - Ko;
	oy = mod(floor(p*K), 7.0)*K - Ko;
	dx = Pf.x - 1.5 + _Worley_Jitter * ox;
	dy = Pf.y - of + _Worley_Jitter * oy;
	float3 d3 = dist2(dx, dy, false); // d31, d32 and d33, squared
	// Sort out the two smallest distances (F1, F2)
	float3 d1a = min(d1, d2);
	d2 = max(d1, d2); // Swap to keep candidates for F2
	d2 = min(d2, d3); // neither F1 nor F2 are now in d3
	d1 = min(d1a, d2); // F1 is now in d1
	d2 = max(d1a, d2); // Swap to keep candidates for F2
	d1.xy = (d1.x < d1.y) ? d1.xy : d1.yx; // Swap if smaller
	d1.xz = (d1.x < d1.z) ? d1.xz : d1.zx; // F1 is in d1.x
	d1.yz = min(d1.yz, d2.yz); // F2 is now not in d2.yz
	d1.y = min(d1.y, d1.z); // nor in  d1.z
	d1.y = min(d1.y, d2.x); // F2 is in d1.y, we're done.
	return sqrt(d1.xy);

#undef dist

}

float2 inoise(float2 P)
{
	float2 Pi = mod(floor(P), 289.0);
	float2 Pf = frac(P);
	float3 oi = float3(-1.0, 0.0, 1.0);
	float3 of = float3(-0.5, 0.5, 1.5);
	float3 px = permute(Pi.x + oi);

	float3 p, ox, oy, dx, dy;
	float2 F = 1e6;

	UNITY_LOOP
	for (int i = 0; i < 3; i++)
	{
		p = permute(px[i] + Pi.y + oi); // pi1, pi2, pi3
		ox = frac(p*K) - Ko;
		oy = mod(floor(p*K), 7.0)*K - Ko;
		dx = Pf.x - of[i] + _Worley_Jitter * ox;
		dy = Pf.y - of + _Worley_Jitter * oy;

		float3 d = dx * dx + dy * dy; // di1, di2 and di3, squared

		//find the lowest and second lowest distances
		UNITY_LOOP
		for (int n = 0; n < 3; n++)
		{
			if (d[n] < F[0])
			{
				F[1] = F[0];
				F[0] = d[n];
			}
			else if (d[n] < F[1])
			{
				F[1] = d[n];
			}
		}
	}

	return F;
}

float2 inoise(float3 P)
{
	float3 Pi = mod289(floor(P));
	float3 Pf = frac(P);
	float3 oi = float3(-1.0, 0.0, 1.0);
	float3 of = float3(-0.5, 0.5, 1.5);
	float3 px = permute(Pi.x + oi);
	float3 py = permute(Pi.y + oi);
	float3 p, ox, oy, oz, dx, dy, dz;
	float2 F = 1e6;

	UNITY_LOOP
	for (int i = 0; i < 3; i++)
	{
		UNITY_LOOP
		for (int j = 0; j < 3; j++)
		{
			p = permute(px[i] + py[j] + Pi.z + oi); // pij1, pij2, pij3

			ox = frac(p * K) - Ko;
			oy = mod(floor(p * K), 7.0) * K - Ko;

			p = permute(p);

			oz = frac(p * K) - Ko;

			dx = Pf.x - of[i] + _Worley_Jitter * ox;
			dy = Pf.y - of[j] + _Worley_Jitter * oy;
			dz = Pf.z - of + _Worley_Jitter * oz;

			float3 d = dx * dx + dy * dy + dz * dz; // dij1, dij2 and dij3, squared

			//Find lowest and second lowest distances
			UNITY_LOOP
			for (int n = 0; n < 3; n++)
			{
				UNITY_FLATTEN
				if (d[n] < F[0])
				{
					F[1] = F[0];
					F[0] = d[n];
				}
				else if (d[n] < F[1])
				{
					F[1] = d[n];
				}
			}
		}
	}
	return F;
}

// https://github.com/Scrawk/GPU-Voronoi-Noise/tree/master/Assets/GPUVoronoiNoise
float2 pinoise(float3 f, float3 valMax)
{
	float3 cellyyy = floor(f);

	const float3 c = float3(-1.0, 0.0, 1.0);
#define MAKE_VAL(swizzle) float3 cell##swizzle = cellyyy + c.swizzle;
	MAKE_VAL(xxx)
		MAKE_VAL(xxy)
		MAKE_VAL(xxz)
		MAKE_VAL(xyx)
		MAKE_VAL(xyy)
		MAKE_VAL(xyz)
		MAKE_VAL(xzx)
		MAKE_VAL(xzy)
		MAKE_VAL(xzz)
		MAKE_VAL(yxx)
		MAKE_VAL(yxy)
		MAKE_VAL(yxz)
		MAKE_VAL(yyx)
		MAKE_VAL(yyz)
		MAKE_VAL(yzx)
		MAKE_VAL(yzy)
		MAKE_VAL(yzz)
		MAKE_VAL(zxx)
		MAKE_VAL(zxy)
		MAKE_VAL(zxz)
		MAKE_VAL(zyx)
		MAKE_VAL(zyy)
		MAKE_VAL(zyz)
		MAKE_VAL(zzx)
		MAKE_VAL(zzy)
		MAKE_VAL(zzz)
#define VAL(swizzle) distance(f, cell##swizzle + lerp(0.5 - cellVariance, 0.5 + cellVariance, hashTo3(WRAP(cell##swizzle))))
#define MIN3(a, b, c) min(a, min(b, c))
#define MIN9(a, b, c, d, e, f, g, h, i) MIN3(MIN3(a, b, c), MIN3(d, e, f), MIN3(g, h, i))
		return MIN3(MIN9(VAL(xxx), VAL(xxy), VAL(xxz),
			VAL(xyx), VAL(xyy), VAL(xyz),
			VAL(xzx), VAL(xzy), VAL(xzz)),
			MIN9(VAL(yxx), VAL(yxy), VAL(yxz),
				VAL(yyx), VAL(yyy), VAL(yyz),
				VAL(yzx), VAL(yzy), VAL(yzz)),
			MIN9(VAL(zxx), VAL(zxy), VAL(zxz),
				VAL(zyx), VAL(zyy), VAL(zyz),
				VAL(zzx), VAL(zzy), VAL(zzz)));
#undef MAKE_VAL
#undef VAL
#undef MIN3
#undef MIN9

	/*
	float3 Pi = mod(floor(P), 289.0);
	float3 Pf = frac(P);
	float3 oi = float3(-1.0, 0.0, 1.0);
	float3 of = float3(-0.5, 0.5, 1.5);
	float3 px = permute(Pi.x + oi);
	float3 py = permute(Pi.y + oi);

	float3 p, ox, oy, oz, dx, dy, dz;
	float2 F = 1e6;

	UNITY_LOOP
	for(int i = 0; i < 3; i++)
	{
		UNITY_LOOP
		for(int j = 0; j < 3; j++)
		{
			p = permute(px[i] + py[j] + Pi.z + oi); // pij1, pij2, pij3

			ox = frac(p*K) - Ko;
			oy = mod(floor(p*K),7.0)*K - Ko;

			p = permute(p);

			oz = frac(p*K) - Ko;

			dx = Pf.x - of[i] + _Worley_Jitter*ox;
			dy = Pf.y - of[j] + _Worley_Jitter*oy;
			dz = Pf.z - of + _Worley_Jitter*oz;

			float3 d = dx * dx + dy * dy + dz * dz; // dij1, dij2 and dij3, squared

			//Find lowest and second lowest distances
			UNITY_LOOP
			for(int n = 0; n < 3; n++)
			{
				UNITY_FLATTEN
				if(d[n] < F[0])
				{
					F[1] = F[0];
					F[0] = d[n];
				}
				else if(d[n] < F[1])
				{
					F[1] = d[n];
				}
			}
		}
	}

	return F;
	*/
}

#if 0

float2 inoise(float4 P)
{
	float4 Pi = mod289(floor(P));
	float4 Pf = frac(P);
	float3 oi = float3(-1.0, 0.0, 1.0);
	float3 of = float3(-0.5, 0.5, 1.5);
	float3 px = permute(Pi.x + oi);
	float3 py = permute(Pi.y + oi);
	float3 pz = permute(Pi.z + oi);

	float3 p, ox, oy, oz, ow, dx, dy, dz, dw, d;
	float2 F = 1e6;
	int i, j, k, n;

	UNITY_LOOP
	for (i = 0; i < 3; i++)
	{
		UNITY_LOOP
		for (j = 0; j < 3; j++)
		{
			UNITY_LOOP
			for (k = 0; k < 3; k++)
			{
				p = permute(px[i] + py[j] + pz[k] + Pi.w + oi); // pijk1, pijk2, pijk3

				ox = frac(p*K) - Ko;
				oy = mod(floor(p*K), 7.0)*K - Ko;

				p = permute(p);

				oz = frac(p*K) - Ko;
				ow = mod(floor(p*K), 7.0)*K - Ko;

				dx = Pf.x - of[i] + _Worley_Jitter * ox;
				dy = Pf.y - of[j] + _Worley_Jitter * oy;
				dz = Pf.z - of[k] + _Worley_Jitter * oz;
				dw = Pf.w - of + _Worley_Jitter * ow;

				d = dx * dx + dy * dy + dz * dz + dw * dw; // dijk1, dijk2 and dijk3, squared

				//Find the lowest and second lowest distances
				UNITY_LOOP
				for (n = 0; n < 3; n++)
				{
					UNITY_FLATTEN
					if (d[n] < F[0])
					{
						F[1] = F[0];
						F[0] = d[n];
					}
					else if (d[n] < F[1])
					{
						F[1] = d[n];
					}
				}
			}
		}
	}

	return F;
}

float2 pinoise(float4 P, float4 rep)
{
	float4 Pi = mod289(floor(P));
	float4 Pf = frac(P);
	float3 oi = float3(-1.0, 0.0, 1.0);
	float3 of = float3(-0.5, 0.5, 1.5);
	float3 px = permute(Pi.x + oi, rep);
	float3 py = permute(Pi.y + oi, rep);
	float3 pz = permute(Pi.z + oi, rep);

	float3 p, ox, oy, oz, ow, dx, dy, dz, dw, d;
	float2 F = 1e6;
	int i, j, k, n;

	UNITY_LOOP
	for (i = 0; i < 3; i++)
	{
		UNITY_LOOP
		for (j = 0; j < 3; j++)
		{
			UNITY_LOOP
			for (k = 0; k < 3; k++)
			{
				p = permute(px[i] + py[j] + pz[k] + Pi.w + oi); // pijk1, pijk2, pijk3

				ox = frac(p * K) - Ko;
				oy = mod(floor(p * K), 7.0) * K - Ko;

				p = permute(p);

				oz = frac(p * K) - Ko;
				ow = mod(floor(p * K), 7.0) * K - Ko;

				dx = Pf.x - of[i] + _Worley_Jitter * ox;
				dy = Pf.y - of[j] - _Worley_Jitter * oy; // +
				dz = Pf.z - of[k] - _Worley_Jitter * oz; // +
				dw = Pf.w - of + _Worley_Jitter * ow;

				d = dx * dx + dy * dy + dz * dz + dw * dw; // dijk1, dijk2 and dijk3, squared

				//Find the lowest and second lowest distances
				UNITY_LOOP
				for (n = 0; n < 3; n++)
				{
					UNITY_FLATTEN
					if (d[n] < F[0])
					{
						F[1] = F[0];
						F[0] = d[n];
					}
					else if (d[n] < F[1])
					{
						F[1] = d[n];
					}
				}
			}
		}
	}

	return F;
}

#endif

// Classic Perlin noise
float pnoise(float2 P)
{
	float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
	float4 Pf = frac(P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
	Pi = mod289(Pi); // To avoid truncation effects in permutation
	float4 ix = Pi.xzxz;
	float4 iy = Pi.yyww;
	float4 fx = Pf.xzxz;
	float4 fy = Pf.yyww;

	float4 i = permute(permute(ix) + iy);

	float4 gx = frac(i / 41.0) * 2.0 - 1.0;
	float4 gy = abs(gx) - 0.5;
	float4 tx = floor(gx + 0.5);
	gx = gx - tx;

	float2 g00 = float2(gx.x, gy.x);
	float2 g10 = float2(gx.y, gy.y);
	float2 g01 = float2(gx.z, gy.z);
	float2 g11 = float2(gx.w, gy.w);

	float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
	g00 *= norm.x;
	g01 *= norm.y;
	g10 *= norm.z;
	g11 *= norm.w;

	float n00 = dot(g00, float2(fx.x, fy.x));
	float n10 = dot(g10, float2(fx.y, fy.y));
	float n01 = dot(g01, float2(fx.z, fy.z));
	float n11 = dot(g11, float2(fx.w, fy.w));

	float2 fade_xy = fade(Pf.xy);
	float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
	float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
	return 2.3 * n_xy;
}

// Classic Perlin noise, periodic variant
float ppnoise(float2 P, float2 rep)
{
	float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
	float4 Pf = frac(P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
	Pi = mod(Pi, rep.xyxy); // To create noise with explicit period
	Pi = mod289(Pi);        // To avoid truncation effects in permutation
	float4 ix = Pi.xzxz;
	float4 iy = Pi.yyww;
	float4 fx = Pf.xzxz;
	float4 fy = Pf.yyww;

	float4 i = permute(permute(ix) + iy);

	float4 gx = frac(i / 41.0) * 2.0 - 1.0;
	float4 gy = abs(gx) - 0.5;
	float4 tx = floor(gx + 0.5);
	gx = gx - tx;

	float2 g00 = float2(gx.x, gy.x);
	float2 g10 = float2(gx.y, gy.y);
	float2 g01 = float2(gx.z, gy.z);
	float2 g11 = float2(gx.w, gy.w);

	float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
	g00 *= norm.x;
	g01 *= norm.y;
	g10 *= norm.z;
	g11 *= norm.w;

	float n00 = dot(g00, float2(fx.x, fy.x));
	float n10 = dot(g10, float2(fx.y, fy.y));
	float n01 = dot(g01, float2(fx.z, fy.z));
	float n11 = dot(g11, float2(fx.w, fy.w));

	float2 fade_xy = fade(Pf.xy);
	float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
	float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
	return 2.3 * n_xy;
}

// Classic Perlin noise
float pnoise(float3 P)
{
	float3 Pi0 = floor(P); // Integer part for indexing
	float3 Pi1 = Pi0 + (float3)1.0; // Integer part + 1
	Pi0 = mod289(Pi0);
	Pi1 = mod289(Pi1);
	float3 Pf0 = frac(P); // fracional part for interpolation
	float3 Pf1 = Pf0 - (float3)1.0; // fracional part - 1.0
	float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
	float4 iy = float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
	float4 iz0 = (float4)Pi0.z;
	float4 iz1 = (float4)Pi1.z;

	float4 ixy = permute(permute(ix) + iy);
	float4 ixy0 = permute(ixy + iz0);
	float4 ixy1 = permute(ixy + iz1);

	float4 gx0 = ixy0 * oneDiv7;
	float4 gy0 = frac(floor(gx0) * oneDiv7) - 0.5;
	gx0 = frac(gx0);
	float4 gz0 = (float4)0.5 - abs(gx0) - abs(gy0);
	float4 sz0 = step(gz0, (float4)0.0);
	gx0 -= sz0 * (step((float4)0.0, gx0) - 0.5);
	gy0 -= sz0 * (step((float4)0.0, gy0) - 0.5);

	float4 gx1 = ixy1 * oneDiv7;
	float4 gy1 = frac(floor(gx1) * oneDiv7) - 0.5;
	gx1 = frac(gx1);
	float4 gz1 = (float4)0.5 - abs(gx1) - abs(gy1);
	float4 sz1 = step(gz1, (float4)0.0);
	gx1 -= sz1 * (step((float4)0.0, gx1) - 0.5);
	gy1 -= sz1 * (step((float4)0.0, gy1) - 0.5);

	float3 g000 = float3(gx0.x, gy0.x, gz0.x);
	float3 g100 = float3(gx0.y, gy0.y, gz0.y);
	float3 g010 = float3(gx0.z, gy0.z, gz0.z);
	float3 g110 = float3(gx0.w, gy0.w, gz0.w);
	float3 g001 = float3(gx1.x, gy1.x, gz1.x);
	float3 g101 = float3(gx1.y, gy1.y, gz1.y);
	float3 g011 = float3(gx1.z, gy1.z, gz1.z);
	float3 g111 = float3(gx1.w, gy1.w, gz1.w);

	float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
	g000 *= norm0.x;
	g010 *= norm0.y;
	g100 *= norm0.z;
	g110 *= norm0.w;

	float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
	g001 *= norm1.x;
	g011 *= norm1.y;
	g101 *= norm1.z;
	g111 *= norm1.w;

	float n000 = dot(g000, Pf0);
	float n100 = dot(g100, float3(Pf1.x, Pf0.y, Pf0.z));
	float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
	float n110 = dot(g110, float3(Pf1.x, Pf1.y, Pf0.z));
	float n001 = dot(g001, float3(Pf0.x, Pf0.y, Pf1.z));
	float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
	float n011 = dot(g011, float3(Pf0.x, Pf1.y, Pf1.z));
	float n111 = dot(g111, Pf1);

	float3 fade_xyz = fade(Pf0);
	float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
	float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
	float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
	return 2.2 * n_xyz;
}

// Classic Perlin noise, periodic variant
float ppnoise(float3 f, float3 valMax)
{
	float3 minXYZ = floor(f);
	float3 maxXYZ = minXYZ + 1.0;
	f = WRAP(f);
	minXYZ = WRAP(minXYZ);
	maxXYZ = WRAP(maxXYZ);

	float3 t = f - minXYZ;
	float3 minXYmaxZ = float3(minXYZ.xy, maxXYZ.z),
	minXmaxYminZ = float3(minXYZ.x, maxXYZ.y, minXYZ.z),
	minXmaxYZ = float3(minXYZ.x, maxXYZ.y, maxXYZ.z),
	maxXminYZ = float3(maxXYZ.x, minXYZ.y, minXYZ.z),
	maxXminYmaxZ = float3(maxXYZ.x, minXYZ.y, maxXYZ.z),
	maxXYminZ = float3(maxXYZ.xy, minXYZ.z);

	float3 minXYZ_V = -1.0 + (2.0 * hashTo3(minXYZ));
	float3 toMinXYZ = -t;

	float3 maxXYZ_V = -1.0 + (2.0 * hashTo3(maxXYZ));
	float3 toMaxXYZ = 1.0 - t;

	float3 minXYmaxZ_V = -1.0 + (2.0 * hashTo3(minXYmaxZ));
	float3 toMinXYmaxZ = float3(toMinXYZ.xy, toMaxXYZ.z);

	float3 minXmaxYminZ_V = -1.0 + (2.0 * hashTo3(minXmaxYminZ));
	float3 toMinXmaxYminZ = float3(toMinXYZ.x, toMaxXYZ.y, toMinXYZ.z);

	float3 minXmaxYZ_V = -1.0 + (2.0 * hashTo3(minXmaxYZ));
	float3 toMinXmaxYZ = float3(toMinXYZ.x, toMaxXYZ.yz);

	float3 maxXminYZ_V = -1.0 + (2.0 * hashTo3(maxXminYZ));
	float3 toMaxXminYZ = float3(toMaxXYZ.x, toMinXYZ.yz);

	float3 maxXminYmaxZ_V = -1.0 + (2.0 * hashTo3(maxXminYmaxZ));
	float3 toMaxXminYmaxZ = float3(toMaxXYZ.x, toMinXYZ.y, toMaxXYZ.z);

	float3 maxXYminZ_V = -1.0 + (2.0 * hashTo3(maxXYminZ));
	float3 toMaxXYminZ = float3(toMaxXYZ.xy, toMinXYZ.z);

	t = smoothstep(0.0, 1.0, t);
	float outVal = lerp(lerp(lerp(dot(minXYZ_V, toMinXYZ),
		dot(maxXminYZ_V, toMaxXminYZ),
		t.x),
		lerp(dot(minXmaxYminZ_V, toMinXmaxYminZ),
			dot(maxXYminZ_V, toMaxXYminZ),
			t.x),
		t.y),
		lerp(lerp(dot(minXYmaxZ_V, toMinXYmaxZ),
			dot(maxXminYmaxZ_V, toMaxXminYmaxZ),
			t.x),
			lerp(dot(minXmaxYZ_V, toMinXmaxYZ),
				dot(maxXYZ_V, toMaxXYZ),
				t.x),
			t.y),
		t.z);
	return (0.5 * outVal);

	/*
	float3 Pi0 = mod(floor(P), rep); // Integer part, modulo period
	float3 Pi1 = mod(Pi0 + 1.0, rep); // Integer part + 1, mod period
	Pi0 = mod289(Pi0);
	Pi1 = mod289(Pi1);
	float3 Pf0 = frac(P); // fracional part for interpolation
	float3 Pf1 = Pf0 - 1.0; // fracional part - 1.0
	float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
	float4 iy = float4(Pi0.yy, Pi1.yy);
	float4 iz0 = Pi0.zzzz;
	float4 iz1 = Pi1.zzzz;

	float4 ixy = permute(permute(ix) + iy);
	float4 ixy0 = permute(ixy + iz0);
	float4 ixy1 = permute(ixy + iz1);

	float4 gx0 = ixy0 * oneDiv7;
	float4 gy0 = frac(floor(gx0) * oneDiv7) - 0.5;
	gx0 = frac(gx0);
	float4 gz0 = 0.5 - abs(gx0) - abs(gy0);
	float4 sz0 = step(gz0, 0.0);
	gx0 -= sz0 * (step(0.0, gx0) - 0.5);
	gy0 -= sz0 * (step(0.0, gy0) - 0.5);

	float4 gx1 = ixy1 * oneDiv7;
	float4 gy1 = frac(floor(gx1) * oneDiv7) - 0.5;
	gx1 = frac(gx1);
	float4 gz1 = 0.5 - abs(gx1) - abs(gy1);
	float4 sz1 = step(gz1, 0.0);
	gx1 -= sz1 * (step(0.0, gx1) - 0.5);
	gy1 -= sz1 * (step(0.0, gy1) - 0.5);

	float3 g000 = float3(gx0.x, gy0.x, gz0.x);
	float3 g100 = float3(gx0.y, gy0.y, gz0.y);
	float3 g010 = float3(gx0.z, gy0.z, gz0.z);
	float3 g110 = float3(gx0.w, gy0.w, gz0.w);
	float3 g001 = float3(gx1.x, gy1.x, gz1.x);
	float3 g101 = float3(gx1.y, gy1.y, gz1.y);
	float3 g011 = float3(gx1.z, gy1.z, gz1.z);
	float3 g111 = float3(gx1.w, gy1.w, gz1.w);

	float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
	g000 *= norm0.x;
	g010 *= norm0.y;
	g100 *= norm0.z;
	g110 *= norm0.w;
	float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
	g001 *= norm1.x;
	g011 *= norm1.y;
	g101 *= norm1.z;
	g111 *= norm1.w;

	float n000 = dot(g000, Pf0);
	float n100 = dot(g100, float3(Pf1.x, Pf0.yz));
	float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
	float n110 = dot(g110, float3(Pf1.xy, Pf0.z));
	float n001 = dot(g001, float3(Pf0.xy, Pf1.z));
	float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
	float n011 = dot(g011, float3(Pf0.x, Pf1.yz));
	float n111 = dot(g111, Pf1);

	float3 fade_xyz = fade(Pf0);
	float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
	float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
	float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
	return 2.2 * n_xyz;
	*/
}

#if 0

float ppnoise(float4 p, float4 rep)
{
	float4 Pi0 = mod(floor(p), rep); // Integer part modulo rep
	float4 Pi1 = mod(Pi0 + 1.0, rep); // Integer part + 1 mod rep
	float4 Pf0 = frac(p); // fracional part for interpolation
	float4 Pf1 = Pf0 - 1.0; // fracional part - 1.0
	float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
	float4 iy = float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
	float4 iz0 = Pi0.zzzz;
	float4 iz1 = Pi1.zzzz;
	float4 iw0 = Pi0.wwww;
	float4 iw1 = Pi1.wwww;

	float4 ixy = permute(permute(ix) + iy);
	float4 ixy0 = permute(ixy + iz0);
	float4 ixy1 = permute(ixy + iz1);
	float4 ixy00 = permute(ixy0 + iw0);
	float4 ixy01 = permute(ixy0 + iw1);
	float4 ixy10 = permute(ixy1 + iw0);
	float4 ixy11 = permute(ixy1 + iw1);

	float4 gx00 = ixy00 * oneDiv7;
	float4 gy00 = floor(gx00) * oneDiv7;
	float4 gz00 = floor(gy00) * oneDiv6;
	gx00 = frac(gx00) - 0.5;
	gy00 = frac(gy00) - 0.5;
	gz00 = frac(gz00) - 0.5;
	float4 gw00 = 0.75 - abs(gx00) - abs(gy00) - abs(gz00);
	float4 sw00 = step(gw00, 0.0);
	gx00 -= sw00 * (step(0, gx00) - 0.5);
	gy00 -= sw00 * (step(0, gy00) - 0.5);

	float4 gx01 = ixy01 * oneDiv7;
	float4 gy01 = floor(gx01) * oneDiv7;
	float4 gz01 = floor(gy01) * oneDiv6;
	gx01 = frac(gx01) - 0.5;
	gy01 = frac(gy01) - 0.5;
	gz01 = frac(gz01) - 0.5;
	float4 gw01 = 0.75 - abs(gx01) - abs(gy01) - abs(gz01);
	float4 sw01 = step(gw01, 0.0);
	gx01 -= sw01 * (step(0, gx01) - 0.5);
	gy01 -= sw01 * (step(0, gy01) - 0.5);

	float4 gx10 = ixy10 * oneDiv7;
	float4 gy10 = floor(gx10) * oneDiv7;
	float4 gz10 = floor(gy10) * oneDiv6;
	gx10 = frac(gx10) - 0.5;
	gy10 = frac(gy10) - 0.5;
	gz10 = frac(gz10) - 0.5;
	float4 gw10 = 0.75 - abs(gx10) - abs(gy10) - abs(gz10);
	float4 sw10 = step(gw10, 0.0);
	gx10 -= sw10 * (step(0, gx10) - 0.5);
	gy10 -= sw10 * (step(0, gy10) - 0.5);

	float4 gx11 = ixy11 * oneDiv7;
	float4 gy11 = floor(gx11) * oneDiv7;
	float4 gz11 = floor(gy11) * oneDiv6;
	gx11 = frac(gx11) - 0.5;
	gy11 = frac(gy11) - 0.5;
	gz11 = frac(gz11) - 0.5;
	float4 gw11 = 0.75 - abs(gx11) - abs(gy11) - abs(gz11);
	float4 sw11 = step(gw11, 0.0);
	gx11 -= sw11 * (step(0, gx11) - 0.5);
	gy11 -= sw11 * (step(0, gy11) - 0.5);

	float4 g0000 = float4(gx00.x, gy00.x, gz00.x, gw00.x);
	float4 g1000 = float4(gx00.y, gy00.y, gz00.y, gw00.y);
	float4 g0100 = float4(gx00.z, gy00.z, gz00.z, gw00.z);
	float4 g1100 = float4(gx00.w, gy00.w, gz00.w, gw00.w);
	float4 g0010 = float4(gx10.x, gy10.x, gz10.x, gw10.x);
	float4 g1010 = float4(gx10.y, gy10.y, gz10.y, gw10.y);
	float4 g0110 = float4(gx10.z, gy10.z, gz10.z, gw10.z);
	float4 g1110 = float4(gx10.w, gy10.w, gz10.w, gw10.w);
	float4 g0001 = float4(gx01.x, gy01.x, gz01.x, gw01.x);
	float4 g1001 = float4(gx01.y, gy01.y, gz01.y, gw01.y);
	float4 g0101 = float4(gx01.z, gy01.z, gz01.z, gw01.z);
	float4 g1101 = float4(gx01.w, gy01.w, gz01.w, gw01.w);
	float4 g0011 = float4(gx11.x, gy11.x, gz11.x, gw11.x);
	float4 g1011 = float4(gx11.y, gy11.y, gz11.y, gw11.y);
	float4 g0111 = float4(gx11.z, gy11.z, gz11.z, gw11.z);
	float4 g1111 = float4(gx11.w, gy11.w, gz11.w, gw11.w);

	float4 norm00 = taylorInvSqrt(float4(dot(g0000, g0000), dot(g0100, g0100), dot(g1000, g1000), dot(g1100, g1100)));
	g0000 *= norm00.x;
	g0100 *= norm00.y;
	g1000 *= norm00.z;
	g1100 *= norm00.w;

	float4 norm01 = taylorInvSqrt(float4(dot(g0001, g0001), dot(g0101, g0101), dot(g1001, g1001), dot(g1101, g1101)));
	g0001 *= norm01.x;
	g0101 *= norm01.y;
	g1001 *= norm01.z;
	g1101 *= norm01.w;

	float4 norm10 = taylorInvSqrt(float4(dot(g0010, g0010), dot(g0110, g0110), dot(g1010, g1010), dot(g1110, g1110)));
	g0010 *= norm10.x;
	g0110 *= norm10.y;
	g1010 *= norm10.z;
	g1110 *= norm10.w;

	float4 norm11 = taylorInvSqrt(float4(dot(g0011, g0011), dot(g0111, g0111), dot(g1011, g1011), dot(g1111, g1111)));
	g0011 *= norm11.x;
	g0111 *= norm11.y;
	g1011 *= norm11.z;
	g1111 *= norm11.w;

	float n0000 = dot(g0000, Pf0);
	float n1000 = dot(g1000, float4(Pf1.x, Pf0.y, Pf0.z, Pf0.w));
	float n0100 = dot(g0100, float4(Pf0.x, Pf1.y, Pf0.z, Pf0.w));
	float n1100 = dot(g1100, float4(Pf1.x, Pf1.y, Pf0.z, Pf0.w));
	float n0010 = dot(g0010, float4(Pf0.x, Pf0.y, Pf1.z, Pf0.w));
	float n1010 = dot(g1010, float4(Pf1.x, Pf0.y, Pf1.z, Pf0.w));
	float n0110 = dot(g0110, float4(Pf0.x, Pf1.y, Pf1.z, Pf0.w));
	float n1110 = dot(g1110, float4(Pf1.x, Pf1.y, Pf1.z, Pf0.w));
	float n0001 = dot(g0001, float4(Pf0.x, Pf0.y, Pf0.z, Pf1.w));
	float n1001 = dot(g1001, float4(Pf1.x, Pf0.y, Pf0.z, Pf1.w));
	float n0101 = dot(g0101, float4(Pf0.x, Pf1.y, Pf0.z, Pf1.w));
	float n1101 = dot(g1101, float4(Pf1.x, Pf1.y, Pf0.z, Pf1.w));
	float n0011 = dot(g0011, float4(Pf0.x, Pf0.y, Pf1.z, Pf1.w));
	float n1011 = dot(g1011, float4(Pf1.x, Pf0.y, Pf1.z, Pf1.w));
	float n0111 = dot(g0111, float4(Pf0.x, Pf1.y, Pf1.z, Pf1.w));
	float n1111 = dot(g1111, Pf1);

	float4 fade_xyzw = fade(Pf0);
	float4 n_0w = mix(float4(n0000, n1000, n0100, n1100), float4(n0001, n1001, n0101, n1101), fade_xyzw.w);
	float4 n_1w = mix(float4(n0010, n1010, n0110, n1110), float4(n0011, n1011, n0111, n1111), fade_xyzw.w);
	float4 n_zw = mix(n_0w, n_1w, fade_xyzw.z);
	float2 n_yzw = mix(float2(n_zw.x, n_zw.y), float2(n_zw.z, n_zw.w), fade_xyzw.y);
	float n_xyzw = mix(n_yzw.x, n_yzw.y, fade_xyzw.x);
	return 2.2 * n_xyzw;
}

#endif

// https://github.com/JTippetts/accidental-noise-library
// https://gamedev.stackexchange.com/questions/23625/how-do-you-generate-tileable-perlin-noise
float4 TileUV4From3(float3 uv, float freq)
{
	float start = 0.0;
	float amount = 1.0;
	float multiplier = amount / (2.0 * PI);
	float s = uv.x;
	float t = uv.y;
	float nx = start + cos(s * 2.0 * PI) * multiplier;
	float ny = start + cos(t * 2.0 * PI) * multiplier;
	float nz = start + sin(s * 2.0 * PI) * multiplier;
	float nw = start + sin(t * 2.0 * PI) * multiplier;
	return freq * (float4(nx, ny, nz, nw) + uv.z);
}

float3 TileUV3(float3 uv, float freq)
{
	return uv * freq;
}

float Noise_Worley(float3 P, float freq)
{
	float2 F;
	if (_Seamless_Tiling)
	{
		F = pinoise(P * freq, freq);
	}
	else
	{
		F = wnoise((P.xy + P.z) * freq);
	}

	return saturate(F.x);
}

float Noise_Worley_Octave(float3 P, int octaves, float freq, float startWeight, float amp, float inverter, float lacunarity, float decay, float power)
{
	float weight = startWeight;
	float sum = amp;
	float weightSum = 0.0;
	float F;
	for (int i = 0; i < octaves; i++)
	{
		F = Noise_Worley(P, freq);
		sum += (lerp(F, 1.0 - F, inverter) * weight);
		freq *= lacunarity;
		weight *= decay;
		weightSum += weight;
	}
	sum = saturate(sum / (weightSum * 2.0));
	return pow(sum, power);
}

float Noise_Worley_Octave(float3 P, int octaves, float freq)
{
	return Noise_Worley_Octave(P, octaves, freq, _Worley_Start_Weight, _Worley_Amp, _Worley_Inverter, _Worley_Lacunarity, _Worley_Decay, _Worley_Power);
}

float Noise_Perlin(float3 P, float freq)
{
	if (_Seamless_Tiling)
	{
		return ppnoise(P * freq, freq);
	}
	else
	{
		return pnoise(P * freq);
	}
}

float Noise_Perlin_Octave(float3 P, int octaves, float freq)
{
	float weight = _Perlin_Start_Weight;
	float sum = _Perlin_Amp;
	float weightSum = 0.0;
	for (int i = 0; i < octaves; i++)
	{
		sum += (Noise_Perlin(P, freq) * weight);
		weightSum += weight;
		freq *= _Perlin_Lacunarity;
		weight *= _Perlin_Decay;
	}
	return pow(saturate((sum / (weightSum))), _Perlin_Power);
}

// fast optimized 5 octave simplex noise without branching and extra multiplies and divides
float Noise_Simplex_Octave_5(float3 P, float sum)
{
	static const float weights[5] = { 0.5, 0.2, 0.15, 0.1, 0.05 };
	sum += (snoise(P) * weights[0]);
	sum += (snoise(P * 2.0) * weights[1]);
	sum += (snoise(P * 4.0) * weights[2]);
	sum += (snoise(P * 8.0) * weights[3]);
	sum += (snoise(P * 16.0) * weights[4]);
	return (sum);
}

float Noise_Simplex_Worley_Octave_5(float3 P, float sum, float simplexWeight, float worleyWeight, float worleyOctaves, float worleyFrequency)
{
	float sum1 = Noise_Simplex_Octave_5(P, sum);
	float sum2 = Noise_Worley_Octave(P, worleyOctaves, worleyFrequency, 0.5, 0.0, 1.0, 2.0, 0.5, 1.0);
	return (sum1 * simplexWeight) + (sum2 * worleyWeight);
	return sum;
}

#endif // _WEATHER_MAKER_ADVANCED_NOISE_SHADER_INCLUDE_
