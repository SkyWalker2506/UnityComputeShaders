﻿Shader "Custom/QuadParticle" {
	Properties     
    {
        _MainTex("Texture", 2D) = "white" {}     
    }  

	SubShader {
		Pass {
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 200
		ZWrite Off
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 5.0
		
		struct v2f{
			float4 position : SV_POSITION;
			float4 color : COLOR;
			float2 uv: TEXCOORD0;
		};

		struct vertex
		{
			float3 position;
			float2 uv;
			float life;
		};
		StructuredBuffer<vertex> vertexBuffer;

		sampler2D _MainTex;
		
		v2f vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
		{
			v2f o = (v2f)0;

			const int vertexIndex = instance_id*6 + vertex_id;
			float lerpValue = vertexBuffer[vertexIndex].life*.25;
						
			o.color = fixed4(1.1-lerpValue,lerpValue+.1,lerpValue*2,lerpValue*.7);
			o.position = UnityWorldToClipPos(float4(vertexBuffer[vertexIndex].position,1));
			o.uv = vertexBuffer[vertexIndex].uv;
			
			return o;
		}

		float4 frag(v2f i) : COLOR
		{
			fixed4 col = tex2D(_MainTex, i.uv)*i.color;
			return col;
		}


		ENDCG
		}
	}
	FallBack Off
}
