/// <summary>
/// Glow shader for generic, single-color glows
/// </summary>
Shader "Custom/GlowShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
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
			float _Opacity;

			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
				color.a = color.a * _Opacity;
				return color;
			}
			ENDCG
		}
	}
}