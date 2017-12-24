
Shader "Erosion/SetRandomValue"
{
	//UNITY_SHADER_NO_UPGRADE
	Properties
	{
		_MainTex("MainTex", 2D) = "black" { }
		_Chance("Chance", Int) = 100
		_Limits("_Limits", Vector) = (1,1,1,1)
	}
		SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma enable_d3d11_debug_symbols

			sampler2D _MainTex;
			uniform float4 _Limits;
			uniform int _Chance;
			int Random(int min, int max, float2 uv)    //Pass this function a minimum and maximum value, as well as your texture UV
			{
				if (min > max)
					return 1;        //If the minimum is greater than the maximum, return a default value

				float cap = max - min;    //Subtract the minimum from the maximum
				int rand = tex2D(_MainTex, uv + _Time.x).r * cap + min;    //Make the texture UV random (add time) and multiply noise texture value by the cap, then add the minimum back on to keep between min and max 
				return rand;    //Return this value
			}
			////////////////////////////////////////////////////////////////////////////////
			// Source
			// http://www.gamedev.net/topic/592001-random-number-generation-based-on-time-in-hlsl/
			// Supposebly from the NVidia Direct3D10 SDK
			// Slightly modified for my purposes
#define RANDOM_IA 16807
#define RANDOM_IM 2147483647
#define RANDOM_AM (1.0f/float(RANDOM_IM))
#define RANDOM_IQ 127773u
#define RANDOM_IR 2836
#define RANDOM_MASK 123459876

			struct NumberGenerator {
				int seed; // Used to generate values.

						  // Returns the current random float.
				float GetCurrentFloat() {
					Cycle();
					return RANDOM_AM * seed;
				}

				// Returns the current random int.
				int GetCurrentInt()
				{
					Cycle();
					return seed;
				}

				// Generates the next number in the sequence.
				void Cycle() {
					seed ^= RANDOM_MASK;
					int k = seed / RANDOM_IQ;
					seed = RANDOM_IA * (seed - k * RANDOM_IQ) - RANDOM_IR * k;

					if (seed < 0)
						seed += RANDOM_IM;

					seed ^= RANDOM_MASK;
				}

				// Cycles the generator based on the input count. Useful for generating a thread unique seed.
				// PERFORMANCE - O(N)
				void Cycle(const uint _count) {
					for (uint i = 0; i < _count; ++i)
						Cycle();
				}

				// Returns a random float within the input range.
				float GetRandomFloat(const float low, const float high) {
					float v = GetCurrentFloat();
					return low * (1.0f - v) + high * v;
				}

				// Sets the seed
				void SetSeed(const uint value) {
					seed = int(value);
					Cycle();
				}
			};
			////////////////////////////////////////////////////////////////////////////////
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
				NumberGenerator rnd;
				rnd.SetSeed(0);
				if (Random(0, _Chance, IN.uv) == 0)
				{
					float4 res;
					
					res.r = rnd.GetRandomFloat(0.0, _Limits.x);
					res.g = rnd.GetRandomFloat(0.0, _Limits.y);
					res.b = rnd.GetRandomFloat(0.0, _Limits.z);					
					res.a = rnd.GetRandomFloat(0.0, _Limits.w);
					/*res.x = rnd.GetRandomFloat(0.0, 10.0);
					res.y = rnd.GetRandomFloat(0.0, 20.0);
					res.z = rnd.GetRandomFloat(0.0, 30.0);
					res.w = rnd.GetRandomFloat(0.0, 10.0);*/
					/*res.r =  1.0;
					res.g = 1.0;
					res.b =  1.0;
					res.a = 1.0;*/
					return res;
				}
					else
						return tex2D(_MainTex, IN.uv);
			}
			ENDCG
	}
	}
}