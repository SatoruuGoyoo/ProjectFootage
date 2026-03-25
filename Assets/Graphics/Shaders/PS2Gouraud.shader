Shader "Custom/PS2Gouraud"
{
    Properties
    {
        _MainTex        ("Texture", 2D)          = "white" {}
        _AmbientColor   ("Ambient Color", Color)  = (0.3, 0.3, 0.35, 1)
        _AmbientStrength("Ambient Strength",  Range(0, 1)) = 0.4
        _LightStrength  ("Light Strength",    Range(0, 3)) = 1.0
        _ShadowStrength ("Shadow Strength",   Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

     
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
#pragma fragment frag

#define _ADDITIONAL_LIGHTS_VERTEX
#define _ADDITIONAL_LIGHTS

#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile _ _SHADOWS_SOFT


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 vertexLight : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _AmbientColor;
                float  _AmbientStrength;
                float  _LightStrength;
                float  _ShadowStrength;
            CBUFFER_END

            Varyings vert(
Attributes IN)
            {
Varyings OUT;

float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                OUT.positionHCS = TransformWorldToHClip(posWS);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);

float3 ambient = _AmbientColor.rgb * _AmbientStrength;

float4 shadowCoord = TransformWorldToShadowCoord(posWS);
Light mainLight = GetMainLight(shadowCoord);
float NdotL = saturate(dot(normalWS, mainLight.direction));
float shadow = lerp(1.0, mainLight.shadowAttenuation, _ShadowStrength);
float3 lighting = mainLight.color * NdotL * shadow * _LightStrength;

           
InputData inputData = (InputData) 0;
                inputData.positionWS =
posWS;
                inputData.normalWS   =
normalWS;

#ifdef _ADDITIONAL_LIGHTS
uint lightCount = GetAdditionalLightsCount();
                for (
uint i = 0u;i <
lightCount; i++)
                {
                
Light light = GetAdditionalLight(i, posWS, half4(1, 1, 1, 1));
float NdotL_add = saturate(dot(normalWS, light.direction));
                    lighting       += light.color * NdotL_add * light.distanceAttenuation *
_LightStrength;
                }
                #endif

                OUT.vertexLight = lighting +
ambient;
                return
OUT;
            }

half4 frag(Varyings IN) : SV_Target
{
 
    return half4(IN.vertexLight, 1.0);
}

            ENDHLSL
        }

        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
    
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}