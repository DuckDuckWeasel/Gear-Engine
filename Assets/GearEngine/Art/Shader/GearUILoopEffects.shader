Shader "Gear/UI/LoopEffects"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _EffectMode ("Effect Mode", Float) = 0
        _Speed ("Animation Speed", Float) = 1
        _Strength ("Strength", Range(0, 1)) = 0.1
        _Frequency ("Frequency", Range(0.01, 80)) = 4
        _PixelSize ("Pixel Size", Range(4, 512)) = 64
        _Angle ("Angle", Range(0, 6.283185)) = 0
        _Threshold ("Threshold", Range(0, 1)) = 0.5
        _Softness ("Softness", Range(0.001, 1)) = 0.1
        _Direction ("Direction", Vector) = (1, 0, 0, 0)
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        [HDR] _ColorA ("Effect Color A", Color) = (1, 1, 1, 1)
        [HDR] _ColorB ("Effect Color B", Color) = (0, 0.5, 1, 1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "LoopEffects"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _EffectMode;
            float _Speed;
            float _Strength;
            float _Frequency;
            float _PixelSize;
            float _Angle;
            float _Threshold;
            float _Softness;
            float4 _Direction;
            float4 _Center;
            fixed4 _ColorA;
            fixed4 _ColorB;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = input.texcoord;
                output.color = input.color * _Color;
                output.worldPosition = input.vertex;
                return output;
            }

            float Hash21(float2 value)
            {
                value = frac(value * float2(123.34, 345.45));
                value += dot(value, value + 34.345);
                return frac(value.x * value.y);
            }

            float2 Rotate(float2 value, float angle)
            {
                float sine = sin(angle);
                float cosine = cos(angle);
                return mul(float2x2(cosine, -sine, sine, cosine), value);
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float time = _Time.y * _Speed;
                float2 uv = input.texcoord;

                if (_EffectMode > 0.5 && _EffectMode < 1.5)
                {
                    uv = frac(uv + _Direction.xy * time);
                }
                else if (_EffectMode > 1.5 && _EffectMode < 2.5)
                {
                    uv += sin((uv.yx * _Frequency + time) * 6.283185) * _Strength;
                }
                else if (_EffectMode > 4.5 && _EffectMode < 5.5)
                {
                    float band = floor(uv.y * _Frequency);
                    float glitch = (Hash21(float2(band, floor(time * 12))) - 0.5) * _Strength;
                    uv.x += glitch * step(0.7, Hash21(float2(band, floor(time * 3))));
                }
                else if (_EffectMode > 6.5 && _EffectMode < 7.5)
                {
                    float2 offset = uv - _Center.xy;
                    float radius = length(offset);
                    float angle = atan2(offset.y, offset.x) + (_Strength * 6.283185 * sin(time));
                    uv = _Center.xy + float2(cos(angle), sin(angle)) * radius;
                }
                else if (_EffectMode > 8.5 && _EffectMode < 9.5)
                {
                    float pixelSize = max(4.0, _PixelSize + sin(time * 6.283185 * _Frequency) * _Strength * 128.0);
                    uv = (floor(uv * pixelSize) + 0.5) / pixelSize;
                }
                else if (_EffectMode > 10.5 && _EffectMode < 11.5)
                {
                    float zoom = 1.0 + sin(time * 6.283185 * _Frequency) * _Strength;
                    uv = _Center.xy + (uv - _Center.xy) / zoom;
                }
                else if (_EffectMode > 11.5 && _EffectMode < 12.5)
                {
                    float2 frame = floor(time * _Frequency);
                    uv += (float2(Hash21(frame), Hash21(frame + 17.0)) - 0.5) * _Strength;
                }
                else if (_EffectMode > 12.5 && _EffectMode < 13.5)
                {
                    float2 offset = uv - _Center.xy;
                    float radius = dot(offset, offset);
                    uv = _Center.xy + offset * (1.0 + radius * _Strength * sin(time));
                }
                else if (_EffectMode > 17.5 && _EffectMode < 18.5)
                {
                    float heat = sin((uv.y * _Frequency + time) * 6.283185);
                    uv.x += heat * _Strength;
                    uv.y += sin((uv.x * _Frequency * 0.5 + time * 0.7) * 6.283185) * _Strength * 0.35;
                }

                fixed4 color = tex2D(_MainTex, uv) + _TextureSampleAdd;
                color *= input.color;

                if (_EffectMode > 13.5 && _EffectMode < 14.5)
                {
                    float2 shift = _Direction.xy * _Strength * sin(time);
                    color.r = tex2D(_MainTex, uv + shift).r * input.color.r * _Color.r;
                    color.b = tex2D(_MainTex, uv - shift).b * input.color.b * _Color.b;
                }

                if (_EffectMode > 16.5 && _EffectMode < 17.5)
                {
                    float2 trailOffset = _Direction.xy * _Strength;
                    fixed4 trailA = tex2D(_MainTex, uv - trailOffset) * input.color;
                    fixed4 trailB = tex2D(_MainTex, uv - trailOffset * 2.0) * input.color;
                    color.rgb = lerp(trailB.rgb, color.rgb, 0.65) + trailA.rgb * _ColorA.a * 0.25;
                    color.a = max(color.a, max(trailA.a, trailB.a) * _ColorA.a);
                }

                if (_EffectMode > 2.5 && _EffectMode < 3.5)
                {
                    float2 axis = float2(cos(_Angle), sin(_Angle));
                    float position = frac(dot(uv, axis) * _Frequency + time) - 0.5;
                    float shine = 1.0 - smoothstep(_Strength, _Strength + _Softness, abs(position));
                    color.rgb += _ColorA.rgb * shine * _ColorA.a * color.a;
                }
                else if (_EffectMode > 3.5 && _EffectMode < 4.5)
                {
                    float scanline = 0.5 + 0.5 * sin((uv.y * _Frequency + time) * 6.283185);
                    color.rgb = lerp(color.rgb, _ColorA.rgb * color.a, scanline * _Strength);
                    color.a *= lerp(1.0 - _Strength, 1.0, scanline);
                }
                else if (_EffectMode > 5.5 && _EffectMode < 6.5)
                {
                    float radialDistance = distance(uv, _Center.xy);
                    float ring = 1.0 - smoothstep(_Strength, _Strength + _Softness,
                        abs(radialDistance - frac(time) * (0.5 + _Strength)));
                    color.rgb += _ColorA.rgb * ring * _ColorA.a * color.a;
                }
                else if (_EffectMode > 7.5 && _EffectMode < 8.5)
                {
                    float edge = min(min(uv.x, uv.y), min(1.0 - uv.x, 1.0 - uv.y));
                    float pulse = 0.5 + 0.5 * sin(time * 6.283185 * _Frequency);
                    float border = 1.0 - smoothstep(_Strength, _Strength + _Softness, edge);
                    color.rgb += _ColorA.rgb * border * pulse * _ColorA.a * color.a;
                }
                else if (_EffectMode > 9.5 && _EffectMode < 10.5)
                {
                    float noise = Hash21(floor(uv * _Frequency) + floor(time * 2));
                    float dissolve = smoothstep(_Threshold, _Threshold + _Softness, noise);
                    float edge = smoothstep(_Threshold - _Softness, _Threshold, noise) - dissolve;
                    color.rgb = lerp(color.rgb, _ColorA.rgb * color.a, edge * _Strength);
                    color.a *= dissolve;
                }
                else if (_EffectMode > 14.5 && _EffectMode < 15.5)
                {
                    float aurora = 0.5 + 0.5 * sin((uv.x + sin(uv.y * _Frequency + time) * _Strength) * _Frequency + time);
                    fixed3 auroraColor = lerp(_ColorA.rgb, _ColorB.rgb, aurora);
                    color.rgb += auroraColor * color.a * _Strength;
                }
                else if (_EffectMode > 15.5 && _EffectMode < 16.5)
                {
                    float frame = floor(time * _Frequency);
                    float flicker = step(_Strength, Hash21(float2(frame, 19.0)));
                    color.rgb *= lerp(_ColorA.rgb, 1.0, flicker);
                    color.a *= lerp(_ColorA.a, 1.0, flicker);
                }

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
