sampler2D _XLayer;
sampler2D _YLayer;
sampler2D _ZLayer;
half4 _XLayerScaleOffset;
half4 _YLayerScaleOffset;
half4 _ZLayerScaleOffset;

sampler2D _BlueNoiseRgba;
half2 _RandomnessTiling;
half2 _SampleJitter;

sampler2D_float _CameraDepthTexture;
sampler2D _CameraGBufferTexture2;

UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(half, _Wetness)
    UNITY_DEFINE_INSTANCED_PROP(half, _Fadeout)
    UNITY_DEFINE_INSTANCED_PROP(half, _EdgeSharpness)
#if defined(LAYERS_SINGLE) || defined(LAYERS_TRIPLANAR)
    UNITY_DEFINE_INSTANCED_PROP(half4, _YLayerInputStart)
    UNITY_DEFINE_INSTANCED_PROP(half4, _YLayerInputExtent)
    UNITY_DEFINE_INSTANCED_PROP(half4, _YLayerOutputStart)
    UNITY_DEFINE_INSTANCED_PROP(half4, _YLayerOutputEnd)
#endif
#if defined(LAYERS_TRIPLANAR)
    UNITY_DEFINE_INSTANCED_PROP(half4, _XLayerInputStart)
    UNITY_DEFINE_INSTANCED_PROP(half4, _XLayerInputExtent)
    UNITY_DEFINE_INSTANCED_PROP(half4, _XLayerOutputStart)
    UNITY_DEFINE_INSTANCED_PROP(half4, _XLayerOutputEnd)
    UNITY_DEFINE_INSTANCED_PROP(half4, _ZLayerInputStart)
    UNITY_DEFINE_INSTANCED_PROP(half4, _ZLayerInputExtent)
    UNITY_DEFINE_INSTANCED_PROP(half4, _ZLayerOutputStart)
    UNITY_DEFINE_INSTANCED_PROP(half4, _ZLayerOutputEnd)
#endif
UNITY_INSTANCING_BUFFER_END(Props)

struct appdata
{
    float4 pos : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
	float4 screenUV : TEXCOORD1;
	float3 ray : TEXCOORD2;
	half3 orientation : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2f mask_vert(appdata v)
{
	v2f o;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);

	o.pos = UnityObjectToClipPos(v.pos);
	o.uv = v.pos.xz + 0.5;
	o.screenUV = ComputeScreenPos(o.pos);
	o.ray = UnityObjectToViewPos(v.pos) * float3(-1, -1, 1);
	o.orientation = mul((float3x3)unity_ObjectToWorld, float3(0, 1, 0));
	return o;
}

inline float ComputeWetness(float4 samples, float4 inputStart, float4 inputExtent, float4 outputStart, float4 outputEnd)
{	
    float4 remapped = saturate((samples - inputStart) / inputExtent);
	float4 wetness = lerp(outputStart, outputEnd, remapped);
	return max(max(wetness.r, wetness.g), max(wetness.b, wetness.a));
}

inline float ComputeFadeout(float3 opos)
{
#if defined (SHAPE_MESH)
    return 1;
#endif

#if defined(SHAPE_CIRCLE)
    float distanceFromEdge = 1 - length(opos) * 2;    
#else
    opos = abs(opos);
    float distanceFromEdge = 1 - max(opos.x, max(opos.y, opos.z)) * 2;
#endif


    float fade = saturate(distanceFromEdge / UNITY_ACCESS_INSTANCED_PROP(Props, _Fadeout));
    return smoothstep(0, 1, fade);
}

inline float2 BlueNoise4(float2 uv)
{
#if defined(JITTER_LAYERS)
	return (tex2D(_BlueNoiseRgba, uv * _RandomnessTiling).rg - float2(0.5, 0.5)) * _SampleJitter;
#else
    return 0;
#endif
}

inline float4 SampleLayer(sampler2D tex, float2 uv, float4 scaleOffset)
{
    uv = uv * scaleOffset.xy + scaleOffset.zw;
    return tex2D(tex, uv + BlueNoise4(uv));
}

