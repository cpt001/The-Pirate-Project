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

#ifndef WATHER_MAKER_NULL_ZONE_SHADER_INCLUDED
#define WATHER_MAKER_NULL_ZONE_SHADER_INCLUDED

#include "WeatherMakerMathShaderInclude.cginc"

// NULL_ZONE_RENDER_MASK
// 1 = precipitation
// 2 = fog
// 4 = overlay (snow, wetness)
// 8 = water

#define MAX_NULL_ZONE_COUNT 16
#define NULL_ZONE_TYPE_BOX_NOT_ROTATED 0
#define NULL_ZONE_TYPE_BOX_ROTATED 1
#define NULL_ZONE_TYPE_SPHERE 2

uniform int _NullZoneCount;
uniform float4 _NullZonesMin[MAX_NULL_ZONE_COUNT];
uniform float4 _NullZonesMax[MAX_NULL_ZONE_COUNT]; // w is sphere radius if zone type is sphere
uniform float4 _NullZonesCenter[MAX_NULL_ZONE_COUNT];
uniform float4 _NullZonesQuaternion[MAX_NULL_ZONE_COUNT];
uniform float4 _NullZonesParams[MAX_NULL_ZONE_COUNT]; // strength, mask, fade, type

bool CheckNullZoneMask(int nullIndex)
{

#if defined(NULL_ZONE_RENDER_MASK)

#if SHADER_TARGET < 40

	switch (abs(_NullZonesParams[nullIndex].y))// (abs(_NullZonesMin[nullIndex].w))
	{
	default: return true;
	case 1: return (NULL_ZONE_RENDER_MASK != 1);
	case 2: return (NULL_ZONE_RENDER_MASK != 2);
	case 3: return (NULL_ZONE_RENDER_MASK != 1 && NULL_ZONE_RENDER_MASK != 2);
	case 4: return (NULL_ZONE_RENDER_MASK != 4);
	case 5: return (NULL_ZONE_RENDER_MASK != 1 && NULL_ZONE_RENDER_MASK != 4);
	case 6: return (NULL_ZONE_RENDER_MASK != 2 && NULL_ZONE_RENDER_MASK != 4);
	case 7: return (NULL_ZONE_RENDER_MASK != 1 && NULL_ZONE_RENDER_MASK != 2 && NULL_ZONE_RENDER_MASK != 4);
	case 8: return (NULL_ZONE_RENDER_MASK != 8);
	case 9: return (NULL_ZONE_RENDER_MASK != 1 && NULL_ZONE_RENDER_MASK != 8);
	case 10: return (NULL_ZONE_RENDER_MASK != 2 && NULL_ZONE_RENDER_MASK != 8);
	case 11: return (NULL_ZONE_RENDER_MASK != 1 && NULL_ZONE_RENDER_MASK != 2 && NULL_ZONE_RENDER_MASK != 8);
	case 12: return (NULL_ZONE_RENDER_MASK != 4 && NULL_ZONE_RENDER_MASK != 8);
	case 13: return (NULL_ZONE_RENDER_MASK != 1 && NULL_ZONE_RENDER_MASK != 4 && NULL_ZONE_RENDER_MASK != 8);
	case 14: return (NULL_ZONE_RENDER_MASK != 2 && NULL_ZONE_RENDER_MASK != 4 && NULL_ZONE_RENDER_MASK != 8);
	case 15: return (NULL_ZONE_RENDER_MASK != 1 && NULL_ZONE_RENDER_MASK != 2 && NULL_ZONE_RENDER_MASK != 4 && NULL_ZONE_RENDER_MASK != 8);
	}

#else

	uint mask = uint(_NullZonesParams[nullIndex].y);// uint(_NullZonesMin[nullIndex].w); // w of min pos is mask
	return !(mask & NULL_ZONE_RENDER_MASK);

#endif

#else

	return false;

#endif

}

