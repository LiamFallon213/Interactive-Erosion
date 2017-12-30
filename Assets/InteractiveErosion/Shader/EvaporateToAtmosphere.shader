
Shader "Erosion/EvaporateToAtmosphere"
{
	//UNITY_SHADER_NO_UPGRADE
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Atmosphere("Base (RGB)", 2D) = "white" {}
	}
		SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D _MainTex, _Atmosphere, _Terrain,  _Lava;
			uniform float _EvaporateConstant, _Layers, _AtmoHeight;


			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};
			struct f2a
			{
				float4 col0 : COLOR0;
				float4 col1 : COLOR1;
			};
			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				OUT.uv = v.texcoord.xy;
				return OUT;
			}
			float GetTotalHeight(float2 uv)// of terrain
			{
				float4 texData = tex2D(_Terrain, uv);
				float4 maskVec = float4(_Layers, _Layers - 1, _Layers - 2, _Layers - 3);
				float4 addVec = min(float4(1, 1, 1, 1), max(float4(0, 0, 0, 0), maskVec));
				float res = dot(texData, addVec);
				res += tex2D(_MainTex, uv).x;//water
				res += tex2D(_Lava, uv).x;
				return res;
			}
			float getPressure(float m, float t, float v)
			{
				float R = 8.314;// gas constant (wapor)
				float M = 0.018;
				if (m == 0.0 || t == 0.0)
					return 0.0;
				return m * R * t / (v*M);
			}
			f2a frag(v2f IN) : COLOR
			{
				float4 oldValue = tex2D(_MainTex, IN.uv);
				float4 atmosphere = tex2D(_Atmosphere, IN.uv);

				//atmosphere.a
				float t = 30.0;//Celsius
				float saturatedVapor = 610.94 * exp(17.625 * t / (t + 243.04));//Pa

				float realAtmoHeight = _AtmoHeight - GetTotalHeight(IN.uv);
				realAtmoHeight = max(realAtmoHeight, 0.0);
				t += 273;
				float rHumidity = getPressure(atmosphere.x, t, realAtmoHeight) / saturatedVapor;
				rHumidity = clamp(rHumidity, 0.0, 1.0);
				
				float maxEvaporation = _EvaporateConstant * t * (1.0 - rHumidity);

				float rest = max(oldValue.x - maxEvaporation, 0.0);
				float possibleEvaporation = oldValue.x - rest;

				f2a OUT;
				//evaporated field newValue
				OUT.col0 = float4(oldValue.x - possibleEvaporation, oldValue.y, oldValue.z, oldValue.w);

				//atmosphere field
				OUT.col1 = float4(atmosphere.x + possibleEvaporation, atmosphere.y, atmosphere.z, atmosphere.w);

				return OUT;
			}

			ENDCG

		}
	}
}