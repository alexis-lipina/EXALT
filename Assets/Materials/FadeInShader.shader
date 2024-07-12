Shader "Unlit/FadeInShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_FadeGradientTex("Fade Gradient", 2D) = "white" {}
		_SourceColor("Source Color", Color) = (1, 1, 1, 1)
		_FadeInMask("Fade-in Mask", Float) = 0.0
		_GradientSize("Gradient size", Float) = 1.0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		Pass
		{
			//ZWrite Off
			Blend SrcAlpha One//OneMinusSrcAlpha

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
			sampler2D _FadeGradientTex;
			float4 _SourceColor;
			float _FadeInMask;
			float _GradientSize;

			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
				float4 outColor;
				outColor = tex2D(_FadeGradientTex, float2(clamp((_FadeInMask * 2 - color.b) * _GradientSize, 0, 0.99), 0.5));
				outColor.a *= color.a;
				return outColor;
			}
			ENDCG
		}
	}
}