float NullZoneRayIntersect(int nullIndex, float3 startPos, float3 rayDir, float depth, out float amount, out float distanceToZone)
{
	uint zoneType = uint(_NullZonesParams[nullIndex].w);

	UNITY_BRANCH
	if (zoneType == NULL_ZONE_TYPE_BOX_NOT_ROTATED)
	{
		// box, no rotation
		return RayBoxIntersect(startPos, rayDir, depth, _NullZonesMin[nullIndex].xyz, _NullZonesMax[nullIndex].xyz, amount, distanceToZone);
	}
	else if (zoneType == NULL_ZONE_TYPE_BOX_ROTATED)
	{
		// box, need to rotate startPos and rayDir
		startPos = RotatePointZeroOriginQuaternion(startPos - _NullZonesCenter[nullIndex].xyz, _NullZonesQuaternion[nullIndex]);
		rayDir = RotatePointZeroOriginQuaternion(rayDir, _NullZonesQuaternion[nullIndex]);
		return RayBoxIntersect(startPos, rayDir, depth, _NullZonesMin[nullIndex].xyz, _NullZonesMax[nullIndex].xyz, amount, distanceToZone);
	}
	else // NULL_ZONE_TYPE_SPHERE
	{
		// sphere
		float2 intersect = RaySphereIntersect(startPos, rayDir, depth, _NullZonesCenter[nullIndex]);
		amount = intersect.y;
		distanceToZone = intersect.x;
		return (amount > 0.0);
	}
}

float3 RotateNullZoneWorldPosBox(int nullIndex, float3 worldPos)
{
	float isRotatedBox = (uint(_NullZonesParams[nullIndex].w) == NULL_ZONE_TYPE_BOX_ROTATED);
	return (worldPos * (1.0 - isRotatedBox)) +
		(isRotatedBox * RotatePointZeroOriginQuaternion(worldPos - _NullZonesCenter[nullIndex].xyz, _NullZonesQuaternion[nullIndex]));
}

void ClipWorldPosNullZone(int nullIndex, float3 worldPos)
{
	float outOfZone;
	UNITY_BRANCH
	if (uint(_NullZonesParams[nullIndex].w) == NULL_ZONE_TYPE_SPHERE)
	{
		outOfZone = PointSphereOutside(worldPos, _NullZonesCenter[nullIndex]);
	}
	else
	{
		float3 worldPosRotated = RotateNullZoneWorldPosBox(nullIndex, worldPos);
		float3 minPos = _NullZonesMin[nullIndex].xyz;
		float3 maxPos = _NullZonesMax[nullIndex].xyz;
		outOfZone = PointBoxOutside(worldPosRotated, minPos, maxPos);
	}

	// clip if we are in the null zone
	clip(-0.5 + outOfZone);
}

fixed ClipWorldPosNullZoneAlpha(int nullIndex, float3 worldPos)
{
	uint isSphere = (uint(_NullZonesParams[nullIndex].w) == NULL_ZONE_TYPE_SPHERE);
	float3 minPos = _NullZonesMin[nullIndex].xyz;
	float3 maxPos = _NullZonesMax[nullIndex].xyz;
	float3 worldPosRotated = float3Zero;
	float fade = _NullZonesParams[nullIndex].z; // _NullZonesMax[nullIndex].w
	float worldPosOutOfZone;

	UNITY_BRANCH
	if (isSphere)
	{
		worldPosOutOfZone = PointSphereOutside(worldPos, _NullZonesCenter[nullIndex]);
	}
	else
	{
		worldPosRotated = RotateNullZoneWorldPosBox(nullIndex, worldPos);
		worldPosOutOfZone = PointBoxOutside(worldPosRotated, minPos, maxPos);
	}

#if NULL_ZONE_RENDER_MASK == 4

	// clip if world pos is in the zone
	float noFade = (fade <= 0.0f);
	clip(noFade + -0.5 + worldPosOutOfZone);

#else

	clip(-0.5 + worldPosOutOfZone);

#endif

#if NULL_ZONE_RENDER_MASK == 4

	// if special fade option and the world pos is in the zone, do special fade option
	UNITY_BRANCH
	if (noFade)
	{
		UNITY_BRANCH
		if (worldPosOutOfZone == 0.0)
		{
			UNITY_BRANCH
			if (isSphere)
			{
				float d = PointSphereDistanceSquared(WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET, float4(_NullZonesCenter[nullIndex].xyz, _NullZonesMax[nullIndex].w));
				return min(1.0, d * abs(fade) * _NullZonesParams[nullIndex].x);
			}
			else
			{
				float3 camPosRotated = RotateNullZoneWorldPosBox(nullIndex, WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET);
				float cameraOutOfZone = PointBoxOutside(camPosRotated, minPos, maxPos);

				// fade as camera gets closer to the zone, camera in zone has 0 fade
				float3 deltaPos = cameraOutOfZone * max(float3Zero, max(minPos - camPosRotated, camPosRotated - maxPos));
				float d = dot(deltaPos, deltaPos);
				return min(1.0, d * abs(fade) * _NullZonesParams[nullIndex].x);
			}
		}
		else
		{
			// out of the zone at full strength
			return 1.0;
		}
	}
	else
	{

#else

		fade = abs(fade);

#endif

		UNITY_BRANCH
		if (isSphere)
		{
			float d = PointSphereDistanceSquared(worldPos, float4(_NullZonesCenter[nullIndex].xyz, _NullZonesMax[nullIndex].w));
			return min(1.0, d * fade * _NullZonesParams[nullIndex].x);
		}
		else
		{
			float3 deltaPos = max(float3Zero, max(minPos - worldPosRotated, worldPosRotated - maxPos));
			float d = dot(deltaPos, deltaPos);
			return min(1.0, d * fade * _NullZonesParams[nullIndex].x);
		}

#if NULL_ZONE_RENDER_MASK == 4

	}

#endif

}

