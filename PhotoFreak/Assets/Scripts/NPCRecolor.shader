// NPC/RegionRecolor — fixed lighting, original recolour logic preserved exactly.
//
// Mask colour → region mapping (unchanged from original):
//   Black   (0,0,0)  → untouched
//   Red     (1,0,0)  → _PrimaryColor  (dress primary)
//   Green   (0,1,0)  → _StripeColor   (stripes)
//   Blue    (0,0,1)  → _SkinColor     (skin)
//   Magenta (1,0,1)  → _HairColor     (hair)
//   Yellow  (1,1,0)  → _ShoeColor     (shoes)
//   Cyan    (0,1,1)  → _LipColor      (accessories)
//   White   (1,1,1)  → _EyeColor      (eyes)
//
// Recolour method (unchanged): luminance-based — original hue is replaced,
// painted shading / highlights / detail survive as brightness.
//
// Lighting fixes over the original:
//   • GI via SampleSH(normalWS) — correct for dynamic objects (NPCs are never
//     baked into a lightmap, so SAMPLE_GI / DECLARE_LIGHTMAP_OR_SH would read
//     garbage TEXCOORD1 data and return a black bakedGI, causing the flat-shading
//     bug that appeared when a MaterialPropertyBlock palette was applied)
//   • Per-vertex additional-light accumulation (VertexLighting) for point/spot lights
//   • Full shadow keyword set including _MAIN_LIGHT_SHADOWS_SCREEN and
//     _SCREEN_SPACE_AMBIENT_OCCLUSION so SSAO and screen-space shadows work
//   • Fog mixing added (MixFog)
//   • Shadow caster rewritten without LitInput.hlsl so the CBUFFER matches
//     the ForwardLit pass — restores SRP Batcher eligibility
//   • DepthOnly pass added (fixes SSAO / DoF holes where NPCs were)

