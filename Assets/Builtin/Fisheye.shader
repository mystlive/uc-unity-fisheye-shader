Shader "UC/Fisheye"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset] _PeripheralTex ("Peripheral Vision", 2D) = "white" {}
        _OutsideColor ("Outside Color", Color) = (0,0,0,0.5)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

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
                float4 vertex : SV_POSITION;
                float2 coord : TEXCOORD0;
            };

            float _LensCoeff;
            float _InverseCHeightHalf;
            float _InversePHeightHalf;
            fixed4 _OutsideColor;
            sampler2D _MainTex;
            sampler2D _PeripheralTex;

            v2f vert (appdata v)
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.coord = (v.uv * 2.0 - 1.0) * _LensCoeff;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float dist = length(i.coord * float2(aspect, 1));
                float2 s = i.coord * tan(dist * UNITY_HALF_PI) / dist;

                float2 t0 = _InverseCHeightHalf * s;
                fixed4 col0 = tex2D(_MainTex, (t0 + 1) * 0.5);

                float2 t1 = _InversePHeightHalf * s;
                fixed4 col1 = tex2D(_PeripheralTex, (t1 + 1) * 0.5);
                
                fixed4 col = lerp(col1, col0, step(abs(t0.x), 1.0) * step(abs(t0.y), 1.0));
                col = lerp(lerp(col, _OutsideColor, _OutsideColor.a), col, step(abs(t1.x), 1.0) * step(abs(t1.y), 1.0) * step(dist, 1.0));
                return col;
            }
            ENDCG
        }
    }
}