
Shader "Erosion/ArrowsShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_LengthMultiplier("LengthMultiplier", float) = 1
		_Width("Width", float) = 1
		_AddHeight("_AddHeight", float) = 1
		_Terrain("_Terrain", 2D) = "white" {}
		_Water("_Water", 2D) = "white" {}
		_WaterVelocity("_WaterVelocity", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma exclude_renderers gles
		#pragma surface surf Lambert vertex:vert
		#pragma target 3.0
		#pragma glsl

		sampler2D _Terrain, _Water, _WaterVelocity;
		float3 _Color;
		uniform float _LengthMultiplier, _Width;
		uniform float _ScaleY, _AddHeight;

		struct Input
		{
			float2 uv_MainTex;			
		};
		struct vInput {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			uint vertexId : SV_VertexID;
			float4 texcoord : TEXCOORD0;
		};
		/*float GetTotalHeight(float4 texData)
		{
			float4 maskVec = float4(1, 1, 1, 1);
			float4 addVec = min(float4(1,1,1,1),max(float4(0,0,0,0), maskVec));
			return dot(texData, addVec); 
		}*/
		float GetTotalHeight(float2 uv)
		{
			float4 terrain = tex2Dlod(_Terrain, float4(uv, 0.0, 0.0));
			float height = terrain.x + terrain.y + terrain.z + terrain.w;
			height += tex2Dlod(_Water, float4(uv, 0.0, 0.0)).x;
			return height;
		}

		void vert(inout vInput v)
		{	
			float4 read = tex2Dlod(_WaterVelocity, float4(v.texcoord.xy, 0.0, 0.0));
			float3 velocity;
			velocity.x = read.x;
			velocity.y = 0.0;
			velocity.z = read.y;
			float3 newPosition;
			if (v.vertexId % 3 == 0) //arrow's spike;
			{
				newPosition.x = velocity.x*_LengthMultiplier;
				newPosition.z = velocity.z*_LengthMultiplier;
			}
			else if (v.vertexId % 3 == 1) // left point
			{
				newPosition.x = velocity.z *-1.0;
				newPosition.y = 0.0;
				newPosition.z = velocity.x;
				newPosition *= _Width;
			}
			else if (v.vertexId % 3 == 2) // right point
			{			
				newPosition.x = velocity.z;
				newPosition.y = 0.0;
				newPosition.z = velocity.x*-1.0;
				newPosition *= _Width; 
			}


			v.vertex.x += newPosition.x;
			v.vertex.y += GetTotalHeight(v.texcoord.xy) * _ScaleY + _AddHeight; //GetTotalHeight(tex2Dlod(_Terrain, float4(v.texcoord.xy, 0.0, 0.0))) * _ScaleY;
			//			
			v.vertex.z += newPosition.z;
		}

		/*float3 FindNormal(float2 uv, float u)
		{

			float ht0 = GetTotalHeight(tex2D(_MainTex, uv + float2(-u, 0)));
			float ht1 = GetTotalHeight(tex2D(_MainTex, uv + float2(u, 0)));
			float ht2 = GetTotalHeight(tex2D(_MainTex, uv + float2(0, -u)));
			float ht3 = GetTotalHeight(tex2D(_MainTex, uv + float2(0, u)));

			float2 _step = float2(1.0, 0.0);

			float3 va = normalize(float3(_step.xy, ht1 - ht0));
			float3 vb = normalize(float3(_step.yx, ht2 - ht3));

		   return cross(va,vb);
		}*/

		void surf(Input IN, inout SurfaceOutput o)
		{
			o.Albedo = _Color;

			o.Alpha = 1.0;
			//float3 n = FindNormal(IN.uv_MainTex, 1.0 / _TexSize);
			//o.Normal = n;

		}
		ENDCG
	}
		FallBack "Diffuse"
}
