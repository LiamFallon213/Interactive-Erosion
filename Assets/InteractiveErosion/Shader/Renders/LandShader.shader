
Shader "Erosion/LandShader"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_LayerColor0("LayerColor0", Color) = (1,1,1,1)
		_LayerColor1("LayerColor1", Color) = (1,1,1,1)
		_LayerColor2("LayerColor2", Color) = (1,1,1,1)
		_LayerColor3("LayerColor3", Color) = (1,1,1,1)
		_LavaColor("LavaColor", Color) = (1,1,1,1)
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

			sampler2D _MainTex, _Lava;
			float3 _LayerColor0, _LayerColor1, _LayerColor2, _LayerColor3, _LavaColor;
			uniform float _ScaleY, _Layers, _TexSize;

			struct Input
			{
				float2 uv_MainTex;
			};
			float saturate(float v)
			{
				return clamp(v, 0.0, 1.0);
			}
			float3 ColorTemperatureToRGB(float temperatureInKelvins)
			{
				float3 retColor;

				//temperatureInKelvins = clamp(temperatureInKelvins, 1000.0, 40000.0);// / 100.0;
				temperatureInKelvins = temperatureInKelvins / 100.0;
								
				//temperatureInKelvins = min(temperatureInKelvins, 13.0); // limit temperature from above
				if (temperatureInKelvins < 5.47)
					retColor = float3(0, 0, 0);
				else
				{
				if (temperatureInKelvins <= 66.0)
				{
					retColor.x = 1.0;
					retColor.y = saturate(0.39008157876901960784 * log(temperatureInKelvins) - 0.63184144378862745098);
				}
				else
				{
					float t = temperatureInKelvins - 60.0;
					retColor.x = saturate(1.29293618606274509804 * pow(t, -0.1332047592));
					retColor.y = saturate(1.12989086089529411765 * pow(t, -0.0755148492));
				}

				if (temperatureInKelvins >= 66.0)
					retColor.z = 1.0;
				else if (temperatureInKelvins <= 19.0)
					retColor.z = 0.0;
				else
					retColor.z = saturate(0.54320678911019607843 * log(temperatureInKelvins - 10.0) - 1.19625408914);
				}
				return retColor;// *0.8;
		}
			float GetTotalHeight(float4 texData, float lavaHeight)
		{
			float4 maskVec = float4(_Layers, _Layers - 1, _Layers - 2, _Layers - 3);
			float4 addVec = min(float4(1, 1, 1, 1), max(float4(0, 0, 0, 0), maskVec));
			float res = dot(texData, addVec);
			res += lavaHeight;
			return res;
		}


		void vert(inout appdata_full v)
		{
			v.tangent = float4(1, 0, 0, 1);

			v.vertex.y += GetTotalHeight(tex2Dlod(_MainTex, float4(v.texcoord.xy, 0.0, 0.0)), tex2Dlod(_Lava, float4(v.texcoord.xy, 0.0, 0.0))) * _ScaleY;
		}

		float3 FindNormal(float2 uv, float u)
		{

			float ht0 = GetTotalHeight(tex2D(_MainTex, uv + float2(-u, 0)), tex2D(_Lava, uv + float2(-u, 0)));
			float ht1 = GetTotalHeight(tex2D(_MainTex, uv + float2(u, 0)), tex2D(_Lava, uv + float2(u, 0)));
			float ht2 = GetTotalHeight(tex2D(_MainTex, uv + float2(0, -u)), tex2D(_Lava, uv + float2(0, -u)));
			float ht3 = GetTotalHeight(tex2D(_MainTex, uv + float2(0, u)), tex2D(_Lava, uv + float2(0, u)));

			float2 _step = float2(1.0, 0.0);

			float3 va = normalize(float3(_step.xy, ht1 - ht0));
			float3 vb = normalize(float3(_step.yx, ht2 - ht3));

			return cross(va, vb);
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			float3 n = FindNormal(IN.uv_MainTex, 1.0 / _TexSize);

			float4 hts = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = lerp(_LayerColor0, _LayerColor1, clamp(hts.y * 2.0, 0.0, 1.0));
			o.Albedo = lerp(o.Albedo, _LayerColor2, clamp(hts.z * 2.0, 0.0, 1.0));
			o.Albedo = lerp(o.Albedo, _LayerColor3, clamp(hts.w * 2.0, 0.0, 1.0));

			float4 lava = tex2D(_Lava, IN.uv_MainTex);
			o.Albedo = lerp(o.Albedo, _LavaColor, clamp(lava.x * 2.0, 0.0, 1.0));

			//if (lava.r > 0.0)
			{
				fixed3 light = ColorTemperatureToRGB(lava.a);
				o.Emission = light;
			}

			o.Alpha = 1.0;
			o.Normal = n;

		}



		ENDCG
		}
			FallBack "Diffuse"
}
