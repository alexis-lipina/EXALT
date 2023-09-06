// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// I believe this material is used on our enemies, since it has damage-flashing and has outline color stuff for when enemies are shielded.
Shader "Custom/TestWhiteShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}

		_Outline("Outline", Float) = 0
		_OutlineColor("Outline Color", Color) = (1, 1, 1, 1)

		_CrystalTex("Crystal Noise Texture", 2D) = "white" {}
		_CrystallizationAmount("Crystallization Amount", Float) = 0
		_IchorGradient("Ichor Gradient", 2D) = "white" {}

		_FireTex("Fire Noise Texture", 2D) = "white" {}
		_FireAmount("Fire Amount", Float) = 0
		_FireGradient("Fire Gradient", 2D) = "white" {}


		//_Time("Time", Color) = (1, 1, 1, 1)
	}

		SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
		}
		Pass
		{
			//Blend SrcAlpha One
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert             
			#pragma fragment frag

			struct vertInput
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD;
			};

			struct vertOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD;
			};

			vertOutput vert(vertInput input)
			{
				vertOutput o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.uv = input.uv;
				return o;
			}

			sampler2D _MainTex;
			float4 _MaskColor;
			float _MaskOn;
			float _Outline;
			float4 _OutlineColor;
			float4 _MainTex_TexelSize;

			sampler2D _CrystalTex;
			float _CrystallizationAmount;
			sampler2D _IchorGradient;
			float _IchorFreezeBreak;


			sampler2D _FireTex;
			float _FireAmount;
			sampler2D _FireGradient;

			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
				//return 0;
				// fire pattern
				/*  [ ] CURRENT PIXEL
			 L1 [ ] [ ] 1 [ ] R1
				* 	[ ] 2
				* 	[ ] 3
				* 	[ ] 4
				* 	[ ] 5
				* 	[ ] 6
				* 	[ ] 7
				* 	[ ] 8
				*/
				// FIRE
				if (color.a == 0 && _FireAmount > 0)
				{
					float4 p1 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 1));
					float4 p2 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 2));
					float4 p3 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 3));
					float4 p4 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 4));
					float4 p5 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 5));
					float4 p6 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 6));
					float4 p7 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 7));
					float4 p8 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 8));
					float4 l1 = tex2D(_MainTex, output.uv - fixed2(_MainTex_TexelSize.x * 1, _MainTex_TexelSize.y * 1));
					float4 r1 = tex2D(_MainTex, output.uv - fixed2(_MainTex_TexelSize.x * -1, _MainTex_TexelSize.y * 1));

					const float p1Weight = 1.0f;
					const float p2Weight = 0.7f;
					const float p3Weight = 0.6f;
					const float p4Weight = 0.5f;
					const float p5Weight = 0.4f;
					const float p6Weight = 0.3f;
					const float p7Weight = 0.2f;
					const float p8Weight = 0.1f;
					const float l1Weight = 0.6f;
					const float r1Weight = 0.6f;


					//create "mask value" based on sum of pixel alphas below. if all alphas are 0, will render nothing. 
					/*float mask = clamp(
						p1.a * p1Weight +
						p2.a * p2Weight +
						p3.a * p3Weight +
						p4.a * p4Weight +
						p5.a * p5Weight
					, 0.0f, 1.0f);*/

					float mask = max(p1.a * p1Weight,
						max(p2.a * p2Weight,
							max(p3.a * p3Weight,
								max(p4.a * p4Weight,
									max(p5.a * p5Weight,
										max(p6.a * p6Weight,
											max(p7.a * p7Weight,
												max(p8.a * p8Weight,
													max(l1.a * l1Weight,
														r1.a * r1Weight)))))))));

					mask *= pow(_FireAmount * 3.0f, 0.4f);

					float noisesample = tex2D(_FireTex, float2(output.uv.x, abs(fmod(output.uv.y * 6 + _Time.g * -0.5, 1.0f))));

					return tex2D(_FireGradient, float2(noisesample + mask * 1.8 - 0.8f, 0.5f));
				}
				//color = tex2D(_CrystalTex, float2(output.uv.x, fmod(output.uv.y * 6 + _Time.g * 0.5, 1.0f)));

				//return fmod(_Time, 1.0f);

				// ICHOR FREEZE
				if (_CrystallizationAmount >= 1.0f)
				{
					/*         N3
					*          N2
					*          N1
					* W3 W2 W1[PX]E1 E2 E3
					*          S1
					*          S2
					*          S3
					*/
					const float4 n1 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +0, _MainTex_TexelSize.y * +1));
					const float4 n2 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +0, _MainTex_TexelSize.y * +2));
					const float4 n3 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +0, _MainTex_TexelSize.y * +3));
					const float4 n4 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +0, _MainTex_TexelSize.y * +4));
					const float4 n5 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +0, _MainTex_TexelSize.y * +5));

					const float4 s1 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +0, _MainTex_TexelSize.y * -1));
					const float4 s2 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +0, _MainTex_TexelSize.y * -2));
					const float4 s3 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +0, _MainTex_TexelSize.y * -3));
					const float4 s4 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +0, _MainTex_TexelSize.y * -4));
					const float4 s5 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +0, _MainTex_TexelSize.y * -5));

					const float4 w1 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * -0, _MainTex_TexelSize.y * +0));
					const float4 w2 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * -1, _MainTex_TexelSize.y * +0));
					const float4 w3 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * -2, _MainTex_TexelSize.y * +0));
					const float4 w4 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * -3, _MainTex_TexelSize.y * +0));
					const float4 w5 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * -4, _MainTex_TexelSize.y * +0));

					const float4 e1 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +0, _MainTex_TexelSize.y * +0));
					const float4 e2 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +1, _MainTex_TexelSize.y * +0));
					const float4 e3 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +2, _MainTex_TexelSize.y * +0));
					const float4 e4 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +3, _MainTex_TexelSize.y * +0));
					const float4 e5 = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * +4, _MainTex_TexelSize.y * +0));

					const float xScale1 = 1.0f;
					const float xScale2 = 0.4f;
					const float xScale3 = 0.4f;
					const float xScale4 = 0.1f;
					const float xScale5 = 0.1f;

					const float yScale1 = 1.0f;
					const float yScale2 = 0.7f;
					const float yScale3 = 0.7f;
					const float yScale4 = 0.5f;
					const float yScale5 = 0.5f;


					// READ WHEN POLISHING
					/*
					* Grungey noise with lots of different values and grit that looks good at larger scale does
					* not translate as well to pixel art. you get weird orphan pixels and muddy undefined shapes.
					* Make the noise textures more clear rectilinear patterns, maybe with slightly different values
					* 
					* Maybe we dont want this gradienty effect with the kernel since the pixels will be extremely stepped? Chunks of 3?
					*/
					float mask = 0; 
					mask =
						color.a +
						n1.a * yScale1 +
						n2.a * yScale2 +
						n3.a * yScale3 +
						n4.a * yScale4 +
						n5.a * yScale5 +
						s1.a * yScale1 +
						s2.a * yScale2 +
						s3.a * yScale3 +
						s4.a * yScale4 +
						s5.a * yScale5 +
						e1.a * xScale1 +
						e2.a * xScale2 +
						e3.a * xScale3 +
						e4.a * xScale4 +
						e5.a * xScale5 +
						w1.a * xScale1 +
						w2.a * xScale2 +
						w3.a * xScale3 +
						w4.a * xScale4 +
						w5.a * xScale5;
					mask *= 0.5f;

					mask =
						max(color.a, 
						max(n1.a * yScale1,
						max(n2.a * yScale2,
						max(n3.a * yScale3,
						max(n4.a * yScale4,
						max(n5.a * yScale5,
						max(s1.a * yScale1,
						max(s2.a * yScale2,
						max(s3.a * yScale3,
						max(s4.a * yScale4,
						max(s5.a * yScale5,
						max(e1.a * xScale1,
						max(e2.a * xScale2,
						max(e3.a * xScale3,
						max(e4.a * xScale4,
						max(e5.a * xScale5,
						max(w1.a * xScale1,
						max(w2.a * xScale2,
						max(w3.a * xScale3,
						max(w4.a * xScale4,
						w5.a * xScale5))))))))))))))))))));
					//return float4(output.uv.x, output.uv.y, 0, 1);
					float maskvalue = step(1.0f, mask + tex2D(_CrystalTex, output.uv) * 0.99);
					float2 offsetuv = float2(fmod(output.uv.x + 0.1f, 1.0f), fmod(output.uv.y + 0.2f, 1.0f));
					return float4(1, 0, 0.5f, maskvalue) + step(0.5f, tex2D(_FireTex, output.uv)) * _IchorFreezeBreak * maskvalue;

					//return tex2D(_IchorGradient, float2(output.uv.y * 1.0f, 0.5f)) * float4(1, 1, 1, maskvalue);
					//return tex2D(_IchorGradient, float2(noisesample + mask * 1.8 - 0.8f, 0.5f)); step(1.0f, mask + tex2D(_CrystalTex, output.uv) * 0.99);
				}

				return 0;
			}
			ENDCG
		}
	}
}