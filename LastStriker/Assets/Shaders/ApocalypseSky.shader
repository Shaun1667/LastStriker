Shader "LastStriker/ApocalypseSky"
{
    Properties
    {
        _ZenithColor ("Zenith Color", Color) = (0.05, 0.06, 0.08, 1)
        _MidColor ("Mid Sky Color", Color) = (0.20, 0.17, 0.15, 1)
        _HorizonColor ("Horizon Haze Color", Color) = (0.52, 0.31, 0.18, 1)
        _GroundColor ("Below Horizon Color", Color) = (0.12, 0.09, 0.07, 1)
        _GradientCurve ("Gradient Curve", Range(0.1, 2)) = 0.5
        _SunDir ("Sun Direction (towards sun)", Vector) = (0.6, 0.35, 0.7, 0)
        _SunColor ("Sun Glow Color", Color) = (1.0, 0.42, 0.16, 1)
        _SunSize ("Sun Disc Size", Range(0.0005, 0.05)) = 0.006
        _SunGlow ("Sun Glow Tightness", Range(1, 64)) = 10
        _SunIntensity ("Sun Intensity", Range(0, 4)) = 1.2
        _CloudDark ("Smoke Cloud Dark", Color) = (0.09, 0.08, 0.08, 1)
        _CloudLit ("Smoke Cloud Lit", Color) = (0.55, 0.30, 0.17, 1)
        _CloudCoverage ("Cloud Coverage", Range(0, 1)) = 0.65
        _CloudScale ("Cloud Scale", Range(0.3, 8)) = 2.2
        _CloudSpeed ("Cloud Speed", Range(0, 0.1)) = 0.006
        _CloudOpacity ("Cloud Opacity", Range(0, 1)) = 0.85
        _Exposure ("Exposure", Range(0, 3)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _ZenithColor, _MidColor, _HorizonColor, _GroundColor;
            float _GradientCurve;
            float4 _SunDir, _SunColor;
            float _SunSize, _SunGlow, _SunIntensity;
            float4 _CloudDark, _CloudLit;
            float _CloudCoverage, _CloudScale, _CloudSpeed, _CloudOpacity, _Exposure;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                for (int i = 0; i < 5; i++)
                {
                    v += a * vnoise(p);
                    p = p * 2.03 + float2(17.1, 9.2);
                    a *= 0.5;
                }
                return v;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float h = dir.y;
                float3 sunDir = normalize(_SunDir.xyz);
                float sd = saturate(dot(dir, sunDir));

                // base gradient
                float up = saturate(h);
                float3 sky = lerp(_HorizonColor.rgb, _MidColor.rgb, saturate(pow(up, _GradientCurve) * 1.7));
                sky = lerp(sky, _ZenithColor.rgb, saturate(pow(up, _GradientCurve * 2.2)));
                float below = saturate(-h * 6.0);
                float3 col = lerp(sky, _GroundColor.rgb, below);

                // sun glow, strongest toward the horizon haze
                float glow = pow(sd, _SunGlow);
                float disc = smoothstep(1.0 - _SunSize, 1.0 - _SunSize * 0.55, sd);
                float sunMask = smoothstep(-0.08, 0.04, h);

                // smoke clouds
                float cloud = 0.0;
                float3 cloudCol = _CloudDark.rgb;
                if (h > -0.02)
                {
                    float2 p = dir.xz / (max(h, 0.0) + 0.22) * _CloudScale;
                    p += float2(_Time.y * _CloudSpeed, _Time.y * _CloudSpeed * 0.6);
                    float n = fbm(p);
                    float n2 = fbm(p * 2.3 + 11.7);
                    n = n * 0.7 + n2 * 0.3;
                    cloud = smoothstep(1.0 - _CloudCoverage, 1.0 - _CloudCoverage + 0.30, n);
                    cloud *= smoothstep(-0.02, 0.18, h);
                    float lit = saturate(pow(sd, 3.0) * 1.4 + (1.0 - saturate(h * 2.5)) * 0.35);
                    cloudCol = lerp(_CloudDark.rgb, _CloudLit.rgb, lit);
                    cloudCol = lerp(cloudCol, cloudCol * (0.6 + 0.8 * n2), 0.5);
                }
                col = lerp(col, cloudCol, cloud * _CloudOpacity);

                // sun is veiled by the smoke
                float veil = 1.0 - cloud * _CloudOpacity * 0.85;
                col += _SunColor.rgb * (glow * 0.55 + disc * 1.6) * _SunIntensity * sunMask * veil;

                return float4(col * _Exposure, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
