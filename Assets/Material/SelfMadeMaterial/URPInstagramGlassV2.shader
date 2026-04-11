Shader "Custom/URP/InstagramGlassV2"
{
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)

        _Alpha ("Base Alpha", Range(0,1)) = 0.18

        _ColorSpeed ("Color Speed", Range(0,5)) = 0.55
        _ColorScale ("Color Scale", Range(0,10)) = 1.8
        _RainbowStrength ("Rainbow Strength", Range(0,3)) = 0.85

        _WaveStrength ("Wave Strength", Range(0,1)) = 0.03
        _WaveFrequency ("Wave Frequency", Range(0,10)) = 2.2
        _DistortStrength ("Distort Strength", Range(0,0.2)) = 0.025

        _FresnelPower ("Fresnel Power", Range(0.1,8)) = 4.0
        _EdgeBrightness ("Edge Brightness", Range(0,5)) = 2.8

        _HighlightStrength ("Highlight Strength", Range(0,5)) = 1.4

        _MetallicFake ("Metallic (Stylized)", Range(0,1)) = 0.25
        _RoughnessFake ("Roughness (Stylized)", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardTransparent"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
                half _DistortStrength;
                half _FresnelPower;
                half _EdgeBrightness;
                half _HighlightStrength;
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

            half3 RainbowFromPhase(half phase)
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

                // 轻微 UV 扭动，让颜色不像静态贴纸
                half wave1 = sin((uv.y + time) * _WaveFrequency * 6.2831853h + uv.x * 2.0h) * _WaveStrength;
                half wave2 = cos((uv.x - time * 0.75h) * (_WaveFrequency * 0.85h) * 6.2831853h + uv.y) * (_WaveStrength * 0.6h);

                half2 uv2 = uv;
                uv2.x += wave1 * _DistortStrength * 6.0h;
                uv2.y += wave2 * _DistortStrength * 6.0h;

                half hue = frac(uv2.y * _ColorScale + uv2.x * (_ColorScale * 0.28h) + time + wave1 + wave2);
                half3 rainbow = RainbowFromPhase(hue * 6.2831853h);

                // 让彩色更柔一点，不要太像儿童彩虹板
                half gray = dot(rainbow, half3(0.3333h, 0.3333h, 0.3333h));
                rainbow = lerp(half3(gray, gray, gray), rainbow, 0.72h) * _RainbowStrength;

                half3 normalWS = normalize(IN.normalWS);
                half3 viewDirWS = normalize(IN.viewDirWS);

                half ndv = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(1.0h - ndv, _FresnelPower);

                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);

                half ndl = saturate(dot(normalWS, lightDir));
                half3 reflectDir = reflect(-lightDir, normalWS);

                half roughness = saturate(_RoughnessFake);
                half metallic = saturate(_MetallicFake);

                // 粗糙度越低，高光越集中
                half specPower = lerp(96.0h, 8.0h, roughness);
                half spec = pow(saturate(dot(reflectDir, viewDirWS)), specPower) * _HighlightStrength;

                // 主体颜色压低，让中间更透
                half3 bodyColor = lerp(_BaseColor.rgb * 0.08h, rainbow, 0.55h);
                bodyColor *= lerp(1.0h, 0.78h, roughness);
                bodyColor *= lerp(0.55h, 0.9h, ndl);

                // 边缘彩光
                half3 edgeColor = rainbow * fresnel * _EdgeBrightness;

                // 高光颜色
                half3 specColor = lerp(half3(1,1,1), rainbow, metallic * 0.45h) * spec;

                half3 finalColor = bodyColor + edgeColor + specColor;

                // 关键：中心更透明，边缘更明显
                half finalAlpha = saturate(_Alpha * 0.45h + fresnel * 0.65h);

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}