/// <summary>
/// All emissive auras for EXALTs elements should make use of this shader.
/// </summary>
Shader "Custom/ElementalGlowShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_ElementGradients("Element Gradients", 2D) = "white"
		_CurrentElement("Current Element", Float) = 1
		_Opacity("Opacity", Float) = 1
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
			sampler2D _ElementGradients;
			float _CurrentElement; // (1=ichor, 2=sol, 3=rift, 4=storm)
			float _Opacity;

			float4 frag(vertOutput output) : COLOR
			{
				const float elementMapOffset = 1 - (_CurrentElement * 0.25f - 0.125f);
				float bloom = clamp(1, 10, _Opacity); // allows overloading opacity for overbrightness
				float intensity = clamp(0, 1, _Opacity);
				float4 color = tex2D(_MainTex, output.uv); // sample glow sprite for intensity
				color *= intensity; // scale intensity
				color = tex2D(_ElementGradients, float2(color.r, elementMapOffset));
				return color * bloom * 1;
			}
			ENDCG
		}
	}
}