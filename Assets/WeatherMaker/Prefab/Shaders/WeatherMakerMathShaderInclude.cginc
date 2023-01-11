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

#ifndef __WEATHER_MAKER_SHADER_MATH__
#define __WEATHER_MAKER_SHADER_MATH__

#include "WeatherMakerCoreShaderInclude.cginc"

float2x2 GetRotationMatrix2D(float angle)
{
	float s, c;
	sincos(angle, s, c);
	return float2x2(c, s, -s, c);
}

float2 SinCos(float angle)
{
	float2 sc;
	sincos(angle, sc.x, sc.y);
	return sc;
}

float PointBoxDistance(float3 p, float3 boxMin, float3 boxMax)
{
	return length(max(float3Zero, max(boxMin - p, p - boxMax)));
}

float PointBoxDistanceSquared(float3 p, float3 boxMin, float3 boxMax)
{
	float3 d = max(float3Zero, max(boxMin - p, p - boxMax));
	return dot(d, d);
}

float RayBoxIntersect(float3 rayOrigin, float3 rayDir, float rayLength, float3 boxMin, float3 boxMax, out float intersectAmount, out float distanceToBox)
{
	// https://tavianator.com/fast-branchless-raybounding-box-intersections/

	/*
	Aos::Vector3 t1(Aos::mulPerElem(m_min - ray.m_pos, ray.m_invDir));
	Aos::Vector3 t2(Aos::mulPerElem(m_max - ray.m_pos, ray.m_invDir));

	Aos::Vector3 tmin1(Aos::minPerElem(t1, t2));
	Aos::Vector3 tmax1(Aos::maxPerElem(t1, t2));

	float tmin = Aos::maxElem(tmin1);
	float tmax = Aos::minElem(tmax1);

	return tmax >= std::max(ray.m_min, tmin) && tmin < ray.m_max;
	*/

	float3 invRayDir = float3One / rayDir;
	float3 boxMinOrigin = boxMin - rayOrigin;
	float3 boxMaxOrigin = boxMax - rayOrigin;
	float3 t1 = boxMinOrigin * invRayDir;
	float3 t2 = boxMaxOrigin * invRayDir;
	float3 tmin1 = min(t1, t2);
	float3 tmax1 = max(t1, t2);
	float tmin = max(max(tmin1.x, tmin1.y), tmin1.z);
	float tmax = min(min(tmax1.x, tmax1.y), tmax1.z);
	float2 tt0 = max(tmin1.xx, tmin1.yz);
	distanceToBox = max(0.0, max(tt0.x, tt0.y));
	tt0 = min(tmax1.xx, tmax1.yz);
	float tt1 = min(tt0.x, tt0.y);
	tt1 = min(tt1, rayLength);
	intersectAmount = max(0.0, tt1 - distanceToBox);

	return intersectAmount > 0.0001;

	/*
	struct Ray
	{
		float3 origin;
		float3 direction;
		float3 inv_direction;
		float3 sign;
	};

	Ray makeRay(float3 origin, float3 direction)
	{
		float3 inv_direction = float3(1.0, 1.0, 1.0) / direction;
		Ray ray;
		ray.origin = origin;
		ray.direction = direction;
		ray.inv_direction = inv_direction;
		ray.sign = inv_direction < 0.0;
		return ray;
	}
	Ray ray = makeRay(rayOrigin, rayDir);
	float3 aabb[2];
	aabb[0] = boxMin;
	aabb[1] = boxMax;
	float tymin, tymax, tzmin, tzmax;
	float tmin = (aabb[ray.sign[0]].x - ray.origin.x) * ray.inv_direction.x;
	float tmax = (aabb[1 - ray.sign[0]].x - ray.origin.x) * ray.inv_direction.x;
	tymin = (aabb[ray.sign[1]].y - ray.origin.y) * ray.inv_direction.y;
	tymax = (aabb[1 - ray.sign[1]].y - ray.origin.y) * ray.inv_direction.y;
	tzmin = (aabb[ray.sign[2]].z - ray.origin.z) * ray.inv_direction.z;
	tzmax = (aabb[1 - ray.sign[2]].z - ray.origin.z) * ray.inv_direction.z;
	distanceToBox = max(max(tmin, tymin), tzmin);
	intersectAmount = min(rayLength, min(min(tmax, tymax), tzmax));
	intersectAmount = intersectAmount - distanceToBox;

	return tmin <= tmax;
	// post condition:
	// if tmin > tmax (in the code above this is represented by a return value of INFINITY)
	//     no intersection
	// else
	//     front intersection point = ray.origin + ray.direction * tmin (normally only this point matters)
	//     back intersection point  = ray.origin + ray.direction * tmax
	*/
}

