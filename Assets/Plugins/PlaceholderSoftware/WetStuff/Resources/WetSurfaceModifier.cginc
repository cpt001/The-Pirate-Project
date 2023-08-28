struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

sampler2D _WetDecalSaturationMask;
sampler2D _GBufferSpecularCopy;
float2 _GBufferSpecularCopy_TexelSize;
float _AmbientDarken;

v2f vert(appdata v)
{
    v2f o;
    o.vertex = float4(v.vertex.xyz, 1);

#if !UNITY_UV_STARTS_AT_TOP
    if (_GBufferSpecularCopy_TexelSize.y > 0)
        v.uv.y = 1 - v.uv.y;
#endif

    o.uv = v.uv;
    return o;
}

void frag(
    v2f i,
    out float4 gbuffer0 : SV_TARGET0,
    out float4 gbuffer1 : SV_TARGET1,
    out float4 ambient : SV_TARGET2)
{
    float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);
    float wetness = tex2D(_WetDecalSaturationMask, uv).r;
    half4 specular = tex2D(_GBufferSpecularCopy, uv);    

    // ignore pixels that are not wet
    clip(wetness - 0.001);

    // calculate approximate surface properties
    float porosity = saturate(((1 - specular.a) - 0.2) / 0.9);
    float metalness = saturate((max(specular.r, max(specular.g, specular.b)) * 1000 - 500));

    // "wetness" factor
    float factor = lerp(1, 0.2, (1 - metalness) * porosity);

    // calculate saturation (surface fill ammount) and oversaturation (pooled water on top)
    float saturationThreshold = 0.8 * (1 - metalness);
    float saturation = saturate(wetness / saturationThreshold);
    float oversaturation = saturate((wetness - saturationThreshold) / (1 - saturationThreshold));

    // diffuse: darken as saturation increases
    gbuffer0.rgb = lerp(1.0, factor, saturation);
    gbuffer0.a = 1;

    // specular: smoother as saturation increases, blends to water as oversaturated
    specular.a = lerp(1.0, specular.a, lerp(1, factor, saturation));
    specular = lerp(specular, float4(max(0.3, specular.rgb), 1), oversaturation);
    gbuffer1 = specular;

    // ambient: darken as we do diffuse
    ambient = lerp(1, gbuffer0, _AmbientDarken);
}