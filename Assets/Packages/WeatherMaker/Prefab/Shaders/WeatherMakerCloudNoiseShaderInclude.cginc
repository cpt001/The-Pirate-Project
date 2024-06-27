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

// MIT license: https://github.com/Fewes/CloudNoiseGen

#ifndef WEATHER_MAKER_CLOUDNOISELIB_INCLUDED
#define WEATHER_MAKER_CLOUDNOISELIB_INCLUDED

// ------------------------------ PERLIN NOISE START ------------------------------

//
// Noise Shader Library for Unity - https://github.com/keijiro/NoiseShader
//
// Original work (webgl-noise) Copyright (C) 2011 Stefan Gustavson
// Translation and modification was made by Keijiro Takahashi.
//
// This shader is based on the webgl-noise GLSL shader. For further details
// of the original shader, please see the following description from the
// original source code.
//

//
// GLSL textureless classic 3D noise "cnoise",
// with an RSL-style periodic variant "pnoise".
// Author:  Stefan Gustavson (stefan.gustavson@liu.se)
// Version: 2011-10-11
//
// Many thanks to Ian McEwan of Ashima Arts for the
// ideas for permutation and gradient selection.
//
// Copyright (c) 2011 Stefan Gustavson. All rights reserved.
// Distributed under the MIT license:

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#include "WeatherMakerNoiseShaderInclude.cginc"

// 1 / 6
#define oneDiv6 0.16666666666666666666666666666667

// 1 / 7
#define oneDiv7 0.14285714285714285714285714285714f

// 1 / 289
#define oneDiv289 0.00346020761245674740484429065744f

float perlinNoise2D(float2 P)
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

float perlinNoise2D(float2 P, float2 rep)
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

float perlinNoise3D(float3 P)
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

