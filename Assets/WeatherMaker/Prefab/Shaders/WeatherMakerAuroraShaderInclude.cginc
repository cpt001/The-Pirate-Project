// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset from a pirate site, please consider buying it from the Unity asset store at https://assetstore.unity.com/packages/slug/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many other asset store publishers) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//
// Since I've been threatened with a lawsuit about this aurora borealis code, I have documented my process,
// creativity and learning. That's why you will see many more comments than normal in this shader file.
//
// Now for my official legal statement:
// I, Jeff Johnson, assert that this source code and the algorithms and techniques in this shader file,
// and Weather Maker as a whole, were derived by my own creativity and by referencing only commercial licensed source code
// (MIT, public domain, etc.), along with snippets and techniques from several books and websites about opengl and procedural
// noise generation.
//
// Furthermore, this text serves as notice that the techniques and code in this file, and Weather Maker in general,
// are copyright (c) 2016 to now by Digital Ruby, LLC and must not be copied, redistributed or reproduced without express written consent
// from Digital Ruby, LLC
//
// Thank you.
//
// - Jeff Johnson
//
// References:
// http://madebyevan.com/shaders/grid/
// https://thebookofshaders.com/07/ - https://thebookofshaders.com/13/
// https://andreashackel.de/tech-art/stripes-shader-1/ - https://andreashackel.de/tech-art/stripes-shader-3/
// https://github.com/ashima/webgl-noise/blob/master/LICENSE
// https://gist.github.com/yiwenl/3f804e80d0930e34a0b33359259b556c
// https://www.npmjs.com/package/grunt-glsl
// https://gamedev.stackexchange.com/questions/141916/antialiasing-shader-grid-lines
// https://iquilezles.org/www/articles/filterableprocedurals/filterableprocedurals.htm
// http://glslsandbox.com/e#52457.0
// https://www.amazon.com/OpenGL-Insights-Patrick-Cozzi/dp/1439893764
//

#ifndef WEATHER_MAKER_AURORA_SHADER_INCLUDE__
#define WEATHER_MAKER_AURORA_SHADER_INCLUDE__

#include "WeatherMakerMathShaderInclude.cginc"

uniform uint _WeatherMakerAuroraSampleCount;
uniform uint _WeatherMakerAuroraSubSampleCount;
uniform float3 _WeatherMakerAuroraAnimationSpeed;
uniform float3 _WeatherMakerAuroraMarchScale;
uniform float3 _WeatherMakerAuroraScale;
uniform fixed3 _WeatherMakerAuroraColor[4];
uniform float4 _WeatherMakerAuroraColorKeys;
uniform float2 _WeatherMakerAuroraHeight;
uniform float _WeatherMakerAuroraPlanetRadius;
uniform fixed _WeatherMakerAuroraIntensity;
uniform fixed _WeatherMakerAuroraPower;
uniform fixed _WeatherMakerAuroraHeightFadePower;
uniform float _WeatherMakerAuroraDistanceFade;
uniform float _WeatherMakerAuroraDither;
uniform float _WeatherMakerAuroraSeed;
uniform float2 _WeatherMakerAuroraOctave;

#define AURORA_MAX_SAMPLE_COUNT 64
#define AURORA_PLANET_RADIUS _WeatherMakerAuroraPlanetRadius

static const float auroraSeed = fmod(_WeatherMakerAuroraSeed, 289.0);

// TODO: At high values of time, things break down
static const float3 auroraAnimationValue = (auroraSeed + (_WeatherMakerTime.y * _WeatherMakerAuroraAnimationSpeed));

float2 GetSinCosAuroraAnimationValue()
{
	float2 v;
	sincos(auroraAnimationValue.z, v.x, v.y);
	return v;
}

static const float2 auroraAnimationValueSinCos = GetSinCosAuroraAnimationValue();

