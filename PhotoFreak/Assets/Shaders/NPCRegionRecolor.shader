// NPCRegionRecolor.shader
// URP shader - 7 recolorable regions driven by combination detection on a single RGB mask.
//
// Mask color -> region mapping:
//   Black   (0,0,0)   -> untouched (original texture preserved)
//   Red     (1,0,0)   -> _PrimaryColor    (dress primary)
//   Green   (0,1,0)   -> _StripeColor     (stripes)
//   Blue    (0,0,1)   -> _SkinColor       (skin)
//   Magenta (1,0,1)   -> _HairColor       (hair)
//   Yellow  (1,1,0)   -> _ShoeColor       (shoes)
//   Cyan    (0,1,1)   -> _LipColor  (accessories)
//   White   (1,1,1)   -> _EyeColor        (eyes)
//
// Lighting features:
//   - Main directional light with soft shadow support
//   - Baked global illumination via spherical harmonics (prevents pure black)
//   - Additional realtime lights (point lights, spot lights, extra directionals)
//   - Light probes for dynamic ambient variation
//
// Recoloring uses luminance, so the original hue is fully replaced while
// painted shading, highlights, and detail are preserved as brightness.

Shader "NPC/RegionRecolorv1"
{
    Properties
    {
        _BaseMap         ("Base Texture",   2D)    = "white" {}
        _MaskMap         ("Region Mask",    2D)    = "black" {}

        _BaseColor       ("Global Tint",    Color) = (1,1,1,1)

        _PrimaryColor    ("Dress Primary",  Color) = (1,1,1,1)
        _StripeColor     ("Stripes",        Color) = (1,1,1,1)
        _SkinColor       ("Skin",           Color) = (1,1,1,1)
        _HairColor       ("Hair",           Color) = (1,1,1,1)
        _ShoeColor       ("Shoes",          Color) = (1,1,1,1)
        _LipColor        ("Lip",            Color) = (1,1,1,1)
        _EyeColor        ("Eyes",           Color) = (1,1,1,1)

        _RecolorStrength ("Recolor Strength", Range(0,1)) = 1.0
        _Smoothness      ("Smoothness",       Range(0,1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            // Main light shadows
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            // Additional lights (point, spot, extra directional)
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);  SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MaskMap);  SAMPLER(sampler_MaskMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _PrimaryColor;
                float4 _StripeColor;
                float4 _SkinColor;
                float4 _HairColor;
                float4 _ShoeColor;
                float4 _LipColor;
                float4 _EyeColor;
                float  _RecolorStrength;
                float  _Smoothness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float3 bakedGI     : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);

                // Sample baked ambient from spherical harmonics (light probes)
                // This replaces the zeroed-out bakedGI that was causing pure-black shadows.
                OUT.bakedGI     = SampleSH(OUT.normalWS);

                OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 mask    = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, IN.uv);

                // Combination detection - each weight is 1.0 ONLY for its exact color.
                // This is what prevents overlap between regions (a white pixel
                // triggers only isWhite, not red+green+blue at once).
                half isRed     = mask.r * (1.0 - mask.g) * (1.0 - mask.b);
                half isGreen   = mask.g * (1.0 - mask.r) * (1.0 - mask.b);
                half isBlue    = mask.b * (1.0 - mask.r) * (1.0 - mask.g);
                half isMagenta = mask.r * mask.b         * (1.0 - mask.g);
                half isYellow  = mask.r * mask.g         * (1.0 - mask.b);
                half isCyan    = mask.g * mask.b         * (1.0 - mask.r);
                half isWhite   = mask.r * mask.g         * mask.b;

                // Weighted sum - at most one weight is non-zero per pixel,
                // so this resolves to a single region's color.
                half3 regionColor =
                      _PrimaryColor.rgb   * isRed
                    + _StripeColor.rgb    * isGreen
                    + _SkinColor.rgb      * isBlue
                    + _HairColor.rgb      * isMagenta
                    + _ShoeColor.rgb      * isYellow
                    + _LipColor.rgb * isCyan
                    + _EyeColor.rgb       * isWhite;

                // Total mask coverage - 1.0 for any recolored region, 0.0 for black.
                half totalMask = saturate(isRed + isGreen + isBlue + isMagenta
                                        + isYellow + isCyan + isWhite);

                // Luminance-based recoloring, strip the original hue, keep brightness.
                half lum = (baseTex.r + baseTex.g + baseTex.b) / 3.0;

                half brightnessBoost = 10.0f; 
                half3 recolored = saturate(lum * brightnessBoost) * regionColor;

                // Blend between original and recolored based on mask coverage.
                half3 finalRGB  = lerp(baseTex.rgb, recolored, totalMask * _RecolorStrength);
                finalRGB       *= _BaseColor.rgb;

                // Build URP lighting inputs with proper baked GI.
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS      = IN.positionWS;
                lightingInput.normalWS        = normalize(IN.normalWS);
                lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                lightingInput.shadowCoord     = TransformWorldToShadowCoord(IN.positionWS);
                lightingInput.bakedGI         = IN.bakedGI;
                lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
                lightingInput.shadowMask      = half4(1, 1, 1, 1);

                SurfaceData surfaceInput = (SurfaceData)0;
                surfaceInput.albedo     = finalRGB;
                surfaceInput.alpha      = baseTex.a * _BaseColor.a;
                surfaceInput.smoothness = _Smoothness;
                surfaceInput.occlusion  = 1.0;

      
                return UniversalFragmentPBR(lightingInput, surfaceInput);
            }
            ENDHLSL
        }

        // Shadow caster pass so NPCs still cast shadows.
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex   ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
