// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Skybox/Blend Cubemap Obj" {
Properties {
    _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    //_Rotation ("Rotation", Range(0, 360)) = 0
     //_Blend ("Blend", Range(0, 1)) = 1
    //[NoScaleOffset] _Tex1 ("Cubemap   (HDR)", Cube) = "grey" {}
    //[NoScaleOffset] _Tex2 ("Cubemap   (HDR)", Cube) = "grey" {}
}

SubShader {
    Tags {"RenderQueue" = "Geometry"  "IgnoreProjector"="True" "RenderType"="Opaque"}
	Cull Back

	CGINCLUDE

        #include "UnityCG.cginc"
        #include "AutoLight.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"     

        float3 RotateAroundYInDegrees (float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }

        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
            float3 worldPos : TEXCOORD1;
            float3 worldNormal : TEXCOORD2;
            float3 worldTangent : TEXCOORD3;
            float3 worldBinormal : TEXCOORD4;
            float4 col : TEXCOORD45;
            UNITY_VERTEX_OUTPUT_STEREO
        };

       struct SurfaceInput {
            float2 uv;
            float3 worldViewDir;
        };

    ENDCG    

    /*
    Pass {
            ZWrite On
            ZTest LEqual

            CGPROGRAM

            struct v2f_outline {
                float4 pos : SV_POSITION;
            };          

            v2f vert_outline(appdata_full v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                half3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
                half3 worldNormal = UnityObjectToWorldNormal( v.normal );
                half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
                half3 tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
                o.worldPos = worldPos;
                o.worldNormal = worldNormal;
                o.worldTangent = worldTangent;
                o.worldBinormal = worldBinormal;

                float3 viewnormal   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                float3 offset = TransformViewToProjection(normalize(viewnormal.xyz));
                float4 clippos = UnityObjectToClipPos(v.vertex);
                clippos.xy += normalize(offset) * 0.01 * clippos.w;   
                o.vertex = clippos;
                o.texcoord = -worldNormal;
                o.col = v.color;
                return o;
            }
            half4 frag_outline( v2f_outline i) :COLOR 
            {
                return half4(0, 1, 0, 1);
            }

            #pragma vertex vert_outline
            #pragma fragment frag_outline
            #pragma fragmentoption ARB_precision_hint_fastest 

            ENDCG
        }
        */

    Pass {

        ZWrite On
        ZTest LEqual
        

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 3.0
   

        samplerCUBE _GlobalCubemap1;
        samplerCUBE _GlobalCubemap2;
        half4 _GlobalCubemap1_HDR;
        half4 _GlobalCubemap2_HDR;
        float _GlobalCubemapBlend;
        float _GlobalCubemapRotation;
        half4 _Tint;
        half _Exposure;



        v2f vert (appdata_full v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			half3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
			half3 worldNormal = UnityObjectToWorldNormal( v.normal );
			half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
			half3 tangentSign = v.tangent.w * unity_WorldTransformParams.w;
			half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
            o.worldPos = worldPos;
            o.worldNormal = worldNormal;
            o.worldTangent = worldTangent;
            o.worldBinormal = worldBinormal;

            float4 clippos = UnityObjectToClipPos(v.vertex);
            //clippos.z+=0.1f*clippos.w;
            o.vertex = clippos;
            o.texcoord = -worldNormal;
            o.col = v.color;
            return o;
        }

        void surf (SurfaceInput IN, inout SurfaceOutputStandard o) {

            half4 tex1 = texCUBE (_GlobalCubemap1, IN.worldViewDir);
            half4 tex2 = texCUBE (_GlobalCubemap2, IN.worldViewDir);
            half3 c1 = DecodeHDR (tex1, _GlobalCubemap1_HDR);
            half3 c2 = DecodeHDR (tex2, _GlobalCubemap2_HDR);
            half3 c = lerp(c1,c2,_GlobalCubemapBlend);
            //cc = cc * _Tint.rgb * unity_ColorSpaceDouble.rgb;
            c = c * 0.22f * unity_ColorSpaceDouble.rgb;
            //cc *= _Exposure;
            o.Albedo = c;
            o.Alpha = 1;
        }

        fixed4 frag (v2f IN) : SV_Target
        {
			SurfaceOutputStandard o;
			UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
			
            SurfaceInput surfIN;
			UNITY_INITIALIZE_OUTPUT( SurfaceInput, surfIN );
			surfIN.uv = IN.texcoord.xy;
			float3 worldPos = IN.worldPos.xyz;;
  #ifndef USING_DIRECTIONAL_LIGHT
            fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
            fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif            
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );         
            worldViewDir = RotateAroundYInDegrees(worldViewDir,_GlobalCubemapRotation);
            worldViewDir.y*=0.5f;
            worldViewDir.y+=0.1f;
            worldViewDir=normalize(worldViewDir);
            surfIN.worldViewDir = worldViewDir;
			surf( surfIN, o );

            // compute lighting & shadowing factor
            UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
            fixed4 c = 0;
            //float3 worldN;
            //worldN.x = dot(_unity_tbn_0, o.Normal);
            //worldN.y = dot(_unity_tbn_1, o.Normal);
            //worldN.z = dot(_unity_tbn_2, o.Normal);
            //worldN = normalize(worldN);
            //o.Normal = worldN;
            o.Normal = IN.worldNormal;

            // Setup lighting environment
            UnityGI gi;
            UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
            gi.indirect.diffuse = 0;
            gi.indirect.specular = 0;
            gi.light.color = _LightColor0.rgb;
            gi.light.dir = lightDir;
            // Call GI (lightmaps/SH/reflections) lighting function
            UnityGIInput giInput;
            UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
            giInput.light = gi.light;
            giInput.worldPos = worldPos;
            giInput.worldViewDir = worldViewDir;
            giInput.atten = atten;
            #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            giInput.lightmapUV = IN.lmap;
            #else
            giInput.lightmapUV = 0.0;
            #endif
            #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
            giInput.ambient = IN.sh;
            #else
            giInput.ambient.rgb = 0.0;
            #endif
            giInput.probeHDR[0] = unity_SpecCube0_HDR;
            giInput.probeHDR[1] = unity_SpecCube1_HDR;
            #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
            giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
            #endif
            #ifdef UNITY_SPECCUBE_BOX_PROJECTION
            giInput.boxMax[0] = unity_SpecCube0_BoxMax;
            giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
            giInput.boxMax[1] = unity_SpecCube1_BoxMax;
            giInput.boxMin[1] = unity_SpecCube1_BoxMin;
            giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
            #endif
            LightingStandard_GI(o, giInput, gi);

            // realtime lighting: call lighting function
            c += LightingStandard (o, worldViewDir, gi);
            c.rgb += o.Emission;
            UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog      


            half4 tex1 = texCUBE (_GlobalCubemap1, worldViewDir);
            half4 tex2 = texCUBE (_GlobalCubemap2, worldViewDir);
            half3 c1 = DecodeHDR (tex1, _GlobalCubemap1_HDR);
            half3 c2 = DecodeHDR (tex2, _GlobalCubemap2_HDR);
            half3 cc = lerp(c1,c2,_GlobalCubemapBlend);
            //cc = cc * _Tint.rgb * unity_ColorSpaceDouble.rgb;
            cc = cc * 0.22f * unity_ColorSpaceDouble.rgb;
            //cc *= _Exposure;


            return half4(cc, 1);

            //c.rgb += cc;
            //return c;
        }
        ENDCG
    }
}


Fallback Off

}