// ray origin is assumed to be 0,0,0
float RayBoxIntersectLocal(float3 rayDir, float rayLength, float3 boxMin, float3 boxMax, out float intersectAmount, out float distanceToBox)
{
	float3 invRayDir = float3One / rayDir;
	float3 t1 = boxMin * invRayDir;
	float3 t2 = boxMax * invRayDir;
	float3 tmin1 = min(t1, t2);
	float3 tmax1 = max(t1, t2);
	float tmin = max(max(tmin1.x, tmin1.y), tmin1.z);
	float tmax = min(min(tmax1.x, tmax1.y), tmax1.z);
	float2 tt0 = max(tmin1.xx, tmin1.yz);
	distanceToBox = max(0.0, max(tt0.x, tt0.y));
	tt0 = min(tmax1.xx, tmax1.yz);
	float tt1 = min(tt0.x, tt0.y);
	tt1 = min(tt1, rayLength);
	intersectAmount = max(0.0, tt1 - distanceToBox);

	return intersectAmount > 0.0001;
}

// determine if point is outside box, 1.0 if outside, 0.0 if inside
float PointBoxOutside(float3 pos, float3 minPos, float3 maxPos)
{
	return (pos.x < minPos.x ||	pos.y < minPos.y ||	pos.z < minPos.z ||
		pos.x > maxPos.x ||	pos.y > maxPos.y ||	pos.z > maxPos.z);
}

/*
float PointBoxIntersect(float3 pos, float3 boxMin, float3 boxMax)
{
	float3 inBoxStep = step(boxMin, pos) - step(boxMax, pos);
	return inBoxStep.x * inBoxStep.y * inBoxStep.z;
}
*/

// determine if point is inside box, 1.0 if inside, 0.0 if outside
float PointBoxIntersect(float3 pos, float3 minPos, float3 maxPos)
{
	return (pos.x >= minPos.x && pos.y >= minPos.y && pos.z >= minPos.z &&
		pos.x <= maxPos.x && pos.y <= maxPos.y && pos.z <= maxPos.z);
}

// determine if point in sphere, 1.0 if inside, 0.0 if outside, spherePosition is x,y,z,radius squared
float PointSphereIntersect(float3 p, float4 spherePosition)
{
	float3 diff = (p - spherePosition.xyz);
	float dist = dot(diff, diff);
	return (dist <= spherePosition.w);
}

// determine if point is not in sphere, 1.0 if outside, 0.0 if inside, spherePosition is x,y,z,radius squared
float PointSphereOutside(float3 p, float4 spherePosition)
{
	float3 diff = (p - spherePosition.xyz);
	float dist = dot(diff, diff);
	return (dist > spherePosition.w);
}

// determine distance of point from sphere shell, return 0 if in sphere, otherwise distance from edge, spherePosition is x, y, z, radius
float PointSphereDistance(float3 p, float4 spherePosition)
{
	float3 diff = (p - spherePosition.xyz);
	return length(diff) - spherePosition.w;
}

// determine distance of point from sphere shell squared, return 0 if in sphere, otherwise distance from edge squared, spherePosition is x,y,z,radius
float PointSphereDistanceSquared(float3 p, float4 spherePosition)
{
	float3 diff = (p - spherePosition.xyz);
	return pow(max(0.0, length(diff) - spherePosition.w), 2.0);
}

