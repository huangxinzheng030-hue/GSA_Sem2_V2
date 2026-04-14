Shader "Skybox/MatrixDigitalRain"
{
    Properties
    {
        _BackgroundColor ("Background Color", Color) = (0,0,0,1)
        _TrailColor ("Trail Color", Color) = (0.05, 1.0, 0.25, 1)
        _HeadColor ("Head Color", Color) = (0.85, 1.0, 0.9, 1)

        _Columns ("Columns", Range(8,120)) = 48
        _Rows ("Rows", Range(16,220)) = 110
        _Speed ("Speed", Range(0,5)) = 1.25
        _TrailLength ("Trail Length", Range(0.01,0.5)) = 0.14

        _DigitBrightness ("Digit Brightness", Range(0,5)) = 1.3
        _HeadBrightness ("Head Brightness", Range(0,10)) = 3.2
        _FlickerSpeed ("Flicker Speed", Range(0,20)) = 8.0

        _GlyphInset ("Glyph Inset", Range(0,0.35)) = 0.10
        _Softness ("Softness", Range(0.001,0.08)) = 0.02

        _HorizontalWarp ("Horizontal Warp", Range(0,0.02)) = 0.003
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

            half4 frag (Varyings IN) : SV_Target
            {
                half3 dir = normalize(IN.dirOS);
                half2 uv = DirToLatLongUV(dir);

                uv.x += sin(uv.y * 20.0h + _Time.y * 0.22h) * _HorizontalWarp;
                uv.x = frac(uv.x);

                half cols = max(_Columns, 1.0h);
                half rows = max(_Rows, 1.0h);

                half2 gridUV = half2(uv.x * cols, uv.y * rows);
                half2 cell = floor(gridUV);
                half2 local = frac(gridUV);

                local = lerp(half2(0.5h, 0.5h), local, 1.0h - _GlyphInset);

                half colIndex = cell.x;
                half row01 = (cell.y + 0.5h) / rows;

                half colSeed = Hash11(colIndex * 3.17h + 1.23h);
                half colSpeed = lerp(0.55h, 1.45h, colSeed);
                half head = frac(_Time.y * _Speed * colSpeed + colSeed);

                half dist = head - row01;
                if (dist < 0.0h) dist += 1.0h;

                half trailMask = 1.0h - smoothstep(0.0h, _TrailLength, dist);
                half headMask  = 1.0h - smoothstep(0.0h, 0.025h, dist);

                half flickerTime = floor(_Time.y * _FlickerSpeed);
                half flicker = step(0.20h, Hash21(cell + flickerTime * 0.173h));

                int digit = (int)floor(Hash21(cell + floor(_Time.y * 7.0h) * 0.071h) * 10.0h);
                half glyph = DrawDigit(local, digit, _Softness);

                half trail = glyph * trailMask * flicker;
                half lead  = glyph * headMask;

                half3 color = _BackgroundColor.rgb;
                color += _TrailColor.rgb * trail * _DigitBrightness;
                color += _HeadColor.rgb  * lead  * _HeadBrightness;

                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}