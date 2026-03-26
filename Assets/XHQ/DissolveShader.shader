Shader "Custom/URP_SimpleDissolve"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Texture", 2D) = "white" {}
        _DissolveMap("Dissolve Guide (Noise)", 2D) = "white" {}
        _DissolveAmount("Dissolve Amount", Range(0, 1)) = 0
        [HDR] _EdgeColor("Edge Color", Color) = (1, 0, 0, 1)
        _EdgeWidth("Edge Width", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DissolveMap); SAMPLER(sampler_DissolveMap);
            float _DissolveAmount;
            float4 _EdgeColor;
            float _EdgeWidth;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float dissolve = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, input.uv).r;
                
                // 核心：丢弃像素实现溶解
                clip(dissolve - _DissolveAmount);

                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                // 边缘发光
                if (dissolve - _DissolveAmount < _EdgeWidth && _DissolveAmount > 0)
                {
                    color.rgb += _EdgeColor.rgb;
                }

                return color;
            }
            ENDHLSL
        }
    }
}