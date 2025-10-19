Shader "Custom/SquareOutlineURP_Sprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Range(0, 0.5)) = 0.1
        _Smoothness ("Edge Softness", Range(0, 0.05)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalRenderPipeline"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Pass
        {
            Name "ForwardLit"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainColor;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _Smoothness;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // 采样Sprite原图
                float4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _MainColor;

                // 计算到边缘的距离
                float dist = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));

                // 使用smoothstep做平滑过渡
                float edge = smoothstep(_OutlineThickness, _OutlineThickness + _Smoothness, dist);

                // 内部与描边混合
                float4 color = lerp(_OutlineColor, baseCol, edge);

                // 透明度取主纹理的alpha
                color.a *= baseCol.a;

                return color;
            }
            ENDHLSL
        }
    }
}
