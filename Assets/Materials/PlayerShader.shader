Shader "Unlit/PlayerShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MaskOn("Mask On (0 = no, else = yes)", Float) = 0
		_MaskColor("Mask Color", Color) = (1, 1, 1, 1)

		//_MagicColor("Magic Color", Color) = (1, 0, 0, 1)
		_SourceColor("Source Color", Color) = (1, 1, 1, 1)
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
				Cull Off

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
				float4 _MagicColor;
				float4 _SourceColor;
				float4 _PaletteSwapA;
				float4 _PaletteSwapB;
				float4 _PaletteSwapC;
				float4 _PaletteSwapD;
				float4 _PaletteSwapE;
				float4 _PaletteSwapF;
				float4 _PaletteSwapG;

				float4 frag(vertOutput output) : COLOR
				{
					float4 color = tex2D(_MainTex, output.uv);
					
					//palette swap
					if (color.r == 0.0 && color.g == 0.0 && color.b == 0.09803921568 && color.a != 0)
					{
						color = _PaletteSwapA;
					}
					if (color.r == 0.0 && color.g == 0.08235294117 && color.b == 0.15294117647 && color.a != 0)
					{
						color = _PaletteSwapB;
					}
					if (color.r == 0.01176470588 && color.g == 0.15686274509 && color.b == 0.21960784313 && color.a != 0)
					{
						color = _PaletteSwapC;
					}
					if (color.r == 53/255.0 && color.g == 21/255.0 && color.b == 39/255.0 && color.a != 0)
					{
						color = _PaletteSwapD;
					}
					if (color.r == 36.0/255.0 && color.g == 15.0/255.0 && color.b == 27.0/255.0 && color.a != 0)
					{
						color = _PaletteSwapE;
					}
					if (color.r == 32.0 / 255.0 && color.g == 32.0 / 255.0 && color.b == 43.0 / 255.0 && color.a != 0)
					{
						color = _PaletteSwapF;
					}
					if (color.r == 19.0 / 255.0 && color.g == 20.0 / 255.0 && color.b == 32.0 / 255.0 && color.a != 0)
					{
						color = _PaletteSwapG;
					}

					/*
					if (abs(color.b - _SourceColor.b) < 0.1 && abs(color.r - _SourceColor.r) < 0.1 && abs(color.g - _SourceColor.g) < 0.1 && color.a != 0)
					{
						color = _MagicColor;
					}*/

					if (color.r == 0.0 / 255.0 && color.g == 255.0 / 255.0 && color.b == 153.0 / 255.0 && color.a != 0)
					{
						color = _MagicColor;
					}
					
					if (color.a != 0 && _MaskOn != 0)
					{
						color = _MaskColor;
					}
					return color;
				}
				ENDCG
			}
		}
}
