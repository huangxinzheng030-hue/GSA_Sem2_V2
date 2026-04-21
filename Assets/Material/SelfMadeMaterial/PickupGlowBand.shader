Shader "Custom/URP/PickupGlowBand"
{
    Properties
    {
        [HDR] _BandColor ("Band Color", Color) = (0.2, 1.8, 3.0, 1)
        [HDR] _EdgeColor ("Edge Color", Color) = (0.1, 0.8, 1.5, 1)

        _BaseAlpha ("Base Alpha", Range(0,1)) = 0.08
        _BandBrightness ("Band Brightness", Range(0,10)) = 3.5
        _EdgeBrightness ("Edge Brightness", Range(0,10)) = 1.5

        _BandCount ("Band Count", Range(1,20)) = 6
        _BandWidth ("Band Width", Range(0.01,0.5)) = 0.12
        _BandSoftness ("Band Softness", Range(0.001,0.3)) = 0.05
        _ScrollSpeed ("Scroll Speed", Range(-10,10)) = 1.8
        _BandAngle ("Band Angle", Range(0,360)) = 35

        _FresnelPower ("Fresnel Power", Range(0.1,8)) = 3.5
        _PulseSpeed ("Pulse Speed", Range(0,10)) = 2.0
        _PulseStrength ("Pulse Strength", Range(0,2)) = 0.3
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

            Blend SrcAlpha One
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
                half4 _EdgeColor;

                half _BaseAlpha;
                half _BandBrightness;
                half _EdgeBrightness;

                half _BandCount;
                half _BandWidth;
                half _BandSoftness;
                half _ScrollSpeed;
                half _BandAngle;

                half _FresnelPower;
                half _PulseSpeed;
                half _PulseStrength;
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

            half BandMask(half coord, half width, half softness)
            {
                half d = abs(coord - 0.5h);
                return 1.0h - smoothstep(width * 0.5h, width * 0.5h + softness, d);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half2 uv = IN.uv;

                half angleRad = radians(_BandAngle);
                half cs = cos(angleRad);
                half sn = sin(angleRad);

                // 旋转 UV，控制发光带方向
                half rotated = (uv.x - 0.5h) * cs - (uv.y - 0.5h) * sn;

                // 条带滚动
                half flow = frac(rotated * _BandCount - _Time.y * _ScrollSpeed);

                half band = BandMask(flow, _BandWidth, _BandSoftness);

                // 轻微呼吸感
                half pulse = 1.0h + sin(_Time.y * _PulseSpeed) * _PulseStrength;

                // Fresnel 边缘光
                half3 normalWS = normalize(IN.normalWS);
                half3 viewDirWS = normalize(IN.viewDirWS);
                half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _FresnelPower);

                half3 bandColor = _BandColor.rgb * band * _BandBrightness * pulse;
                half3 edgeColor = _EdgeColor.rgb * fresnel * _EdgeBrightness;

                half3 finalColor = bandColor + edgeColor;

                half finalAlpha = saturate(_BaseAlpha + band * 0.8h + fresnel * 0.35h);

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}