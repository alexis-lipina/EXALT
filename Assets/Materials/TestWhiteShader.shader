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

				// fire pattern
				/*  [ ] CURRENT PIXEL
				*   [ ] 1
				* 	[ ] 2	
				* 	[ ] 3	
				* 	[ ] 4	
				* 	[ ] 5	
				*/ 
				/*
				if (color.a ==  0)
				{
					float4 p1 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 1));
					float4 p2 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 2));
					float4 p3 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 3));
					float4 p4 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 4));
					float4 p5 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 5));
					float4 p6 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 6));
					float4 p7 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 7));
					float4 p8 = tex2D(_MainTex, output.uv - fixed2(0, _MainTex_TexelSize.y * 8));

					const float p1Weight = 1.0f;
					const float p2Weight = 0.7f;
					const float p3Weight = 0.6f;
					const float p4Weight = 0.5f;
					const float p5Weight = 0.4f;
					const float p6Weight = 0.3f;
					const float p7Weight = 0.2f;
					const float p8Weight = 0.1f;

					
					//create "mask value" based on sum of pixel alphas below. if all alphas are 0, will render nothing. 
					float mask = clamp(
						p1.a * p1Weight +
						p2.a * p2Weight +
						p3.a * p3Weight +
						p4.a * p4Weight +
						p5.a * p5Weight
					, 0.0f, 1.0f);

					mask = max(p1.a * p1Weight,
						max(p2.a * p2Weight,
							max(p3.a * p3Weight,
								max(p4.a * p4Weight,
									max(p5.a * p5Weight,
										max(p6.a * p5Weight, 
											max(p7.a * p5Weight, 
												p8.a * p5Weight)))))));

					float noisesample = tex2D(_CrystalTex, float2(output.uv.x, abs(fmod(output.uv.y * 6 + _Time.g * -0.5, 1.0f))));

					return tex2D(_FireGradient, float2(noisesample + mask * 1.8 - 0.8f, 0.5f));
				}*/
				//color = tex2D(_CrystalTex, float2(output.uv.x, fmod(output.uv.y * 6 + _Time.g * 0.5, 1.0f)));

				//return fmod(_Time, 1.0f);

				return color;
			}
			ENDCG
		}
	}
}