// spherePosition is x,y,z,radius squared
// returns distance to sphere, intersect amount, amount will be <= 0 if miss
float2 RaySphereIntersect(float3 rayOrigin, float3 rayDir, float rayLength, float4 spherePosition)
{
	// optimized version, seems to work as well, but watch out for artifacts
	// https://gamedev.stackexchange.com/questions/96459/fast-ray-sphere-collision-code
	float3 sphereCenter = rayOrigin - spherePosition.xyz;
	float b = -dot(rayDir, sphereCenter);
	float c = dot(sphereCenter, sphereCenter) - spherePosition.w;
	float discr = (b * b) - c;
	float t = sqrt(max(0.0, discr));
	float distanceToSphere = clamp(b - t, 0.0, rayLength);
	float intersectAmount = clamp(b + t, 0.0, rayLength);
	intersectAmount = intersectAmount - distanceToSphere;
	return float2(distanceToSphere, intersectAmount);

	/* // older version which is known to work in all cases
	// http://www.cosinekitty.com/raytrace/chapter06_sphere.html
	float3 sphereCenter = rayOrigin - spherePosition.xyz;
	float fA = dot(rayDir, rayDir);
	float fB = 2.0 * dot(rayDir, sphereCenter);
	float fC = dot(sphereCenter, sphereCenter) - spherePosition.w;
	float fD = (fB * fB) - (4.0 * fA * fC);
	float recpTwoA = (fD > 0.0) * (0.5 / fA);
	float DSqrt = sqrt(fD);
	fB = -fB;

	// the distance to the front of sphere - will be 0 if in sphere or miss
	distanceToSphere = clamp((fB - DSqrt) * recpTwoA, 0.0, rayLength);

	// total distance to the back of the sphere, will be 0 if miss
	intersectAmount = clamp((fB + DSqrt) * recpTwoA, 0.0, rayLength);

	// get intersect amount - we know that distance to back of sphere is greater than distance to front of sphere at this point
	intersectAmount = intersectAmount - distanceToSphere;
	*/
}

// spherePosition is x,y,z,radius squared
// returns distance to sphere, intersect amount, amount will be <= 0 if miss
float2 RaySphereIntersectInfinite(float3 rayOrigin, float3 rayDir, float4 spherePosition)
{
	float3 sphereCenter = rayOrigin - spherePosition.xyz;
	float b = -dot(rayDir, sphereCenter);
	float c = dot(sphereCenter, sphereCenter) - spherePosition.w;
	float discr = (b * b) - c;
	float t = sqrt(max(0.0, discr));
	return float2(b - t, b + t);
}

float PointPlaneDistanceSquared(float3 planeNormal, float3 planePos, float3 p)
{
	return dot(planeNormal, p - planePos);
}

// return 0 if no intersect
float RayPlaneIntersect(float3 rayOrigin, float3 rayDir, float3 planeNormal, float3 planePos, out float distanceToPlane)
{
	float denom = dot(planeNormal, rayDir);
	distanceToPlane = dot(planePos - rayOrigin, planeNormal) / max(0.00001, denom);
	return (denom > 0.0 && distanceToPlane > 0.0);
}

// distanceToPlane becomes distance to disc center
// intersectsDisc is true if the plane intersect is within the disc
void RayPlaneDiscIntersect(float3 rayOrigin, float3 rayDir, float3 planeCenter, float discRadiusSquared, inout float distanceToPlane, out float intersectsDisc)
{
	float3 planeIntersect = (rayOrigin + (rayDir * distanceToPlane));
	float3 planeIntersectLocal = planeIntersect - planeCenter;
	float distanceFromDiscCenter = dot(planeIntersectLocal, planeIntersectLocal);
	distanceToPlane = distance(planeIntersect, rayOrigin);
	intersectsDisc = (distanceFromDiscCenter < discRadiusSquared);
}

float LinePointDistanceSquared(float3 lineStart, float3 lineDir, float3 p)
{
	// http://wiki.unity3d.com/index.php?title=3d_Math_functions

	// distance vector from point to line start
	float3 linePointToPoint = p - lineStart;

	// distance from line start in lineDir direction where point p is closest
	float t = dot(linePointToPoint, lineDir);

	// point on line where p is closest
	float3 onLine = lineStart + (lineDir * t);

	// distance from point to line point
	onLine = onLine - p;

	// return squared distance
	return dot(onLine, onLine);
}

