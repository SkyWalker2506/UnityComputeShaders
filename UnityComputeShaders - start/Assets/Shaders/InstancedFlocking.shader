﻿Shader "Flocking/Instanced" { 

   Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
		_MetallicGlossMap("Metallic", 2D) = "white" {}
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Glossiness ("Smoothness", Range(0,1)) = 1.0
	}

   SubShader {
 
		CGPROGRAM

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _MetallicGlossMap;
		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldPos;
		};
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
 
        #pragma surface surf Standard vertex:vert addshadow nolightmap
        #pragma instancing_options procedural:setup

        float4x4 _Matrix;
        float3 _BoidPosition;

         #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            struct Boid
            {
                float3 position;
                float3 direction;
                float noise_offset;
            };

            StructuredBuffer<Boid> boidsBuffer; 
         #endif
     
        float4x4 create_matrix(float3 pos,float3 direction, float3 up)
        {
            float3 zaxis = normalize(direction);
            float3 xaxis = normalize(cross(up, zaxis));
            float3 yaxis = cross(zaxis, xaxis);
            float4x4 flockMatrix = float4x4(
                float4(xaxis.x, yaxis.x, zaxis.x, pos.x),
                float4(xaxis.y, yaxis.y, zaxis.y, pos.y),
                float4(xaxis.z, yaxis.z, zaxis.z, pos.z),
                float4(pos.x, pos.y, pos.z, 1)
            );
            return flockMatrix;
        }

         void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                v.vertex = mul(_Matrix, v.vertex);
            #endif
        }

        void setup()
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                _Matrix = create_matrix(boidsBuffer[unity_InstanceID].position,boidsBuffer[unity_InstanceID].direction, float3(0, 1, 0));
                _BoidPosition = boidsBuffer[unity_InstanceID].position;
            #endif
        }
 
         void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 m = tex2D (_MetallicGlossMap, IN.uv_MainTex); 
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
			o.Metallic = m.r;
			o.Smoothness = _Glossiness * m.a;
         }
 
         ENDCG
   }
}