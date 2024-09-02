/// <summary>
/// Used for glowy murals that are built over time
/// </summary>
Shader "Custom/GlowMuralShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MaskTex("Gradient Mask", 2D) = "black" {}
		_ElementGradient("Element Gradient", 2D) = "white"
		_GlowLowerBound("Glow Lower Bound", Float) = 0.25
		_GlowUpperBound("Glow Upper Bound", Float) = 0.75
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
			sampler2D _MaskTex;
			sampler2D _ElementGradient;
			float _GlowLowerBound;
			float _GlowUpperBound;

			float invLerp(float from, float to, float value)
			{
				return (value - from) / (to - from);
			}

			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv); // base color for the surfaces
				float4 mask = tex2D(_MaskTex, output.uv); // sample glow sprite for intensity

				float4 newcolor = tex2D(_ElementGradient, float2(invLerp(_GlowLowerBound, _GlowUpperBound, mask.r), 0.5f));
				return newcolor;
			}

			
			ENDCG
		}
	}
}