float LineLineClosestDistanceSquared(float3 line1Point1, float3 line1Point2, float3 line2Point1, float3 line2Point2, out float3 closePoint1, out float3 closePoint2)
{
	float3 u = line1Point2 - line1Point1;
	float3 v = line2Point2 - line2Point1;
	float3 w = line1Point1 - line2Point1;
	float a = dot(u, u);         // always >= 0
	float b = dot(u, v);
	float c = dot(v, v);         // always >= 0
	float d = dot(u, w);
	float e = dot(v, w);
	float D = a * c - b * b;        // always >= 0
	float sc, tc;

	// compute the line parameters of the two closest points
	if (D < 0.0001)
	{
		// the lines are almost parallel
		sc = 0.0;
		tc = (b > c ? d / b : e / c);    // use the largest denominator
	}
	else
	{
		sc = (b * e - c * d) / D;
		tc = (a * e - b * d) / D;
	}

	// get the difference of the two closest points
	closePoint1 = w + (sc * u);
	closePoint2 = (tc * v);
	float3 dP = closePoint1 - closePoint2; // =  L1(sc) - L2(tc)

										   // return norm(dP);   // return the closest distance
	return dot(dP, dP);
}

// coneposition is xyz, range
// conedir is xyz, end radius squared
// coneatten is cos(angle * 0.5)
// coneend is base center xyz, slant squared
float RayConeIntersect(float3 rayOrigin, float3 rayDir, float rayLength, float4 conePosition, float4 coneDir, float4 coneEnd, float coneAtten, out float intersectAmount, out float distanceToCone)
{
	float2 t;

	// https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrLine3Cone3.h
	// https://www.shadertoy.com/view/4s23DR
	// https://github.com/mayank127/raytracer/blob/master/object.cpp
	float3 PmV = rayOrigin - conePosition.xyz;
	float DdU = dot(coneDir.xyz, rayDir);
	float DdPmV = dot(coneDir.xyz, PmV);
	float UdPmV = dot(rayDir, PmV);
	float PmVdPmV = dot(PmV, PmV);
	float halfCosAngle = coneAtten.x * coneAtten.x;
	float c2 = DdU * DdU - halfCosAngle;
	float c1 = DdU * DdPmV - halfCosAngle * UdPmV;
	float c0 = DdPmV * DdPmV - halfCosAngle * PmVdPmV;
	float discr = c1 * c1 - c0 * c2;
	discr *= (discr > 0.0);
	float root = sqrt(discr);
	float invC2 = (1.0 / c2);
	c1 = -c1;
	t.y = (c1 - root) * invC2;
	t.x = (c1 + root) * invC2;

	// zero out negative cone
	t *= (DdPmV + (DdU * t) > 0.0);

	if (t.x == 0.0 && t.y == 0.0)
	{
		intersectAmount = 0.0;
		distanceToCone = 0.0;
		return false;
	}
	else
	{
		// intersect cone base (disc) and subsitute where appropriate
		float distanceToPlane1, distanceToPlane2;
		float hasCap1, hasCap2;

		// case 1: ray passes down through cone disc plane
		// handle case where the ray passes down through the cone disc plane
		float intersectPlane1 = RayPlaneIntersect(rayOrigin, rayDir, coneDir.xyz, coneEnd.xyz, distanceToPlane1);
		RayPlaneDiscIntersect(rayOrigin, rayDir, coneEnd.xyz, coneDir.w, distanceToPlane1, hasCap1);

		// if hasCap, y becomes cap intersect, else y is min of distance to plane or y
		t.y = (intersectPlane1 * ((distanceToPlane1 * hasCap1) + (min(distanceToPlane1, t.y) * !hasCap1))) + (t.y * !intersectPlane1);
		// ---

		// case 2: ray passes up through cone disc plane
		// handle case where the ray passes up through the cone disc plane
		float intersectPlane2 = RayPlaneIntersect(rayOrigin, rayDir, -coneDir.xyz, coneEnd.xyz, distanceToPlane2);
		RayPlaneDiscIntersect(rayOrigin, rayDir, coneEnd.xyz, coneDir.w, distanceToPlane2, hasCap2);

		// if hasCap, x becomes cap intersect, else x is un-modified
		t.x = (intersectPlane2 * ((distanceToPlane2 * hasCap2) + (t.x * !hasCap2))) + (t.x * !intersectPlane2);

		// if the plane intersect is closer than the distance to the back cone intersect, y is unmodified, else y is 0
		t.y *= (!intersectPlane2 || distanceToPlane2 < t.y);
		// ---

		// case 3: ray does not pass through cone disc plane
		// handle case where ray does not intersect the cone disc plane
		// point must be within slant distance, else throw it out (squared distance for performance)
		float3 distanceVector = rayOrigin + (rayDir * t.y) - conePosition.xyz;
		t.y *= (intersectPlane1 || intersectPlane2 || dot(distanceVector, distanceVector) < coneEnd.w);

		// clamp results to ray length, remove negative values
		distanceToCone = clamp(t.x, 0.0, rayLength);
		intersectAmount = clamp(t.y, 0.0, rayLength);
		intersectAmount = max(0.0, intersectAmount - distanceToCone);

		return intersectAmount > 0.0001;
	}
}

