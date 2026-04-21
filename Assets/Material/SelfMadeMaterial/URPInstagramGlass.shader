Shader "Custom/URP/InstagramGlass"
{
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)

        _Alpha ("Alpha", Range(0,1)) = 0.35

        _ColorSpeed ("Color Speed", Range(0,5)) = 0.8
        _ColorScale ("Color Scale", Range(0,10)) = 2.5
        _RainbowStrength ("Rainbow Strength", Range(0,3)) = 1.2

        _WaveStrength ("Wave Strength", Range(0,1)) = 0.08
        _WaveFrequency ("Wave Frequency", Range(0,10)) = 3.0

        _FresnelPower ("Fresnel Power", Range(0.1,8)) = 2.5
        _EdgeBrightness ("Edge Brightness", Range(0,5)) = 1.5

        _MetallicFake ("Metallic (Stylized)", Range(0,1)) = 1
        _RoughnessFake ("Roughness (Stylized)", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
           "RenderType"="Opaque"
           "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            //Blend SrcAlpha OneMinusSrcAlpha
            //ZWrite Off
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float3 viewDirWS   : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Alpha;
                half _ColorSpeed;
                half _ColorScale;
                half _RainbowStrength;
                half _WaveStrength;
                half _WaveFrequency;
                half _FresnelPower;
                half _EdgeBrightness;
                half _MetallicFake;
                half _RoughnessFake;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.uv = IN.uv;
                OUT.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);

                return OUT;
            }

            half3 RainbowFromPhase(float phase)
            {
                half r = 0.5h + 0.5h * sin(phase);
                half g = 0.5h + 0.5h * sin(phase + 2.0943951h);
                half b = 0.5h + 0.5h * sin(phase + 4.1887902h);
                return half3(r, g, b);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half2 uv = IN.uv;
                half time = _Time.y * _ColorSpeed;

                half waveA = sin((uv.y + time) * _WaveFrequency * 6.2831853h + uv.x * 2.0h) * _WaveStrength;
                half waveB = sin((uv.x - time * 0.7h) * (_WaveFrequency * 0.75h) * 6.2831853h) * (_WaveStrength * 0.5h);

                half hue = frac(uv.y * _ColorScale + uv.x * (_ColorScale * 0.35h) + time + waveA + waveB);
                half3 rainbow = RainbowFromPhase(hue * 6.2831853h) * _RainbowStrength;

                half3 normalWS = normalize(IN.normalWS);
                half3 viewDirWS = normalize(IN.viewDirWS);

                half ndv = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(1.0h - ndv, _FresnelPower);

                half roughness = saturate(_RoughnessFake);
                half metallic = saturate(_MetallicFake);

                // 粗糙度高：高光更宽更软；粗糙度低：高光更锐利
                half scan = sin((uv.x * 6.0h) + (uv.y * 2.0h) - time * 4.0h) * 0.5h + 0.5h;
                half specPower = lerp(28.0h, 6.0h, roughness);
                half spec = pow(saturate(scan), specPower) * lerp(1.4h, 0.7h, roughness) * metallic;

                half edgeGlow = fresnel * _EdgeBrightness * lerp(0.9h, 1.3h, metallic);
                half3 edgeColor = rainbow * edgeGlow;
                half3 specColor = rainbow * spec;

                half3 bodyColor = lerp(rainbow, _BaseColor.rgb * 0.35h + rainbow * 0.65h, 0.35h);
                bodyColor *= lerp(0.95h, 0.75h, roughness);

                half3 finalColor = bodyColor + edgeColor + specColor;

                half finalAlpha = saturate(_Alpha + fresnel * 0.18h);

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}