
Shader "Erosion/Atmosphere"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_MaxVaporForBlackColor("MaxVaporForBlackColor", Float) = 0.1
		_MinVaporToShow("MinVaporToShow", Float) = 5
		_ScaleCloudsHeight("_ScaleCloudsHeight", Float) = 1
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" }
			LOD 200
			Cull Off

			CGPROGRAM
			#pragma exclude_renderers gles
			#pragma surface surf Lambert vertex:vert
			#pragma target 3.0
			#pragma glsl

			sampler2D _MainTex;
			uniform float _AtmoHeight, _ScaleY, _MaxVaporForBlackColor, _MinVaporToShow, _TexSize, _ScaleCloudsHeight;

			struct Input
			{
				float2 uv_MainTex;
			};



			void vert(inout appdata_full v)
			{
				v.tangent = float4(1,0,0,1);

				v.vertex.y += _AtmoHeight * _ScaleY + tex2Dlod(_MainTex, float4(v.texcoord.xy, 0.0, 0.0))*_ScaleCloudsHeight;
			}

			float3 FindNormal(float2 uv, float u)
			{
				float ht0 = tex2D(_MainTex, uv + float2(-u, 0)) ;
				float ht1 = tex2D(_MainTex, uv + float2(u, 0)) ;
				float ht2 = tex2D(_MainTex, uv + float2(0, -u)) ;
				float ht3 = tex2D(_MainTex, uv + float2(0, u)) ;

				float2 _step = float2(1.0, 0.0);

				float3 va = normalize(float3(_step.xy, ht1-ht0));
				float3 vb = normalize(float3(_step.yx, ht2-ht3));

			   return cross(va,vb);
			}

			void surf(Input IN, inout SurfaceOutput o)
			{
				float vapor = tex2D(_MainTex, IN.uv_MainTex).x;
				half3 color = 1.0 - clamp(vapor / _MaxVaporForBlackColor, 0.0, 1.0);

				color *= 1;
				//color += 0.2;			
				o.Albedo = color;
				if (vapor < _MinVaporToShow)
					discard;
				o.Alpha = 1.0;
				float3 n = FindNormal(IN.uv_MainTex, 1.0 / _TexSize);
				o.Normal = n;// fixed3(0, 1, 0);

			}
			ENDCG
		}
			FallBack "Diffuse"
}
