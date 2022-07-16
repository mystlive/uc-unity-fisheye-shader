Shader "Hidden/CustomPassFisheye"
{
    HLSLINCLUDE

    #pragma vertex CustomVert

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    struct CustomVaryings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };
    //float _Fov;
    float _LensCoeff;
    float _InverseCHeightHalf;
    float _InversePHeightHalf;
    float4 _OutsideColor;
    TEXTURE2D_X(_PeripheralTex);

    CustomVaryings CustomVert(Attributes input)
    {
        CustomVaryings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID, UNITY_RAW_FAR_CLIP_VALUE);
        float2 texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        output.texcoord = (texcoord * 2.0 - 1.0) * _LensCoeff;
        return output;
    }

    float4 FullScreenPass(CustomVaryings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        float2 aspect = float2(_ScreenParams.x/_ScreenParams.y, 1);
        float2 coord = varyings.texcoord.xy;
        float dist = length(coord * aspect);
        float2 s = coord * tan(dist * HALF_PI) / dist;

        float2 t0 = _InverseCHeightHalf * s;
        float3 color0 = CustomPassLoadCameraColor((t0 + 1) * 0.5 * _ScreenParams.xy, 0);
        float2 t1 = _InversePHeightHalf * s;
        float3 color1 = SAMPLE_TEXTURE2D_X_LOD(_PeripheralTex, s_linear_clamp_sampler, (t1 + 1) * 0.5, 0).rgb;
                
        float3 color = lerp(color1, color0, step(abs(t0.x), 1.0) * step(abs(t0.y), 1.0));
        color = lerp(lerp(color, _OutsideColor, _OutsideColor.a), color, step(abs(t1.x), 1.0) * step(abs(t1.y), 1.0) * step(dist, 1.0));
        return float4(color.rgb, 1);
    }
    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Custom Pass 0"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