float3 RotatePointZeroOriginQuaternion(float3 position, float4 quat)
{
	// https://twistedpairdevelopment.wordpress.com/2013/02/11/rotating-a-vector-by-a-quaternion-in-glsl/
	return position + 2.0 * cross(cross(position, quat.xyz) + quat.w * position, quat.xyz);
}

float4 QuaternionFromAxisAngle(float3 axis, float angle)
{
	float half_angle = angle * 0.5;
	float s, c;
	sincos(half_angle, s, c);
	return float4(axis.xyz * s, c);
}

float4 QuaternionFromEuler(float3 angles)
{
	/* // broken
	float3 s, c;
	sincos(angles * 0.5, s, c);
	float4 qx = float4(s.x, 0.0f, 0.0f, c.x);
	float4 qy = float4(0.0f, s.y, 0.0f, c.y);
	float4 qz = float4(0.0f, 0.0f, s.z, c.z);
	float4 q = (qy * qx) * qz;
	return q;
	*/
	float3 s, c;
	sincos(angles * 0.5, s, c);
	float4 q;
	q.x = s.x * c.y * c.z + c.x * s.y * s.z;
	q.y = c.x * s.y * c.z - s.x * c.y * s.z;
	q.z = c.x * c.y * s.z - s.x * s.y * c.z;
	q.w = c.x * c.y * c.z + s.x * s.y * s.z;
	return q;
}

// only works for wrapping coordinates, sin / cos
float2 RotateUV(float2 uv, float s, float c)
{
	return float2((uv.x * c) - (uv.y * s), (uv.x * s) + (uv.y * c));
}

float3 RotatePoint(float3 pointToRotate, float3 angles)
{
	// note this is inefficient, a matrix could be passed as a shader param, just for quick and dirty testing...
	float3 rotatedPoint = pointToRotate;
	if (angles.x != 0.0)
	{
		float3x3 rotateXMatrix = float3x3(1.0, 0.0, 0.0, 0.0, cos(angles.x), sin(angles.x), 0.0, -sin(angles.x), cos(angles.x));
		rotatedPoint = mul(rotatedPoint, rotateXMatrix);
	}
	if (angles.y != 0.0)
	{
		float3x3 rotateYMatrix = float3x3(cos(angles.y), 0.0, -sin(angles.y), 0.0, 1.0, 0.0, sin(angles.y), 0.0, cos(angles.y));
		rotatedPoint = mul(rotatedPoint, rotateYMatrix);
	}
	if (angles.z != 0.0)
	{
		float3x3 rotateZMatrix = float3x3(cos(angles.z), -sin(angles.z), 0.0, sin(angles.z), cos(angles.z), 0.0, 0.0, 0.0, 1.0);
		rotatedPoint = mul(rotatedPoint, rotateZMatrix);
	}
	return rotatedPoint;
}

