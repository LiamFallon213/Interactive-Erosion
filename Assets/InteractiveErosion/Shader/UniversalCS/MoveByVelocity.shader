
Shader "Erosion/MoveByVelocity"
{
	//UNITY_SHADER_NO_UPGRADE
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
		SubShader
	{

		Pass
	{
		ZTest Always

		CGPROGRAM
#include "UnityCG.cginc"
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag

	uniform sampler2D _MainTex;
	uniform float T, _Coefficient, _TexSize;
	uniform sampler2D _Velocity;


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
	bool CoordsExist(float2 coord)
	{
		if (coord.x >= 0 && coord.x <= 1 && coord.y >= 0 && coord.y <= 1)
			return true;
		else
			return false;
	}
	float4 frag(v2f IN) : COLOR
	{
		float deltaHL;
		float deltaHR;
		float deltaHT;
		float deltaHB;

		float u = 1.0f / _TexSize;

		float2 side;
		float flowIn = 0.0;
		side = IN.uv + float2(-u, 0);
		if (CoordsExist(side))
		{
			float flow = tex2D(_Velocity, side).x;
			if (flow > 0.0)
				flowIn += flow;
		}
		side = IN.uv + float2(u, 0);
		if (CoordsExist(side))
		{
			float flow = tex2D(_Velocity, side).x;
			if (flow < 0.0)
				flowIn += abs(flow);
		}
		side = IN.uv + float2(0,-u);
		if (CoordsExist(side))
		{
			float flow = tex2D(_Velocity, side).y;
			if (flow > 0.0)
				flowIn += flow;
		}
		side = IN.uv + float2(0, u);
		if (CoordsExist(side))
		{
			float flow = tex2D(_Velocity, side).y;
			if (flow < 0.0)
				flowIn += abs(flow);
		}
		float3 velocity = tex2D(_Velocity, IN.uv);
		float flowAway = (abs(velocity.x) + abs(velocity.y)) * T * _Coefficient;

		float4 level = tex2D(_MainTex, IN.uv);
		level.x -= flowAway;
		level.x += flowIn* T * _Coefficient;
		return level;

	}

		ENDCG
			}

	}
}
