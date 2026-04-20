// Assets/Shaders/AreaIndicator.shader
// 2026-04-20 장판 인디케이터 URP Unlit 셰이더
Shader "Custom/AreaIndicator"
{
    Properties
    {
        _MainTex    ("Texture", 2D)             = "white" {}
        _Color      ("Color", Color)            = (1, 0.3, 0.3, 0.4)
        _Alpha      ("Alpha", Range(0, 1))      = 1
        _Progress   ("Progress", Range(0, 1))   = 0
        _PulseSpeed ("Pulse Speed", Float)      = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue"      = "Transparent"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float  _Alpha;
                float  _Progress;
                float  _PulseSpeed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                float2 centered = IN.uv - 0.5;
                float angle = atan2(centered.y, centered.x);
                float normalizedAngle = frac(angle / (2.0 * 3.14159265) + 0.75);
                float radialMask = step(normalizedAngle, _Progress);

                float pulse = 1.0;
                if (_PulseSpeed > 0.0)
                    pulse = 0.7 + 0.3 * sin(_Time.y * _PulseSpeed);

                half4 col = tex * _Color;
                col.a = col.a * _Alpha * pulse * radialMask;

                return col;
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float  _Alpha;
                float  _Progress;
                float  _PulseSpeed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                float2 centered = IN.uv - 0.5;
                float angle = atan2(centered.y, centered.x);
                float normalizedAngle = frac(angle / (2.0 * 3.14159265) + 0.75);
                float radialMask = step(normalizedAngle, _Progress);

                float pulse = 1.0;
                if (_PulseSpeed > 0.0)
                    pulse = 0.7 + 0.3 * sin(_Time.y * _PulseSpeed);

                half4 col = tex * _Color;
                col.a = col.a * _Alpha * pulse * radialMask;

                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
