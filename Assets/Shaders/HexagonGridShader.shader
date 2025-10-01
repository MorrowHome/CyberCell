Shader "Unlit/HexagonOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}   // SpriteRenderer 要求
        _FillColor ("Fill Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.001,0.2)) = 0.05
        _Scale ("Hexagon Scale", Range(0.1,2)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _FillColor;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex) * 2.0 - 1.0; // 把UV映射到 [-1,1]
                return o;
            }

            // 六边形SDF
            float hexSDF(float2 p)
            {
                p /= _Scale;
                p = abs(p);
                return max(p.x * 0.8660254 + p.y * 0.5, p.y) - 0.5;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = hexSDF(i.uv);

                if (d < -_OutlineWidth)
                {
                    return _FillColor;
                }
                else if (abs(d) < _OutlineWidth)
                {
                    return _OutlineColor;
                }
                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}
