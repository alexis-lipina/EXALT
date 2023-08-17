// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// I believe this material is used on our enemies, since it has damage-flashing and has outline color stuff for when enemies are shielded.
Shader "Custom/TestWhiteShader" 
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MaskOn("Mask On (0 = no, else = yes)", Float) = 0
		_MaskColor("Mask Color", Color) = (1, 1, 1, 1)

		_Outline("Outline", Float) = 0
		_OutlineColor("Outline Color", Color) = (1, 1, 1, 1)

		_CrystalTex("Crystal Noise Texture", 2D) = "white" {}
		_CrystallizationAmount("Crystallization Amount", Float) = 0

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

			sampler2D _FireGradient;


			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
				if (color.a != 0 && _MaskOn != 0)
				{
					color = _MaskColor;
				}

				// If outline is enabled and there is a pixel, try to draw an outline for the inside
				if (_Outline > 0 && color.a != 0) {
					// Get the neighbouring four pixels.
					float4 pixelUp = tex2D(_MainTex, output.uv + fixed2(0, _MainTex_TexelSize.y));
					float4 pixelDown = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y));
					float4 pixelRight = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x, 0));
					float4 pixelLeft = tex2D(_MainTex, output.uv - fixed2(_MainTex_TexelSize.x, 0));

					// If one of the neighbouring pixels is invisible, we render an outline.
					if (pixelUp.a * pixelDown.a * pixelRight.a * pixelLeft.a == 0) {
						color.rgba = float4(1, 1, 1, 1); // * _OutlineColor;
					}
				}
				//draw outer outline
				if (_Outline > 0 && color.a == 0) {
					// Get the neighbouring four pixels.
					float4 pixelUp = tex2D(_MainTex, output.uv + fixed2(0, _MainTex_TexelSize.y));
					float4 pixelDown = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y));
					float4 pixelRight = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x, 0));
					float4 pixelLeft = tex2D(_MainTex, output.uv - fixed2(_MainTex_TexelSize.x, 0));


					float4 pixelUpLeft = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * -1, _MainTex_TexelSize.y));
					float4 pixelDownLeft = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x * -1, _MainTex_TexelSize.y * -1));
					float4 pixelUpRight = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x, _MainTex_TexelSize.y));
					float4 pixelDownRight = tex2D(_MainTex, output.uv + fixed2(_MainTex_TexelSize.x, _MainTex_TexelSize.y * -1));
					// If one of the neighbouring pixels is visible, we render an outline.
					if (pixelUp.a + pixelDown.a + pixelRight.a + pixelLeft.a + pixelUpLeft.a + pixelUpRight.a + pixelDownLeft.a + pixelDownRight.a > 0) {
						color.rgba = float4(1, 1, 1, 1) * _OutlineColor;
					}
				}

				//Ichor freezing effect
				float ichorFreezeAmount = 0.45f;
				if (ichorFreezeAmount > 0)
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
					const float xScale2 = 0.2f;
					const float xScale3 = 0.2f;
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
						1.0f - n1.a * yScale1 +
						1.0f - n2.a * yScale2 +
						1.0f - n3.a * yScale3 +
						1.0f - n4.a * yScale4 +
						1.0f - n5.a * yScale5 +
						1.0f - s1.a * yScale1 +
						1.0f - s2.a * yScale2 +
						1.0f - s3.a * yScale3 +
						1.0f - s4.a * yScale4 +
						1.0f - s5.a * yScale5 +
						1.0f - e1.a * xScale1 +
						1.0f - e2.a * xScale2 +
						1.0f - e3.a * xScale3 +
						1.0f - e4.a * xScale4 +
						1.0f - e5.a * xScale5 +
						1.0f - w1.a * xScale1 +
						1.0f - w2.a * xScale2 +
						1.0f - w3.a * xScale3 +
						1.0f - w4.a * xScale4 +
						1.0f - w5.a * xScale5;
					mask *= 0.5f;

					mask =
						//max(color.a,
						max((1.0f - n1.a) * yScale1,
						max((1.0f - n2.a) * yScale2,
						max((1.0f - n3.a) * yScale3,
						max((1.0f - n4.a) * yScale4,
						max((1.0f - n5.a) * yScale5,
						max((1.0f - s1.a) * yScale1,
						max((1.0f - s2.a) * yScale2,
						max((1.0f - s3.a) * yScale3,
						max((1.0f - s4.a) * yScale4,
						max((1.0f - s5.a) * yScale5,
						max((1.0f - e1.a) * xScale1,
						max((1.0f - e2.a) * xScale2,
						max((1.0f - e3.a) * xScale3,
						max((1.0f - e4.a) * xScale4,
						max((1.0f - e5.a) * xScale5,
						max((1.0f - w1.a) * xScale1,
						max((1.0f - w2.a) * xScale2,
						max((1.0f - w3.a) * xScale3,
						max((1.0f - w4.a) * xScale4,
							(1.0f - w5.a) * xScale5)))))))))))))))))));

					float noisetexpix = tex2D(_CrystalTex, output.uv);
					color = color = color * (1 - step(ichorFreezeAmount, noisetexpix)) + float4(1, 1, 0.5f, 1) * step(ichorFreezeAmount, noisetexpix) * color.a;
					float maskvalue = step(0.5f, mask + tex2D(_CrystalTex, output.uv) * 0.99);
					//color = color * (1 - maskvalue) + float4(1, 0, 0.5f, 1) * maskvalue * color.a;
					//return float4(1, 0, 0.5f, maskvalue);

					//return tex2D(_IchorGradient, float2(output.uv.y * 1.0f, 0.5f)) * float4(1, 1, 1, maskvalue);
				}

				return color;
			}
			ENDCG
		}
	}
}