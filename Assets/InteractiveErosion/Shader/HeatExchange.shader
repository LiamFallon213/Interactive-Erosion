
Shader "Erosion/HeatExchange"
{
	//UNITY_SHADER_NO_UPGRADE
	Properties
	{
		_MainTex("MainTex", 2D) = "black" { }
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

		sampler _MainTex;
	//uniform sampler2D _OutFlowField;
	uniform float _StefanBoltzmannConstant, _Emissivity, _HeatCapacity, T;

	struct v2f
	{
		float4  pos : SV_POSITION;
		float2  uv : TEXCOORD0;
	};

	v2f vert(appdata_base v)
	{
		v2f OUT;
		OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		OUT.uv = v.texcoord.xy;
		return OUT;
	}

	float4 frag(v2f IN) : COLOR
	{
		float4 oldValue = tex2D(_MainTex, IN.uv);
		float amount = oldValue.r;
		if (amount > 0.0)
		{
			float temperature = oldValue.a;
			float QChange = _StefanBoltzmannConstant*pow(temperature, 4)*_Emissivity *-1 * T + 342 * amount;
			
			{ 				
				float temperatureChange = QChange / (_HeatCapacity * amount);//* amount
				//if (!isinf(temperatureChange)) 
				oldValue.a += temperatureChange;//change only Alpha channel - temperature!
				oldValue.a = max(oldValue.a, 0.0);
			}
		}
		else
			oldValue.a = 0.0;
		return oldValue;
	}

		ENDCG

	}
	}
}