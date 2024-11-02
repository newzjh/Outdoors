// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Skybox/Blend Cubemap" {
Properties {
    _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    //_Rotation ("Rotation", Range(0, 360)) = 0
     //_Blend ("Blend", Range(0, 1)) = 1
    //[NoScaleOffset] _Tex1 ("Cubemap   (HDR)", Cube) = "grey" {}
    //[NoScaleOffset] _Tex2 ("Cubemap   (HDR)", Cube) = "grey" {}
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0

        #include "UnityCG.cginc"

        samplerCUBE _GlobalCubemap1;
        samplerCUBE _GlobalCubemap2;
        half4 _GlobalCubemap1_HDR;
        half4 _GlobalCubemap2_HDR;
        float _GlobalCubemapBlend;
        float _GlobalCubemapRotation;
        half4 _Tint;
        half _Exposure;

        float3 RotateAroundYInDegrees (float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }

        struct appdata_t {
            float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert (appdata_t v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float3 rotated = RotateAroundYInDegrees(v.vertex, _GlobalCubemapRotation);
            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = -v.vertex.xyz;
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            half3 worldViewDir = i.texcoord;
            worldViewDir.y*=0.5f;
            worldViewDir.y+=0.1f;
            worldViewDir=normalize(worldViewDir);
            half4 tex1 = texCUBE (_GlobalCubemap1, worldViewDir);
            half4 tex2 = texCUBE (_GlobalCubemap2, worldViewDir);
            half3 c1 = DecodeHDR (tex1, _GlobalCubemap1_HDR);
            half3 c2 = DecodeHDR (tex2, _GlobalCubemap2_HDR);
            half3 c = lerp(c1,c2,_GlobalCubemapBlend);
            //c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
            //c *= _Exposure;
            return half4(c, 1);
        }
        ENDCG
    }
}


Fallback Off

}
