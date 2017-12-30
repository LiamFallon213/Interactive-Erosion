
Shader "Erosion/RainFromAtmosphere"
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
			uniform float _CondensationConst, _Layers, _AtmoHeight;

			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};
			struct f2a
			{
				//evaporated field newValue
				float4 col0 : COLOR0;
				//atmosphere field
				float4 col1 : COLOR1;
				// rain amount
				float4 col2 : COLOR2;
			};
			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				OUT.uv = v.texcoord.xy;
				return OUT;
			}
			/*float GetTotalHeight(float4 texData, float lavaHeight)
			{
				float4 maskVec = float4(_Layers, _Layers - 1, _Layers - 2, _Layers - 3);
				float4 addVec = min(float4(1, 1, 1, 1), max(float4(0, 0, 0, 0), maskVec));
				float res = dot(texData, addVec);
				res += lavaHeight;
				return res;
			}*/
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
		
			f2a frag(v2f IN) : COLOR
			{
				//atmosphere.a
				float t = 30.0;
				float saturatedVaporPressure = 610.94 * exp(17.625 * t / (t + 243.04));

				float realAtmoHeight = _AtmoHeight - GetTotalHeight(IN.uv);
				realAtmoHeight = max(realAtmoHeight, 0.0);

				t += 273;
				float R = 8.314;// gas constant (vapor)
				float saturatedMass = saturatedVaporPressure * realAtmoHeight *0.018 / (R * t);
				//float saturatedMass = 4236 * realAtmoHeight *0.018 / (R * t);

								
				//atmosphere.a
				//float maxVapor = 303.15 * _CondensationConst * realAtmoHeight / 100;

				float4 atmosphere = tex2D(_Atmosphere, IN.uv);
				float rain = atmosphere.x - saturatedMass;
				//float rain = atmosphere.x - 2.0;
				rain = max(rain, 0.0);//always positive

				float4 oldLiquid = tex2D(_MainTex, IN.uv);

				f2a OUT;

				//evaporated field newValue
				OUT.col0 = float4(oldLiquid.x + rain, oldLiquid.y, oldLiquid.z, oldLiquid.w);
				//atmosphere field
				OUT.col1 = float4(atmosphere.x - rain, atmosphere.y, atmosphere.z, atmosphere.w);
				//rain field
				OUT.col2 = float4(rain, 0, 0, 0);
				return OUT;
			}

			ENDCG

		}
	}
}