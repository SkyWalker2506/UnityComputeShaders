﻿Shader "Custom/GrassTrample"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags{ "RenderType"="Opaque" }
        
		LOD 200
		Cull Off
		
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types    
        #pragma surface surf Standard vertex:vert addshadow fullforwardshadows
        #pragma instancing_options procedural:setup

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Scale;
        float _Trample;
        float4x4 _Matrix;

        float4x4 create_matrix(float3 pos, float theta){
            float c = cos(theta);
            float s = sin(theta);
            return float4x4(
                1, 0,  0, pos.x,
                0, c, -s, pos.y,
                0, s,  c, pos.z,
                0, 0,  0, 1
            );
        }
        
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            struct GrassClump
            {
                float3 position;
                float lean;
                float noise;
                float trample;
            };
            StructuredBuffer<GrassClump> clumpsBuffer; 
        #endif

        void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                v.vertex.y *= _Trample;
                v.vertex.xyz *= _Scale;
                v.vertex = mul(_Matrix, v.vertex);
            #endif
        }

        void setup()
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                GrassClump clump = clumpsBuffer[unity_InstanceID];
                _Trample = 1 - clump.trample;
                _Matrix = create_matrix(clump.position, clump.lean);
            #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            clip(c.a-0.4);
        }
        ENDCG
    }
    FallBack "Diffuse"
}