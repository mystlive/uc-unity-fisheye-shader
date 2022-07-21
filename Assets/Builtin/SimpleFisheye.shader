Shader "UC/FisheyeSimple"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        [KeywordEnum(DIAGONAL, CIRCULAR, VERTICAL180, HORIZONTAL180)] _Format ("Format", Float) = 0
        _Fov ("Field of View", Range(1,180)) = 180
        _CameraFov ("Camera Field of View", Range(1,179)) = 170
        _OutsideColor ("Outside Color", Color) = (1,0,0,0.5)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _FORMAT_DIAGONAL _FORMAT_CIRCULAR _FORMAT_VERTICAL180 _FORMAT_HORIZONTAL180

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

            float _Fov;
            float _CameraFov;
            fixed4 _OutsideColor;
            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;

                float coeff = _Fov / 180.0;
            #ifdef _FORMAT_DIAGONAL
                coeff /= sqrt(aspect * aspect + 1);
            #elif _FORMAT_HORIZONTAL180
                coeff /= aspect;
            #elif _FORMAT_CIRCULAR
                coeff /= min(aspect, 1);
            #endif

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.coord = (v.uv * 2.0 - 1.0) * coeff;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float dist = length(i.coord * float2(aspect, 1));
                float2 p = i.coord * tan(dist * UNITY_HALF_PI) / tan(radians(_CameraFov) * 0.5) / dist;
                
                fixed4 col = tex2D(_MainTex, (p + 1) * 0.5);
                col = lerp(col, _OutsideColor, step(_Fov/180.0, dist) * _OutsideColor.a);
                return col;
            }
            ENDCG
        }
    }
}
