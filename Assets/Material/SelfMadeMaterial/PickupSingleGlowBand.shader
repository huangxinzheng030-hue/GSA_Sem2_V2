Shader "Custom/URP/PickupSingleGlowBand"
{
    Properties
    {
        [HDR] _BandColor ("Band Color", Color) = (0.2, 1.8, 3.0, 1)
        [HDR] _CoreColor ("Core Color", Color) = (0.8, 2.5, 4.0, 1)

        _BaseAlpha ("Base Alpha", Range(0,1)) = 0.02
        _BandAlpha ("Band Alpha", Range(0,1)) = 0.65

        _CoreBrightness ("Core Brightness", Range(0,10)) = 4.0
        _GlowBrightness ("Glow Brightness", Range(0,10)) = 1.6

        _BandWidth ("Band Width", Range(0.01,1)) = 0.16
        _EdgeSoftness ("Edge Softness", Range(0.001,1)) = 0.18

        _ScrollSpeed ("Scroll Speed", Range(-5,5)) = 0.8
        _BandAngle ("Band Angle", Range(0,360)) = 35

        _FresnelPower ("Fresnel Power", Range(0.1,8)) = 3.5
        _FresnelStrength ("Fresnel Strength", Range(0,2)) = 0.2
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
            Name "ForwardTransparent"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
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
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 viewDirWS   : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BandColor;
                half4 _CoreColor;

                half _BaseAlpha;
                half _BandAlpha;

                half _CoreBrightness;
                half _GlowBrightness;

                half _BandWidth;
                half _EdgeSoftness;

                half _ScrollSpeed;
                half _BandAngle;

                half _FresnelPower;
                half _FresnelStrength;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.uv = IN.uv;
                OUT.normalWS = normalInputs.normalWS;
                OUT.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);

                return OUT;
            }

            half SmoothBand(half coord, half width, half softness)
            {
                // coord 是 0~1 循环坐标，中心在 0.5
                half d = abs(coord - 0.5h);

                // 外圈软边
                half outer = 1.0h - smoothstep(width * 0.5h, width * 0.5h + softness, d);

                return saturate(outer);
            }

            half CoreBand(half coord, half width)
            {
                half d = abs(coord - 0.5h);

                // 中心更窄更亮
                half coreWidth = width * 0.28h;
                return 1.0h - smoothstep(coreWidth * 0.5h, coreWidth * 0.8h, d);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half2 uv = IN.uv;

                // 旋转 UV，决定灯带移动方向
                half angleRad = radians(_BandAngle);
                half cs = cos(angleRad);
                half sn = sin(angleRad);

                half rotated = (uv.x - 0.5h) * cs - (uv.y - 0.5h) * sn;

                // 单条灯带循环移动
                half flow = frac(rotated - _Time.y * _ScrollSpeed);

                half softBand = SmoothBand(flow, _BandWidth, _EdgeSoftness);
                half coreBand = CoreBand(flow, _BandWidth);

                half3 normalWS = normalize(IN.normalWS);
                half3 viewDirWS = normalize(IN.viewDirWS);
                half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _FresnelPower) * _FresnelStrength;

                // 颜色：外圈柔和，中心更亮
                half3 glowColor = _BandColor.rgb * softBand * _GlowBrightness;
                half3 coreColor = _CoreColor.rgb * coreBand * _CoreBrightness;

                half3 finalColor = glowColor + coreColor + (_BandColor.rgb * fresnel);

                // Alpha：主体由软边控制，中心更实一点
                half finalAlpha = _BaseAlpha
                                + softBand * (_BandAlpha * 0.65h)
                                + coreBand * (_BandAlpha * 0.35h)
                                + fresnel * 0.08h;

                finalAlpha = saturate(finalAlpha);

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}