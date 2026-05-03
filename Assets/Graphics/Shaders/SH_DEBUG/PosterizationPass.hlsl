#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

CBUFFER_START(UnityPerMaterial)
    float _Steps;
    float _Saturation;
CBUFFER_END

float3 RGBtoHSV(float3 c)
{
    float4 K = float4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
    float d  = q.x - min(q.w, q.y);
    float e  = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HSVtoRGB(float3 c)
{
    float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

half4 frag(Varyings IN) : SV_Target
{
    // _BlitTexture y SamplerState_linear_clamp vienen del Blit.hlsl
    float2 uv    = IN.texcoord;
    float3 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;

    float3 hsv = RGBtoHSV(color);
    hsv.z = floor(hsv.z * _Steps + 0.5) / _Steps;
    hsv.y = floor(hsv.y * _Saturation * _Steps + 0.5) / (_Saturation * _Steps);

    return half4(HSVtoRGB(hsv), 1.0);
}