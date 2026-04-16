Shader "Skybox/MatrixDigitalRainDense"
{
    Properties
    {
        _BackgroundColor ("Background Color", Color) = (0,0,0,1)
        _TrailColor ("Trail Color", Color) = (0.05, 1.0, 0.25, 1)
        _HeadColor ("Head Color", Color) = (0.85, 1.0, 0.9, 1)

        _Columns ("Columns", Range(16,240)) = 96
        _Rows ("Rows", Range(32,420)) = 220
        _Speed ("Speed", Range(0,5)) = 1.25
        _TrailLength ("Trail Length", Range(0.01,0.5)) = 0.10

        _DigitBrightness ("Digit Brightness", Range(0,5)) = 1.1
        _HeadBrightness ("Head Brightness", Range(0,10)) = 2.6
        _FlickerSpeed ("Flicker Speed", Range(0,20)) = 9.0

        _GlyphInset ("Glyph Inset", Range(0,0.2)) = 0.04
        _Softness ("Softness", Range(0.001,0.05)) = 0.010

        _HorizontalWarp ("Horizontal Warp", Range(0,0.02)) = 0.002

        _Layer2Strength ("Layer2 Strength", Range(0,2)) = 0.75
        _Layer2Scale ("Layer2 Scale", Range(1,4)) = 1.65
        _Layer2Speed ("Layer2 Speed", Range(0,5)) = 1.6
        _Layer2TrailLength ("Layer2 Trail Length", Range(0.01,0.5)) = 0.07
    }

    SubShader
    {
        Tags
        {
            "Queue"="Background"
            "RenderType"="Background"
            "PreviewType"="Skybox"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "SkyboxPass"
            Cull Off
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define PI 3.14159265359

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 dirOS       : TEXCOORD0;
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
                half _HorizontalWarp;

                half _Layer2Strength;
                half _Layer2Scale;
                half _Layer2Speed;
                half _Layer2TrailLength;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.dirOS = IN.positionOS.xyz;
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
                p.x += (p.y - 0.5h) * 0.05h;

                half a=0, b=0, c=0, d=0, e=0, f=0, g=0;

                if (digit == 0)      { a=1; b=1; c=1; d=1; e=1; f=1; }
                else if (digit == 1) { b=1; c=1; }
                else if (digit == 2) { a=1; b=1; d=1; e=1; g=1; }
                else if (digit == 3) { a=1; b=1; c=1; d=1; g=1; }
                else if (digit == 4) { b=1; c=1; f=1; g=1; }
                else if (digit == 5) { a=1; c=1; d=1; f=1; g=1; }
                else if (digit == 6) { a=1; c=1; d=1; e=1; f=1; g=1; }
                else if (digit == 7) { a=1; b=1; c=1; }
                else if (digit == 8) { a=1; b=1; c=1; d=1; e=1; f=1; g=1; }
                else                 { a=1; b=1; c=1; d=1; f=1; g=1; }

                half h = 0.21h;
                half v = 0.18h;
                half t = 0.05h;

                half top = RectMask(p, half2(0.50h, 0.87h), half2(h, t), blur) * a;
                half mid = RectMask(p, half2(0.50h, 0.50h), half2(h, t), blur) * g;
                half bot = RectMask(p, half2(0.50h, 0.13h), half2(h, t), blur) * d;

                half ul  = RectMask(p, half2(0.24h, 0.70h), half2(t, v), blur) * f;
                half ll  = RectMask(p, half2(0.24h, 0.30h), half2(t, v), blur) * e;
                half ur  = RectMask(p, half2(0.76h, 0.70h), half2(t, v), blur) * b;
                half lr  = RectMask(p, half2(0.76h, 0.30h), half2(t, v), blur) * c;

                return saturate(top + mid + bot + ul + ll + ur + lr);
            }

            half2 DirToLatLongUV(float3 dir)
            {
                dir = normalize(dir);

                half2 uv;
                uv.x = atan2(dir.x, dir.z) / (2.0h * PI) + 0.5h;
                uv.y = 0.5h - asin(clamp(dir.y, -1.0h, 1.0h)) / PI;
                return uv;
            }

            half ComputeRainLayer(
                half2 uv,
                half cols,
                half rows,
                half speed,
                half trailLength,
                half glyphInset,
                half softness,
                half timeOffset)
            {
                half2 gridUV = half2(uv.x * cols, uv.y * rows);
                half2 cell = floor(gridUV);
                half2 local = frac(gridUV);

                local = lerp(half2(0.5h, 0.5h), local, 1.0h - glyphInset);

                half colIndex = cell.x;
                half row01 = (cell.y + 0.5h) / rows;

                half colSeed = Hash11(colIndex * 3.17h + 1.23h + timeOffset * 5.13h);
                half colSpeed = lerp(0.55h, 1.55h, colSeed);
                half head = frac(_Time.y * speed * colSpeed + colSeed + timeOffset);

                half dist = head - row01;
                if (dist < 0.0h) dist += 1.0h;

                half trailMask = 1.0h - smoothstep(0.0h, trailLength, dist);
                half headMask  = 1.0h - smoothstep(0.0h, 0.02h, dist);

                half flickerTime = floor(_Time.y * _FlickerSpeed + timeOffset * 11.0h);
                half flicker = step(0.14h, Hash21(cell + flickerTime * 0.173h + timeOffset * 7.0h));

                int digit = (int)floor(Hash21(cell + floor(_Time.y * 7.0h + timeOffset * 13.0h) * 0.071h) * 10.0h);
                half glyph = DrawDigit(local, digit, softness);

                half trail = glyph * trailMask * flicker;
                half lead  = glyph * headMask;

                return trail * 0.75h + lead * 1.25h;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half3 dir = normalize(IN.dirOS);
                half2 uv = DirToLatLongUV(dir);

                uv.x += sin(uv.y * 20.0h + _Time.y * 0.22h) * _HorizontalWarp;
                uv.x = frac(uv.x);

                half layer1 = ComputeRainLayer(
                    uv,
                    max(_Columns, 1.0h),
                    max(_Rows, 1.0h),
                    _Speed,
                    _TrailLength,
                    _GlyphInset,
                    _Softness,
                    0.0h
                );

                half2 uv2 = uv;
                uv2.x = frac(uv2.x * 1.07h + 0.137h);
                uv2.y = frac(uv2.y * 1.03h + 0.061h);

                half layer2 = ComputeRainLayer(
                    uv2,
                    max(_Columns * _Layer2Scale, 1.0h),
                    max(_Rows * _Layer2Scale, 1.0h),
                    _Layer2Speed,
                    _Layer2TrailLength,
                    max(_GlyphInset * 0.65h, 0.0h),
                    max(_Softness * 0.8h, 0.001h),
                    0.37h
                );

                half3 color = _BackgroundColor.rgb;
                color += _TrailColor.rgb * layer1 * _DigitBrightness;
                color += _HeadColor.rgb  * layer1 * _HeadBrightness * 0.35h;

                color += _TrailColor.rgb * layer2 * _DigitBrightness * _Layer2Strength * 0.65h;
                color += _HeadColor.rgb  * layer2 * _HeadBrightness * _Layer2Strength * 0.18h;

                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}