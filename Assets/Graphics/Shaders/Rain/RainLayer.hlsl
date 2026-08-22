#ifndef RAINLAYER_INCLUDED
#define RAINLAYER_INCLUDED

void RainLayer_float(float2 UV, float2 Tiling, float2 Speed, float2 Seed,
                     UnityTexture2D Tex, UnitySamplerState SS, out float Out)
{
    float2 uv = UV * Tiling + Seed + _Time.y * Speed;
    Out = SAMPLE_TEXTURE2D(Tex, SS, uv).a;
}

#endif