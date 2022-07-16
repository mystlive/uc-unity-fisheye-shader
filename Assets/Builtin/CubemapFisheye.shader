Shader "UC/CubemapFisheye"
{
    Properties
    {
		[NoScaleOffset] _MainTex ("Texture", CUBE) = "white" {}
        _Fov ("Field of View", Range(1,360)) = 180
        _LensPosition ("Lens Position", Vector) = (0,0,1,0)
        _OutsideColor ("Outside Color", Color) = (0,0,0,0.5)

		_Rotation ("Rotation", Vector) = (0.0, 0.0, 0.0, 1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
//      Cull Off ZWrite Off ZTest Always

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

			samplerCUBE _MainTex;
            float _Fov;
            fixed4 _OutsideColor;
            fixed4 _Rotation;
			float3 RotateVector(float4 quat, float3 vec)
			{
				// Rotate a vector using a quaternion.
				return vec + 2.0 * cross(cross(vec, quat.xyz) + quat.w * vec, quat.xyz);
			}
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
                o.coord = (v.uv * 2.0 - 1.0) * coeff * float2(aspect, 1);
                return o;
            }
            float3 cossin(float theta)
            {
                return float3(cos(theta), sin(theta), 1.0);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = length(i.coord);
                float3 phi = cossin(atan2(i.coord.y, i.coord.x));
                float3 theta = cossin(dist * radians(_Fov) * 0.5);

                // Convert phi/theta to a cartesian direction.
                // t.x = sin(theta) * cos(phi);
                // t.y = sin(theta) * sin(phi);
                // t.z = cos(theta);
                float4 t = theta.yyxz * phi.xyzz;

                // fixed4 col = texCUBE(_MainTex, t);
                float4 col = texCUBE(_MainTex, RotateVector(_Rotation, t));
                col = col < 0.0031308 ? 12.92 * col : 1.055 * pow(col, 1.0 / 2.4) - 0.055;

                col = lerp(col, _OutsideColor, step(_Fov/180.0, dist) * _OutsideColor.a);
                return col;
            }
            ENDCG
        }
    }
}