Shader "Custom/PS2Toon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0.2, 0.2, 0.3, 1)
        _Steps ("Steps", Range(1,4)) = 2
        _OutlineColor ("Outline Color", Color) = (0.0, 0.0, 0.0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.03
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "Outline"
            Cull Front

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _ShadowColor;
                float4 _OutlineColor;
                float  _Steps;
                float  _OutlineWidth;
            CBUFFER_END

            struct OA { float4 pos : POSITION; float3 nor : NORMAL; };
            struct OV { float4 pos : SV_POSITION; };

            OV vertOutline(OA IN)
            {
                OV OUT;
                float3 posWS = TransformObjectToWorld(IN.pos.xyz);
                float3 norWS = TransformObjectToWorldNormal(IN.nor);
                posWS = posWS + norWS * _OutlineWidth;
                OUT.pos = TransformWorldToHClip(posWS);
                return OUT;
            }

            half4 fragOutline(OV IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _ShadowColor;
                float4 _OutlineColor;
                float  _Steps;
                float  _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 toonColor : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 norWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionHCS = TransformWorldToHClip(posWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(norWS, mainLight.direction));
                float stepped = floor(NdotL * _Steps) / _Steps;
                OUT.toonColor = lerp(_ShadowColor.rgb, mainLight.color, stepped);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return half4(tex.rgb * IN.toonColor, 1.0);
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
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct SA { float4 pos : POSITION; float3 nor : NORMAL; };
            struct SV { float4 pos : SV_POSITION; };

            SV vertShadow(SA IN)
            {
                SV OUT;
                float3 posWS = TransformObjectToWorld(IN.pos.xyz);
                float3 norWS = TransformObjectToWorldNormal(IN.nor);
                posWS = ApplyShadowBias(posWS, norWS, _LightDirection);
                OUT.pos = TransformWorldToHClip(posWS);
                return OUT;
            }

            half4 fragShadow(SV IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}