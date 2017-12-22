//UNITY_SHADER_NO_UPGRADE
Shader "Erosion/ChangeValueGaussWithHeat"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
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

		sampler2D _MainTex;
	uniform float2 _Point;
	uniform float _Radius;
	uniform float4 _Value;

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

	float GetGaussFactor(float2 diff, float rad2)
	{
		return exp(-(diff.x*diff.x + diff.y*diff.y) / rad2);
	}

	float4 frag(v2f IN) : COLOR
	{

		float gauss = GetGaussFactor(_Point - IN.uv, _Radius*_Radius);

		float4 changeValue = gauss * _Value;
		float4 oldValue = tex2D(_MainTex, IN.uv);
		float oldTemperature = oldValue.a;
		float oldAmount = oldValue.r;
		float newTemperature =  _Value.a;
		float newAmount = changeValue.r;
	
		float4 res = oldValue + changeValue;
		if (oldAmount + newAmount > 0.0)
			res.a = (oldTemperature * oldAmount + newTemperature * newAmount) / (oldAmount + newAmount);
		else
			res.a = 0.0;
		return res;
	}

		ENDCG

	}
	}
}