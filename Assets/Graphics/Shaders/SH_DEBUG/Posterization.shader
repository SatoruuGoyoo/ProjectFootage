Shader"Custom/Posterization"
{
    Properties
    {
        _Steps      ("Color Steps", Range(2, 16)) = 4
        _Saturation ("Saturation Steps", Range(0.5, 2)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
ZTest Always
ZWrite Off
Cull Off

        Pass
        {
Name"PosterizationPass"

            HLSLPROGRAM
            #pragma vertex Vert      // Vert (mayúscula) viene del Blit.hlsl
            #pragma fragment frag
            #include "PosterizationPass.hlsl"
            ENDHLSL
        }
    }
}