// Assets/Shaders/InvertedCrosshairShader.shader

Shader "UI/InvertedCrosshairAlpha"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
        }

        Pass
        {
            // A operação de blend que inverte a cor de fundo.
            Blend One OneMinusDstColor
            
            Cull Off
            Lighting Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                half4 color         : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                half4 color         : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half4 _Color;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 textureColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 finalColor = IN.color; // Começa com a cor do tint da UI

                // A linha CRÍTICA: Multiplica o alfa final pelo alfa da textura.
                // Onde a textura é transparente (alfa=0), o resultado será transparente.
                // Onde a textura é opaca (alfa=1), a cor do tint será usada na operação de blend.
                finalColor.a *= textureColor.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
}