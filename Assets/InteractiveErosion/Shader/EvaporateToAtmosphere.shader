
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

			uniform sampler2D _MainTex, _Atmosphere;
			float _Value;

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

			f2a frag(v2f IN) : COLOR
			{
				float4 oldValue = tex2D(_MainTex, IN.uv);
								
				float newValue = oldValue.x + _Value; 
				newValue = max(newValue, 0.0); 

				float change = oldValue.x - newValue;
				float4 atmospehere = tex2D(_Atmosphere, IN.uv);

				f2a OUT;
				//evaporated field newValue
				OUT.col0 = float4(newValue, oldValue.y, oldValue.z, oldValue.w);
				//OUT.col0 = float4(1,1,1,1);
				//atmosphere field
				OUT.col1 = float4(atmospehere.x + change, atmospehere.y, atmospehere.z, atmospehere.w);
				//OUT.col1 = float4(1,1,1,1);
				return OUT;
			}

			ENDCG

		}
	}
}