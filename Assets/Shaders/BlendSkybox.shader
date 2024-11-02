// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Skybox/Blend 6 Sided" {
Properties {
    _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _Rotation ("Rotation", Range(0, 360)) = 0
    _Blend ("Blend", Range(0, 1)) = 1
    [NoScaleOffset] _FrontTex1 ("Front1 [+Z]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _BackTex1 ("Back1 [-Z]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _LeftTex1 ("Left1 [+X]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _RightTex1 ("Right1 [-X]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _UpTex1 ("Up1 [+Y]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _DownTex1 ("Down1 [-Y]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _FrontTex2 ("Front2 [+Z]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _BackTex2 ("Back2 [-Z]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _LeftTex2 ("Left2 [+X]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _RightTex2 ("Right2 [-X]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _UpTex2 ("Up2 [+Y]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _DownTex2 ("Down2 [-Y]   (HDR)", 2D) = "grey" {}    
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    CGINCLUDE
    #include "UnityCG.cginc"

    half4 _Tint;
    half _Exposure;
    float _Rotation;
    float _Blend;

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
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    struct v2f {
        float4 vertex : SV_POSITION;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };
    v2f vert (appdata_t v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
        o.vertex = UnityObjectToClipPos(rotated);
        o.texcoord = v.texcoord;
        return o;
    }
    half4 skybox_frag (v2f i, sampler2D smp1,sampler2D smp2, half4 smp1Decode,half4 smp2Decode)
    {
        half4 tex1 = tex2D (smp1, i.texcoord);
        half4 tex2 = tex2D (smp2, i.texcoord);
        half3 c1 = DecodeHDR (tex1, smp1Decode);
        half3 c2 = DecodeHDR (tex2, smp2Decode);
        half3 c = lerp(c1,c2,_Blend);
        c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
        c *= _Exposure;
        return half4(c, 1);
    }
    ENDCG

    Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _FrontTex1;
        sampler2D _FrontTex2;
        half4 _FrontTex1_HDR;
        half4 _FrontTex2_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_FrontTex1,_FrontTex2, _FrontTex1_HDR,_FrontTex2_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _BackTex1;
        sampler2D _BackTex2;
        half4 _BackTex1_HDR;
        half4 _BackTex2_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_BackTex1,_BackTex2, _BackTex1_HDR,_BackTex2_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _LeftTex1;
        sampler2D _LeftTex2;
        half4 _LeftTex1_HDR;
        half4 _LeftTex2_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_LeftTex1,_LeftTex2, _LeftTex1_HDR,_LeftTex2_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _RightTex1;
        sampler2D _RightTex2;
        half4 _RightTex1_HDR;
        half4 _RightTex2_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_RightTex1,_RightTex2, _RightTex1_HDR,_RightTex2_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _UpTex1;
        sampler2D _UpTex2;
        half4 _UpTex1_HDR;
        half4 _UpTex2_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_UpTex1,_UpTex2, _UpTex1_HDR,_UpTex2_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _DownTex1;
        sampler2D _DownTex2;
        half4 _DownTex1_HDR;
        half4 _DownTex2_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_DownTex1,_DownTex2, _DownTex1_HDR,_DownTex2_HDR); }
        ENDCG
    }
}
}