float4x4 InverseMatrix(float4x4 input)
{

#define wm_minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))

	float4x4 cofactors = float4x4
	(
		wm_minor(_22_23_24, _32_33_34, _42_43_44),
		-wm_minor(_21_23_24, _31_33_34, _41_43_44),
		wm_minor(_21_22_24, _31_32_34, _41_42_44),
		-wm_minor(_21_22_23, _31_32_33, _41_42_43),

		-wm_minor(_12_13_14, _32_33_34, _42_43_44),
		wm_minor(_11_13_14, _31_33_34, _41_43_44),
		-wm_minor(_11_12_14, _31_32_34, _41_42_44),
		wm_minor(_11_12_13, _31_32_33, _41_42_43),

		wm_minor(_12_13_14, _22_23_24, _42_43_44),
		-wm_minor(_11_13_14, _21_23_24, _41_43_44),
		wm_minor(_11_12_14, _21_22_24, _41_42_44),
		-wm_minor(_11_12_13, _21_22_23, _41_42_43),

		-wm_minor(_12_13_14, _22_23_24, _32_33_34),
		wm_minor(_11_13_14, _21_23_24, _31_33_34),
		-wm_minor(_11_12_14, _21_22_24, _31_32_34),
		wm_minor(_11_12_13, _21_22_23, _31_32_33)
	);

#undef wm_minor

	return transpose(cofactors) / determinant(input);
}

uint SetupPlanetRaymarch(float3 worldSpaceCameraPos, float3 rayDir, float depth, float depth2,
	float4 surfaceSphereXYZRadSq, float4 innerSphereXYZRadSq, float4 outterSphereXYZRadSq,
	out float3 startPos, out float3 endPos, out float rayLength, out float distanceToSphere,
	out float3 startPos2, out float3 endPos2, out float rayLength2, out float distanceToSphere2)
{
	uint iterations;

	// intersect surface sphere and reduce depth if needed
	float2 surfaceSphere = RaySphereIntersect(worldSpaceCameraPos, rayDir, depth, surfaceSphereXYZRadSq);
	if (surfaceSphere.x > 0.0 && surfaceSphere.y > 0.0)
	{
		depth = min(surfaceSphere.x, depth);
		depth2 = min(surfaceSphere.x, depth);
	}

	// find where we intersect the lower cloud layer
	float2 innerSphere = RaySphereIntersect(worldSpaceCameraPos, rayDir, depth, innerSphereXYZRadSq);

	// find where we intersect the upper cloud layer
	float2 outterSphere = RaySphereIntersect(worldSpaceCameraPos, rayDir, depth, outterSphereXYZRadSq);

	float intersectAmount = innerSphere.y;
	distanceToSphere = innerSphere.x;
	float intersectAmount2 = outterSphere.y;
	distanceToSphere2 = outterSphere.x;

	// we need to handle case where we are in the inner sphere, in the outter sphere and not in either sphere
	UNITY_BRANCH
	if (intersectAmount > 0.0 && distanceToSphere <= 0.0)
	{
		// we are inside the inner sphere, we can see both spheres
		startPos = worldSpaceCameraPos + (rayDir * intersectAmount);
		rayLength = intersectAmount2 - intersectAmount;
		distanceToSphere = intersectAmount;
		rayLength = min(depth - distanceToSphere, rayLength);

		iterations = (rayLength > 0.01);
		startPos2 = float3Zero;
		rayLength2 = 0.0;
	}
	else if (intersectAmount2 > 0.0 && distanceToSphere2 <= 0.0)
	{
		if (intersectAmount > 0.0)
		{
			// in outter sphere, ray hit both spheres
			// first intersect in the outter sphere
			rayLength = min(depth, distanceToSphere);
			startPos = worldSpaceCameraPos;
			iterations = (rayLength > 0.01);

			// second intersect on backside of outter sphere
			float ignoreRayLength = distanceToSphere + intersectAmount;
			startPos2 = worldSpaceCameraPos + (rayDir * ignoreRayLength);
			rayLength2 = min(depth2 - ignoreRayLength, (intersectAmount2 - ignoreRayLength));
			iterations += (rayLength2 > 0.01);

			distanceToSphere2 = ignoreRayLength;
			distanceToSphere = 0.0;
		}
		else
		{
			// else in outter sphere, ray missed inner sphere
			startPos = worldSpaceCameraPos;
			rayLength = min(depth, intersectAmount2);
			iterations = (rayLength > 0.01);
			distanceToSphere = 0.0;
			startPos2 = float3Zero;
			rayLength2 = 0.0;
		}
	}
	else if (intersectAmount2 <= 0.0)
	{
		// we are outside the outter sphere and have no intersection with the outter sphere, nothing to do
		startPos = float3Zero;
		rayLength = 0.0;
		startPos2 = float3Zero;
		rayLength2 = 0.0;
		iterations = 0;
	}
	else if (intersectAmount <= 0.0)
	{
		// we are outside the outter sphere and missed inner sphere, just calculate outter sphere intersect
		startPos = rayDir * distanceToSphere2;

		// if depth buffer blocks, exit out
		float startPosLength = length(startPos);
		startPos += worldSpaceCameraPos;
		rayLength = min(depth - startPosLength, intersectAmount2);
		iterations = (rayLength > 0.01);
		distanceToSphere = distanceToSphere2;
		startPos2 = float3Zero;
		rayLength2 = 0.0;
	}
	else
	{
		// we are outside the outter sphere and hit both spheres, start at outter sphere intersect point
		startPos = rayDir * distanceToSphere2;

		// if depth buffer blocks, exit out
		float startPosLength = length(startPos);
		startPos += worldSpaceCameraPos;
		rayLength = min(depth - startPosLength, (distanceToSphere - distanceToSphere2));
		iterations = (rayLength > 0.01);
		distanceToSphere = distanceToSphere2;

		startPos2 = float3Zero;
		rayLength2 = 0.0;
	}

	rayLength = lerp(min(rayLength, depth - distanceToSphere), rayLength, depth < distanceToSphere);
	rayLength2 = lerp(min(rayLength2, depth - distanceToSphere2), rayLength2, depth < distanceToSphere2);

	// set end pos to far away if blocked by depth buffer or depth buffer is beyond the sphere
	endPos = lerp(startPos + (rayDir * rayLength), startPos + (rayDir * 1000000.0), depth < distanceToSphere || depth > distanceToSphere + rayLength);
	endPos2 = lerp(startPos2 + (rayDir * rayLength2), startPos2 + (rayDir * 1000000.0), depth < distanceToSphere2 || depth > distanceToSphere2 + rayLength2);

	return iterations;
}

