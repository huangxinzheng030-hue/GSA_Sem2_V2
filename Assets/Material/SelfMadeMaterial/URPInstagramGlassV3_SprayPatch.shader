Shader "Custom/URP/InstagramGlassV3_SprayPatch"
{
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)

        _Alpha ("Base Alpha", Range(0,1)) = 0.18

        _PatchDensity ("Patch Density", Range(1,20)) = 5.0
        _PatchSoftness ("Patch Softness", Range(1,24)) = 9.0
        _ColorChangeSpeed ("Color Change Speed", Range(0,2)) = 0.18
        _HueVariation ("Hue Variation", Range(0,1)) = 1.0

        _Saturation ("Saturation", Range(0,1)) = 0.62
        _Brightness ("Brightness", Range(0,2)) = 1.0
        _SecondaryLayerStrength ("Secondary Layer Strength", Range(0,1)) = 0.45

        _FresnelPower ("Fresnel Power", Range(0.1,8)) = 4.2
        _EdgeBrightness ("Edge Brightness", Range(0,5)) = 3.0
        _HighlightStrength ("Highlight Strength", Range(0,5)) = 1.8

        _MetallicFake ("Metallic (Stylized)", Range(0,1)) = 0.18
        _RoughnessFake ("Roughness (Stylized)", Range(0,1)) = 0.12
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

            void SprayPatchField(
                float2 uv,
                float density,
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

                        float baseW = exp(-dist2 * _PatchSoftness);

                        float deposit = lerp(0.35, 1.0, Hash21(cellId * 2.73 + 7.13));

                        float baseHue = Hash21(cellId * 1.91 + 13.7);
                        float phase = Hash21(cellId * 3.17 + 91.1) * 6.2831853;

                        // 颜色在原地慢慢变，不是整片平移
                        float hueShift = sin(timeValue * _ColorChangeSpeed + phase) * 0.18 * _HueVariation;
                        float hue = frac(baseHue + hueShift + timeValue * _ColorChangeSpeed * 0.03);

                        half sat = _Saturation * lerp(0.88h, 1.08h, (half)Hash21(cellId * 5.11 + 2.4));
                        half val = _Brightness * lerp(0.85h, 1.05h, (half)Hash21(cellId * 6.41 + 4.8));

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
                SprayPatchField(uv, _PatchDensity, timeValue, colorA, maskA);

                half3 colorB;
                half maskB;
                SprayPatchField(uv * 1.63h + half2(2.17h, 1.31h), _PatchDensity * 0.62h, timeValue * 0.87h, colorB, maskB);

                half3 sprayColor = lerp(colorA, colorB, _SecondaryLayerStrength);
                half sprayMask = saturate(lerp(maskA, maskB, 0.5h));

                half3 normalWS = normalize(IN.normalWS);
                half3 viewDirWS = normalize(IN.viewDirWS);

                half ndv = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(1.0h - ndv, _FresnelPower);

                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 reflectDir = reflect(-lightDir, normalWS);

                half roughness = saturate(_RoughnessFake);
                half metallic = saturate(_MetallicFake);

                half specPower = lerp(96.0h, 8.0h, roughness);
                half spec = pow(saturate(dot(reflectDir, viewDirWS)), specPower) * _HighlightStrength;

                // 主体颜色：不是整块实色，而是喷漆一样浮在表面
                half3 bodyColor = lerp(_BaseColor.rgb * 0.06h, sprayColor, 0.68h);
                bodyColor *= lerp(0.75h, 1.0h, sprayMask);

                // 边缘彩光
                half3 edgeColor = sprayColor * fresnel * _EdgeBrightness;

                // 高光
                half3 specColor = lerp(half3(1,1,1), sprayColor, metallic * 0.35h) * spec;

                half3 finalColor = bodyColor + edgeColor + specColor;

                // 中间更透，边缘更明显
                half finalAlpha = saturate(_Alpha * 0.35h + sprayMask * 0.16h + fresnel * 0.62h);

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}