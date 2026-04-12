Shader "UI/VHSStatic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 0
        _ScanlineCount ("Scanline Count", Float) = 300
        _ScanlineAlpha ("Scanline Alpha", Range(0, 1)) = 0.3
        _NoiseScale ("Noise Scale", Float) = 500
        _ScrollSpeed ("Scroll Speed", Float) = 10

        // Required for UI
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Intensity;
            float _ScanlineCount;
            float _ScanlineAlpha;
            float _NoiseScale;
            float _ScrollSpeed;
            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Hash pseudo-random
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_Intensity <= 0.001)
                    discard;

                float time = _Time.y;

                // Ruido estático — cambia cada frame
                float2 noiseUV = i.uv * _NoiseScale + float2(time * 137.3, time * 241.7);
                float noise = hash(noiseUV);

                // Scanlines finas que se desplazan
                float scanline = sin((i.uv.y + time * _ScrollSpeed) * _ScanlineCount * 3.14159) * 0.5 + 0.5;
                scanline = smoothstep(0.4, 0.6, scanline);

                // Combinar: ruido base + scanlines encima
                float gray = noise;
                float scanlineMask = lerp(1.0, 1.0 - _ScanlineAlpha, scanline);
                gray *= scanlineMask;

                // Variación horizontal tipo VHS (bandas que cruzan)
                float band = sin((i.uv.y + time * 2.3) * 5.0) * 0.5 + 0.5;
                band = smoothstep(0.3, 0.7, band);
                float bandNoise = hash(float2(floor(i.uv.y * 50.0), floor(time * 20.0)));
                gray = lerp(gray, gray * (0.7 + bandNoise * 0.3), band * 0.3 * _Intensity);

                float alpha = _Intensity;

                return fixed4(gray, gray, gray, alpha);
            }
            ENDCG
        }
    }
}
