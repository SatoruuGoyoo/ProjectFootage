Shader"Custom/HybridQuantizedLit"
{
    Properties
    {
        _BaseColor    ("Base Color", Color) = (1,1,1,1)
        _BaseMap      ("Albedo", 2D) = "white" {}
        _Steps        ("Light Steps", Range(2, 8)) = 3
        _Smoothness   ("Band Smoothness", Range(0, 1)) = 0.25
        _QuantizeAmount ("Quantize Amount", Range(0, 1)) = 0.6
        _SpecularIntensity ("Specular", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
Name"ForwardLit"
            Tags
{"LightMode" = "UniversalForward"
}

HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Keywords de URP que necesitás para sombras y luces adicionales
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // -------------------------------------------------------
            // Propiedades → CBUFFER (SRP Batcher friendly)
            // -------------------------------------------------------
            CBUFFER_START(UnityPerMaterial)

float4 _BaseColor;
float4 _BaseMap_ST;
float _Steps;
float _Smoothness;
float _QuantizeAmount;
float _SpecularIntensity;
CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // -------------------------------------------------------
            // Structs
            // -------------------------------------------------------
struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float2 uv : TEXCOORD2;
    float4 shadowCoord : TEXCOORD3;
    float fogFactor : TEXCOORD4;
};

            // -------------------------------------------------------
            // Helpers
            // -------------------------------------------------------
float3 QuantizeDiffuse(float NdotL, float3 lightColor, float steps, float smoothness)
{
    float q = floor(NdotL * steps) / steps;
    float edge = smoothstep(q, q + smoothness / steps, NdotL);
    float final = lerp(q, edge, smoothness);
    return lightColor * final;
}

float3 PhysicalDiffuse(float NdotL, float3 lightColor)
{
    return lightColor * NdotL;
}

float3 HybridDiffuse(float NdotL, float3 lightColor, float steps, float smoothness, float amount)
{
    float3 physical = PhysicalDiffuse(NdotL, lightColor);
    float3 quantized = QuantizeDiffuse(NdotL, lightColor, steps, smoothness);
    return lerp(physical, quantized, amount);
}

            // -------------------------------------------------------
            // Vertex
            // -------------------------------------------------------
Varyings vert(Attributes IN)
{
    Varyings OUT;

    VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
    VertexNormalInputs norInputs = GetVertexNormalInputs(IN.normalOS);

    OUT.positionCS = posInputs.positionCS;
    OUT.positionWS = posInputs.positionWS;
    OUT.normalWS = norInputs.normalWS;
    OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    OUT.shadowCoord = GetShadowCoord(posInputs);
#else
    OUT.shadowCoord = float4(0, 0, 0, 0);
#endif
    OUT.fogFactor = ComputeFogFactor(posInputs.positionCS.z);

    return OUT;
}

            // -------------------------------------------------------
            // Fragment
            // -------------------------------------------------------
half4 frag(Varyings IN) : SV_Target
{
    float3 normal = normalize(IN.normalWS);
    float3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;
    float3 viewDir = normalize(GetCameraPositionWS() - IN.positionWS);

                // --- Main light ---
#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    Light mainLight = GetMainLight(IN.shadowCoord);
#else
    Light mainLight = GetMainLight(TransformWorldToShadowCoord(IN.positionWS));
#endif
    float NdotL_main = saturate(dot(normal, mainLight.direction));
    float3 lightColor_main = mainLight.color * mainLight.distanceAttenuation;
    float shadow = mainLight.shadowAttenuation; // solo para specular

    float3 diffuse = HybridDiffuse(NdotL_main, lightColor_main, _Steps, _Smoothness, _QuantizeAmount);

                // Specular mínimo — no quiero Blinn-Phong completo, solo un toque
    float3 halfDir = normalize(mainLight.direction + viewDir);
    float NdotH = saturate(dot(normal, halfDir));
    float3 specular = mainLight.color * pow(NdotH, 32) * _SpecularIntensity * mainLight.shadowAttenuation;

                // --- Luces adicionales ---
    int additionalCount = GetAdditionalLightsCount();
    for (int i = 0; i < additionalCount; i++)
    {
        Light light = GetAdditionalLight(i, IN.positionWS, half4(1, 1, 1, 1));
        float NdotL = saturate(dot(normal, light.direction));
        float3 lColor = light.color * light.distanceAttenuation;

        float3 diffuse = HybridDiffuse(NdotL_main, lightColor_main, _Steps, _Smoothness, _QuantizeAmount);

        float3 h = normalize(light.direction + viewDir);
        float3 specular = mainLight.color * pow(NdotH, 32) * _SpecularIntensity * shadow; // shadow aquí sí
    }

                // --- Ambient ---
    float3 ambient = SampleSH(normal) * albedo;

                // --- Composición final ---
    

    float3 finalColor = albedo * diffuse + specular + albedo * 0.15;
    finalColor = MixFog(finalColor, IN.fogFactor);

    return half4(finalColor, 1.0);
}
            ENDHLSL
        }

        // Pass de sombras — sin esto tu objeto no proyecta sombras
        Pass
        {
Name"ShadowCaster"
            Tags
{"LightMode" = "ShadowCaster"
}

ZWrite On

ZTest LEqual

HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}