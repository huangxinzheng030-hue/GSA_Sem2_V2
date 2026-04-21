Shader "Custom/URP/MatrixDigitalRain"
{
    Properties
    {
        _BackgroundColor ("Background Color", Color) = (0,0,0,1)
        _TrailColor ("Trail Color", Color) = (0.1, 1.0, 0.25, 1)
        _HeadColor ("Head Color", Color) = (0.85, 1.0, 0.9, 1)

        _Columns ("Columns", Range(8,120)) = 36
        _Rows ("Rows", Range(16,200)) = 72
        _Speed ("Speed", Range(0,5)) = 1.35
        _TrailLength ("Trail Length", Range(0.02,0.5)) = 0.18

        _DigitBrightness ("Digit Brightness", Range(0,5)) = 1.4
        _HeadBrightness ("Head Brightness", Range(0,8)) = 2.8
        _FlickerSpeed ("Flicker Speed", Range(0,20)) = 8.0

        _GlyphInset ("Glyph Inset", Range(0,0.3)) = 0.08
        _Softness ("Softness", Range(0.001,0.08)) = 0.02
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

            Cull Back
            ZWrite On
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BackgroundColor;
                half4 _TrailColor;
                half4 _HeadColor;

                half _Columns;
                half _Rows;
                half _Speed;
                half _TrailLength;

                half _DigitBrightness;
                half _HeadBrightness;
                half _FlickerSpeed;

                half _GlyphInset;
                half _Softness;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = posInputs.positionCS;
                OUT.uv = IN.uv;
                return OUT;
            }

            float Hash11(float p)
            {
                return frac(sin(p * 127.1) * 43758.5453123);
            }

            float Hash21(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            half RectMask(half2 p, half2 center, half2 halfSize, half blur)
            {
                half2 d = abs(p - center) - halfSize;
                half m = max(d.x, d.y);
                return 1.0h - smoothstep(0.0h, blur, m);
            }

            half DrawDigit(half2 p, int digit, half blur)
            {
                // 稍微做一点倾斜，更有电子字符味道
                p.x += (p.y - 0.5h) * 0.06h;

                half a = 0, b = 0, c = 0, d = 0, e = 0, f = 0, g = 0;

                switch (digit)
                {
                    case 0: a=1; b=1; c=1; d=1; e=1; f=1; break;
                    case 1: b=1; c=1; break;
                    case 2: a=1; b=1; d=1; e=1; g=1; break;
                    case 3: a=1; b=1; c=1; d=1; g=1; break;
                    case 4: b=1; c=1; f=1; g=1; break;
                    case 5: a=1; c=1; d=1; f=1; g=1; break;
                    case 6: a=1; c=1; d=1; e=1; f=1; g=1; break;
                    case 7: a=1; b=1; c=1; break;
                    case 8: a=1; b=1; c=1; d=1; e=1; f=1; g=1; break;
                    default: a=1; b=1; c=1; d=1; f=1; g=1; break; // 9
                }

                half h = 0.21h;
                half v = 0.18h;
                half t = 0.05h;

                half top    = RectMask(p, half2(0.50h, 0.87h), half2(h, t), blur) * a;
                half mid    = RectMask(p, half2(0.50h, 0.50h), half2(h, t), blur) * g;
                half bot    = RectMask(p, half2(0.50h, 0.13h), half2(h, t), blur) * d;

                half ul     = RectMask(p, half2(0.24h, 0.70h), half2(t, v), blur) * f;
                half ll     = RectMask(p, half2(0.24h, 0.30h), half2(t, v), blur) * e;
                half ur     = RectMask(p, half2(0.76h, 0.70h), half2(t, v), blur) * b;
                half lr     = RectMask(p, half2(0.76h, 0.30h), half2(t, v), blur) * c;

                return saturate(top + mid + bot + ul + ll + ur + lr);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half2 uv = IN.uv;

                // 可选：轻微扰动，让列流动更自然
                uv.x += sin(uv.y * 18.0h + _Time.y * 0.25h) * 0.002h;

                half cols = max(_Columns, 1.0h);
                half rows = max(_Rows, 1.0h);

                half2 gridUV = half2(uv.x * cols, uv.y * rows);
                half2 cell = floor(gridUV);
                half2 local = frac(gridUV);

                // 给字符留一点边距
                local = lerp(half2(0.5h, 0.5h), local, 1.0h - _GlyphInset);

                half colIndex = cell.x;
                half row01 = (cell.y + 0.5h) / rows;

                // 每一列各自不同速度和初相位
                half colSeed = Hash11(colIndex * 3.17h + 1.23h);
                half colSpeed = lerp(0.55h, 1.45h, colSeed);
                half head = frac(1.0h - (_Time.y * _Speed * colSpeed) - colSeed);

                // 计算当前格子距离“头部”的距离（带循环）
                half dist = head - row01;
                dist = dist < 0.0h ? dist + 1.0h : dist;

                // 头部以下形成拖尾
                half trailMask = 1.0h - smoothstep(0.0h, _TrailLength, dist);

                // 头部更亮
                half headMask = 1.0h - smoothstep(0.0h, 0.03h, dist);

                // 让部分格子闪烁 / 熄灭，不要太整齐
                half flickerTime = floor(_Time.y * _FlickerSpeed);
                half flicker = step(0.18h, Hash21(cell + flickerTime * 0.137h));

                // 每个格子一个数字
                int digit = (int)floor(Hash21(cell + floor(_Time.y * 7.0h) * 0.071h) * 10.0h);
                half glyph = DrawDigit(local, digit, _Softness);

                half body = glyph * trailMask * flicker;
                half lead = glyph * headMask;

                half3 color = _BackgroundColor.rgb;

                color += _TrailColor.rgb * body * _DigitBrightness;
                color += _HeadColor.rgb * lead * _HeadBrightness;

                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}