// pos xy is the shape position, pos zw is the detail position
float auroraLineNoise(inout float4 pos)
{
	// aurora noise notes:
	// aurora does not need 3D noise, if you look at all the pictures, they are very flat in the vertical
	// direction with lots of bending and twisting - and they are basically always parallel
	// this noise function fails completely at making parallel aurora...
	// While this noise function is great at making general noise and foggy aurora in the sky,
	// it's not all that great for a realistic aurora, so in the future I need to figure out how
	// to use sin to make them curve more and not overlap, but for now is mostly good enough

	// aurora noise process and discovery notes:
	// for the aurora we want curves and lines - I started looking up grid shaders and made
	// some lines: horizontal, vertical, jagged and curved. Simple enough, except the lines had some hard edges
	// and box lines showing, this eventually led me to look up anti-aliasing grid techniques, where I found the
	// wonderful abs(frac(v) - 0.5) function! this created uniform and smooth lines
	// this was a great start, but I had to now somehow randomize these lines to look more curvy, jagged and random
	// my volumetric clouds have the concept of a shape and detail pass, so I finally settled on a similar technique
	// pos.xy is the shape position, pos.zw is the detail position
	
	// make horizontal and vertical straight lines, use anti-aliasing technique to soften edges on both sides
	float2 detailXY = ANTI_ALIAS(pos.wz);

	// make another set of lines to further increase randomization
	float detailZ = ANTI_ALIAS(pos.z + detailXY.x); // jagged vertical
	//detailZ += ANTI_ALIAS(pos.z + sin(detailXY.x)); // smooth vertical
	//detailZ += ANTI_ALIAS(pos.w + detailXY.y); // jagged horizontal
	//detailZ += ANTI_ALIAS(pos.w + sin(detailXY.y)); // smooth horizontal

	// I tried adding additional jagged and curved lines, but things got too noisy
	// there are many combinations here to map the detail noise values to a 2d offset,
	// but I found this one works pretty well
	float2 detailPosNoise = float2(detailZ, detailXY.x + detailXY.y); // detailXY;

	// scale detail noise
	detailPosNoise *= _WeatherMakerAuroraScale.z; // inner detail scale

	// 2d rotation around origin for animation, for an aurora this creates a nice wobble
	// effect, this works better than my original attempt which was to just scroll the
	// detail noise using the position, so I decided instead of translating the detail noise, why
	// not rotate it instead?
	detailPosNoise = RotateUV(detailPosNoise, auroraAnimationValueSinCos.x, auroraAnimationValueSinCos.y);

	// note scale and rotate order does not matter

	// we are done with the detail calculations
	// add detail position noise to shape position, even a small value added is quite enough to
	// dramatically change the noise, this warps and randomizes the shape noise
	pos.xy += detailPosNoise;

	// compute shape noise from the shape position, in a similar way as the detail noise was computed
	// the shape noise is the general structure of the noise, without the details noise it
	// looks like either jagged or smooth curved lines depending on if sin is used or not
	float shapeX = (pos.y); // sin(pos.y); // looks a little better without the ANTI_ALIAS
	float shapeY = ANTI_ALIAS(pos.x); // sin(pos.x)
	float shapeNoise = ANTI_ALIAS(shapeX + shapeY); // sin(shapeX + shapeY)

	return shapeNoise;
}

float auroraLineNoiseFbm(float2 pos)
{
	static const uint auroraNoiseFbmCount = ceil(_WeatherMakerAuroraSubSampleCount *
	(
		WM_CAMERA_RENDER_MODE_NORMAL +
		lerp(0.0, 0.5, WM_CAMERA_RENDER_MODE_REFLECTION) +
		lerp(0.0, 0.5, WM_CAMERA_RENDER_MODE_CUBEMAP)
	));

	// const
	// allow scaling shape pos and detail pos at separate rates to further increase randomness
	static const float shapePosMultiplier = _WeatherMakerAuroraScale.x;
	static const float detailPosMultiplier = _WeatherMakerAuroraScale.y; // lower for less detail, raise for more detail
	static const float octaveDecay = _WeatherMakerAuroraOctave.x; // how much noise should decay at each octave

	// vars
	float noise = 0.0; // accumulate noise, since this is fbm style loop
	float octaveAmp = _WeatherMakerAuroraOctave.y; // multiply final noise by this value each octave, reduces by octaveDecay each octave

	// scrolling animation
	pos.xy += auroraAnimationValue.xy;
	float4 noisePos = float4(pos.xy, pos.xy);

	// standard fbm loop, after 4 iterations, there is not much of a difference
	UNITY_LOOP
	for (uint i = 0; i < auroraNoiseFbmCount; i++)
	{
		// xy is the shape position
		noisePos.xy *= shapePosMultiplier;

		// zw is detail position
		noisePos.zw *= detailPosMultiplier;

		// accumulate noise
		noise += (auroraLineNoise(noisePos) * octaveAmp);

		// decay to next octave
		octaveAmp *= octaveDecay;
	}

	// at this point we have some thick lines, whispy areas and some dark areas
	// after playing around for half a day I realized the dark areas were more interesting
	// for purposes of our ray march we leave the noise as is and let the caller decide how to
	// use the dark areas and remap the noise, I tried a simple 1.0 - noise + pow and thought that
	// would work well, but the ray march likes to have softer, dimmer noise values,
	// otherwise there is a lot of banding, so we use the auroraMapNoiseValue function to do an inverse multiply
	return noise;
}

