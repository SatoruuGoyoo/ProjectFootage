Shader "Custom/RetroBlinnPhong"
{
  Properties
  {
    _Color     ("Diffuse Color",  Color)        = (1,1,1,1)
    _SpecColor  ("Specular Color", Color)        = (1,1,1,1)
    _Shininess  ("Shininess",      Range(1,128)) = 32
  }

  SubShader
  {
    // RenderPipeline = UniversalPipeline es obligatorio en URP.
    // Sin esto Unity ignora el SubShader y muestra rosa.
    Tags
    {
      "RenderType"     = "Opaque"
      "RenderPipeline" = "UniversalPipeline"
      "Queue"          = "Geometry"
    }

    Pass
    {
      Name "ForwardLit"
      // En URP el LightMode es "UniversalForward", no "ForwardBase".
      Tags { "LightMode" = "UniversalForward" }

      HLSLPROGRAM
      // En URP se usa HLSLPROGRAM, no CGPROGRAM.
      #pragma vertex   vert
      #pragma fragment frag

      // Keyword para que URP incluya luces adicionales por vértice.
      // Se activa en el Renderer Asset → Additional Lights → Per Vertex.
      #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX

      // Includes de URP — reemplazan a UnityCG.cginc
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

      // En URP las propiedades van dentro de CBUFFER para ser compatibles
      // con SRP Batcher (optimizacion de draw calls).
      CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _SpecColor;
        float  _Shininess;
      CBUFFER_END

      struct Attributes
      {
        float4 positionOS : POSITION;
        float3 normalOS   : NORMAL;
      };

      // Solo necesitamos pasar el color calculado en vertex al fragment.
      // El fragment no hace nada de lighting — eso es el look de 6ta gen.
      struct Varyings
      {
        float4 positionCS : SV_POSITION;
        float4 color      : COLOR;
      };

      // Funcion auxiliar: calcula Blinn-Phong para una luz.
      // Devuelve diffuse + specular para no repetir el mismo bloque
      // en la luz principal y en el loop de luces adicionales.
      float3 BlinnPhong(float3 normalWS, float3 lightDir, float3 viewDir,
                        float3 lightColor, float atten)
      {
        float NdotL = max(0.0, dot(normalWS, lightDir));

        float3 diffuse = lightColor * atten * _Color.rgb * NdotL;

        // Halfway vector — esto es Blinn-Phong real.
        // El original usaba reflect() que es Phong clasico.
        float3 halfDir = normalize(lightDir + viewDir);
        float  NdotH   = max(0.0, dot(normalWS, halfDir));

        // NdotL al final anula el specular cuando la luz esta
        // del lado oscuro — sin if statement.
        float3 specular = lightColor * atten
                        * _SpecColor.rgb
                        * pow(NdotH, _Shininess)
                        * NdotL;

        return diffuse + specular;
      }

      Varyings vert(Attributes IN)
      {
        Varyings OUT;

        // TransformObjectToWorld / TransformObjectToWorldNormal reemplazan
        // los mul(unity_ObjectToWorld, ...) manuales.
        float3 posWS    = TransformObjectToWorld(IN.positionOS.xyz);
        float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
        float3 viewDir  = normalize(GetCameraPositionWS() - posWS);

        // TransformWorldToHClip reemplaza UnityObjectToClipPos.
        OUT.positionCS = TransformWorldToHClip(posWS);

        // Luz principal
        Light mainLight  = GetMainLight();
        float3 lightDir  = normalize(mainLight.direction);
        float3 finalColor = BlinnPhong(normalWS, lightDir, viewDir,
                                       mainLight.color,
                                       mainLight.distanceAttenuation);

        // Ambient
        // SampleSH devuelve el color de ambient de la escena.
        // Si en Lighting Settings → Environment → Source = Color,
        // esto es un color plano — que es el look de epoca.
        finalColor += SampleSH(normalWS) * _Color.rgb;

        // Luces adicionales per-vertice
        // Solo activo si _ADDITIONAL_LIGHTS_VERTEX esta habilitado
        // en el URP Renderer Asset.
        #ifdef _ADDITIONAL_LIGHTS_VERTEX
          uint addLightCount = GetAdditionalLightsCount();
          for (uint i = 0u; i < addLightCount; ++i)
          {
            Light addLight    = GetAdditionalLight(i, posWS);
            float3 addLightDir = normalize(addLight.direction);
            finalColor += BlinnPhong(normalWS, addLightDir, viewDir,
                                     addLight.color,
                                     addLight.distanceAttenuation);
          }
        #endif

        OUT.color = float4(finalColor, 1.0);
        return OUT;
      }

      // Fragment no hace nada — recibe el color interpolado del vertex stage.
      // El gradiente Gouraud entre vertices es el look de 6ta gen.
      float4 frag(Varyings IN) : SV_Target
      {
        return IN.color;
      }

      ENDHLSL
    }
  }
}
