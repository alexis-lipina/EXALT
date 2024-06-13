/// <summary>
/// Shader used for sky stars, which need to flicker with white cores in a funky way. Wanted to do this in a shader.
/// </summary>
Shader "Custom/StarShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_ColorGradient("Element Gradient", 2D) = "white"
		_LowerBound("Lower Bound", Float) = 0
		_UpperBound("Upper Bound", Float) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
		}

		Pass
		{
			Blend SrcAlpha One
			//SetTexture[_MainTex] {combine texture }


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
			sampler2D _ColorGradient;
			float _LowerBound;
			float _UpperBound;

			float map(float s, float a1, float a2, float b1, float b2)
			{
				return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
			}

			float4 frag(vertOutput output) : COLOR
			{
				float originalPixelValue = tex2D(_MainTex, output.uv).r;
				float remappedPixelValue = map(originalPixelValue, 0, 1, _LowerBound, _UpperBound);
				remappedPixelValue = clamp(remappedPixelValue, 0.01, 0.99) * step(0.01, originalPixelValue);
				return tex2D(_ColorGradient, float2(remappedPixelValue, 0.5f));


				/* So theres a couple ways this could work.
				*	1) using the element gradient map (which includes a color ramp and a white threshold) and an upper and lower bound, sample values from the texture and determine their color based on the gradient.
				*/

				
				//const float elementMapOffset = 1 - (_CurrentElement * 0.25f - 0.125f);
				//float bloom = clamp(1, 10, _Opacity); // allows overloading opacity for overbrightness
				//float intensity = clamp(0, 1, _Opacity);
				//float4 color = tex2D(_MainTex, output.uv); // sample glow sprite for intensity
				//color *= intensity; // scale intensity
				//color = tex2D(_ElementGradients, float2(color.r, 0.5f));
				//return color * bloom * 1;
			}
			ENDCG
		}
	}
}