float perlinNoise3D(float3 P, float3 rep)
{
	P *= rep;
	float3 Pi0 = mod(floor(P), rep); // Integer part, modulo period
	float3 Pi1 = mod(Pi0 + (float3)1.0, rep); // Integer part + 1, mod period
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

	float4 gx0 = ixy0 / 7.0;
	float4 gy0 = frac(floor(gx0) / 7.0) - 0.5;
	gx0 = frac(gx0);
	float4 gz0 = (float4)0.5 - abs(gx0) - abs(gy0);
	float4 sz0 = step(gz0, (float4)0.0);
	gx0 -= sz0 * (step((float4)0.0, gx0) - 0.5);
	gy0 -= sz0 * (step((float4)0.0, gy0) - 0.5);

	float4 gx1 = ixy1 / 7.0;
	float4 gy1 = frac(floor(gx1) / 7.0) - 0.5;
	gx1 = frac(gx1);
	float4 gz1 = (float4)0.5 - abs(gx1) - abs(gy1);
	float4 sz1 = step(gz1, (float4)0.0);
	gx1 -= sz1 * (step((float4)0.0, gx1) - 0.5);
	gy1 -= sz1 * (step((float4)0.0, gy1) - 0.5);

	float3 g000 = float3(gx0.x,gy0.x,gz0.x);
	float3 g100 = float3(gx0.y,gy0.y,gz0.y);
	float3 g010 = float3(gx0.z,gy0.z,gz0.z);
	float3 g110 = float3(gx0.w,gy0.w,gz0.w);
	float3 g001 = float3(gx1.x,gy1.x,gz1.x);
	float3 g101 = float3(gx1.y,gy1.y,gz1.y);
	float3 g011 = float3(gx1.z,gy1.z,gz1.z);
	float3 g111 = float3(gx1.w,gy1.w,gz1.w);

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
// ------------------------------ PERLIN NOISE END ------------------------------

// ------------------------------ WORLEY NOISE START ------------------------------
//
// Worley noise implementation for WebGL shaders - https://github.com/Erkaman/glsl-worley
//
// This software is released under the MIT license:

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#define USE_WORLEY_HASH // uncomment to use non-worley hash

float2 worleyNoise2D(float2 P, float jitter, bool manhattanDistance)
{
	float2 Pi = mod(floor(P), 289.0);
	float2 Pf = frac(P);
	float4 Pfx = Pf.x + float4(-0.5, -1.5, -0.5, -1.5);
	float4 Pfy = Pf.y + float4(-0.5, -0.5, -1.5, -1.5);
	float4 p = permute(Pi.x + float4(0.0, 1.0, 0.0, 1.0));
	p = permute(p + Pi.y + float4(0.0, 0.0, 1.0, 1.0));
	float4 ox = mod(p, 7.0)*K + K2;
	float4 oy = mod(floor(p*K), 7.0)*K + K2;
	float4 dx = Pfx + jitter * ox;
	float4 dy = Pfy + jitter * oy;
	float4 d = dist2(dx, dy, manhattanDistance); // d11, d12, d21 and d22, squared
	// Sort out the two smallest distances

	// Do it right and find both F1 and F2
	d.xy = (d.x < d.y) ? d.xy : d.yx; // Swap if smaller
	d.xz = (d.x < d.z) ? d.xz : d.zx;
	d.xw = (d.x < d.w) ? d.xw : d.wx;
	d.y = min(d.y, d.z);
	d.y = min(d.y, d.w);
	return sqrt(d.xy);

}

// Worley noise, periodic variant
#if defined(USE_WORLEY_HASH)
float2 worleyNoise3D(float3 f, float jitter, bool manhattanDistance, float valMax)
#else
float2 worleyNoise3D(float3 P, float jitter, bool manhattanDistance, float rep)
#endif
{

#if defined(USE_WORLEY_HASH)	

	f *= valMax;
	float3 cellVariance = float3(jitter, jitter, jitter);
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

#else

	P += offset;
	P *= rep;

	float3 Pi = mod(floor(P), 289.0);
 	float3 Pf = frac(P) - 0.5;

 	float3 oi = float3(-1.0, 0.0, 1.0);
 	float3 io = float3( 1.0, 0.0,-1.0);

	float3 Pfx = Pf.x + io;
	float3 Pfy = Pf.y + io;
	float3 Pfz = Pf.z + io;

	float3 p  = permute(mod(Pi.x + oi, rep));
	float3 p1 = permute(mod(p + Pi.y - 1.0, rep));
	float3 p2 = permute(mod(p + Pi.y, rep));
	float3 p3 = permute(mod(p + Pi.y + 1.0, rep));

	float3 p11 = permute(mod(p1 + Pi.z - 1.0, rep));
	float3 p12 = permute(mod(p1 + Pi.z, rep));
	float3 p13 = permute(mod(p1 + Pi.z + 1.0, rep));

	float3 p21 = permute(mod(p2 + Pi.z - 1.0, rep));
	float3 p22 = permute(mod(p2 + Pi.z, rep));
	float3 p23 = permute(mod(p2 + Pi.z + 1.0, rep));

	float3 p31 = permute(mod(p3 + Pi.z - 1.0, rep));
	float3 p32 = permute(mod(p3 + Pi.z, rep));
	float3 p33 = permute(mod(p3 + Pi.z + 1.0, rep));

	float3 ox11 = frac(p11*K) - Ko;
	float3 oy11 = mod(floor(p11*K), 7.0)*K - Ko;
	float3 oz11 = floor(p11*K2)*Kz - Kzo; // p11 < 289 guaranteed

	float3 ox12 = frac(p12*K) - Ko;
	float3 oy12 = mod(floor(p12*K), 7.0)*K - Ko;
	float3 oz12 = floor(p12*K2)*Kz - Kzo;

	float3 ox13 = frac(p13*K) - Ko;
	float3 oy13 = mod(floor(p13*K), 7.0)*K - Ko;
	float3 oz13 = floor(p13*K2)*Kz - Kzo;

	float3 ox21 = frac(p21*K) - Ko;
	float3 oy21 = mod(floor(p21*K), 7.0)*K - Ko;
	float3 oz21 = floor(p21*K2)*Kz - Kzo;

	float3 ox22 = frac(p22*K) - Ko;
	float3 oy22 = mod(floor(p22*K), 7.0)*K - Ko;
	float3 oz22 = floor(p22*K2)*Kz - Kzo;

	float3 ox23 = frac(p23*K) - Ko;
	float3 oy23 = mod(floor(p23*K), 7.0)*K - Ko;
	float3 oz23 = floor(p23*K2)*Kz - Kzo;

	float3 ox31 = frac(p31*K) - Ko;
	float3 oy31 = mod(floor(p31*K), 7.0)*K - Ko;
	float3 oz31 = floor(p31*K2)*Kz - Kzo;

	float3 ox32 = frac(p32*K) - Ko;
	float3 oy32 = mod(floor(p32*K), 7.0)*K - Ko;
	float3 oz32 = floor(p32*K2)*Kz - Kzo;

	float3 ox33 = frac(p33*K) - Ko;
	float3 oy33 = mod(floor(p33*K), 7.0)*K - Ko;
	float3 oz33 = floor(p33*K2)*Kz - Kzo;

	float3 dx11 = Pfx + jitter*ox11;
	float3 dy11 = Pfy.x + jitter*oy11;
	float3 dz11 = Pfz.x + jitter*oz11;

	float3 dx12 = Pfx + jitter*ox12;
	float3 dy12 = Pfy.x + jitter*oy12;
	float3 dz12 = Pfz.y + jitter*oz12;

	float3 dx13 = Pfx + jitter*ox13;
	float3 dy13 = Pfy.x + jitter*oy13;
	float3 dz13 = Pfz.z + jitter*oz13;

	float3 dx21 = Pfx + jitter*ox21;
	float3 dy21 = Pfy.y + jitter*oy21;
	float3 dz21 = Pfz.x + jitter*oz21;

	float3 dx22 = Pfx + jitter*ox22;
	float3 dy22 = Pfy.y + jitter*oy22;
	float3 dz22 = Pfz.y + jitter*oz22;

	float3 dx23 = Pfx + jitter*ox23;
	float3 dy23 = Pfy.y + jitter*oy23;
	float3 dz23 = Pfz.z + jitter*oz23;

	float3 dx31 = Pfx + jitter*ox31;
	float3 dy31 = Pfy.z + jitter*oy31;
	float3 dz31 = Pfz.x + jitter*oz31;

	float3 dx32 = Pfx + jitter*ox32;
	float3 dy32 = Pfy.z + jitter*oy32;
	float3 dz32 = Pfz.y + jitter*oz32;

	float3 dx33 = Pfx + jitter*ox33;
	float3 dy33 = Pfy.z + jitter*oy33;
	float3 dz33 = Pfz.z + jitter*oz33;

	float3 d11 = dist3(dx11, dy11, dz11, manhattanDistance);
	float3 d12 = dist3(dx12, dy12, dz12, manhattanDistance);
	float3 d13 = dist3(dx13, dy13, dz13, manhattanDistance);
	float3 d21 = dist3(dx21, dy21, dz21, manhattanDistance);
	float3 d22 = dist3(dx22, dy22, dz22, manhattanDistance);
	float3 d23 = dist3(dx23, dy23, dz23, manhattanDistance);
	float3 d31 = dist3(dx31, dy31, dz31, manhattanDistance);
	float3 d32 = dist3(dx32, dy32, dz32, manhattanDistance);
	float3 d33 = dist3(dx33, dy33, dz33, manhattanDistance);

	float3 d1a = min(d11, d12);
	d12 = max(d11, d12);
	d11 = min(d1a, d13); // Smallest now not in d12 or d13
	d13 = max(d1a, d13);
	d12 = min(d12, d13); // 2nd smallest now not in d13
	float3 d2a = min(d21, d22);
	d22 = max(d21, d22);
	d21 = min(d2a, d23); // Smallest now not in d22 or d23
	d23 = max(d2a, d23);
	d22 = min(d22, d23); // 2nd smallest now not in d23
	float3 d3a = min(d31, d32);
	d32 = max(d31, d32);
	d31 = min(d3a, d33); // Smallest now not in d32 or d33
	d33 = max(d3a, d33);
	d32 = min(d32, d33); // 2nd smallest now not in d33
	float3 da = min(d11, d21);
	d21 = max(d11, d21);
	d11 = min(da, d31); // Smallest now in d11
	d31 = max(da, d31); // 2nd smallest now not in d31
	d11.xy = (d11.x < d11.y) ? d11.xy : d11.yx;
	d11.xz = (d11.x < d11.z) ? d11.xz : d11.zx; // d11.x now smallest
	d12 = min(d12, d21); // 2nd smallest now not in d21
	d12 = min(d12, d22); // nor in d22
	d12 = min(d12, d31); // nor in d31
	d12 = min(d12, d32); // nor in d32
	d11.yz = min(d11.yz,d12.xy); // nor in d12.yz
	d11.y = min(d11.y,d12.z); // Only two more to go
	d11.y = min(d11.y,d11.z); // Done! (Phew!)
	return sqrt(d11.xy); // F1, F2

#undef dist

#endif

}

// MIT license: https://gist.github.com/fadookie/25adf86ae7e2753d717c
float simplexNoise2D(float2 v)
{
	const float4 C = float4
	(
		0.211324865405187, // (3.0-sqrt(3.0))/6.0
		0.366025403784439, // 0.5*(sqrt(3.0)-1.0)
		-0.577350269189626, // -1.0 + 2.0 * C.x
		0.024390243902439  // 1.0 / 41.0
	);

	// First corner
	float2 i = floor(v + dot(v, C.yy));
	float2 x0 = v - i + dot(i, C.xx);

	// Other corners
	// float2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
	// Lex-DRL: afaik, step() in GPU is faster than if(), so:
	// step(x, y) = x <= y

	// Actually, a simple conditional without branching is faster than that madness :)
	int2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
	float4 x12 = x0.xyxy + C.xxzz;
	x12.xy -= i1;

	// Permutations
	i = mod289(i); // Avoid truncation effects in permutation
	float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));

	float3 m = max
	(
		0.5 - float3
		(
			dot(x0, x0),
			dot(x12.xy, x12.xy),
			dot(x12.zw, x12.zw)
		),
		0.0
	);
	m = m * m;
	m = m * m;

	// Gradients: 41 points uniformly over a line, mapped onto a diamond.
	// The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

	float3 x = 2.0 * frac(p * C.www) - 1.0;
	float3 h = abs(x) - 0.5;
	float3 ox = floor(x + 0.5);
	float3 a0 = x - ox;

	// Normalise gradients implicitly by scaling m
	// Approximation of: m *= inversesqrt( a0*a0 + h*h );
	m *= 1.79284291400159 - 0.85373472095314 * (a0*a0 + h * h);

	// Compute final noise value at P
	float3 g;
	g.x = a0.x * x0.x + h.x * x0.y;
	g.yz = a0.yz * x12.xz + h.yz * x12.yw;
	return 130.0 * dot(m, g);
}

