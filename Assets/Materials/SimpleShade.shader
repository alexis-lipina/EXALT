/// <summary>
/// EXTREMELY simple shader - simply does additive blend, includes color, use a white gradient with this
/// </summary>

Shader "Custom/SimpleShade"
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
			Blend Zero SrcColor

			CGPROGRAM
			#pragma vertex vert             
			#pragma fragment frag

			struct vertInput
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD;
				fixed4 color : COLOR;
			};

			struct vertOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD;
				float4 color : COLOR;
			};

			vertOutput vert(vertInput input)
			{
				vertOutput o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.uv = input.uv;
				o.color = input.color;
				return o;
			}

			sampler2D _MainTex;
			float4 _Color;
			float _Opacity;

			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
				color *= output.color;
				color.a = output.color.a * _Opacity;
				color = color * (_Opacity) + float4(1, 1, 1, 0) * (1 - _Opacity);
				return color;
			}
			ENDCG
		}
	}
}