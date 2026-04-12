Shader"Custom/AltPhong"
{
  Properties
  {
    _MainTex       ("Diffuse",                    2D)           = "white" {}
    _Color         ("Diffuse Tint",               Color)        = (1,1,1,1)

    [Toggle] _UseLightmap ("Use Lightmap",         Float)       = 0
    _LightMap      ("Lightmap (UV2)",              2D)          = "white" {}
    _LightmapInt   ("Lightmap Intensity",          Range(0,2))  = 1.0

    [Toggle] _UseDetail ("Use Detail Tex",         Float)       = 0
    _DetailTex     ("Detail (tiling)",             2D)          = "gray" {}
    _DetailStr     ("Detail Strength",             Range(0,1))  = 0.5

    [Toggle] _UseSphere ("Use Sphere Map",         Float)       = 0
    _SphereTex     ("Sphere Map",                  2D)          = "black" {}
    _SphereStr     ("Sphere Strength",             Range(0,1))  = 0.4

    [Toggle] _UseEmission ("Use Emission",         Float)       = 0
    _EmissionTex   ("Emission",                    2D)          = "black" {}
    _EmissionStr   ("Emission Strength",           Range(0,4))  = 1.0

    _SpecColor     ("Specular Color",              Color)       = (1,1,1,1)
    _Shininess     ("Shininess",                   Range(1,64)) = 16
    _AmbientMult   ("Ambient Multiplier",          Range(0,1))  = 1.0
    _SpecStrength  ("Spec Strength",               Range(0,1))  = 0.4
    [Toggle] _FlatSpec ("No Specular (PS2 mode)", Float)        = 0

    _ColorDepth    ("Color Depth (bits/canal)",    Range(1,32)) = 5
    _LightSteps    ("Light Steps (0=off)",         Range(0,16)) = 0
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
      #pragma multi_compile_fog
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
float _LightSteps;
CBUFFER_END

      TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
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
    float3 positionWS : TEXCOORD0; 
    float3 normalWS : TEXCOORD1; 
    float2 uv : TEXCOORD2;
    float2 uv2 : TEXCOORD3;
    float2 uvDetail : TEXCOORD4;
    float2 uvSphere : TEXCOORD5;
    float4 vertexColor : TEXCOORD6;
    float fogCoord : TEXCOORD7;
};

      
float3 PosterizeLight(float3 lit, float steps)
{
    if (steps < 1.0)
        return lit;
    return floor(lit * steps + 0.5) / steps;
}

      
void BlinnPhong(float3 N, float3 L, float3 V,
                      float3 lightColor, float atten,
                      out float3 diffuse, out float3 specular)
{
    float NdotL = max(0.0, dot(N, L));
    diffuse = lightColor * NdotL * atten;

    specular = float3(0, 0, 0);
#ifndef _FLATSPEC_ON
    float3 H = normalize(L + V);
    float NdotH = max(0.0, dot(N, H));
    specular = lightColor * _SpecColor.rgb
                       * pow(NdotH, _Shininess)
                       * NdotL * atten * _SpecStrength;
#endif
}

      
float3 QuantizeColor(float3 col, float bits)
{
    float levels = pow(2.0, bits) - 1.0;
    return round(col * levels) / levels;
}

Varyings vert(Attributes IN)
{
    Varyings OUT;

    OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
    OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
    OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
    OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
    OUT.uv2 = IN.uv2;
    OUT.uvDetail = TRANSFORM_TEX(IN.uv, _DetailTex);
    OUT.vertexColor = IN.vertexColor;
    OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);

#ifdef _USESPHERE_ON
          float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalize(OUT.normalWS));
          OUT.uvSphere    = normalVS.xy * 0.5 + 0.5;
#else
    OUT.uvSphere = float2(0, 0);
#endif

    return OUT;
}

float4 frag(Varyings IN) : SV_Target
{
       
    float3 normalWS = normalize(IN.normalWS);
    float3 viewDir = normalize(GetCameraPositionWS() - IN.positionWS);

        
    float3 ambient = SampleSH(normalWS) * _Color.rgb * _AmbientMult;

        
    Light main = GetMainLight();
    float3 diff, spec;
    BlinnPhong(normalWS, normalize(main.direction), viewDir,
                   main.color, main.distanceAttenuation, diff, spec);

       
    float3 addDiff = float3(0, 0, 0);
    float3 addSpec = float3(0, 0, 0);

#if defined(_ADDITIONAL_LIGHTS_VERTEX) || defined(_ADDITIONAL_LIGHTS)
          uint addCount = min(GetAdditionalLightsCount(), 8u);
          for (uint i = 0u; i < addCount; ++i)
          {
            Light  addL = GetAdditionalLight(i, IN.positionWS);
            float3 ad, as;
            BlinnPhong(normalWS, normalize(addL.direction), viewDir,
                       addL.color,
                       addL.distanceAttenuation * addL.shadowAttenuation,
                       ad, as);
            addDiff += ad;
            addSpec += as;
          }
#endif

        
    float3 totalDiff = PosterizeLight(diff + addDiff, _LightSteps);
    float3 totalSpec = spec + addSpec;

    float3 lighting = ambient + totalDiff + totalSpec;

        
    float4 diffuseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
    float3 col = diffuseTex.rgb;

#ifdef _USEDETAIL_ON
          float3 detail = SAMPLE_TEXTURE2D(_DetailTex, sampler_DetailTex, IN.uvDetail).rgb;
          col = lerp(col, col * detail * 2.0, _DetailStr);
#endif

    col *= IN.vertexColor.rgb;

#ifdef _USELIGHTMAP_ON
          float3 lm = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, IN.uv2).rgb;
          col = diffuseTex.rgb * IN.vertexColor.rgb * lm * _LightmapInt;
#else
    col *= _Color.rgb * lighting;
#endif

#ifdef _USESPHERE_ON
          float3 sphere = SAMPLE_TEXTURE2D(_SphereTex, sampler_SphereTex, IN.uvSphere).rgb;
          col += sphere * _SphereStr;
#endif

#ifdef _USEEMISSION_ON
          float3 emission = SAMPLE_TEXTURE2D(_EmissionTex, sampler_EmissionTex, IN.uv).rgb;
          col += emission * _EmissionStr;
#endif

    col = saturate(col);
    col = QuantizeColor(col, _ColorDepth);
    col = MixFog(col, IN.fogCoord);

    return float4(col, diffuseTex.a);
}
      ENDHLSL
    }
  }
}