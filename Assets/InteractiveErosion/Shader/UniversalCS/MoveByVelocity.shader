
// NOT REALISTIC VERSION
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

		sampler2D _MainTex;
	uniform float T, _Coefficient, _TexSize, _Limit;
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

		/*float2 velocity = tex2D(_Velocity, IN.uv).xy;
		float4 thatPoint = tex2D(_MainTex, IN.uv);
		tex2D(_MainTex, IN.uv + velocity).x += tex2D(_MainTex, IN.uv).x;
		return float4(0, thatPoint.y, thatPoint.z,thatPoint.w);*/

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
			float level = tex2D(_MainTex, side).x;
			if (flow > 0.0 && level > _Limit + 1)
				flowIn += flow;
		}
		side = IN.uv + float2(u, 0);
		if (CoordsExist(side))
		{
			float flow = tex2D(_Velocity, side).x;
			float level = tex2D(_MainTex, side).x;
			if (flow < 0.0&& level > _Limit + 1)
				flowIn += abs(flow);
		}
		side = IN.uv + float2(0,-u);
		if (CoordsExist(side))
		{
			float flow = tex2D(_Velocity, side).y;
			float level = tex2D(_MainTex, side).x;
			if (flow > 0.0&& level > _Limit + 1)
				flowIn += flow;
		}
		side = IN.uv + float2(0, u);
		if (CoordsExist(side))
		{
			float flow = tex2D(_Velocity, side).y;
			float level = tex2D(_MainTex, side).x;
			if (flow < 0.0&& level > _Limit + 1)
				flowIn += abs(flow);
		}
		// add limits+ 
		//add proportions
		// add y vector+
		float3 velocity = tex2D(_Velocity, IN.uv);
		float flowAway = (abs(velocity.x) + abs(velocity.y)) * T * _Coefficient;

		float4 level = tex2D(_MainTex, IN.uv);

		float newLevel = level.x - flowAway;

		if (newLevel < _Limit)
			newLevel = _Limit;

		level.x = newLevel;
		level.x += flowIn * T * _Coefficient;
		level.x += velocity.z * T * _Coefficient / 20.0;
		return level;

		}

			ENDCG
		}

	}
}


