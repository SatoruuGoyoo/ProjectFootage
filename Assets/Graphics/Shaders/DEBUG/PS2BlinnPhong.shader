Shader"Custom/SixthGenPhong"
{
  Properties
  {
    // ── Diffuse ──────────────────────────────────────────────────
    _MainTex       ("Diffuse",                    2D)           = "white" {}
    _Color         ("Diffuse Tint",               Color)        = (1,1,1,1)

    // ── Lightmap (UV2) ───────────────────────────────────────────
    [Toggle] _UseLightmap ("Use Lightmap",         Float)       = 0
    _LightMap      ("Lightmap (UV2)",              2D)          = "white" {}
    _LightmapInt   ("Lightmap Intensity",          Range(0,2))  = 1.0

    // ── Detail texture ───────────────────────────────────────────
    [Toggle] _UseDetail ("Use Detail Tex",         Float)       = 0
    _DetailTex     ("Detail (tiling)",             2D)          = "gray" {}
    _DetailStr     ("Detail Strength",             Range(0,1))  = 0.5

    // ── Sphere map ───────────────────────────────────────────────
    [Toggle] _UseSphere ("Use Sphere Map",         Float)       = 0
    _SphereTex     ("Sphere Map",                  2D)          = "black" {}
    _SphereStr     ("Sphere Strength",             Range(0,1))  = 0.4

    // ── Emission ─────────────────────────────────────────────────
    [Toggle] _UseEmission ("Use Emission",         Float)       = 0
    _EmissionTex   ("Emission",                    2D)          = "black" {}
    _EmissionStr   ("Emission Strength",           Range(0,4))  = 1.0

    // ── Lighting ─────────────────────────────────────────────────
    _SpecColor     ("Specular Color",              Color)       = (1,1,1,1)
    _Shininess     ("Shininess",                   Range(1,64)) = 16
    _AmbientMult   ("Ambient Multiplier",          Range(0,1))  = 1.0  // ← apaga/enciende el environment
    _SpecStrength  ("Spec Strength",               Range(0,1))  = 0.4
    [Toggle] _FlatSpec ("No Specular (PS2 mode)", Float)        = 0

    // ── Look de consola ──────────────────────────────────────────
    _ColorDepth    ("Color Depth (bits)",          Range(1,32))  = 5
  }

  SubShader
  {
    Tags
    {
      "RenderType"     = "Opaque"
      "RenderPipeline" = "UniversalPipeline"
      "Queue"          = "Geometry"
    }
    Pass
    {
Name"ForwardLit"
      Tags
{"LightMode" = "UniversalForward"
}

HLSLPROGRAM

      #pragma vertex   vert
      #pragma fragment frag

      #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
      #pragma multi_compile_fog                              // ← fog keywords de URP
      #pragma shader_feature_local _USELIGHTMAP_ON
      #pragma shader_feature_local _USEDETAIL_ON
      #pragma shader_feature_local _USESPHERE_ON
      #pragma shader_feature_local _USEEMISSION_ON
      #pragma shader_feature_local _FLATSPEC_ON

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

      CBUFFER_START(UnityPerMaterial)

float4 _MainTex_ST;
float4 _DetailTex_ST;
float4 _Color;
float4 _SpecColor;
float _Shininess;
float _AmbientMult;
float _SpecStrength;
float _ColorDepth;
float _LightmapInt;
float _DetailStr;
float _SphereStr;
float _EmissionStr;
CBUFFER_END

      TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
      TEXTURE2D(_LightMap);    SAMPLER(sampler_LightMap);
      TEXTURE2D(_DetailTex);   SAMPLER(sampler_DetailTex);
      TEXTURE2D(_SphereTex);   SAMPLER(sampler_SphereTex);
      TEXTURE2D(_EmissionTex); SAMPLER(sampler_EmissionTex);

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1;
    float4 vertexColor : COLOR;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1;
    float2 uvDetail : TEXCOORD2;
    float2 uvSphere : TEXCOORD3;
    float3 lighting : TEXCOORD4;
    float4 vertexColor : TEXCOORD5;
    float fogCoord : TEXCOORD6; // ← fog
};

      // ── Blinn-Phong ────────────────────────────────────────────────────
float3 SixthGenLight(float3 N, float3 L, float3 V,
                           float3 lightColor, float atten)
{
    float NdotL = max(0.0, dot(N, L));
    float3 diff = lightColor * _Color.rgb * NdotL * atten;

#ifndef _FLATSPEC_ON
    float3 H = normalize(L + V);
    float NdotH = max(0.0, dot(N, H));
    float3 spec = lightColor * _SpecColor.rgb
                       * pow(NdotH, _Shininess)
                       * NdotL * atten * _SpecStrength;
    return diff + spec;
#else
          return diff;
#endif
}

      // ── Banding de época ───────────────────────────────────────────────
float3 QuantizeColor(float3 col, float bits)
{
    float levels = pow(2.0, bits) - 1.0;
    return round(col * levels) / levels;
}

Varyings vert(Attributes IN)
{
    Varyings OUT;

    float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
    float3 normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
    float3 viewDir = normalize(GetCameraPositionWS() - posWS);

    OUT.positionCS = TransformWorldToHClip(posWS);
    OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
    OUT.uv2 = IN.uv2;
    OUT.uvDetail = TRANSFORM_TEX(IN.uv, _DetailTex);
    OUT.vertexColor = IN.vertexColor;

        // ── Fog — se calcula en vertex, se aplica en frag ──────────────
    OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);

        // ── Sphere map UVs ─────────────────────────────────────────────
#ifdef _USESPHERE_ON
          float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);
          OUT.uvSphere    = normalVS.xy * 0.5 + 0.5;
#else
    OUT.uvSphere = float2(0, 0);
#endif

        // ── Ambient: ahora lee el Environment de Unity ─────────────────
        // SampleSH lee el Skybox/Ambient Color que configuraste en Lighting
        // _AmbientMult = 0 → todo negro, 1 → respeta Unity, 0.1 → casi negro
    float3 lit = SampleSH(normalWS) * _Color.rgb * _AmbientMult;

        // ── Luz principal ──────────────────────────────────────────────
    Light main = GetMainLight();
    lit += SixthGenLight(normalWS, normalize(main.direction),
                             viewDir, main.color,
                             main.distanceAttenuation);

        // ── Luces adicionales en vertex (point/spot lights) ────────────
#define MAX_LIGHTS 8

#if defined(_ADDITIONAL_LIGHTS_VERTEX) || defined(_ADDITIONAL_LIGHTS)
          uint addCount = min(GetAdditionalLightsCount(), MAX_LIGHTS);
          for (uint i = 0u; i < addCount; ++i)
          {
            Light addL = GetAdditionalLight(i, posWS);
            lit += SixthGenLight(normalWS, normalize(addL.direction),
                                 viewDir, addL.color,
                                 addL.distanceAttenuation * addL.shadowAttenuation);
          }
#endif

    OUT.lighting = lit;
    return OUT;
}

