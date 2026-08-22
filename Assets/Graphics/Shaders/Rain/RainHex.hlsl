#ifndef RAINHEX_INCLUDED
#define RAINHEX_INCLUDED

float2 RainHash(float2 p)
{
    float3 p3 = frac(p.xyx * float3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.xx + p3.yz) * p3.zy);
}

void RainTriGrid(float2 uv, out float3 w, out float2 v1, out float2 v2, out float2 v3)
{
    uv *= 3.464;
    float2x2 skew = float2x2(1.0, 0.0, -0.57735027, 1.15470054);
    float2 s = mul(skew, uv);
    float2 baseId = floor(s);
    float3 t = float3(frac(s), 0.0);
    t.z = 1.0 - t.x - t.y;

    if (t.z > 0.0)
    {
        w = float3(t.z, t.y, t.x);
        v1 = baseId;
        v2 = baseId + float2(0.0, 1.0);
        v3 = baseId + float2(1.0, 0.0);
    }
    else
    {
        w = float3(-t.z, 1.0 - t.y, 1.0 - t.x);
        v1 = baseId + float2(1.0, 1.0);
        v2 = baseId + float2(1.0, 0.0);
        v3 = baseId + float2(0.0, 1.0);
    }
}

void RainLayerHex_float(float2 UV, float2 Tiling, float2 Speed, float2 Seed,
                        float Sharpness, UnityTexture2D Tex, UnitySamplerState SS,
                        out float Out)
{
    float2 uv = UV * Tiling + Seed + _Time.y * Speed;

    float3 w;
    float2 v1, v2, v3;
    RainTriGrid(uv, w, v1, v2, v3);

    float2 dx = ddx(uv);
    float2 dy = ddy(uv);

    float a = Tex.SampleGrad(SS, uv + RainHash(v1), dx, dy).a;
    float b = Tex.SampleGrad(SS, uv + RainHash(v2), dx, dy).a;
    float c = Tex.SampleGrad(SS, uv + RainHash(v3), dx, dy).a;

    w = pow(max(w, 1e-5), Sharpness);
    w /= (w.x + w.y + w.z);

    Out = a * w.x + b * w.y + c * w.z;
}

#endif