// 4 gradient lerp of aurora based on percentage of height through the aurora layer
fixed3 auroraLerpColorGradient(float heightFrac)
{
	// apply color depending on what percentage height we are in the aurora volume

	static const fixed3 auroraColorLerps = fixed3(1.0 / (_WeatherMakerAuroraColorKeys.y - _WeatherMakerAuroraColorKeys.x),
		1.0 / (_WeatherMakerAuroraColorKeys.z - _WeatherMakerAuroraColorKeys.y),
		1.0 / (_WeatherMakerAuroraColorKeys.w - _WeatherMakerAuroraColorKeys.z));

	// lerp color gradient
	// TODO: Is there possibility of further optimization here?
	fixed3 heightColor = fixed3Zero;
	fixed l = heightFrac * auroraColorLerps.x;
	heightColor += (lerp(_WeatherMakerAuroraColor[0], _WeatherMakerAuroraColor[1], l) * (l <= 1.0));
	l = (heightFrac - _WeatherMakerAuroraColorKeys.y) * auroraColorLerps.y;
	heightColor += (lerp(_WeatherMakerAuroraColor[1], _WeatherMakerAuroraColor[2], l) * (l >= 0.0 && l <= 1.0));
	l = (heightFrac - _WeatherMakerAuroraColorKeys.z) * auroraColorLerps.z;
	heightColor += (lerp(_WeatherMakerAuroraColor[2], _WeatherMakerAuroraColor[3], l) * (l >= 0.0 && l <= 1.0));
	return heightColor;
}

// map dark areas to light and remove most of the light areas, then soften it up greatly
float auroraMapNoiseValue(float noiseValue)
{
	// the ray march REALLY wants soft noise otherwise there is banding
	// so we invert the noise to get the dark areas to large values
	// and then multiply by a tiny value to reduce out anything that
	// was not really dark to begin with
	return min(0.02, saturate((1.0 / max(noiseValue, 0.0001)) * 0.002));
}

