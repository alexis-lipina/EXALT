// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/TestWhiteShader" 
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MaskOn("Mask On (0 = no, else = yes)", Float) = 0
		_MaskColor("Mask Color", Color) = (1, 1, 1, 1)

		_Outline("Outline", Float) = 0
		_OutlineColor("Outline Color", Color) = (1, 1, 1, 1)

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

				return color;
			}
			ENDCG
		}
	}
}