fixed4 mask_frag(v2f i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);

	i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
	float2 screenUV = i.screenUV.xy / i.screenUV.w;

	// read depth and reconstruct world position
	float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
	depth = Linear01Depth(depth);
	float4 vpos = float4(i.ray * depth, 1);
	float3 wpos = mul(unity_CameraToWorld, vpos).xyz;
	float3 opos = mul(unity_WorldToObject, float4(wpos, 1)).xyz;
    float3 wposRotated = mul((float3x3)unity_WorldToObject, wpos) / unity_WorldToObject._m00_m11_m22;

	// clip pixels that are outside the volume
    clip(float3(0.5,0.5,0.5) - abs(opos.xyz));

	i.uv = opos.xz + 0.5;

	// find surface normal
    half3 normal = tex2D(_CameraGBufferTexture2, screenUV).rgb;
    half3 wnormal = normal.rgb * 2.0 - 1.0;
        
	// calculate wetness
    float wetness;

#if defined(LAYER_PROJECTION_WORLD)
    float3 coord = wposRotated + 0.5;
#else
    float3 coord = opos + 0.5;
#endif

#if defined(LAYERS_SINGLE) || defined(LAYERS_TRIPLANAR)
    float4 yMask = SampleLayer(_YLayer, coord.xz, _YLayerScaleOffset);
    float yMaskWetness = ComputeWetness(
        yMask,
        UNITY_ACCESS_INSTANCED_PROP(Props, _YLayerInputStart),
        UNITY_ACCESS_INSTANCED_PROP(Props, _YLayerInputExtent),
        UNITY_ACCESS_INSTANCED_PROP(Props, _YLayerOutputStart),
        UNITY_ACCESS_INSTANCED_PROP(Props, _YLayerOutputEnd));
#endif

#if defined (SHAPE_MESH)
    wetness = 1;
#else
    #if defined(LAYERS_SINGLE)
        wetness = yMaskWetness * pow(saturate(dot(normalize(i.orientation), wnormal)), UNITY_ACCESS_INSTANCED_PROP(Props, _EdgeSharpness));
    #elif defined(LAYERS_TRIPLANAR)
        float4 xMask = SampleLayer(_XLayer, coord.zy, _XLayerScaleOffset);
        float xMaskWetness = ComputeWetness(
            xMask,
            UNITY_ACCESS_INSTANCED_PROP(Props, _XLayerInputStart),
            UNITY_ACCESS_INSTANCED_PROP(Props, _XLayerInputExtent),
            UNITY_ACCESS_INSTANCED_PROP(Props, _XLayerOutputStart),
            UNITY_ACCESS_INSTANCED_PROP(Props, _XLayerOutputEnd));

        float4 zMask = SampleLayer(_ZLayer, coord.xy, _ZLayerScaleOffset);
        float zMaskWetness = ComputeWetness(
            zMask,
            UNITY_ACCESS_INSTANCED_PROP(Props, _ZLayerInputStart),
            UNITY_ACCESS_INSTANCED_PROP(Props, _ZLayerInputExtent),
            UNITY_ACCESS_INSTANCED_PROP(Props, _ZLayerOutputStart),
            UNITY_ACCESS_INSTANCED_PROP(Props, _ZLayerOutputEnd));
    
        half3 lnormal = normalize(mul(wnormal, (float3x3)unity_ObjectToWorld) / unity_ObjectToWorld._m00_m11_m22);
        float3 maskWetness = float3(xMaskWetness, yMaskWetness, zMaskWetness);
        float3 weights = abs(lnormal);    
        weights = pow(weights, UNITY_ACCESS_INSTANCED_PROP(Props, _EdgeSharpness));
        weights = weights / dot(weights, 1);

        wetness = dot(maskWetness, weights);
    #else
        wetness = pow(saturate(dot(normalize(i.orientation), wnormal)), UNITY_ACCESS_INSTANCED_PROP(Props, _EdgeSharpness));
    #endif
#endif

    return wetness
        * UNITY_ACCESS_INSTANCED_PROP(Props, _Wetness)
        * ComputeFadeout(opos);
}