float4 frag(Varyings IN) : SV_Target
{
        // Diffuse base
    float4 diffuseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
    float3 col = diffuseTex.rgb;

        // Detail
#ifdef _USEDETAIL_ON
          float3 detail = SAMPLE_TEXTURE2D(_DetailTex, sampler_DetailTex, IN.uvDetail).rgb;
          col = lerp(col, col * detail * 2.0, _DetailStr);
#endif

        // Vertex colors (AO bakeado)
    col *= IN.vertexColor.rgb;

        // Lighting vertex
    col *= IN.lighting;

        // Lightmap — reemplaza el lighting dinámico para escenarios estáticos
#ifdef _USELIGHTMAP_ON
          float3 lm = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, IN.uv2).rgb;
          col = diffuseTex.rgb * IN.vertexColor.rgb * lm * _LightmapInt;
#endif

        // Sphere map — additive
#ifdef _USESPHERE_ON
          float3 sphere = SAMPLE_TEXTURE2D(_SphereTex, sampler_SphereTex, IN.uvSphere).rgb;
          col += sphere * _SphereStr;
#endif

        // Emission — additive, ignora el lighting
#ifdef _USEEMISSION_ON
          float3 emission = SAMPLE_TEXTURE2D(_EmissionTex, sampler_EmissionTex, IN.uv).rgb;
          col += emission * _EmissionStr;
#endif

    col = saturate(col);
    col = QuantizeColor(col, _ColorDepth);

        // ── Fog aplicado al final ──────────────────────────────────────
        // Usa el color y settings de Lighting → Fog automáticamente
    col = MixFog(col, IN.fogCoord);

    return float4(col, diffuseTex.a);
}
      ENDHLSL
    }
  }
} 