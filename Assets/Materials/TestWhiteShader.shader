// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/TestWhiteShader" 
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MaskOn("Mask On (0 = no, else = yes)", Float) = 0
		_MaskColor("Mask Color", Color) = (1, 1, 1, 1)
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

			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
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