// get aurora color for a given ray origin, rayDir, uv screen coordinates and depth value
fixed4 ComputeAurora(float3 rayOrigin, float3 rayDir, float2 uv, float depth)
{
	// TODO: Fix banding, especially for higher aurora heights
	// TODO: Fix seams in noise, especially at 0,0

	// scale by default down by quite a lot
	static const float3 auroraMarchScale = float3(_WeatherMakerAuroraMarchScale.x * 0.000005, _WeatherMakerAuroraMarchScale.y, _WeatherMakerAuroraMarchScale.z * 0.000005);
	static const float invAuroraSampleCount = 1.0 / float(_WeatherMakerAuroraSampleCount);
	static const float auroraHeight = _WeatherMakerAuroraHeight.y - _WeatherMakerAuroraHeight.x;
	static const float invAuroraHeight = 1.0 / auroraHeight;
	static const float distFade = _WeatherMakerAuroraHeight.x * 2.0;
	static const float distFade2 = distFade * 10.0;
	static const float invDistFade = _WeatherMakerAuroraDistanceFade * (1.0 / distFade);
	static const float ditherStartPos = _WeatherMakerAuroraDither * 0.1;
	static const float auroraIntensity = (WM_CAMERA_RENDER_MODE_NORMAL ? _WeatherMakerAuroraIntensity : _WeatherMakerAuroraIntensity * 2.0);

	// exit out if we are not rendering aurora
	UNITY_BRANCH
	if (_WeatherMakerAuroraSampleCount == 0 || auroraIntensity < 0.001)
	{
		return fixed4Zero;
	}

	static const float planetRadiusStart = AURORA_PLANET_RADIUS + _WeatherMakerAuroraHeight.x;
	static const float innerRadius = pow((AURORA_PLANET_RADIUS + _WeatherMakerAuroraHeight.x), 2.0);
	static const float outterRadius = pow((AURORA_PLANET_RADIUS + _WeatherMakerAuroraHeight.y), 2.0);

	// move sphere away from 0,0 to reduce artifacts
	float4 auroraSphereSurface = float4(rayOrigin.x + 3769.325, -AURORA_PLANET_RADIUS, rayOrigin.z + 6134.356, AURORA_PLANET_RADIUS * AURORA_PLANET_RADIUS);
	float4 auroraSphereInner = float4(rayOrigin.x + 3769.325, -AURORA_PLANET_RADIUS, rayOrigin.z + 6134.356, innerRadius);
	float4 auroraSphereOutter = float4(rayOrigin.x + 3769.325, -AURORA_PLANET_RADIUS, rayOrigin.z + 6134.356, outterRadius);

	// accumulate aurora color in ray march
	fixed4 color = fixed4Zero;

	// noise full screen debug, correct for screen aspect ratio
	// uv = 4.0 * uv - 1.0; uv.x *= (_ScreenParams.x / _ScreenParams.y); return fixed4((auroraLineNoiseFbm(uv)).xxx, 1.0);

	// use same ray match as clouds
	float3 marchPos, marchPos2;
	float3 endPos, endPos2;
	float rayLength, rayLength2;
	float distanceToSphere, distanceToSphere2;
	uint iterations = SetupPlanetRaymarch(rayOrigin, rayDir, depth, depth, auroraSphereSurface, auroraSphereInner, auroraSphereOutter,
		marchPos, endPos, rayLength, distanceToSphere, marchPos2, endPos2, rayLength2, distanceToSphere2);

	// miss aurora sphere, exit out
	UNITY_BRANCH
	if (iterations == 0 || rayLength < 0.1)
	{
		return color;
	}

	// if we are right inside at the bottom of the layer, we can have a ray that hits inside and then exits and re-enters in the distant sphere
	UNITY_LOOP
	for (uint iteration = 0; iteration < iterations; iteration++)
	{
		// if specified, fade aurora at distance
		fixed distanceFadeMultiplier = lerp(1.0, 0.0, saturate((lerp(distanceToSphere, distanceToSphere2, iteration) - distFade) * invDistFade));

		// distance fade check, if it is tiny, just exit out
		UNITY_BRANCH
		if (distanceFadeMultiplier < 0.001)
		{
			break;
		}

		// pick our position based on which interation without branching
		float3 pos = lerp(marchPos, marchPos2, iteration);

		// as with volumetric clouds, banding can be reduced by perturbing the rayDir slightly
		float dither = RandomFloat(marchPos);
		rayDir *= (1.0 + (dither * _WeatherMakerAuroraDither));

		// additional dithering with start pos, similar to clouds
		pos += (rayDir * (dither * ditherStartPos));

		float3 marchDir = rayDir * lerp(rayLength, rayLength2, iteration) * invAuroraSampleCount;
		marchDir.y *= auroraMarchScale.y;
		float3 origMarchDir = marchDir;
		float2 scaledPos;

		// noise values
		fixed noiseValue;
		fixed heightFrac = 0.0;
		fixed heightFracSat;
		fixed4 subColor = fixed4Zero;
		uint i;

		// march while we are inside the aurora planet layer
		UNITY_LOOP
		for (i = 0; i < _WeatherMakerAuroraSampleCount && color.a < 0.99 && heightFrac > -0.01 && heightFrac < 1.01; i++)
		{
			// store pos in world space units, we need to scale to noise units
			scaledPos = (pos.xz * auroraMarchScale.xz);

			// sample the aurora at the current position
			noiseValue = auroraMapNoiseValue(auroraLineNoiseFbm(scaledPos));

			// compute height frac, the percentage to the top of the aurora layer we are, just like volumetric clouds
			heightFrac = (invAuroraHeight * (distance(pos, auroraSphereInner.xyz) - planetRadiusStart));
			heightFracSat = saturate(heightFrac);

			// reduce noise with height (aurora are fainter the higher up they go)
			noiseValue *= (1.0 - pow(heightFracSat, _WeatherMakerAuroraHeightFadePower));
			subColor.rgb += (noiseValue * auroraLerpColorGradient(heightFracSat));
			subColor.a += noiseValue;

			// march forward
			pos += marchDir;
		}

		// apply distance fade
		subColor *= distanceFadeMultiplier;

		color += subColor;
	}

	// ray march cost
	// return fixed4((float(i) / float(_WeatherMakerAuroraSampleCount)).xxx, 1.0);

	// ensure alpha is valid
	color.a = min(1.0, color.a);

	// fade as ray goes straight up or down, aurora converges to single point
	color.a *= min(1.0, (1.25 - pow(abs(rayDir.y), 2.0)));

	// increase fade out, we don't want foggy/whispy areas
	color.a = pow(color.a, _WeatherMakerAuroraPower);

	// add intensity
	color.rgb *= auroraIntensity;

	// pre-multiply
	color *= color.a;

	return color;
}

#endif
