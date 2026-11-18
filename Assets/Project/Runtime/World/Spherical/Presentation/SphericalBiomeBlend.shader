Shader "Pokemon/World/SphericalBiomeBlend"
{
    Properties
    {
        _PlainsColor ("Plains Tint", Color) = (0.72, 0.68, 0.36, 1)
        _ForestColor ("Forest Tint", Color) = (0.12, 0.42, 0.20, 1)
        _DesertColor ("Desert Tint", Color) = (0.86, 0.56, 0.25, 1)
        _MountainsColor ("Mountains Tint", Color) = (0.62, 0.63, 0.66, 1)
        _WaterColor ("Ocean", Color) = (0.04, 0.28, 0.62, 1)
        _PlainsTex ("Plains Detail", 2D) = "white" {}
        _ForestTex ("Forest Detail", 2D) = "white" {}
        _DesertTex ("Desert Detail", 2D) = "white" {}
        _MountainsTex ("Mountains Detail", 2D) = "white" {}
        _TextureWorldSize ("Texture World Size (m)", Float) = 18
        _GroundSmoothness ("Ground Smoothness", Range(0,1)) = 0.12
        _WaterSmoothness ("Water Smoothness", Range(0,1)) = 0.82
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 250
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_PlainsTex); SAMPLER(sampler_PlainsTex);
            TEXTURE2D(_ForestTex); SAMPLER(sampler_ForestTex);
            TEXTURE2D(_DesertTex); SAMPLER(sampler_DesertTex);
            TEXTURE2D(_MountainsTex); SAMPLER(sampler_MountainsTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _PlainsColor;
                half4 _ForestColor;
                half4 _DesertColor;
                half4 _MountainsColor;
                half4 _WaterColor;
                half _TextureWorldSize;
                half _GroundSmoothness;
                half _WaterSmoothness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 waterData : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half4 biomeWeights : TEXCOORD2;
                half waterWeight : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                half fogFactor : TEXCOORD5;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                output.biomeWeights = max(input.color, 0.0);
                output.waterWeight = saturate(input.waterData.x);
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                return output;
            }

            half3 SampleTriplanar(TEXTURE2D_PARAM(tex, samp), float3 positionWS, half3 normalWS)
            {
                half3 blend = pow(abs(normalWS), 4.0h);
                blend /= max(blend.x + blend.y + blend.z, 0.0001h);
                float scale = max((float)_TextureWorldSize, 0.1);
                float2 uvX = positionWS.zy / scale;
                float2 uvY = positionWS.xz / scale;
                float2 uvZ = positionWS.xy / scale;
                half3 x = SAMPLE_TEXTURE2D(tex, samp, uvX).rgb;
                half3 y = SAMPLE_TEXTURE2D(tex, samp, uvY).rgb;
                half3 z = SAMPLE_TEXTURE2D(tex, samp, uvZ).rgb;
                return x * blend.x + y * blend.y + z * blend.z;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 weights = input.biomeWeights;
                half weightTotal = weights.r + weights.g + weights.b + weights.a;
                half3 normalWS = NormalizeNormalPerPixel(input.normalWS);
                half3 plainsDetail = SampleTriplanar(TEXTURE2D_ARGS(_PlainsTex, sampler_PlainsTex), input.positionWS, normalWS);
                half3 forestDetail = SampleTriplanar(TEXTURE2D_ARGS(_ForestTex, sampler_ForestTex), input.positionWS, normalWS);
                half3 desertDetail = SampleTriplanar(TEXTURE2D_ARGS(_DesertTex, sampler_DesertTex), input.positionWS, normalWS);
                half3 mountainDetail = SampleTriplanar(TEXTURE2D_ARGS(_MountainsTex, sampler_MountainsTex), input.positionWS, normalWS);

                half3 plains = _PlainsColor.rgb * plainsDetail;
                half3 forest = _ForestColor.rgb * forestDetail;
                half3 desert = _DesertColor.rgb * desertDetail;
                half3 mountains = _MountainsColor.rgb * mountainDetail;
                half3 groundColor = weightTotal > 0.0001h
                    ? (plains * weights.r + forest * weights.g + desert * weights.b + mountains * weights.a) / weightTotal
                    : mountains;
                half3 albedo = lerp(groundColor, _WaterColor.rgb, input.waterWeight);
                half smoothness = lerp(_GroundSmoothness, _WaterSmoothness, input.waterWeight);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = input.shadowCoord;
                inputData.fogCoord = input.fogFactor;
                inputData.vertexLighting = half3(0, 0, 0);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionHCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = 0;
                surfaceData.specular = half3(0.04, 0.04, 0.04);
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.occlusion = 1;
                surfaceData.emission = half3(0, 0, 0);
                surfaceData.alpha = 1;
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, input.fogFactor);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