float simplexNoise3D(float3 v)
{
	const float2 C = float2
	(
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

float simplexNoise3D(float3 v, float3 rep)
{
	// TODO: Make this actually tile
	v *= rep;

	const float2 C = float2
		(
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

#define WORLEY_JITTER 1.2
#define WORLEY_MANHATTAN_DISTANCE false // true
#define INVERT_NOISE(noise, invert) lerp(noise, 1.0 - noise, invert)

float CloudPerlinFBMNoise(float3 pos, int octaves, float rep, int invert, float offset)
{
	float value = 0.0;
	float amplitude = 0.5;
	float noise;
	pos.z += offset;

	UNITY_LOOP
	while (octaves > 0)
	{
		noise = perlinNoise3D(pos, rep);
		value += (INVERT_NOISE(noise, invert) * amplitude);
		amplitude *= 0.5;
		rep *= 2.0;
		octaves--;
	}

	return (value * 0.5) + 0.5; // [-1, 1] -> [0, 1]
}

float CloudSimplexFBMNoise(float3 pos, int octaves, float rep, int invert, float offset)
{
	float value = 0.0;
	float amplitude = 0.5;
	pos.z += offset;

	UNITY_LOOP
	while (octaves > 0)
	{
		value += (INVERT_NOISE(simplexNoise3D(pos, rep), invert) * amplitude);
		amplitude *= 0.5;
		rep *= 2.0;
		octaves--;
	}

	return (value * 0.5) + 0.5; // [-1, 1] -> [0, 1]
}

float CloudWorleyFBMNoise(float3 pos, int octaves, float rep, int invert, float offset)
{
	float value = 0;
	float amplitude = 0.5;
	pos.z += offset;

	UNITY_LOOP
	while (octaves > 0)
	{

#if defined(USE_WORLEY_HASH)

		value += (amplitude * INVERT_NOISE(worleyNoise3D(pos, 0.5, true, rep).x, invert));
		//value += (amplitude * INVERT_NOISE(optimizeWorleyNoise3D(pos, rep), invert));

#else

		value += (amplitude * INVERT_NOISE(1.0 - worleyNoise3D(pos, WORLEY_JITTER, WORLEY_MANHATTAN_DISTANCE, rep).x, invert));

#endif

		amplitude *= 0.5;
		rep *= 2.0;
		octaves--;
	}

	return saturate(value);
}

float CloudPerlinNoise(float3 pos, float4 param1, float4 param2)
{
	float perlin = CloudPerlinFBMNoise(pos, param1.x, param1.y, param1.w, param2.z);
	perlin += param1.z;
	perlin *= param2.x;
	perlin = pow(max(0.0, perlin), param2.y);

	return saturate(perlin);
}

float CloudSimplexNoise(float3 pos, float4 param1, float4 param2)
{
	float perlin = CloudSimplexFBMNoise(pos, param1.x, param1.y, param1.w, param2.z);
	perlin += param1.z;
	perlin *= param2.x;
	perlin = pow(max(0.0, perlin), param2.y);

	return saturate(perlin);
}

float CloudWorleyNoise(float3 pos, float4 param1, float4 param2)
{
	float worley = CloudWorleyFBMNoise(pos, param1.x, param1.y, param1.w, param2.z);
	worley += param1.z;
	worley *= param2.x;
	worley = pow(max(0.0, worley), param2.y);

	return saturate(worley);
}

float CloudPerlinWorleyNoise(float3 pos, float4 perlinParam1, float4 perlinParam2, float4 worleyParam1, float4 worleyParam2)
{
	float perlin = CloudPerlinNoise(pos, perlinParam1, perlinParam2);
	float worley = CloudWorleyNoise(pos, worleyParam1, worleyParam2);
	return worley + (perlin * (1.0 - worley));
	//return lerp(perlin, worley, worley);
}

float CloudSimplexWorleyNoise(float3 pos, float4 perlinParam1, float4 perlinParam2, float4 worleyParam1, float4 worleyParam2)
{
	float perlin = CloudSimplexNoise(pos, perlinParam1, perlinParam2);
	float worley = CloudWorleyNoise(pos, worleyParam1, worleyParam2);
	return saturate(worley + (perlin * (1.0 - worley)));
	//return lerp(perlin, worley, worley);
}

float CloudPerlinWorleyNoise2(float3 pos, float4 perlinParam1, float4 perlinParam2, float4 worleyParam1, float4 worleyParam2)
{
	float perlin = CloudPerlinNoise(pos, perlinParam1, perlinParam2);
	float worley = CloudWorleyNoise(pos, worleyParam1, worleyParam2);
	return saturate(perlin * worley);
}

float CloudSimplexWorleyNoise2(float3 pos, float4 perlinParam1, float4 perlinParam2, float4 worleyParam1, float4 worleyParam2)
{
	float perlin = CloudSimplexNoise(pos, perlinParam1, perlinParam2);
	float worley = CloudWorleyNoise(pos, worleyParam1, worleyParam2);
	return perlin * worley;
}

float CloudCumulusNoise(float3 pos, float4 perlinParam1, float4 perlinParam2, float4 worleyParam1, float4 worleyParam2)
{
	float weight1 = 0.6;
	float weight2 = 0.3;
	float weight3 = 0.1;

	float perlin = saturate(CloudPerlinNoise(pos, perlinParam1, perlinParam2) * weight1);
	float worley = saturate(CloudWorleyNoise(pos, worleyParam1, worleyParam2) * weight1);

	perlinParam1.x *= 2;
	worleyParam1.x *= 2;
	//perlinParam1.y++;
	//worleyParam1.y++;
	perlin += saturate(CloudPerlinNoise(pos, perlinParam1, perlinParam2) * weight2);
	worley += saturate(CloudWorleyNoise(pos, worleyParam1, worleyParam2) * weight2);

	perlinParam1.x *= 2;
	worleyParam1.x *= 2;
	//perlinParam1.y++;
	//worleyParam1.y++;

	perlin += saturate(CloudPerlinNoise(pos, perlinParam1, perlinParam2) * weight3);
	worley += saturate(CloudWorleyNoise(pos, worleyParam1, worleyParam2) * weight3);

	return saturate(worley + (perlin * (1.0 - worley)));
}

float3 CloudCurlNoiseInner(float3 x, float4 perlinParam1, float4 perlinParam2)
{
	float period = perlinParam1.y;
	float3 offsets1 = float3(-19.1, 33.4, 47.2) / period;
	float3 offsets2 = float3(74.2, -124.5, 99.4) / period;
	float s0 = CloudPerlinNoise(x, perlinParam1, perlinParam2);
	float s1 = CloudPerlinNoise(x + offsets1, perlinParam1, perlinParam2);
	float s2 = CloudPerlinNoise(x + offsets2, perlinParam1, perlinParam2);
	return float3(s0, s1, s2);
}

float3 CloudCurlNoise(float3 p, float4 perlinParam1, float4 perlinParam2)
{
	// https://codepen.io/mnmxmx/pen/rzqoeW
	const float e = 0.5 / perlinParam1.y;
	float3 dx = float3(e, 0.0, 0.0);
	float3 dy = float3(0.0, e, 0.0);
	float3 dz = float3(0.0, 0.0, e);

	float3 p_x0 = CloudCurlNoiseInner(p - dx, perlinParam1, perlinParam2);
	float3 p_x1 = CloudCurlNoiseInner(p + dx, perlinParam1, perlinParam2);
	float3 p_y0 = CloudCurlNoiseInner(p - dy, perlinParam1, perlinParam2);
	float3 p_y1 = CloudCurlNoiseInner(p + dy, perlinParam1, perlinParam2);
	float3 p_z0 = CloudCurlNoiseInner(p - dz, perlinParam1, perlinParam2);
	float3 p_z1 = CloudCurlNoiseInner(p + dz, perlinParam1, perlinParam2);

	float x = p_y1.z - p_y0.z - p_z1.y + p_z0.y;
	float y = p_z1.x - p_z0.x - p_x1.z + p_x0.z;
	float z = p_x1.y - p_x0.y - p_y1.x + p_y0.x;

	const float divisor = 1.0 / (2.0 * e);
	return normalize(float3(x, y, z) * divisor);
}

float3 CloudNoise(float3 pos, int type, float4 perlinParam1, float4 perlinParam2, float4 worleyParam1, float4 worleyParam2)
{
	UNITY_BRANCH
	switch (type)
	{
	default:
		return CloudPerlinNoise(pos, perlinParam1, perlinParam2);

	case 20:
		return CloudSimplexNoise(pos, perlinParam1, perlinParam2);

	case 50:
		return CloudWorleyNoise(pos, worleyParam1, worleyParam2);

	case 100:
		return CloudPerlinWorleyNoise(pos, perlinParam1, perlinParam2, worleyParam1, worleyParam2);

	case 110:
		return CloudSimplexWorleyNoise(pos, perlinParam1, perlinParam2, worleyParam1, worleyParam2);

	case 120:
		return CloudPerlinWorleyNoise2(pos, perlinParam1, perlinParam2, worleyParam1, worleyParam2);

	case 130:
		return CloudSimplexWorleyNoise2(pos, perlinParam1, perlinParam2, worleyParam1, worleyParam2);

	case 140:
		return CloudCumulusNoise(pos, perlinParam1, perlinParam2, worleyParam1, worleyParam2);

	case 150:
		return CloudCurlNoise(pos, perlinParam1, perlinParam2);
	}
}

#endif // CLOUDNOISELIB_INCLUDED