void ClipWorldPosNullZones(float3 worldPos)
{
	UNITY_BRANCH
	if (_NullZoneCount > 0)
	{
		UNITY_LOOP
		for (int nullIndex = 0; nullIndex < _NullZoneCount; nullIndex++)
		{
			UNITY_BRANCH
			if (CheckNullZoneMask(nullIndex))
			{
				ClipWorldPosNullZone(nullIndex, worldPos);
			}
		}
	}
}

fixed ClipWorldPosNullZonesAlpha(float3 worldPos)
{
	fixed alpha = 1.0;

	UNITY_BRANCH
	if (_NullZoneCount > 0)
	{
		UNITY_LOOP
		for (int nullIndex = 0; nullIndex < _NullZoneCount; nullIndex++)
		{
			UNITY_BRANCH
			if (CheckNullZoneMask(nullIndex))
			{
				alpha *= ClipWorldPosNullZoneAlpha(nullIndex, worldPos);
			}
		}
	}

	return alpha;
}

float ClipWorldPosNullZonesFade(float3 worldPos, float3 rayDir, float depth, float worldPosDepth, float invFade)
{
	float alpha = 1.0;
	UNITY_BRANCH
	if (_NullZoneCount > 0)
	{
		rayDir = normalize(rayDir);
		float amount, toZone, sceneZ, diff;
		UNITY_LOOP
		for (int nullIndex = 0; nullIndex < _NullZoneCount; nullIndex++)
		{
			UNITY_BRANCH
			if (CheckNullZoneMask(nullIndex))
			{
				ClipWorldPosNullZone(nullIndex, worldPos);
				float intersect = NullZoneRayIntersect(nullIndex, WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET, rayDir, depth, amount, toZone);

				UNITY_FLATTEN
				if (intersect)
				{
					UNITY_FLATTEN
					if (toZone < 0.01)
					{
						// inside the null zone looking out
						sceneZ = length(rayDir * amount);
						diff = (worldPosDepth - sceneZ);
					}
					else
					{
						// outside the null zone looking in
						sceneZ = length(rayDir * toZone);
						diff = (sceneZ - worldPosDepth);
					}
					alpha *= saturate(invFade * diff * _NullZonesParams[nullIndex].x);
				}
			}
		}
	}
	return alpha;
}

void GetNullZonesDepth(float3 startPos, float3 rayDir, inout float depth)
{
	float nullDepth, distanceToZone, intersect;
	UNITY_LOOP
	for (int nullIndex = 0; nullIndex < _NullZoneCount; nullIndex++)
	{
		UNITY_BRANCH
		if (CheckNullZoneMask(nullIndex))
		{
			intersect = NullZoneRayIntersect(nullIndex, startPos, rayDir, depth, nullDepth, distanceToZone);
			depth = max(0.0, depth - (intersect * nullDepth));
		}
	}
}

#endif