uint SetupPlanetRaymarchBox(float3 worldSpaceCameraPos, float3 rayDir, float depth,	float2 yStartEnd,
	out float3 startPos, out float3 endPos, out float rayLength, out float distanceToSphere,
	out float3 startPos2, out float3 endPos2, out float rayLength2, out float distanceToSphere2)
{
	float3 boxMin = float3(worldSpaceCameraPos.x - 1000000.0, yStartEnd.x, worldSpaceCameraPos.z - 1000000.0);
	float3 boxMax = float3(worldSpaceCameraPos.x + 1000000.0, yStartEnd.y, worldSpaceCameraPos.z + 1000000.0);
	float hit = RayBoxIntersect(worldSpaceCameraPos, rayDir, depth, boxMin, boxMax, rayLength, distanceToSphere);
	startPos = worldSpaceCameraPos + (rayDir * distanceToSphere);
	endPos = startPos + (rayDir * rayLength);
	startPos2 = float3Zero;
	endPos2 = float3Zero;
	rayLength2 = 0.0;
	distanceToSphere2 = 0.0;
	return uint(hit);
}

uint SetupPlanetRaymarchBoxArea(float3 worldSpaceCameraPos, float3 rayDir, float depth, float3 boxMin, float3 boxMax,
	out float3 startPos, out float3 endPos, out float rayLength, out float distanceToSphere,
	out float3 startPos2, out float3 endPos2, out float rayLength2, out float distanceToSphere2)
{
	float hit = RayBoxIntersect(worldSpaceCameraPos, rayDir, depth, boxMin, boxMax, rayLength, distanceToSphere);
	startPos = worldSpaceCameraPos + (rayDir * distanceToSphere);
	endPos = startPos + (rayDir * rayLength);
	startPos2 = float3Zero;
	endPos2 = float3Zero;
	rayLength2 = 0.0;
	distanceToSphere2 = 0.0;
	return uint(hit);
}

#endif
