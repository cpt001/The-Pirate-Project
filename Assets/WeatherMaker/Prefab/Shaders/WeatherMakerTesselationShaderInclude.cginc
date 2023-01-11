#if !defined(WEATHER_MAKER_TESSELLATION_INCLUDED)
#define WEATHER_MAKER_TESSELLATION_INCLUDED

#if defined(TESSELATION_ENABLE) && defined(TESSELATION_VERTEX_OUTPUT) && defined(TESSELATION_VERTEX_INPUT) && defined(TESSELATION_VERTEX_FUNC)

// before including, you must define (replace water with your own identifiers):
// #define TESSELATION_ENABLE
// #define TESSELATION_VERTEX_OUTPUT v2fWater
// #define TESSELATION_VERTEX_INPUT appdata_water
// #define TESSELATION_VERTEX_FUNC vertWater

// optional override:
// TESSELATION_DOMAIN_PROGRAM_INTERPOLATE_IMPLEMENTATION
// TESSELATION_MAX_DISPLACEMENT

#include "Tessellation.cginc"

// factor, max displace, 0, 0
uniform float4 _TesselationParams;

struct TessellationControlPoint
{
	float4 vertex : INTERNALTESSPOS;
	float3 normal : NORMAL;
	float4 uv : TEXCOORD0;
};

struct TesselationVertexData
{
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 uv : TEXCOORD0;
};

#define TESSELATION_DOMAIN_PROGRAM_INTERPOLATE(fieldName) data.fieldName = patch[0].fieldName * barycentricCoordinates.x + patch[1].fieldName * barycentricCoordinates.y + patch[2].fieldName * barycentricCoordinates.z;

#if !defined(TESSELATION_DOMAIN_PROGRAM_INTERPOLATE_CUSTOM)
#define TESSELATION_DOMAIN_PROGRAM_INTERPOLATE_IMPLEMENTATION \
	TESSELATION_DOMAIN_PROGRAM_INTERPOLATE(vertex) \
	TESSELATION_DOMAIN_PROGRAM_INTERPOLATE(normal) \
	TESSELATION_DOMAIN_PROGRAM_INTERPOLATE(uv) \
	UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(patch[0], data); \
	data.uv.w = factors.inside;
#endif

TessellationControlPoint TessellationVertexProgram(TesselationVertexData v)
{
	TessellationControlPoint p;
	p.vertex = v.vertex;
	p.normal = v.normal;
	p.uv = v.uv;
	return p;
}

UnityTessellationFactors TesselationPatchConstantFunction(InputPatch<TessellationControlPoint, 3> patch)
{
	float4 t = UnityEdgeLengthBasedTessCull(patch[0].vertex, patch[1].vertex, patch[2].vertex, _TesselationParams.x, _TesselationParams.y);
	//float4 t = UnityEdgeLengthBasedTess(patch[0].vertex, patch[1].vertex, patch[2].vertex, _TesselationParams.x);
	UnityTessellationFactors f;
	f.edge[0] = t.x;
	f.edge[1] = t.y;
	f.edge[2] = t.z;
	f.inside = t.w;

	return f;
}

[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
[UNITY_partitioning("fractional_odd")]
[UNITY_patchconstantfunc("TesselationPatchConstantFunction")]
TessellationControlPoint TesselationHullProgram(InputPatch<TessellationControlPoint, 3> patch, uint id : SV_OutputControlPointID)
{
	return patch[id];
}

[UNITY_domain("tri")]
TESSELATION_VERTEX_OUTPUT TesselationDomainProgram(UnityTessellationFactors factors, OutputPatch<TessellationControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
{
	TESSELATION_VERTEX_INPUT data;
    UNITY_SETUP_INSTANCE_ID(data);
    UNITY_INITIALIZE_OUTPUT(TESSELATION_VERTEX_INPUT, data);

#if UNITY_VERSION >= 201803 && defined(WM_STEREO_INSTANCING_ENABLED)

	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(data);

#endif

	TESSELATION_DOMAIN_PROGRAM_INTERPOLATE_IMPLEMENTATION
	return TESSELATION_VERTEX_FUNC(data);
}

#endif // defined(TESSELATION_VERTEX_OUTPUT) && defined(TESSELATION_VERTEX_INPUT) && defined(TESSELATION_VERTEX_FUNC)

#endif // WEATHER_MAKER_TESSELLATION_INCLUDED