Shader "Custom/URP/InstagramSprayObjectV4"
{
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)

        _Alpha ("Alpha", Range(0,1)) = 0.38

        _PatchDensity ("Patch Density", Range(1,12)) = 3.2
        _PatchSoftness ("Patch Softness", Range(1,20)) = 7.0
        _ColorChangeSpeed ("Color Change Speed", Range(0,1)) = 0.12
        _HueVariation ("Hue Variation", Range(0,1)) = 1.0

        _Saturation ("Saturation", Range(0,1)) = 0.8
        _Brightness ("Brightness", Range(0.5,2)) = 1.08
        _SecondaryLayerStrength ("Secondary Layer Strength", Range(0,1)) = 0.65
        _MistStrength ("Mist Strength", Range(0,1)) = 0.35

        _BlendContrast ("Blend Contrast", Range(0.2,3)) = 1.15

        _FresnelPower ("Fresnel Power", Range(0.1,8)) = 2.2
        _EdgeBrightness ("Edge Brightness", Range(0,3)) = 1.1
        _HighlightStrength ("Highlight Strength", Range(0,3)) = 0.55

        _MetallicFake ("Metallic (Stylized)", Range(0,1)) = 0.05
        _RoughnessFake ("Roughness (Stylized)", Range(0,1)) = 0.45
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

                half _PatchDensity;
                half _PatchSoftness;
                half _ColorChangeSpeed;
                half _HueVariation;

                half _Saturation;
                half _Brightness;
                half _SecondaryLayerStrength;
                half _MistStrength;
                half _BlendContrast;

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
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = normalInputs.normalWS;
                OUT.uv          = IN.uv;
                OUT.viewDirWS   = GetWorldSpaceViewDir(posInputs.positionWS);

                return OUT;
            }

            float Hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float2 Hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            half3 HSVToRGB(half3 c)
            {
                half4 K = half4(1.0h, 2.0h / 3.0h, 1.0h / 3.0h, 3.0h);
                half3 p = abs(frac(c.xxx + K.xyz) * 6.0h - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            void PatchField(
                float2 uv,
                float density,
                float softness,
                float timeValue,
                out half3 outColor,
                out half outMask)
            {
                float2 gridUV = uv * density;
                float2 cell = floor(gridUV);
                float2 local = frac(gridUV);

                half3 colorAcc = half3(0,0,0);
                float weightAcc = 0.0;
                float baseWeightAcc = 0.0;

                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 cellId = cell + float2(x, y);

                        float2 rnd = Hash22(cellId);
                        float2 center = float2(x, y) + rnd;

                        float2 delta = center - local;
                        float dist2 = dot(delta, delta);

                        float baseW = exp(-dist2 * softness);

                        float deposit = lerp(0.45, 1.0, Hash21(cellId * 2.37 + 8.12));

                        float baseHue = Hash21(cellId * 1.73 + 17.9);
                        float phase = Hash21(cellId * 2.91 + 71.3) * 6.2831853;

                        float hueShift = sin(timeValue * _ColorChangeSpeed + phase) * 0.13 * _HueVariation;
                        float hue = frac(baseHue + hueShift);

                        half sat = _Saturation * lerp(0.92h, 1.08h, (half)Hash21(cellId * 4.91 + 1.7));
                        half val = _Brightness * lerp(0.9h, 1.06h, (half)Hash21(cellId * 6.07 + 5.4));

                        half3 c = HSVToRGB(half3((half)hue, sat, val));

                        float w = baseW * deposit;

                        colorAcc += c * (half)w;
                        weightAcc += w;
                        baseWeightAcc += baseW;
                    }
                }

                outColor = colorAcc / max((half)weightAcc, 0.0001h);
                outMask = saturate((half)(weightAcc / max(baseWeightAcc, 0.0001)));
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half2 uv = IN.uv;
                half timeValue = _Time.y;

                half3 colorA;
                half maskA;
                PatchField(uv, _PatchDensity, _PatchSoftness, timeValue, colorA, maskA);

                half3 colorB;
                half maskB;
                PatchField(uv * 1.37h + half2(1.83h, 2.41h), _PatchDensity * 0.72h, _PatchSoftness * 0.72h, timeValue * 0.82h, colorB, maskB);

                half3 colorC;
                half maskC;
                PatchField(uv * 0.63h + half2(4.12h, 0.76h), _PatchDensity * 0.45h, _PatchSoftness * 0.38h, timeValue * 0.55h, colorC, maskC);

                half3 sprayColor = lerp(colorA, colorB, _SecondaryLayerStrength);
                sprayColor = lerp(sprayColor, colorC, _MistStrength);

                half sprayMask = saturate(maskA * 0.5h + maskB * 0.3h + maskC * 0.2h);
                sprayMask = pow(sprayMask, 1.0h / _BlendContrast);

                half3 normalWS = normalize(IN.normalWS);
                half3 viewDirWS = normalize(IN.viewDirWS);

                half ndv = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(1.0h - ndv, _FresnelPower);

                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 reflectDir = reflect(-lightDir, normalWS);

                half roughness = saturate(_RoughnessFake);
                half metallic = saturate(_MetallicFake);

                half specPower = lerp(48.0h, 8.0h, roughness);
                half spec = pow(saturate(dot(reflectDir, viewDirWS)), specPower) * _HighlightStrength;

                // 重点：主体颜色更实，不再是玻璃边缘主导
                half3 bodyColor = lerp(_BaseColor.rgb * 0.18h, sprayColor, 0.82h);
                bodyColor *= lerp(0.82h, 1.0h, sprayMask);

                // 很轻的边缘提亮
                half3 edgeColor = sprayColor * fresnel * _EdgeBrightness * 0.45h;

                // 很轻的高光，避免塑料反光太重
                half3 specColor = lerp(half3(1,1,1), sprayColor, metallic * 0.2h) * spec;

                half3 finalColor = bodyColor + edgeColor + specColor;

                // alpha 更稳定，避免太玻璃
                half finalAlpha = saturate(_Alpha + sprayMask * 0.12h + fresnel * 0.08h);

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}