Shader "Custom/RetroBlinnPhong"
{
  Properties
  {
    _Color     ("Diffuse Color",   Color)  = (1,1,1,1)
    _SpecColor  ("Specular Color",  Color)  = (1,1,1,1)
    _Shininess  ("Shininess",       Range(1, 128)) = 32
  }

  SubShader
  {
    // ------------------------------------------------------------------ //
    // PASS 1 — ForwardBase                                                //
    // Ambient + primera luz direccional/point                             //
    // ------------------------------------------------------------------ //
    Pass
    {
      Tags { "LightMode" = "ForwardBase" }

      CGPROGRAM
      #pragma vertex   vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      uniform float4 _LightColor0;
      uniform float4 _Color;
      uniform float4 _SpecColor;
      uniform float  _Shininess;

      struct vertexInput
      {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
      };

      // El color ya viene calculado del vertex stage.
      // El fragment no hace nada — eso es el look de 6ta gen.
      struct vertexOutput
      {
        float4 pos   : SV_POSITION;
        float4 color : COLOR;
      };

      vertexOutput vert(vertexInput input)
      {
        vertexOutput output;

        // ── Posición y normal en world space ─────────────────────────── //
        // unity_ObjectToWorld / unity_WorldToObject reemplaza las APIs
        // deprecadas _Object2World / _World2Object.
        float4 posWorld  = mul(unity_ObjectToWorld, input.vertex);
        float3 normalDir = normalize(
          mul(float4(input.normal, 0.0), unity_WorldToObject).xyz
        );
        float3 viewDir = normalize(_WorldSpaceCameraPos - posWorld.xyz);

        // ── Dirección de luz — branchless ─────────────────────────────── //
        // _WorldSpaceLightPos0.w == 0 → luz direccional (sin posición)
        // _WorldSpaceLightPos0.w == 1 → point / spot light
        //
        // Truco: posWorld.xyz * w cancela la posición cuando w=0
        // (light direction pura) y la activa cuando w=1 (point light).
        // Elimina el if (w == 0) sin costo extra.
        float3 toLight   = _WorldSpaceLightPos0.xyz
                         - posWorld.xyz * _WorldSpaceLightPos0.w;
        float  dist      = length(toLight);
        float3 lightDir  = normalize(toLight);

        // lerp(1, 1/dist, w): atenuación 1.0 para direccional, lineal para point.
        float  atten     = lerp(1.0, 1.0 / dist, _WorldSpaceLightPos0.w);

        // ── Diffuse (Lambert) ─────────────────────────────────────────── //
        float  NdotL     = max(0.0, dot(normalDir, lightDir));
        float3 diffuse   = atten * _LightColor0.rgb * _Color.rgb * NdotL;

        // ── Specular (Blinn-Phong, per-vértice) ───────────────────────── //
        // H = halfway vector entre luz y cámara — esto es Blinn-Phong real.
        // El shader original usaba reflect() → eso es Phong, no Blinn-Phong.
        //
        // Multiplicar por NdotL al final elimina el segundo if
        // (specular = 0 cuando la luz está del lado equivocado).
        // max(0, NdotL) ya es 0 en ese caso, sin branch.
        float3 halfDir   = normalize(lightDir + viewDir);
        float  NdotH     = max(0.0, dot(normalDir, halfDir));
        float3 specular  = atten
                         * _LightColor0.rgb
                         * _SpecColor.rgb
                         * pow(NdotH, _Shininess)
                         * NdotL; // terminator — anula specular en lado oscuro

        // ── Ambient ───────────────────────────────────────────────────── //
        float3 ambient   = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb;

        // ── Output — todo calculado en vertex, fragment solo pasa color ── //
        // UnityObjectToClipPos reemplaza la API deprecada UNITY_MATRIX_MVP.
        output.pos   = UnityObjectToClipPos(input.vertex);
        output.color = float4(ambient + diffuse + specular, 1.0);
        return output;
      }

      // Fragment no hace nada de lighting — lo recibe interpolado del vertex.
      // El Gouraud gradient entre vértices es exactamente el look de 6ta gen.
      float4 frag(vertexOutput input) : SV_Target
      {
        return input.color;
      }

      ENDCG
    } // END PASS 1

    // ------------------------------------------------------------------ //
    // PASS 2 — ForwardAdd                                                 //
    // Una pasada por cada luz adicional (additive blend, sin ambient)     //
    // ------------------------------------------------------------------ //
    Pass
    {
      Tags  { "LightMode" = "ForwardAdd" }
      Blend One One // additive — se suma sobre el pass base

      CGPROGRAM
      #pragma vertex   vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      uniform float4 _LightColor0;
      uniform float4 _Color;
      uniform float4 _SpecColor;
      uniform float  _Shininess;

      struct vertexInput
      {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
      };

      struct vertexOutput
      {
        float4 pos   : SV_POSITION;
        float4 color : COLOR;
      };

      vertexOutput vert(vertexInput input)
      {
        vertexOutput output;

        float4 posWorld  = mul(unity_ObjectToWorld, input.vertex);
        float3 normalDir = normalize(
          mul(float4(input.normal, 0.0), unity_WorldToObject).xyz
        );
        float3 viewDir   = normalize(_WorldSpaceCameraPos - posWorld.xyz);

        float3 toLight   = _WorldSpaceLightPos0.xyz
                         - posWorld.xyz * _WorldSpaceLightPos0.w;
        float  dist      = length(toLight);
        float3 lightDir  = normalize(toLight);
        float  atten     = lerp(1.0, 1.0 / dist, _WorldSpaceLightPos0.w);

        float  NdotL     = max(0.0, dot(normalDir, lightDir));
        float3 diffuse   = atten * _LightColor0.rgb * _Color.rgb * NdotL;

        float3 halfDir   = normalize(lightDir + viewDir);
        float  NdotH     = max(0.0, dot(normalDir, halfDir));
        float3 specular  = atten
                         * _LightColor0.rgb
                         * _SpecColor.rgb
                         * pow(NdotH, _Shininess)
                         * NdotL;

        // Sin ambient en el pass adicional — solo diffuse + specular.
        output.pos   = UnityObjectToClipPos(input.vertex);
        output.color = float4(diffuse + specular, 1.0);
        return output;
      }

      float4 frag(vertexOutput input) : SV_Target
      {
        return input.color;
      }

      ENDCG
    } // END PASS 2
  }
}