Shader "NPC/RegionRecolorv2"
{
    Properties
    {
        _BaseMap         ("Base Texture",     2D)          = "white" {}
        _MaskMap         ("Region Mask",      2D)          = "black" {}

        _BaseColor       ("Global Tint",      Color)       = (1,1,1,1)

        _PrimaryColor    ("Dress Primary",    Color)       = (1,1,1,1)
        _StripeColor     ("Stripes",          Color)       = (1,1,1,1)
        _SkinColor       ("Skin",             Color)       = (1,1,1,1)
        _HairColor       ("Hair",             Color)       = (1,1,1,1)
        _ShoeColor       ("Shoes",            Color)       = (1,1,1,1)
        _LipColor        ("Lip",              Color)       = (1,1,1,1)
        _EyeColor        ("Eyes",             Color)       = (1,1,1,1)

        _RecolorStrength ("Recolor Strength", Range(0,1))  = 1.0
        _Smoothness      ("Smoothness",       Range(0,1))  = 0.2
        _Midtone         ("Midtone Anchor",   Range(0.01, 0.99)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Opaque"
            "RenderPipeline"  = "UniversalPipeline"
            "Queue"           = "Geometry"
            "IgnoreProjector" = "True"
        }
        LOD 300

        // ─────────────────────────────────────────────────────────────────────
        // Pass 1 — Forward Lit
        // ─────────────────────────────────────────────────────────────────────
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend  One Zero
            ZWrite On
            Cull   Back

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            // ── Shadow keywords ───────────────────────────────────────────
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            // ── Additional lights ─────────────────────────────────────────
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            // ── Post-processing / GI ──────────────────────────────────────
            #pragma multi_compile_fragment _ _SCREEN_SPACE_AMBIENT_OCCLUSION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _CLUSTERED_RENDERING

            // ── Global ────────────────────────────────────────────────────
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);  SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MaskMap);  SAMPLER(sampler_MaskMap);

            // ── Per-material constant buffer ──────────────────────────────
            // Must be identical across ALL passes for SRP Batcher to work.
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
                float _Midtone; 
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
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                // Spherical harmonics sampled per-vertex — correct for dynamic
                // objects (NPCs are never baked, so we must use SH probes, not
                // lightmap UVs; reading TEXCOORD1 as a lightmap UV would return
                // garbage and produce the flat-ambient bug seen with MPB palettes).
                float3 bakedGI     : TEXCOORD3;
                // x = fog factor,  yzw = accumulated per-vertex additional lights
                half4  fogFactorAndVertexLight : TEXCOORD4;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                float4 shadowCoord : TEXCOORD5;
            #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs posInputs  = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   normInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS   = normInputs.normalWS;
                OUT.uv = IN.uv;

                // Sample spherical harmonics for ambient GI.  NPCs are dynamic
                // objects — they are never baked into a lightmap — so SH probes
                // are always the correct source.  This mirrors the original shader
                // and avoids the flat-ambient bug caused by SAMPLE_GI reading an
                // uninitialised TEXCOORD1 as a lightmap UV when LIGHTMAP_ON is set.
                OUT.bakedGI = SampleSH(OUT.normalWS);

                // Accumulate per-vertex additional lights into the fog interpolator
                // (same trick URP/Lit uses to avoid a separate interpolator).
                half  fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                half3 vtxLight  = VertexLighting(posInputs.positionWS, normInputs.normalWS);
                OUT.fogFactorAndVertexLight = half4(fogFactor, vtxLight);

            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                OUT.shadowCoord = GetShadowCoord(posInputs);
            #endif

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 mask    = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, IN.uv);

                // ── Zone detection ────────────────────────────────────────
                // Combination detection: each expression equals 1.0 only for
                // its specific RGB combination, preventing any overlap between
                // zones even at mask colour boundaries.
                half isRed     = mask.r * (1.0 - mask.g) * (1.0 - mask.b);  // (1,0,0)
                half isGreen   = mask.g * (1.0 - mask.r) * (1.0 - mask.b);  // (0,1,0)
                half isBlue    = mask.b * (1.0 - mask.r) * (1.0 - mask.g);  // (0,0,1)
                half isMagenta = mask.r * mask.b         * (1.0 - mask.g);  // (1,0,1)
                half isYellow  = mask.r * mask.g         * (1.0 - mask.b);  // (1,1,0)
                half isCyan    = mask.g * mask.b         * (1.0 - mask.r);  // (0,1,1)
                half isWhite   = mask.r * mask.g         * mask.b;          // (1,1,1)

                // Weighted sum resolves to a single zone's colour per pixel.
                half3 regionColor =
                      _PrimaryColor.rgb * isRed
                    + _StripeColor.rgb  * isGreen
                    + _SkinColor.rgb    * isBlue
                    + _HairColor.rgb    * isMagenta
                    + _ShoeColor.rgb    * isYellow
                    + _LipColor.rgb     * isCyan
                    + _EyeColor.rgb     * isWhite;

                // 1.0 for any recoloured region, 0.0 for black (untouched).
                half totalMask = saturate(isRed + isGreen + isBlue + isMagenta
                                        + isYellow + isCyan + isWhite);

                half lum = dot(baseTex.rgb, half3(0.299, 0.587, 0.114));

                // 2. Remap the luminance so your custom _Midtone becomes exactly 0.5
                half remappedLum = (lum < _Midtone) 
                    ? (lum / _Midtone) * 0.5 
                    : 0.5 + ((lum - _Midtone) / (1.0 - _Midtone)) * 0.5;

                // 3. Apply the gradient map using the corrected luminance
                half3 recolored;
                if (remappedLum < 0.5) 
                {
                    // Shadows map from Black to your Region Color
                    recolored = regionColor * (remappedLum * 2.0);
                } 
                else 
                {
                    // Highlights map from your Region Color to White
                    recolored = lerp(regionColor, half3(1.0, 1.0, 1.0), (remappedLum - 0.5) * 2.0);
                }

                // Blend between original texture and recoloured result
                half3 finalRGB = lerp(baseTex.rgb, recolored, totalMask * _RecolorStrength);
                finalRGB      *= _BaseColor.rgb;              // // Blend between original texture and recoloured result.
              

                // ── Shadow coordinate ─────────────────────────────────────
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                float4 shadowCoord = IN.shadowCoord;
            #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
            #else
                float4 shadowCoord = float4(0, 0, 0, 0);
            #endif

                // ── InputData ─────────────────────────────────────────────
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS              = IN.positionWS;
                lightingInput.normalWS                = NormalizeNormalPerPixel(IN.normalWS);
                lightingInput.viewDirectionWS         = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                lightingInput.shadowCoord             = shadowCoord;
                lightingInput.fogCoord                = IN.fogFactorAndVertexLight.x;
                lightingInput.vertexLighting          = IN.fogFactorAndVertexLight.yzw;
                // SAMPLE_GI picks up either baked lightmaps or the SH probe
                // accumulated in the vertex shader — whichever the scene uses.
                lightingInput.bakedGI                 = IN.bakedGI;
                lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                lightingInput.shadowMask              = half4(1, 1, 1, 1);

                // ── SurfaceData ───────────────────────────────────────────
                SurfaceData surfaceInput = (SurfaceData)0;
                surfaceInput.albedo     = finalRGB;
                surfaceInput.alpha      = baseTex.a * _BaseColor.a;
                surfaceInput.smoothness = _Smoothness;
                surfaceInput.occlusion  = 1.0;
                surfaceInput.normalTS   = half3(0.0h, 0.0h, 1.0h);

                half4 color = UniversalFragmentPBR(lightingInput, surfaceInput);
                color.rgb   = MixFog(color.rgb, lightingInput.fogCoord);
                color.a     = surfaceInput.alpha;
                return color;
            }
            ENDHLSL
        }

        // ─────────────────────────────────────────────────────────────────────
        // Pass 2 — Shadow Caster
        // Rewritten without LitInput.hlsl so the CBUFFER matches ForwardLit
        // exactly, which is required for SRP Batcher to batch these materials.
        // ─────────────────────────────────────────────────────────────────────
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest   LEqual
            ColorMask 0
            Cull    Back

            HLSLPROGRAM
            #pragma vertex   ShadowPassVertex
            #pragma fragment ShadowPassFragment
            // #pragma multi_compile_instancing
            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

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

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings ShadowPassVertex(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS   = TransformObjectToWorldNormal(IN.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDir = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDir = _LightDirection;
                #endif

                OUT.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDir));

                #if UNITY_REVERSED_Z
                    OUT.positionCS.z = min(OUT.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    OUT.positionCS.z = max(OUT.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return OUT;
            }

            half4 ShadowPassFragment(Varyings IN) : SV_TARGET { return 0; }
            ENDHLSL
        }

        // ─────────────────────────────────────────────────────────────────────
        // Pass 3 — Depth Only
        // Required for the depth prepass, SSAO, and depth-of-field.
        // The original shader omitted this, causing those effects to have
        // holes where NPCs were.
        // ─────────────────────────────────────────────────────────────────────
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma vertex   DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings DepthOnlyVertex(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 DepthOnlyFragment(Varyings IN) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
