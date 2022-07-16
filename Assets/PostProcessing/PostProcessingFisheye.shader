Shader "Hidden/PostProcessingFisheye"
{
    Properties
    {
        // This property is necessary to make the CommandBuffer.Blit bind the source texture to _MainTex
        _MainTex("Main Texture", 2DArray) = "grey" {}
        _OutsideColor ("Outside Color", Color) = (1,0,0,0.5)
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    // List of properties to control your post process effect
    float _LensCoeff;
    float _InverseCHeightHalf;
    float _InversePHeightHalf;
    float4 _OutsideColor;
    TEXTURE2D_X(_MainTex);
    TEXTURE2D_X(_WideView);

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        // positionCS.xy/_ScreenSize.xy が [0,1] にマッピングされてるっぽい
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        float2 texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        output.texcoord = (texcoord * 2.0 - 1.0) * _LensCoeff;
        return output;
    }

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 aspect = float2(_ScreenParams.x/_ScreenParams.y, 1);
        float dist = length(input.texcoord * aspect);
        float2 s = input.texcoord * tan(dist * HALF_PI) / dist;
        float2 t0 = _InverseCHeightHalf * s;
        float4 cColor = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, (t0 + 1) * 0.5);
        return lerp(float4(0,0,0,0), cColor, step(abs(t0.x), 1.0) * step(abs(t0.y), 1.0) * step(dist, 1.0));
    }

    Varyings VertPassthru(Attributes input)
    {
        Varyings output;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);

        return output;
    }

    float4 PostProcessPassthru(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        return LOAD_TEXTURE2D_X(_MainTex, positionSS);
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Fisheye"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
        Pass
        {
            Name "FisheyePassthru"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment PostProcessPassthru
                #pragma vertex VertPassthru
            ENDHLSL
        }
    }
    Fallback Off
}
