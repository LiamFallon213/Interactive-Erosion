
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

			uniform sampler2D _MainTex, _Atmosphere;
			float _MaxVapor;

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

			f2a frag(v2f IN) : COLOR
			{
				float4 atmospehere = tex2D(_Atmosphere, IN.uv);
								
				float rain = atmospehere.x - _MaxVapor;
				rain = max(rain, 0.0);//always positive
				
				float4 oldLiquid = tex2D(_MainTex, IN.uv);  

				f2a OUT;
				
				//evaporated field newValue
				OUT.col0 = float4(oldLiquid.x + rain, oldLiquid.y, oldLiquid.z, oldLiquid.w);
				//atmosphere field
				OUT.col1 = float4(atmospehere.x - rain, atmospehere.y, atmospehere.z, atmospehere.w);
				//rain field
				OUT.col2 = float4(rain, 0, 0, 0);
				return OUT;
			}

			ENDCG

		}
	}
}